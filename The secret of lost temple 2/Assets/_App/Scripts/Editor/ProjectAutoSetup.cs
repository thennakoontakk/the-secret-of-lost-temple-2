#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

namespace SecretOfLostTemple.EditorScripts
{
    [InitializeOnLoad]
    public static class ProjectAutoSetup
    {
        static ProjectAutoSetup()
        {
            EditorApplication.delayCall += RunAutoSetupOnce;
        }

        [MenuItem("Lost Temple/Setup Project Scenes & Splash", false, 1)]
        public static void SetupAllScenesManual()
        {
            SetupAllScenes(true);
        }

        private static void RunAutoSetupOnce()
        {
            string flagKey = "LostTemple_InitialSetupDone_v5";
            if (!SessionState.GetBool(flagKey, false))
            {
                SessionState.SetBool(flagKey, true);
                SetupAllScenes(false); // Non-destructive: preserves existing manual scene modifications!
            }
        }

        private static void SetupAllScenes(bool force)
        {
            string scenesDir = "Assets/_App/Scenes";
            if (!AssetDatabase.IsValidFolder(scenesDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_App"))
                {
                    AssetDatabase.CreateFolder("Assets", "_App");
                }
                AssetDatabase.CreateFolder("Assets/_App", "Scenes");
            }

            string splashPath = $"{scenesDir}/SplashScreen.unity";
            string authPath = $"{scenesDir}/AuthScene.unity";
            string homeMapPath = $"{scenesDir}/HomeMapScene.unity";
            string arPath = $"{scenesDir}/MainARScene.unity";
            string freePracticePath = $"{scenesDir}/FreePracticeScene.unity";
            string physics3DPath = $"{scenesDir}/Physics3DLabScene.unity";

            bool needCreateSplash = force || !File.Exists(splashPath);
            bool needCreateAuth = force || !File.Exists(authPath);
            bool needCreateHome = force || !File.Exists(homeMapPath);
            bool needCreateAR = force || !File.Exists(arPath);
            bool needCreatePhysics3D = force || !File.Exists(physics3DPath);

            EnsureTargetSprites();

            if (needCreateSplash) CreateSplashScreen(splashPath);
            if (needCreateAuth) CreateAuthScene(authPath);
            if (needCreateHome) CreateHomeMapScene(homeMapPath);
            if (needCreateAR) CreateMainARScene(arPath);
            if (needCreatePhysics3D) CreatePhysics3DLabScene(physics3DPath);

            // Update Build Settings
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(splashPath, true),
                new EditorBuildSettingsScene(authPath, true),
                new EditorBuildSettingsScene(homeMapPath, true),
                new EditorBuildSettingsScene(arPath, true),
                new EditorBuildSettingsScene(freePracticePath, File.Exists(freePracticePath)),
                new EditorBuildSettingsScene(physics3DPath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Open Splash Screen scene by default
            if (File.Exists(splashPath))
            {
                EditorSceneManager.OpenScene(splashPath);
            }

            Debug.Log("<color=green>[Lost Temple] All Scenes and Splash Screen configured successfully!</color>");
        }

        private static void CreateSplashScreen(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject camObj = new GameObject("Splash Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";

            // 2. Video Player
            GameObject videoObj = new GameObject("Video Splash Controller");
            VideoPlayer vp = videoObj.AddComponent<VideoPlayer>();
            AudioSource audioSrc = videoObj.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;

            VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Art/Animations/Three_heroes_stand_in_rainforest_202608291018.mp4");
            if (clip != null)
            {
                vp.clip = clip;
            }
            vp.renderMode = VideoRenderMode.CameraNearPlane;
            vp.targetCamera = cam;
            vp.aspectRatio = VideoAspectRatio.FitOutside;
            vp.playOnAwake = true;
            vp.isLooping = false;
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
            vp.SetTargetAudioSource(0, audioSrc);

            // 3. UI Canvas (Skip prompt & Fade transition)
            GameObject canvasObj = new GameObject("Splash Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Fade Overlay
            GameObject fadeObj = new GameObject("FadeOverlay");
            fadeObj.transform.SetParent(canvasObj.transform, false);
            RectTransform fadeRect = fadeObj.AddComponent<RectTransform>();
            fadeRect.anchorMin = Vector2.zero;
            fadeRect.anchorMax = Vector2.one;
            fadeRect.sizeDelta = Vector2.zero;
            Image fadeImg = fadeObj.AddComponent<Image>();
            fadeImg.color = Color.black;
            fadeImg.raycastTarget = false;
            CanvasGroup fadeGroup = fadeObj.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;

            // 4. Attach SplashScreenController
            var splashCtrl = videoObj.AddComponent<SecretOfLostTemple.Core.SplashScreenController>();
            var so = new SerializedObject(splashCtrl);
            so.FindProperty("videoPlayer").objectReferenceValue = vp;
            so.FindProperty("nextSceneName").stringValue = "AuthScene";
            so.FindProperty("fadeCanvasGroup").objectReferenceValue = fadeGroup;
            so.FindProperty("skipOnTap").boolValue = true;
            so.FindProperty("minimumPlayTime").floatValue = 1.5f;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void CreateAuthScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.09f, 0.08f, 1f);
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";

            // 2. EventSystem (StandaloneInputModule for universal click & touch handling)
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 3. Canvas
            GameObject canvasObj = new GameObject("Auth Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 4. Background Image (RawImage for reliable JPG rendering)
            GameObject bgObj = new GameObject("BackgroundImage");
            bgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            RawImage bgImg = bgObj.AddComponent<RawImage>();
            Texture bgTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/background.jpeg");
            if (bgTexture != null) bgImg.texture = bgTexture;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            // Load Card Sprites, Character Sprites, Aura Glow, Title & Buttons
            Sprite loginCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/login card.png");
            Sprite signupCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/signup card.png");
            Sprite mayaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/maya character.png");
            Sprite senukSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/senuk character.png");
            Sprite backBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/back_button.png");
            Sprite auraSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/character_glow_aura.png");
            Sprite chooseTitleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/choose_character_title.png");
            Sprite confirmBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/confirm_button.png");

            // --- 5. LOGIN PANEL ---
            GameObject loginPanel = new GameObject("LoginPanel");
            loginPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform lpRect = loginPanel.AddComponent<RectTransform>();
            lpRect.anchorMin = new Vector2(0.5f, 0.5f);
            lpRect.anchorMax = new Vector2(0.5f, 0.5f);
            lpRect.sizeDelta = new Vector2(900, 820);
            Image lpImg = loginPanel.AddComponent<Image>();
            if (loginCardSprite != null) lpImg.sprite = loginCardSprite;
            lpImg.color = Color.white;

            // Login Panel -> Switch to Sign Up Button (Top Right stone button)
            GameObject lToSBtnObj = new GameObject("Button_SwitchToSignup");
            lToSBtnObj.transform.SetParent(loginPanel.transform, false);
            RectTransform lToSRect = lToSBtnObj.AddComponent<RectTransform>();
            lToSRect.anchorMin = new Vector2(0.52f, 0.60f);
            lToSRect.anchorMax = new Vector2(0.78f, 0.68f);
            lToSRect.sizeDelta = Vector2.zero;
            Button lToSBtn = lToSBtnObj.AddComponent<Button>();
            Image lToSImg = lToSBtnObj.AddComponent<Image>();
            lToSImg.color = new Color(1f, 0.95f, 0.6f, 0.005f);

            // Login Panel -> Username Input
            var loginUser = CreateCardInput(loginPanel, "InputField_LoginUsername", 0.21f, 0.79f, 0.46f, 0.54f, "Enter Explorer Name / Email...", false);
            // Login Panel -> Password Input
            var loginPass = CreateCardInput(loginPanel, "InputField_LoginPassword", 0.21f, 0.79f, 0.33f, 0.41f, "Enter Secret Password...", true);

            // Login Panel -> Submit Button (Bottom LOGIN stone button)
            GameObject lSubBtnObj = new GameObject("Button_LoginSubmit");
            lSubBtnObj.transform.SetParent(loginPanel.transform, false);
            RectTransform lSubRect = lSubBtnObj.AddComponent<RectTransform>();
            lSubRect.anchorMin = new Vector2(0.22f, 0.18f);
            lSubRect.anchorMax = new Vector2(0.78f, 0.28f);
            lSubRect.sizeDelta = Vector2.zero;
            Button lSubBtn = lSubBtnObj.AddComponent<Button>();
            Image lSubImg = lSubBtnObj.AddComponent<Image>();
            lSubImg.color = new Color(1f, 0.95f, 0.6f, 0.005f);

            // --- 6. SIGNUP PANEL ---
            GameObject signupPanel = new GameObject("SignupPanel");
            signupPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform spRect = signupPanel.AddComponent<RectTransform>();
            spRect.anchorMin = new Vector2(0.5f, 0.5f);
            spRect.anchorMax = new Vector2(0.5f, 0.5f);
            spRect.sizeDelta = new Vector2(1050, 700);
            Image spImg = signupPanel.AddComponent<Image>();
            if (signupCardSprite != null) spImg.sprite = signupCardSprite;
            spImg.color = Color.white;
            signupPanel.SetActive(false);

            // Signup Panel -> Back Button (Top Left - clearly visible)
            GameObject sBackBtnObj = new GameObject("Button_SignupBack");
            sBackBtnObj.transform.SetParent(signupPanel.transform, false);
            RectTransform sBackRect = sBackBtnObj.AddComponent<RectTransform>();
            sBackRect.anchorMin = new Vector2(0f, 1f);
            sBackRect.anchorMax = new Vector2(0f, 1f);
            sBackRect.pivot = new Vector2(0.5f, 0.5f);
            sBackRect.anchoredPosition = new Vector2(55f, -55f);
            sBackRect.sizeDelta = new Vector2(90f, 90f);
            Button sBackBtn = sBackBtnObj.AddComponent<Button>();
            Image sBackImg = sBackBtnObj.AddComponent<Image>();
            if (backBtnSprite != null) sBackImg.sprite = backBtnSprite;
            sBackImg.preserveAspect = true;

            // Signup Panel -> Username Input
            var signUser = CreateCardInput(signupPanel, "InputField_SignupUsername", 0.29f, 0.71f, 0.58f, 0.67f, "Choose Explorer Name...", false);
            // Signup Panel -> Email Input
            var signEmail = CreateCardInput(signupPanel, "InputField_SignupEmail", 0.29f, 0.71f, 0.43f, 0.52f, "Enter Email Address...", false);
            // Signup Panel -> Password Input
            var signPass = CreateCardInput(signupPanel, "InputField_SignupPassword", 0.29f, 0.71f, 0.28f, 0.37f, "Create Secret Password (min 6 chars)...", true);

            // Signup Panel -> Submit Button (Bottom CREATE ACCOUNT stone button)
            GameObject sSubBtnObj = new GameObject("Button_SignupSubmit");
            sSubBtnObj.transform.SetParent(signupPanel.transform, false);
            RectTransform sSubRect = sSubBtnObj.AddComponent<RectTransform>();
            sSubRect.anchorMin = new Vector2(0.28f, 0.14f);
            sSubRect.anchorMax = new Vector2(0.72f, 0.24f);
            sSubRect.sizeDelta = Vector2.zero;
            Button sSubBtn = sSubBtnObj.AddComponent<Button>();
            Image sSubImg = sSubBtnObj.AddComponent<Image>();
            sSubImg.color = new Color(1f, 0.95f, 0.6f, 0.005f);

            // Signup Panel -> Back to Login Button
            GameObject sToLBtnObj = new GameObject("Button_BackToLogin");
            sToLBtnObj.transform.SetParent(signupPanel.transform, false);
            RectTransform sToLRect = sToLBtnObj.AddComponent<RectTransform>();
            sToLRect.anchorMin = new Vector2(0.30f, 0.02f);
            sToLRect.anchorMax = new Vector2(0.70f, 0.10f);
            sToLRect.sizeDelta = Vector2.zero;
            Button sToLBtn = sToLBtnObj.AddComponent<Button>();
            Image sToLImg = sToLBtnObj.AddComponent<Image>();
            sToLImg.color = new Color(0f, 0f, 0f, 0.01f);

            GameObject sToLTextObj = new GameObject("Text");
            sToLTextObj.transform.SetParent(sToLBtnObj.transform, false);
            RectTransform sToLTxtRect = sToLTextObj.AddComponent<RectTransform>();
            sToLTxtRect.anchorMin = Vector2.zero; sToLTxtRect.anchorMax = Vector2.one; sToLTxtRect.sizeDelta = Vector2.zero;
            Text sToLText = sToLTextObj.AddComponent<Text>();
            sToLText.text = "Already have an account? Log In";
            sToLText.alignment = TextAnchor.MiddleCenter;
            sToLText.fontSize = 20;
            sToLText.fontStyle = FontStyle.Bold;
            sToLText.color = new Color(0.95f, 0.85f, 0.45f, 0.95f);

            // --- 7. CHARACTER SELECT PANEL ---
            GameObject charPanel = new GameObject("CharacterSelectPanel");
            charPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform cpRect = charPanel.AddComponent<RectTransform>();
            cpRect.anchorMin = Vector2.zero;
            cpRect.anchorMax = Vector2.one;
            cpRect.sizeDelta = Vector2.zero;
            Image cpImg = charPanel.AddComponent<Image>();
            cpImg.color = new Color(0.05f, 0.08f, 0.06f, 0.88f);
            charPanel.SetActive(false);

            // Header Title Image ("CHOOSE YOUR CHARACTER")
            GameObject headerObj = new GameObject("Title_ChooseCharacter");
            headerObj.transform.SetParent(charPanel.transform, false);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.22f, 0.84f);
            headerRect.anchorMax = new Vector2(0.78f, 0.98f);
            headerRect.sizeDelta = Vector2.zero;
            Image headerImg = headerObj.AddComponent<Image>();
            if (chooseTitleSprite != null) headerImg.sprite = chooseTitleSprite;
            headerImg.preserveAspect = true;
            headerImg.raycastTarget = false;

            // Card Maya
            GameObject mayaCardObj = new GameObject("Card_Maya");
            mayaCardObj.transform.SetParent(charPanel.transform, false);
            RectTransform mayaRect = mayaCardObj.AddComponent<RectTransform>();
            mayaRect.anchorMin = new Vector2(0.12f, 0.22f);
            mayaRect.anchorMax = new Vector2(0.46f, 0.84f);
            mayaRect.sizeDelta = Vector2.zero;
            Image mayaCardImg = mayaCardObj.AddComponent<Image>();
            mayaCardImg.color = new Color(0f, 0f, 0f, 0.005f);
            Button mayaBtn = mayaCardObj.AddComponent<Button>();

            // Maya Glow Aura (Radiant backlight directly behind character silhouette)
            GameObject mayaGlowObj = new GameObject("MayaGlowBorder");
            mayaGlowObj.transform.SetParent(mayaCardObj.transform, false);
            RectTransform mayaGlowRect = mayaGlowObj.AddComponent<RectTransform>();
            mayaGlowRect.anchorMin = new Vector2(0f, 0.12f);
            mayaGlowRect.anchorMax = new Vector2(1f, 1.0f);
            mayaGlowRect.sizeDelta = new Vector2(60f, 60f);
            mayaGlowRect.pivot = new Vector2(0.5f, 0.5f);
            Image mayaGlowImg = mayaGlowObj.AddComponent<Image>();
            if (auraSprite != null) mayaGlowImg.sprite = auraSprite;
            mayaGlowImg.color = new Color(1f, 0.88f, 0.35f, 0.95f);
            mayaGlowImg.raycastTarget = false;

            // Maya Image (In front of aura)
            GameObject mayaImgObj = new GameObject("MayaImage");
            mayaImgObj.transform.SetParent(mayaCardObj.transform, false);
            RectTransform mayaImgRect = mayaImgObj.AddComponent<RectTransform>();
            mayaImgRect.anchorMin = new Vector2(0.05f, 0.16f);
            mayaImgRect.anchorMax = new Vector2(0.95f, 0.96f);
            mayaImgRect.sizeDelta = Vector2.zero;
            Image mayaImg = mayaImgObj.AddComponent<Image>();
            if (mayaSprite != null) mayaImg.sprite = mayaSprite;
            mayaImg.preserveAspect = true;

            // Maya Label
            GameObject mayaLblObj = new GameObject("MayaLabel");
            mayaLblObj.transform.SetParent(mayaCardObj.transform, false);
            RectTransform mayaLblRect = mayaLblObj.AddComponent<RectTransform>();
            mayaLblRect.anchorMin = new Vector2(0f, 0.02f);
            mayaLblRect.anchorMax = new Vector2(1f, 0.16f);
            mayaLblRect.sizeDelta = Vector2.zero;
            Text mayaLblText = mayaLblObj.AddComponent<Text>();
            mayaLblText.text = "<b>MAYA</b>\n<size=17><color=#d5c59f>Lore Scholar</color></size>";
            mayaLblText.fontSize = 24;
            mayaLblText.fontStyle = FontStyle.Bold;
            mayaLblText.alignment = TextAnchor.MiddleCenter;
            mayaLblText.color = new Color(1f, 0.95f, 0.8f, 1f);

            // Card Senuk
            GameObject senukCardObj = new GameObject("Card_Senuk");
            senukCardObj.transform.SetParent(charPanel.transform, false);
            RectTransform senukRect = senukCardObj.AddComponent<RectTransform>();
            senukRect.anchorMin = new Vector2(0.54f, 0.22f);
            senukRect.anchorMax = new Vector2(0.88f, 0.84f);
            senukRect.sizeDelta = Vector2.zero;
            Image senukCardImg = senukCardObj.AddComponent<Image>();
            senukCardImg.color = new Color(0f, 0f, 0f, 0.005f);
            Button senukBtn = senukCardObj.AddComponent<Button>();

            // Senuk Glow Aura (Radiant backlight directly behind character silhouette)
            GameObject senukGlowObj = new GameObject("SenukGlowBorder");
            senukGlowObj.transform.SetParent(senukCardObj.transform, false);
            RectTransform senukGlowRect = senukGlowObj.AddComponent<RectTransform>();
            senukGlowRect.anchorMin = new Vector2(0f, 0.12f);
            senukGlowRect.anchorMax = new Vector2(1f, 1.0f);
            senukGlowRect.sizeDelta = new Vector2(60f, 60f);
            senukGlowRect.pivot = new Vector2(0.5f, 0.5f);
            Image senukGlowImg = senukGlowObj.AddComponent<Image>();
            if (auraSprite != null) senukGlowImg.sprite = auraSprite;
            senukGlowImg.color = new Color(1f, 0.88f, 0.35f, 0.95f);
            senukGlowImg.raycastTarget = false;
            senukGlowObj.SetActive(false);

            // Senuk Image (In front of aura)
            GameObject senukImgObj = new GameObject("SenukImage");
            senukImgObj.transform.SetParent(senukCardObj.transform, false);
            RectTransform senukImgRect = senukImgObj.AddComponent<RectTransform>();
            senukImgRect.anchorMin = new Vector2(0.05f, 0.16f);
            senukImgRect.anchorMax = new Vector2(0.95f, 0.96f);
            senukImgRect.sizeDelta = Vector2.zero;
            Image senukImg = senukImgObj.AddComponent<Image>();
            if (senukSprite != null) senukImg.sprite = senukSprite;
            senukImg.preserveAspect = true;

            // Senuk Label
            GameObject senukLblObj = new GameObject("SenukLabel");
            senukLblObj.transform.SetParent(senukCardObj.transform, false);
            RectTransform senukLblRect = senukLblObj.AddComponent<RectTransform>();
            senukLblRect.anchorMin = new Vector2(0f, 0.02f);
            senukLblRect.anchorMax = new Vector2(1f, 0.16f);
            senukLblRect.sizeDelta = Vector2.zero;
            Text senukLblText = senukLblObj.AddComponent<Text>();
            senukLblText.text = "<b>SENUK</b>\n<size=17><color=#d5c59f>Agile Explorer</color></size>";
            senukLblText.fontSize = 24;
            senukLblText.fontStyle = FontStyle.Bold;
            senukLblText.alignment = TextAnchor.MiddleCenter;
            senukLblText.color = new Color(1f, 0.95f, 0.8f, 1f);

            // Selected Character Info Text
            GameObject infoObj = new GameObject("Text_SelectedInfo");
            infoObj.transform.SetParent(charPanel.transform, false);
            RectTransform infoRect = infoObj.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.15f, 0.12f);
            infoRect.anchorMax = new Vector2(0.85f, 0.20f);
            infoRect.sizeDelta = Vector2.zero;
            Text infoText = infoObj.AddComponent<Text>();
            infoText.text = "<b>MAYA SELECTED</b>\nAncient Lore Scholar & Temple Inscription Decoder";
            infoText.fontSize = 20;
            infoText.alignment = TextAnchor.MiddleCenter;
            infoText.color = new Color(0.95f, 0.9f, 0.8f, 1f);

            // Confirm Character Button (Graphic Image Button)
            GameObject confirmBtnObj = new GameObject("Button_ConfirmCharacter");
            confirmBtnObj.transform.SetParent(charPanel.transform, false);
            RectTransform confRect = confirmBtnObj.AddComponent<RectTransform>();
            confRect.anchorMin = new Vector2(0.36f, 0.015f);
            confRect.anchorMax = new Vector2(0.64f, 0.125f);
            confRect.sizeDelta = Vector2.zero;
            Button confBtn = confirmBtnObj.AddComponent<Button>();
            Image confImg = confirmBtnObj.AddComponent<Image>();
            if (confirmBtnSprite != null) confImg.sprite = confirmBtnSprite;
            confImg.color = Color.white;
            confImg.preserveAspect = true;

            // 8. Status Text (At the bottom of Canvas)
            GameObject statusObj = new GameObject("Text_Status");
            statusObj.transform.SetParent(canvasObj.transform, false);
            RectTransform statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.2f, 0.03f);
            statusRect.anchorMax = new Vector2(0.8f, 0.09f);
            statusRect.sizeDelta = Vector2.zero;
            Text statusText = statusObj.AddComponent<Text>();
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.fontSize = 22;
            statusText.fontStyle = FontStyle.Bold;
            statusText.color = Color.white;
            statusText.text = "";

            // 9. Attach AuthController
            GameObject controllerObj = new GameObject("AuthController");
            controllerObj.transform.SetParent(canvasObj.transform, false);
            var authCtrl = controllerObj.AddComponent<SecretOfLostTemple.Auth.AuthController>();
            var so = new SerializedObject(authCtrl);
            so.FindProperty("loginPanel").objectReferenceValue = loginPanel;
            so.FindProperty("signupPanel").objectReferenceValue = signupPanel;
            so.FindProperty("characterSelectPanel").objectReferenceValue = charPanel;
            so.FindProperty("loginUsernameInput").objectReferenceValue = loginUser;
            so.FindProperty("loginPasswordInput").objectReferenceValue = loginPass;
            so.FindProperty("loginSubmitButton").objectReferenceValue = lSubBtn;
            so.FindProperty("loginToSignupButton").objectReferenceValue = lToSBtn;
            so.FindProperty("signupUsernameInput").objectReferenceValue = signUser;
            so.FindProperty("signupEmailInput").objectReferenceValue = signEmail;
            so.FindProperty("signupPasswordInput").objectReferenceValue = signPass;
            so.FindProperty("signupSubmitButton").objectReferenceValue = sSubBtn;
            so.FindProperty("signupToLoginButton").objectReferenceValue = sToLBtn;
            so.FindProperty("signupBackButton").objectReferenceValue = sBackBtn;
            so.FindProperty("mayaCardButton").objectReferenceValue = mayaBtn;
            so.FindProperty("senukCardButton").objectReferenceValue = senukBtn;
            so.FindProperty("mayaSelectedGlow").objectReferenceValue = mayaGlowObj;
            so.FindProperty("senukSelectedGlow").objectReferenceValue = senukGlowObj;
            so.FindProperty("confirmCharacterButton").objectReferenceValue = confBtn;
            so.FindProperty("selectedCharacterInfoText").objectReferenceValue = infoText;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.FindProperty("targetSceneName").stringValue = "HomeMapScene";
            so.ApplyModifiedProperties();

            // Set OnClick persistent listeners directly on buttons
            UnityEditor.Events.UnityEventTools.AddPersistentListener(lToSBtn.onClick, authCtrl.ShowSignupPanel);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(lSubBtn.onClick, authCtrl.OnLoginSubmit);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sSubBtn.onClick, authCtrl.OnSignupSubmit);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sToLBtn.onClick, authCtrl.ShowLoginPanel);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sBackBtn.onClick, authCtrl.ShowLoginPanel);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(mayaBtn.onClick, authCtrl.SelectMaya);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(senukBtn.onClick, authCtrl.SelectSenuk);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(confBtn.onClick, authCtrl.ConfirmCharacterAndStart);

            // 10. FirebaseAuthService, FirestoreService & AudioManager
            GameObject fbObj = new GameObject("FirebaseAuthService");
            fbObj.AddComponent<SecretOfLostTemple.Auth.FirebaseAuthService>();

            GameObject fsObj = new GameObject("FirestoreService");
            fsObj.AddComponent<SecretOfLostTemple.Auth.FirestoreService>();

            GameObject audioObj = new GameObject("AudioManager");
            AudioSource bgmSrc = audioObj.AddComponent<AudioSource>();
            bgmSrc.loop = true;
            bgmSrc.playOnAwake = true;
            bgmSrc.volume = 0.65f;
            AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/tribal_jungle_inca_bgm.mp3");
            if (bgmClip != null) bgmSrc.clip = bgmClip;

            var audioMgr = audioObj.AddComponent<SecretOfLostTemple.Core.AudioManager>();
            var audioSo = new SerializedObject(audioMgr);
            audioSo.FindProperty("bgmSource").objectReferenceValue = bgmSrc;
            audioSo.FindProperty("templeBGMClip").objectReferenceValue = bgmClip;
            audioSo.FindProperty("defaultBGMVolume").floatValue = 0.65f;
            audioSo.FindProperty("autoPlayAfterSplash").boolValue = true;
            audioSo.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static InputField CreateCardInput(GameObject parent, string name, float xMin, float xMax, float yMin, float yMax, string placeholder, bool isPassword)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent.transform, false);
            RectTransform rt = row.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.sizeDelta = Vector2.zero;
            Image rowBg = row.AddComponent<Image>();
            rowBg.color = new Color(0f, 0f, 0f, 0.01f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(row.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0.04f, 0.1f); textRt.anchorMax = new Vector2(0.96f, 0.9f);
            textRt.sizeDelta = Vector2.zero;
            Text t = textObj.AddComponent<Text>();
            t.fontSize = 24;
            t.color = new Color(1f, 0.95f, 0.8f, 1f); // Warm glowing text
            t.alignment = TextAnchor.MiddleLeft;

            GameObject phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(row.transform, false);
            RectTransform phRt = phObj.AddComponent<RectTransform>();
            phRt.anchorMin = new Vector2(0.04f, 0.1f); phRt.anchorMax = new Vector2(0.96f, 0.9f);
            phRt.sizeDelta = Vector2.zero;
            Text ph = phObj.AddComponent<Text>();
            ph.text = placeholder;
            ph.fontSize = 22;
            ph.fontStyle = FontStyle.Italic;
            ph.color = new Color(0.7f, 0.65f, 0.55f, 0.7f);
            ph.alignment = TextAnchor.MiddleLeft;

            InputField inf = row.AddComponent<InputField>();
            inf.textComponent = t;
            inf.placeholder = ph;
            inf.caretColor = new Color(0.95f, 0.85f, 0.45f, 1f);
            inf.customCaretColor = true;
            inf.selectionColor = new Color(0.8f, 0.65f, 0.3f, 0.5f);
            inf.shouldHideMobileInput = true;
            inf.shouldActivateOnSelect = true;
            if (isPassword)
            {
                inf.inputType = InputField.InputType.Password;
                inf.contentType = InputField.ContentType.Password;
            }
            return inf;
        }

        private static void CreateHomeMapScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.09f, 0.08f, 1f);
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";

            // 2. EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 3. Canvas
            GameObject canvasObj = new GameObject("Map Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 4. Background Video / Image (Tropical birds flying in jungle video loop)
            GameObject bgObj = new GameObject("MapBackground");
            bgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            RawImage bgImg = bgObj.AddComponent<RawImage>();
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            VideoClip birdVideoClip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Art/Animations/Tropical_birds_flying_in_jungle_20261020001704.mp4");
            string rtPath = "Assets/Art/Animations/HomeMap_VideoBg_RT.renderTexture";
            RenderTexture bgRt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
            if (bgRt == null)
            {
                bgRt = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32);
                bgRt.name = "HomeMap_VideoBg_RT";
                AssetDatabase.CreateAsset(bgRt, rtPath);
            }
            bgImg.texture = bgRt;

            VideoPlayer bgVp = bgObj.AddComponent<VideoPlayer>();
            bgVp.playOnAwake = true;
            bgVp.isLooping = true;
            bgVp.renderMode = VideoRenderMode.RenderTexture;
            bgVp.targetTexture = bgRt;
            bgVp.aspectRatio = VideoAspectRatio.FitOutside;
            bgVp.clip = birdVideoClip;

            AudioSource bgAudio = bgObj.AddComponent<AudioSource>();
            bgAudio.playOnAwake = true;
            bgAudio.loop = true;
            bgAudio.volume = 0.35f;

            bgVp.audioOutputMode = VideoAudioOutputMode.AudioSource;
            bgVp.EnableAudioTrack(0, true);
            bgVp.SetTargetAudioSource(0, bgAudio);

            // Load Sprites
            Sprite mayaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/profile_maya.png");
            Sprite senukSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/profile_senuk.png");
            Sprite profileCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/profile_card_bg.png");
            Sprite questBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/map_quest_button.png");
            Sprite logoutBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/temple_logout_button.png");
            Sprite sandboxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/card_sandbox.png");
            Sprite physics3DSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/card_3d_physics.png");
            Sprite starCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/card_star.png");
            Sprite rankCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/card_rank.png");
            Font montserratFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/MontserratFont.ttf") ?? Resources.Load<Font>("MontserratFont");

            // --- 5. TOP HUD BAR ---
            GameObject topHudObj = new GameObject("TopHUD");
            topHudObj.transform.SetParent(canvasObj.transform, false);
            RectTransform topHudRt = topHudObj.AddComponent<RectTransform>();
            topHudRt.anchorMin = new Vector2(0f, 0.85f);
            topHudRt.anchorMax = new Vector2(1f, 0.99f);
            topHudRt.sizeDelta = Vector2.zero;

            // Profile Card (Top-Left HUD)
            GameObject profileCard = new GameObject("ProfileCard");
            profileCard.transform.SetParent(topHudObj.transform, false);
            RectTransform profRt = profileCard.AddComponent<RectTransform>();
            profRt.anchorMin = new Vector2(0.02f, 0.0f);
            profRt.anchorMax = new Vector2(0.38f, 1.0f);
            profRt.sizeDelta = Vector2.zero;
            Image profBg = profileCard.AddComponent<Image>();
            if (profileCardSprite != null) profBg.sprite = profileCardSprite;
            profBg.color = Color.white;
            profBg.preserveAspect = true;
            profileCard.AddComponent<Button>();

            // Avatar Frame & Image
            GameObject avatarObj = new GameObject("AvatarImage");
            avatarObj.transform.SetParent(profileCard.transform, false);
            RectTransform avRt = avatarObj.AddComponent<RectTransform>();
            avRt.anchorMin = new Vector2(0.02f, 0.06f);
            avRt.anchorMax = new Vector2(0.30f, 0.94f);
            avRt.sizeDelta = Vector2.zero;
            Image avatarImg = avatarObj.AddComponent<Image>();
            if (mayaSprite != null) avatarImg.sprite = mayaSprite;
            avatarImg.preserveAspect = true;

            // Player Name Text (Dark Carved Color, Montserrat-Bold, 32px)
            GameObject pNameObj = new GameObject("PlayerNameText");
            pNameObj.transform.SetParent(profileCard.transform, false);
            RectTransform pnRt = pNameObj.AddComponent<RectTransform>();
            pnRt.anchorMin = new Vector2(0.34f, 0.48f);
            pnRt.anchorMax = new Vector2(0.96f, 0.92f);
            pnRt.sizeDelta = Vector2.zero;
            Text pnText = pNameObj.AddComponent<Text>();
            if (montserratFont != null) pnText.font = montserratFont;
            pnText.text = "Explorer";
            pnText.fontSize = 32;
            pnText.fontStyle = FontStyle.Bold;
            pnText.color = new Color(0.16f, 0.10f, 0.06f, 1f);
            pnText.alignment = TextAnchor.MiddleLeft;

            // Player Title Text (Dark Warm Brown, Montserrat-Bold, 22px)
            GameObject pTitleObj = new GameObject("PlayerTitleText");
            pTitleObj.transform.SetParent(profileCard.transform, false);
            RectTransform ptRt = pTitleObj.AddComponent<RectTransform>();
            ptRt.anchorMin = new Vector2(0.34f, 0.08f);
            ptRt.anchorMax = new Vector2(0.96f, 0.48f);
            ptRt.sizeDelta = Vector2.zero;
            Text ptText = pTitleObj.AddComponent<Text>();
            if (montserratFont != null) ptText.font = montserratFont;
            ptText.text = "Lore Scholar • Lv. 1";
            ptText.fontSize = 22;
            ptText.fontStyle = FontStyle.Bold;
            ptText.color = new Color(0.30f, 0.20f, 0.14f, 0.95f);
            ptText.alignment = TextAnchor.MiddleLeft;

            // Exit / Logout Button (Top-Right Transparent Themed Button)
            GameObject exitBtnObj = new GameObject("Button_ExitToEntrance");
            exitBtnObj.transform.SetParent(topHudObj.transform, false);
            RectTransform exitRt = exitBtnObj.AddComponent<RectTransform>();
            exitRt.anchorMin = new Vector2(0.84f, 0.05f);
            exitRt.anchorMax = new Vector2(0.98f, 0.95f);
            exitRt.sizeDelta = Vector2.zero;
            Button exitBtn = exitBtnObj.AddComponent<Button>();
            Image exitImg = exitBtnObj.AddComponent<Image>();
            if (logoutBtnSprite != null) exitImg.sprite = logoutBtnSprite;
            exitImg.color = Color.white;
            exitImg.preserveAspect = true;

            // --- 6. LEFT PROGRESS BADGES ---
            // Star Count Card (Montserrat-Bold, 32px)
            GameObject starCardObj = new GameObject("Card_StarCount");
            starCardObj.transform.SetParent(canvasObj.transform, false);
            RectTransform starRt = starCardObj.AddComponent<RectTransform>();
            starRt.anchorMin = new Vector2(0.02f, 0.44f);
            starRt.anchorMax = new Vector2(0.16f, 0.78f);
            starRt.anchoredPosition = Vector2.zero;
            starRt.sizeDelta = Vector2.zero;
            Image starImg = starCardObj.AddComponent<Image>();
            if (starCardSprite != null) starImg.sprite = starCardSprite;
            starImg.color = Color.white;
            starImg.preserveAspect = true;

            GameObject starTxtObj = new GameObject("StarValueText");
            starTxtObj.transform.SetParent(starCardObj.transform, false);
            RectTransform starTxtRt = starTxtObj.AddComponent<RectTransform>();
            starTxtRt.anchorMin = new Vector2(0.05f, 0.06f);
            starTxtRt.anchorMax = new Vector2(0.95f, 0.30f);
            starTxtRt.sizeDelta = Vector2.zero;
            Text starValText = starTxtObj.AddComponent<Text>();
            if (montserratFont != null) starValText.font = montserratFont;
            starValText.text = "24 Stars";
            starValText.fontSize = 32;
            starValText.fontStyle = FontStyle.Bold;
            starValText.color = new Color(1f, 0.94f, 0.45f, 1f);
            starValText.alignment = TextAnchor.MiddleCenter;

            // Player Rank Card (Montserrat-Bold, 30px)
            GameObject rankCardObj = new GameObject("Card_PlayerRank");
            rankCardObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rankRt = rankCardObj.AddComponent<RectTransform>();
            rankRt.anchorMin = new Vector2(0.02f, 0.08f);
            rankRt.anchorMax = new Vector2(0.16f, 0.42f);
            rankRt.anchoredPosition = Vector2.zero;
            rankRt.sizeDelta = Vector2.zero;
            Image rankImg = rankCardObj.AddComponent<Image>();
            if (rankCardSprite != null) rankImg.sprite = rankCardSprite;
            rankImg.color = Color.white;
            rankImg.preserveAspect = true;

            GameObject rankTxtObj = new GameObject("RankValueText");
            rankTxtObj.transform.SetParent(rankCardObj.transform, false);
            RectTransform rankTxtRt = rankTxtObj.AddComponent<RectTransform>();
            rankTxtRt.anchorMin = new Vector2(0.05f, 0.06f);
            rankTxtRt.anchorMax = new Vector2(0.95f, 0.30f);
            rankTxtRt.sizeDelta = Vector2.zero;
            Text rankValText = rankTxtObj.AddComponent<Text>();
            if (montserratFont != null) rankValText.font = montserratFont;
            rankValText.text = "#1 Scholar";
            rankValText.fontSize = 30;
            rankValText.fontStyle = FontStyle.Bold;
            rankValText.color = new Color(1f, 0.88f, 0.45f, 1f);
            rankValText.alignment = TextAnchor.MiddleCenter;

            // --- 7. QUEST BUTTON (Center Map Expedition, 3D Golden Carved Text) ---
            GameObject questBtnObj = new GameObject("Button_QuestStart");
            questBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform qRt = questBtnObj.AddComponent<RectTransform>();
            qRt.anchorMin = new Vector2(0.5f, 0.46f);
            qRt.anchorMax = new Vector2(0.5f, 0.46f);
            qRt.anchoredPosition = Vector2.zero;
            qRt.sizeDelta = new Vector2(880f, 520f);
            Button questBtn = questBtnObj.AddComponent<Button>();
            Image questImg = questBtnObj.AddComponent<Image>();
            if (questBtnSprite != null) questImg.sprite = questBtnSprite;
            questImg.color = Color.white;
            questImg.preserveAspect = true;
            AddCarvedGoldenButtonLabel(questBtnObj, "START EXPEDITION", 38, montserratFont, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.28f));

            // --- 8. RIGHT SECONDARY CARDS (3D Golden Carved Text) ---
            // Sandbox Mode Card
            GameObject sbBtnObj = new GameObject("Button_SandboxMode");
            sbBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform sbRt = sbBtnObj.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0.83f, 0.24f);
            sbRt.anchorMax = new Vector2(0.98f, 0.68f);
            sbRt.anchoredPosition = Vector2.zero;
            sbRt.sizeDelta = Vector2.zero;
            Button sbBtn = sbBtnObj.AddComponent<Button>();
            Image sbImg = sbBtnObj.AddComponent<Image>();
            if (sandboxSprite != null) sbImg.sprite = sandboxSprite;
            sbImg.color = Color.white;
            sbImg.preserveAspect = true;
            AddCarvedGoldenButtonLabel(sbBtnObj, "FREE PRACTICE", 36, montserratFont, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.28f));

// 3D Physics button removed as per user requirement

            // --- 9. STATUS TEXT (Montserrat-Bold, 28px, 3D Outline & Shadow) ---
            GameObject statusTxtObj = new GameObject("StatusText");
            statusTxtObj.transform.SetParent(canvasObj.transform, false);
            RectTransform stRt = statusTxtObj.AddComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.15f, 0.02f);
            stRt.anchorMax = new Vector2(0.85f, 0.09f);
            stRt.sizeDelta = Vector2.zero;
            Text statusText = statusTxtObj.AddComponent<Text>();
            if (montserratFont != null) statusText.font = montserratFont;
            statusText.text = "Tap Start Expedition or Sandbox to explore!";
            statusText.fontSize = 28;
            statusText.fontStyle = FontStyle.Bold;
            statusText.color = new Color(1f, 0.949f, 0.659f, 1f); // #FFF2A8 Rich Warm Gold
            statusText.alignment = TextAnchor.MiddleCenter;

            var stOutline = statusTxtObj.AddComponent<Outline>();
            stOutline.effectColor = new Color(0.165f, 0.086f, 0.031f, 1f); // #2A1608 Dark carved border
            stOutline.effectDistance = new Vector2(2f, -2f);

            var stShadow = statusTxtObj.AddComponent<Shadow>();
            stShadow.effectColor = new Color(0.043f, 0.020f, 0.008f, 0.85f); // #0B0502 @ 85% alpha
            stShadow.effectDistance = new Vector2(3f, -4f);

            // 10. Attach HomeMapController
            GameObject ctrlObj = new GameObject("HomeMapController");
            ctrlObj.transform.SetParent(canvasObj.transform, false);
            var mapCtrl = ctrlObj.AddComponent<SecretOfLostTemple.Home.HomeMapController>();
            var so = new SerializedObject(mapCtrl);
            so.FindProperty("profileCardButton").objectReferenceValue = profBg.GetComponent<Button>();
            so.FindProperty("characterAvatarImage").objectReferenceValue = avatarImg;
            so.FindProperty("playerNameText").objectReferenceValue = pnText;
            so.FindProperty("playerTitleText").objectReferenceValue = ptText;
            so.FindProperty("mayaAvatarSprite").objectReferenceValue = mayaSprite;
            so.FindProperty("senukAvatarSprite").objectReferenceValue = senukSprite;
            so.FindProperty("questButton").objectReferenceValue = questBtn;
            so.FindProperty("logoutButton").objectReferenceValue = exitBtn;
            so.FindProperty("sandboxButton").objectReferenceValue = sbBtn;
            // so.FindProperty("physics3DButton").objectReferenceValue = physBtn;
            so.FindProperty("starCountText").objectReferenceValue = starValText;
            so.FindProperty("rankTierText").objectReferenceValue = rankValText;
            so.FindProperty("statusInfoText").objectReferenceValue = statusText;
            so.FindProperty("backgroundVideoPlayer").objectReferenceValue = bgVp;
            so.FindProperty("backgroundRawImage").objectReferenceValue = bgImg;
            so.FindProperty("backgroundVideoClip").objectReferenceValue = birdVideoClip;
            so.ApplyModifiedProperties();

            // Set OnClick persistent listeners directly on buttons
            UnityEditor.Events.UnityEventTools.AddPersistentListener(questBtn.onClick, mapCtrl.EnterLevel1AR);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sbBtn.onClick, mapCtrl.OnSandboxClicked);
            // UnityEditor.Events.UnityEventTools.AddPersistentListener(physBtn.onClick, mapCtrl.On3DPhysicsClicked);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(exitBtn.onClick, mapCtrl.OnLogoutClicked);

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static Text AddCarvedGoldenButtonLabel(GameObject buttonObj, string label, int fontSize, Font font, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject txtObj = new GameObject("LabelText");
            txtObj.transform.SetParent(buttonObj.transform, false);
            RectTransform rt = txtObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Text txt = txtObj.AddComponent<Text>();
            if (font != null) txt.font = font;
            txt.text = label;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(1f, 0.949f, 0.659f, 1f); // #FFF2A8 Rich Warm Gold
            txt.raycastTarget = false;

            // Outline: Color #2A1608 (Dark carved border), Effect Distance (2f, -2f)
            var outline = txtObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.165f, 0.086f, 0.031f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Shadow: Color #0B0502 with 85% alpha (Deep 3D cast shadow), Effect Distance (4f, -5f)
            var shadow = txtObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.043f, 0.020f, 0.008f, 0.85f);
            shadow.effectDistance = new Vector2(4f, -5f);

            return txt;
        }

        private static void CreateMainARScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Light & Ambient Lighting
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 2. XR Origin (Mobile AR Rig) & Fallback Camera
            GameObject xrOriginObj = new GameObject("XR Origin (Mobile AR)");
            GameObject cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOriginObj.transform, false);

            GameObject arCamObj = new GameObject("Main Camera");
            arCamObj.transform.SetParent(cameraOffset.transform, false);
            arCamObj.transform.localPosition = new Vector3(0f, 0.6f, -1.8f);
            arCamObj.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            arCamObj.tag = "MainCamera";

            Camera cam = arCamObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.12f, 0.10f, 1f); // Dark ancient temple ambient color
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            arCamObj.AddComponent<AudioListener>();

            // AR Foundation dynamic components
            var xrOriginType = System.Type.GetType("Unity.XR.CoreUtils.XROrigin, Unity.XR.CoreUtils");
            if (xrOriginType != null)
            {
                var xrOriginComp = xrOriginObj.AddComponent(xrOriginType);
                var camProp = xrOriginType.GetProperty("Camera");
                if (camProp != null) camProp.SetValue(xrOriginComp, cam);
                var camOffsetProp = xrOriginType.GetProperty("CameraFloorOffsetObject");
                if (camOffsetProp != null) camOffsetProp.SetValue(xrOriginComp, cameraOffset);
            }

            var arCamMgrType = System.Type.GetType("UnityEngine.XR.ARFoundation.ARCameraManager, Unity.XR.ARFoundation");
            if (arCamMgrType != null) arCamObj.AddComponent(arCamMgrType);

            var arCamBgType = System.Type.GetType("UnityEngine.XR.ARFoundation.ARCameraBackground, Unity.XR.ARFoundation");
            if (arCamBgType != null) arCamObj.AddComponent(arCamBgType);

            var poseDriverType = System.Type.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver, Unity.InputSystem");
            if (poseDriverType != null) arCamObj.AddComponent(poseDriverType);

            var arPlaneMgrType = System.Type.GetType("UnityEngine.XR.ARFoundation.ARPlaneManager, Unity.XR.ARFoundation");
            if (arPlaneMgrType != null) xrOriginObj.AddComponent(arPlaneMgrType);

            var arRaycastMgrType = System.Type.GetType("UnityEngine.XR.ARFoundation.ARRaycastManager, Unity.XR.ARFoundation");
            if (arRaycastMgrType != null) xrOriginObj.AddComponent(arRaycastMgrType);

            // AR Session
            GameObject arSessionObj = new GameObject("AR Session");
            var arSessionType = System.Type.GetType("UnityEngine.XR.ARFoundation.ARSession, Unity.XR.ARFoundation");
            if (arSessionType != null) arSessionObj.AddComponent(arSessionType);
            var arInputMgrType = System.Type.GetType("UnityEngine.XR.ARFoundation.ARInputManager, Unity.XR.ARFoundation");
            if (arInputMgrType != null) arSessionObj.AddComponent(arInputMgrType);

            // 3. EventSystem
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 4. 3D Environment & Simple Machine Testbeds
            GameObject envRoot = new GameObject("TempleEnvironmentRoot");

            // Stone Pedestal / Altar
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Temple_Altar_Pedestal";
            pedestal.transform.SetParent(envRoot.transform, false);
            pedestal.transform.position = new Vector3(0f, -0.4f, 1.2f);
            pedestal.transform.localScale = new Vector3(2.5f, 0.1f, 2.5f);
            SetPrimitiveColor(pedestal, new Color(0.25f, 0.28f, 0.24f));

            // --- 3D Machine 1: LEVER ---
            GameObject leverObj = new GameObject("SimpleMachine_Lever");
            leverObj.transform.SetParent(envRoot.transform, false);
            leverObj.transform.position = new Vector3(0f, -0.2f, 1.2f);

            GameObject fulcrum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fulcrum.name = "Fulcrum_Pivot";
            fulcrum.transform.SetParent(leverObj.transform, false);
            fulcrum.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            fulcrum.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            fulcrum.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);
            SetPrimitiveColor(fulcrum, new Color(0.2f, 0.8f, 0.4f));

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Lever_Beam";
            beam.transform.SetParent(leverObj.transform, false);
            beam.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            beam.transform.localRotation = Quaternion.Euler(0f, 0f, 8f);
            beam.transform.localScale = new Vector3(2.0f, 0.08f, 0.25f);
            SetPrimitiveColor(beam, new Color(0.7f, 0.55f, 0.35f));

            GameObject loadStone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            loadStone.name = "Load_Stone_DoorWeight";
            loadStone.transform.SetParent(beam.transform, false);
            loadStone.transform.localPosition = new Vector3(-0.42f, 0.8f, 0f);
            loadStone.transform.localScale = new Vector3(0.35f, 1.5f, 0.35f);
            SetPrimitiveColor(loadStone, new Color(0.85f, 0.3f, 0.3f));

            GameObject effortMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effortMarker.name = "Effort_Point_Handle";
            effortMarker.transform.SetParent(beam.transform, false);
            effortMarker.transform.localPosition = new Vector3(0.45f, 0.8f, 0f);
            effortMarker.transform.localScale = new Vector3(0.25f, 1.2f, 0.25f);
            SetPrimitiveColor(effortMarker, new Color(1f, 0.85f, 0.2f));

            var leverSM = leverObj.AddComponent<SecretOfLostTemple.AR.SimpleMachine>();
            var smSo = new SerializedObject(leverSM);
            smSo.FindProperty("machineType").enumValueIndex = (int)SecretOfLostTemple.AR.MachineType.Lever;
            smSo.ApplyModifiedProperties();

            // --- 3D Machine 2: PULLEY ---
            GameObject pulleyObj = new GameObject("SimpleMachine_Pulley");
            pulleyObj.transform.SetParent(envRoot.transform, false);
            pulleyObj.transform.position = new Vector3(0f, 0.1f, 1.2f);

            GameObject archFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archFrame.name = "Arch_Support";
            archFrame.transform.SetParent(pulleyObj.transform, false);
            archFrame.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            archFrame.transform.localScale = new Vector3(1.2f, 0.1f, 0.2f);

            GameObject pWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pWheel.name = "Pulley_Wheel";
            pWheel.transform.SetParent(pulleyObj.transform, false);
            pWheel.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            pWheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            pWheel.transform.localScale = new Vector3(0.4f, 0.08f, 0.4f);
            SetPrimitiveColor(pWheel, new Color(1f, 0.84f, 0f));

            GameObject suspendedWeight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            suspendedWeight.name = "Suspended_Relic";
            suspendedWeight.transform.SetParent(pulleyObj.transform, false);
            suspendedWeight.transform.localPosition = new Vector3(-0.2f, -0.1f, 0f);
            suspendedWeight.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            SetPrimitiveColor(suspendedWeight, new Color(0.85f, 0.35f, 0.35f));

            var pulleySM = pulleyObj.AddComponent<SecretOfLostTemple.AR.SimpleMachine>();
            var pSmSo = new SerializedObject(pulleySM);
            pSmSo.FindProperty("machineType").enumValueIndex = (int)SecretOfLostTemple.AR.MachineType.Pulley;
            pSmSo.ApplyModifiedProperties();
            pulleyObj.SetActive(false);

            // --- 3D Machine 3: INCLINED PLANE ---
            GameObject rampObj = new GameObject("SimpleMachine_InclinedPlane");
            rampObj.transform.SetParent(envRoot.transform, false);
            rampObj.transform.position = new Vector3(0f, -0.25f, 1.2f);

            GameObject rampMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampMesh.name = "Ramp_Slope";
            rampMesh.transform.SetParent(rampObj.transform, false);
            rampMesh.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            rampMesh.transform.localRotation = Quaternion.Euler(0f, 0f, -22f);
            rampMesh.transform.localScale = new Vector3(1.8f, 0.08f, 0.5f);
            SetPrimitiveColor(rampMesh, new Color(0.6f, 0.5f, 0.4f));

            GameObject slidingBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slidingBox.name = "Sliding_Crate";
            slidingBox.transform.SetParent(rampMesh.transform, false);
            slidingBox.transform.localPosition = new Vector3(-0.15f, 1.2f, 0f);
            slidingBox.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
            SetPrimitiveColor(slidingBox, new Color(1f, 0.85f, 0.3f));

            var rampSM = rampObj.AddComponent<SecretOfLostTemple.AR.SimpleMachine>();
            var rSmSo = new SerializedObject(rampSM);
            rSmSo.FindProperty("machineType").enumValueIndex = (int)SecretOfLostTemple.AR.MachineType.InclinedPlane;
            rSmSo.ApplyModifiedProperties();
            rampObj.SetActive(false);

            // --- 3D Machine 4: WHEEL & AXLE ---
            GameObject wheelObj = new GameObject("SimpleMachine_WheelAndAxle");
            wheelObj.transform.SetParent(envRoot.transform, false);
            wheelObj.transform.position = new Vector3(0f, 0.0f, 1.2f);

            GameObject largeWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            largeWheel.name = "Large_Wheel";
            largeWheel.transform.SetParent(wheelObj.transform, false);
            largeWheel.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            largeWheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            largeWheel.transform.localScale = new Vector3(1.2f, 0.08f, 1.2f);
            SetPrimitiveColor(largeWheel, new Color(1f, 0.85f, 0.3f));

            GameObject axleShaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            axleShaft.name = "Small_Axle_Shaft";
            axleShaft.transform.SetParent(wheelObj.transform, false);
            axleShaft.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            axleShaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            axleShaft.transform.localScale = new Vector3(0.25f, 0.4f, 0.25f);
            SetPrimitiveColor(axleShaft, new Color(0.3f, 0.7f, 0.9f));

            var wheelSM = wheelObj.AddComponent<SecretOfLostTemple.AR.SimpleMachine>();
            var wSmSo = new SerializedObject(wheelSM);
            wSmSo.FindProperty("machineType").enumValueIndex = (int)SecretOfLostTemple.AR.MachineType.WheelAndAxle;
            wSmSo.ApplyModifiedProperties();
            wheelObj.SetActive(false);

            // 5. Full AR Screen Space Overlay Canvas
            GameObject canvasObj = new GameObject("AR Main Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Load Sprites
            Sprite mayaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/profile_maya.png");
            Sprite senukSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/profile_senuk.png");
            Sprite backBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/back_button.png");

            // --- TOP HUD BAR ---
            GameObject topBar = new GameObject("TopNavBar");
            topBar.transform.SetParent(canvasObj.transform, false);
            RectTransform tbRt = topBar.AddComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 0.88f);
            tbRt.anchorMax = new Vector2(1f, 0.99f);
            tbRt.sizeDelta = Vector2.zero;
            Image tbBg = topBar.AddComponent<Image>();
            tbBg.color = new Color(0.05f, 0.08f, 0.06f, 0.85f);

            // Back Button
            GameObject backBtnObj = new GameObject("Button_BackToMap");
            backBtnObj.transform.SetParent(topBar.transform, false);
            RectTransform bbRt = backBtnObj.AddComponent<RectTransform>();
            bbRt.anchorMin = new Vector2(0.015f, 0.1f);
            bbRt.anchorMax = new Vector2(0.12f, 0.9f);
            bbRt.sizeDelta = Vector2.zero;
            Button backBtn = backBtnObj.AddComponent<Button>();
            Image bbImg = backBtnObj.AddComponent<Image>();
            if (backBtnSprite != null) bbImg.sprite = backBtnSprite;
            bbImg.preserveAspect = true;

            // Title & Subtitle
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(topBar.transform, false);
            RectTransform titRt = titleObj.AddComponent<RectTransform>();
            titRt.anchorMin = new Vector2(0.13f, 0.48f);
            titRt.anchorMax = new Vector2(0.65f, 0.95f);
            titRt.sizeDelta = Vector2.zero;
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.text = "🏛️ TEMPLE GATES • ANCIENT SIMPLE MACHINES LAB";
            titleTxt.fontSize = 26;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1f, 0.88f, 0.4f, 1f);
            titleTxt.alignment = TextAnchor.MiddleLeft;

            GameObject subTitleObj = new GameObject("SubtitleText");
            subTitleObj.transform.SetParent(topBar.transform, false);
            RectTransform subRt = subTitleObj.AddComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.13f, 0.05f);
            subRt.anchorMax = new Vector2(0.65f, 0.48f);
            subRt.sizeDelta = Vector2.zero;
            Text subTxt = subTitleObj.AddComponent<Text>();
            subTxt.text = "Research Testbed: AR Interaction (RQ1) • Adaptive Hints (RQ2) • 3D Assessment (RQ3)";
            subTxt.fontSize = 17;
            subTxt.color = new Color(0.8f, 0.85f, 0.8f, 0.9f);
            subTxt.alignment = TextAnchor.MiddleLeft;

            // Character Info & Avatar
            GameObject charAvObj = new GameObject("CharacterAvatar");
            charAvObj.transform.SetParent(topBar.transform, false);
            RectTransform caRt = charAvObj.AddComponent<RectTransform>();
            caRt.anchorMin = new Vector2(0.68f, 0.08f);
            caRt.anchorMax = new Vector2(0.74f, 0.92f);
            caRt.sizeDelta = Vector2.zero;
            Image charAvImg = charAvObj.AddComponent<Image>();
            if (mayaSprite != null) charAvImg.sprite = mayaSprite;
            charAvImg.preserveAspect = true;

            GameObject charTxtObj = new GameObject("CharacterText");
            charTxtObj.transform.SetParent(topBar.transform, false);
            RectTransform ctRt = charTxtObj.AddComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0.75f, 0.1f);
            ctRt.anchorMax = new Vector2(0.98f, 0.9f);
            ctRt.sizeDelta = Vector2.zero;
            Text charTxt = charTxtObj.AddComponent<Text>();
            charTxt.text = "<b>Explorer Maya</b>\n<size=16><color=#f5d77f>Tier 1 Apprentice</color></size>";
            charTxt.fontSize = 20;
            charTxt.fontStyle = FontStyle.Bold;
            charTxt.color = Color.white;
            charTxt.alignment = TextAnchor.MiddleLeft;

            // --- COMPONENT 1: MODE SWITCHER HUD (Left Side) ---
            GameObject modePanel = new GameObject("Component1_ModePanel");
            modePanel.transform.SetParent(canvasObj.transform, false);
            RectTransform mpRt = modePanel.AddComponent<RectTransform>();
            mpRt.anchorMin = new Vector2(0.015f, 0.58f);
            mpRt.anchorMax = new Vector2(0.24f, 0.86f);
            mpRt.sizeDelta = Vector2.zero;
            Image mpBg = modePanel.AddComponent<Image>();
            mpBg.color = new Color(0.06f, 0.10f, 0.08f, 0.88f);

            var modeSwitcher = modePanel.AddComponent<SecretOfLostTemple.AR.ARModeSwitcher>();

            // View-Only Button
            GameObject voBtnObj = new GameObject("Button_ViewOnly");
            voBtnObj.transform.SetParent(modePanel.transform, false);
            RectTransform voRt = voBtnObj.AddComponent<RectTransform>();
            voRt.anchorMin = new Vector2(0.06f, 0.56f);
            voRt.anchorMax = new Vector2(0.48f, 0.90f);
            voRt.sizeDelta = Vector2.zero;
            Button voBtn = voBtnObj.AddComponent<Button>();
            Image voImg = voBtnObj.AddComponent<Image>();
            voImg.color = new Color(0.15f, 0.35f, 0.45f, 0.9f);

            GameObject voTxtObj = new GameObject("Text");
            voTxtObj.transform.SetParent(voBtnObj.transform, false);
            RectTransform voTRt = voTxtObj.AddComponent<RectTransform>();
            voTRt.anchorMin = Vector2.zero; voTRt.anchorMax = Vector2.one; voTRt.sizeDelta = Vector2.zero;
            Text voTxt = voTxtObj.AddComponent<Text>();
            voTxt.text = "👁️ <b>View-Only</b>";
            voTxt.alignment = TextAnchor.MiddleCenter;
            voTxt.fontSize = 19;
            voTxt.color = Color.white;

            // Interactive Button
            GameObject intBtnObj = new GameObject("Button_Interactive");
            intBtnObj.transform.SetParent(modePanel.transform, false);
            RectTransform intRt = intBtnObj.AddComponent<RectTransform>();
            intRt.anchorMin = new Vector2(0.52f, 0.56f);
            intRt.anchorMax = new Vector2(0.94f, 0.90f);
            intRt.sizeDelta = Vector2.zero;
            Button intBtn = intBtnObj.AddComponent<Button>();
            Image intImg = intBtnObj.AddComponent<Image>();
            intImg.color = new Color(0.6f, 0.45f, 0.15f, 0.9f);

            GameObject intTxtObj = new GameObject("Text");
            intTxtObj.transform.SetParent(intBtnObj.transform, false);
            RectTransform intTRt = intTxtObj.AddComponent<RectTransform>();
            intTRt.anchorMin = Vector2.zero; intTRt.anchorMax = Vector2.one; intTRt.sizeDelta = Vector2.zero;
            Text intTxt = intTxtObj.AddComponent<Text>();
            intTxt.text = "⚡ <b>Interactive</b>";
            intTxt.alignment = TextAnchor.MiddleCenter;
            intTxt.fontSize = 19;
            intTxt.color = Color.white;

            // Mode Label & Instruction
            GameObject modeLblObj = new GameObject("Text_ModeStatus");
            modeLblObj.transform.SetParent(modePanel.transform, false);
            RectTransform mlRt = modeLblObj.AddComponent<RectTransform>();
            mlRt.anchorMin = new Vector2(0.06f, 0.34f);
            mlRt.anchorMax = new Vector2(0.94f, 0.52f);
            mlRt.sizeDelta = Vector2.zero;
            Text modeLbl = modeLblObj.AddComponent<Text>();
            modeLbl.text = "MODE: <b>VIEW-ONLY</b> (Observation)";
            modeLbl.fontSize = 18;
            modeLbl.fontStyle = FontStyle.Bold;
            modeLbl.color = new Color(0.4f, 0.85f, 1f);
            modeLbl.alignment = TextAnchor.MiddleLeft;

            GameObject modeDescObj = new GameObject("Text_ModeDescription");
            modeDescObj.transform.SetParent(modePanel.transform, false);
            RectTransform mdRt = modeDescObj.AddComponent<RectTransform>();
            mdRt.anchorMin = new Vector2(0.06f, 0.04f);
            mdRt.anchorMax = new Vector2(0.94f, 0.32f);
            mdRt.sizeDelta = Vector2.zero;
            Text modeDesc = modeDescObj.AddComponent<Text>();
            modeDesc.text = "🔍 <b>View-Only:</b> Rotate and inspect the ancient mechanical parts from 360°.";
            modeDesc.fontSize = 15;
            modeDesc.color = new Color(0.85f, 0.85f, 0.85f);
            modeDesc.alignment = TextAnchor.UpperLeft;

            // --- BOTTOM MACHINE SELECTOR BAR ---
            GameObject btmBar = new GameObject("BottomMachineBar");
            btmBar.transform.SetParent(canvasObj.transform, false);
            RectTransform bbBarRt = btmBar.AddComponent<RectTransform>();
            bbBarRt.anchorMin = new Vector2(0.10f, 0.02f);
            bbBarRt.anchorMax = new Vector2(0.90f, 0.15f);
            bbBarRt.sizeDelta = Vector2.zero;
            Image bbBarImg = btmBar.AddComponent<Image>();
            bbBarImg.color = new Color(0.05f, 0.08f, 0.06f, 0.90f);

            // 4 Machine Buttons
            Button btnLev = CreateStoneButton(btmBar, "Btn_Lever", 0.02f, 0.24f, "1. LEVER\n<size=14>Door Lifter</size>");
            Button btnPul = CreateStoneButton(btmBar, "Btn_Pulley", 0.26f, 0.48f, "2. PULLEY\n<size=14>Block & Tackle</size>");
            Button btnRmp = CreateStoneButton(btmBar, "Btn_Ramp", 0.50f, 0.72f, "3. INCLINE\n<size=14>Temple Ramp</size>");
            Button btnWhl = CreateStoneButton(btmBar, "Btn_Wheel", 0.74f, 0.96f, "4. WHEEL\n<size=14>Capstan Axle</size>");

            // Active Machine & Mechanical Advantage Text (Center Top)
            GameObject maObj = new GameObject("Text_MechanicalAdvantage");
            maObj.transform.SetParent(canvasObj.transform, false);
            RectTransform maRt = maObj.AddComponent<RectTransform>();
            maRt.anchorMin = new Vector2(0.26f, 0.74f);
            maRt.anchorMax = new Vector2(0.74f, 0.86f);
            maRt.sizeDelta = Vector2.zero;
            Image maBg = maObj.AddComponent<Image>();
            maBg.color = new Color(0.04f, 0.07f, 0.05f, 0.78f);

            GameObject maTxtObj = new GameObject("Text");
            maTxtObj.transform.SetParent(maObj.transform, false);
            RectTransform maTRt = maTxtObj.AddComponent<RectTransform>();
            maTRt.anchorMin = new Vector2(0.04f, 0.05f); maTRt.anchorMax = new Vector2(0.96f, 0.95f); maTRt.sizeDelta = Vector2.zero;
            Text maText = maTxtObj.AddComponent<Text>();
            maText.text = "<b>Mechanical Advantage:</b> MA = Effort Arm / Load Arm = <b>2.50x</b>\n<color=#f5d77f>Effort required: 400N to lift 1000N stone gate</color>";
            maText.fontSize = 20;
            maText.color = Color.white;
            maText.alignment = TextAnchor.MiddleCenter;

            // --- COMPONENT 2: HINT & ADAPTIVE BUTTONS (Right Side) ---
            GameObject hintBtnObj = new GameObject("Button_RequestHint");
            hintBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform hbRt = hintBtnObj.AddComponent<RectTransform>();
            hbRt.anchorMin = new Vector2(0.82f, 0.72f);
            hbRt.anchorMax = new Vector2(0.985f, 0.84f);
            hbRt.sizeDelta = Vector2.zero;
            Button hintBtn = hintBtnObj.AddComponent<Button>();
            Image hbImg = hintBtnObj.AddComponent<Image>();
            hbImg.color = new Color(0.85f, 0.65f, 0.15f, 0.95f);

            GameObject hbTxtObj = new GameObject("Text");
            hbTxtObj.transform.SetParent(hintBtnObj.transform, false);
            RectTransform hbTRt = hbTxtObj.AddComponent<RectTransform>();
            hbTRt.anchorMin = Vector2.zero; hbTRt.anchorMax = Vector2.one; hbTRt.sizeDelta = Vector2.zero;
            Text hbTxt = hbTxtObj.AddComponent<Text>();
            hbTxt.text = "💡 <b>HINT</b>\n<size=15>Adaptive Clue</size>";
            hbTxt.alignment = TextAnchor.MiddleCenter;
            hbTxt.fontSize = 20;
            hbTxt.fontStyle = FontStyle.Bold;
            hbTxt.color = Color.white;

            // --- COMPONENT 3: QUIZ BUTTON ---
            GameObject quizBtnObj = new GameObject("Button_StartQuiz");
            quizBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform qbRt = quizBtnObj.AddComponent<RectTransform>();
            qbRt.anchorMin = new Vector2(0.82f, 0.58f);
            qbRt.anchorMax = new Vector2(0.985f, 0.70f);
            qbRt.sizeDelta = Vector2.zero;
            Button quizBtn = quizBtnObj.AddComponent<Button>();
            Image qbImg = quizBtnObj.AddComponent<Image>();
            qbImg.color = new Color(0.2f, 0.6f, 0.4f, 0.95f);

            GameObject qbTxtObj = new GameObject("Text");
            qbTxtObj.transform.SetParent(quizBtnObj.transform, false);
            RectTransform qbTRt = qbTxtObj.AddComponent<RectTransform>();
            qbTRt.anchorMin = Vector2.zero; qbTRt.anchorMax = Vector2.one; qbTRt.sizeDelta = Vector2.zero;
            Text qbTxt = qbTxtObj.AddComponent<Text>();
            qbTxt.text = "📝 <b>TEMPLE QUIZ</b>\n<size=15>3D Challenge</size>";
            qbTxt.alignment = TextAnchor.MiddleCenter;
            qbTxt.fontSize = 20;
            qbTxt.fontStyle = FontStyle.Bold;
            qbTxt.color = Color.white;

            // --- RESEARCH TELEMETRY HUD (Top-Left under Mode) ---
            GameObject telemPanel = new GameObject("TelemetryPanel");
            telemPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform tpRt = telemPanel.AddComponent<RectTransform>();
            tpRt.anchorMin = new Vector2(0.015f, 0.22f);
            tpRt.anchorMax = new Vector2(0.24f, 0.56f);
            tpRt.sizeDelta = Vector2.zero;
            Image tpBg = telemPanel.AddComponent<Image>();
            tpBg.color = new Color(0.03f, 0.06f, 0.05f, 0.92f);

            GameObject telemTxtObj = new GameObject("TelemetryText");
            telemTxtObj.transform.SetParent(telemPanel.transform, false);
            RectTransform ttRt = telemTxtObj.AddComponent<RectTransform>();
            ttRt.anchorMin = new Vector2(0.06f, 0.06f); ttRt.anchorMax = new Vector2(0.94f, 0.94f); ttRt.sizeDelta = Vector2.zero;
            Text telemTxt = telemTxtObj.AddComponent<Text>();
            telemTxt.text = "Research measurements: not collected. Cloud: not synced";
            telemTxt.fontSize = 16;
            telemTxt.color = Color.white;
            telemTxt.alignment = TextAnchor.UpperLeft;

            // --- STORY DIALOGUE PANEL (Modal at Bottom) ---
            GameObject storyDlg = new GameObject("StoryDialoguePanel");
            storyDlg.transform.SetParent(canvasObj.transform, false);
            RectTransform sdRt = storyDlg.AddComponent<RectTransform>();
            sdRt.anchorMin = new Vector2(0.18f, 0.18f);
            sdRt.anchorMax = new Vector2(0.82f, 0.44f);
            sdRt.sizeDelta = Vector2.zero;
            Image sdBg = storyDlg.AddComponent<Image>();
            sdBg.color = new Color(0.05f, 0.08f, 0.06f, 0.96f);

            // Dialogue Avatar
            GameObject dlgAvObj = new GameObject("Avatar");
            dlgAvObj.transform.SetParent(storyDlg.transform, false);
            RectTransform daRt = dlgAvObj.AddComponent<RectTransform>();
            daRt.anchorMin = new Vector2(0.02f, 0.1f);
            daRt.anchorMax = new Vector2(0.20f, 0.9f);
            daRt.sizeDelta = Vector2.zero;
            Image dlgAvImg = dlgAvObj.AddComponent<Image>();
            if (mayaSprite != null) dlgAvImg.sprite = mayaSprite;
            dlgAvImg.preserveAspect = true;

            // Speaker Name
            GameObject spkObj = new GameObject("SpeakerName");
            spkObj.transform.SetParent(storyDlg.transform, false);
            RectTransform spkRt = spkObj.AddComponent<RectTransform>();
            spkRt.anchorMin = new Vector2(0.22f, 0.72f);
            spkRt.anchorMax = new Vector2(0.85f, 0.95f);
            spkRt.sizeDelta = Vector2.zero;
            Text spkTxt = spkObj.AddComponent<Text>();
            spkTxt.text = "<b>MAYA (Scholar Insight)</b>";
            spkTxt.fontSize = 22;
            spkTxt.fontStyle = FontStyle.Bold;
            spkTxt.color = new Color(1f, 0.85f, 0.4f);
            spkTxt.alignment = TextAnchor.MiddleLeft;

            // Content Text
            GameObject cntObj = new GameObject("ContentText");
            cntObj.transform.SetParent(storyDlg.transform, false);
            RectTransform cntRt = cntObj.AddComponent<RectTransform>();
            cntRt.anchorMin = new Vector2(0.22f, 0.10f);
            cntRt.anchorMax = new Vector2(0.85f, 0.70f);
            cntRt.sizeDelta = Vector2.zero;
            Text cntTxt = cntObj.AddComponent<Text>();
            cntTxt.text = "“The ancient glyphs show: Torque = Force × Distance. A longer effort arm multiplies your mechanical advantage!”";
            cntTxt.fontSize = 19;
            cntTxt.color = Color.white;
            cntTxt.alignment = TextAnchor.MiddleLeft;

            // Dismiss Button
            GameObject dsmBtnObj = new GameObject("Button_DismissDialogue");
            dsmBtnObj.transform.SetParent(storyDlg.transform, false);
            RectTransform dsmRt = dsmBtnObj.AddComponent<RectTransform>();
            dsmRt.anchorMin = new Vector2(0.86f, 0.25f);
            dsmRt.anchorMax = new Vector2(0.98f, 0.75f);
            dsmRt.sizeDelta = Vector2.zero;
            Button dsmBtn = dsmBtnObj.AddComponent<Button>();
            Image dsmImg = dsmBtnObj.AddComponent<Image>();
            dsmImg.color = new Color(0.7f, 0.55f, 0.2f, 0.95f);

            GameObject dsmTxtObj = new GameObject("Text");
            dsmTxtObj.transform.SetParent(dsmBtnObj.transform, false);
            RectTransform dsmTRt = dsmTxtObj.AddComponent<RectTransform>();
            dsmTRt.anchorMin = Vector2.zero; dsmTRt.anchorMax = Vector2.one; dsmTRt.sizeDelta = Vector2.zero;
            Text dsmTxt = dsmTxtObj.AddComponent<Text>();
            dsmTxt.text = "<b>OK</b>";
            dsmTxt.alignment = TextAnchor.MiddleCenter;
            dsmTxt.fontSize = 20;
            dsmTxt.color = Color.white;

            // --- 6. ATTACH MASTER AR CONTROLLER & MANAGERS ---
            GameObject arManagerObj = new GameObject("ARSceneMasterController");
            var arCtrl = arManagerObj.AddComponent<SecretOfLostTemple.AR.ARSceneUIController>();
            var arSo = new SerializedObject(arCtrl);
            arSo.FindProperty("backToMapButton").objectReferenceValue = backBtn;
            arSo.FindProperty("characterInfoText").objectReferenceValue = charTxt;
            arSo.FindProperty("characterAvatarImage").objectReferenceValue = charAvImg;
            arSo.FindProperty("mayaAvatarSprite").objectReferenceValue = mayaSprite;
            arSo.FindProperty("senukAvatarSprite").objectReferenceValue = senukSprite;
            arSo.FindProperty("modeSwitcher").objectReferenceValue = modeSwitcher;
            arSo.FindProperty("viewOnlyModeBtn").objectReferenceValue = voBtn;
            arSo.FindProperty("interactiveModeBtn").objectReferenceValue = intBtn;
            arSo.FindProperty("currentModeText").objectReferenceValue = modeLbl;
            arSo.FindProperty("modeInstructionText").objectReferenceValue = modeDesc;
            arSo.FindProperty("selectLeverBtn").objectReferenceValue = btnLev;
            arSo.FindProperty("selectPulleyBtn").objectReferenceValue = btnPul;
            arSo.FindProperty("selectRampBtn").objectReferenceValue = btnRmp;
            arSo.FindProperty("selectWheelBtn").objectReferenceValue = btnWhl;
            arSo.FindProperty("leverRoot").objectReferenceValue = leverObj;
            arSo.FindProperty("pulleyRoot").objectReferenceValue = pulleyObj;
            arSo.FindProperty("inclinedPlaneRoot").objectReferenceValue = rampObj;
            arSo.FindProperty("wheelAxleRoot").objectReferenceValue = wheelObj;
            arSo.FindProperty("mechanicalAdvantageText").objectReferenceValue = maText;
            arSo.FindProperty("hintButton").objectReferenceValue = hintBtn;
            arSo.FindProperty("storyDialoguePanel").objectReferenceValue = storyDlg;
            arSo.FindProperty("dialogueSpeakerText").objectReferenceValue = spkTxt;
            arSo.FindProperty("dialogueContentText").objectReferenceValue = cntTxt;
            arSo.FindProperty("dialogueAvatarImage").objectReferenceValue = dlgAvImg;
            arSo.FindProperty("dismissDialogueBtn").objectReferenceValue = dsmBtn;
            arSo.FindProperty("startQuizButton").objectReferenceValue = quizBtn;
            arSo.FindProperty("liveTelemetryText").objectReferenceValue = telemTxt;
            arSo.FindProperty("telemetryPanel").objectReferenceValue = telemPanel;
            arSo.ApplyModifiedProperties();

            // Attach Research Singletons
            arManagerObj.AddComponent<SecretOfLostTemple.AR.CognitiveTelemetryLogger>();
            arManagerObj.AddComponent<SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine>();
            arManagerObj.AddComponent<SecretOfLostTemple.Adaptive.HintSystem>();
            arManagerObj.AddComponent<SecretOfLostTemple.Adaptive.EngagementTracker>();
            arManagerObj.AddComponent<SecretOfLostTemple.Assessment.LearningRetentionTracker>();
            arManagerObj.AddComponent<SecretOfLostTemple.Assessment.ResearchDataExporter>();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void EnsureTargetSprites()
        {
            string[] targetPaths = new string[]
            {
                "Assets/Art/Targets/lever_target_scaled.jpg",
                "Assets/Art/Targets/pulley_target_scaled.jpg",
                "Assets/Art/Targets/inclined_plane_target_scaled.jpg"
            };

            foreach (var p in targetPaths)
            {
                if (File.Exists(p))
                {
                    TextureImporter ti = AssetImporter.GetAtPath(p) as TextureImporter;
                    if (ti != null && ti.textureType != TextureImporterType.Sprite)
                    {
                        ti.textureType = TextureImporterType.Sprite;
                        ti.SaveAndReimport();
                    }
                }
            }
        }

        private static void CreatePhysics3DLabScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Lighting & Camera
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.3f;
            lightObj.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            GameObject camObj = new GameObject("Main Camera");
            camObj.transform.position = new Vector3(0f, 1.3f, -3.4f);
            camObj.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.10f, 0.08f, 1f); // Ancient temple ambient background
            camObj.AddComponent<AudioListener>();

            // 2. EventSystem
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 3. Environment Pedestal / Ground
            GameObject envObj = new GameObject("TempleAltarRoot");
            GameObject altar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            altar.name = "Altar_Pedestal";
            altar.transform.SetParent(envObj.transform, false);
            altar.transform.position = new Vector3(0f, -0.45f, 0f);
            altar.transform.localScale = new Vector3(3.8f, 0.12f, 3.8f);
            SetPrimitiveColor(altar, new Color(0.22f, 0.26f, 0.22f));

            // 4. 3D Lever with GLB Assets (Fulcrum.glb, Beam.glb, Boulder.glb)
            GameObject lever3DRoot = new GameObject("3D_Lever_Mechanism");
            lever3DRoot.transform.position = new Vector3(0f, 0f, 0f);

            // Load 3D GLB Models
            GameObject fulcrumGlb = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Fulcrum.glb");
            GameObject beamGlb = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Beam.glb");
            GameObject boulderGlb = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Boulder.glb");

            // --- Fulcrum Pivot ---
            GameObject fulcrumObj;
            if (fulcrumGlb != null)
            {
                fulcrumObj = PrefabUtility.InstantiatePrefab(fulcrumGlb) as GameObject;
                fulcrumObj.name = "Fulcrum_3D_Model";
                fulcrumObj.transform.SetParent(lever3DRoot.transform, false);
                fulcrumObj.transform.localPosition = Vector3.zero;
                fulcrumObj.transform.localScale = Vector3.one * 0.85f;
            }
            else
            {
                fulcrumObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fulcrumObj.name = "Fulcrum_Primitive";
                fulcrumObj.transform.SetParent(lever3DRoot.transform, false);
                fulcrumObj.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                fulcrumObj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                fulcrumObj.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
                SetPrimitiveColor(fulcrumObj, new Color(0.2f, 0.8f, 0.4f));
            }

            // --- Beam Assembly ---
            GameObject beamPivot = new GameObject("Beam_Pivot_Assembly");
            beamPivot.transform.SetParent(lever3DRoot.transform, false);
            beamPivot.transform.localPosition = new Vector3(0f, 0.45f, 0f);

            GameObject beamObj;
            if (beamGlb != null)
            {
                beamObj = PrefabUtility.InstantiatePrefab(beamGlb) as GameObject;
                beamObj.name = "Beam_3D_Model";
                beamObj.transform.SetParent(beamPivot.transform, false);
                beamObj.transform.localPosition = Vector3.zero;
                beamObj.transform.localScale = Vector3.one * 0.9f;
            }
            else
            {
                beamObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beamObj.name = "Beam_Primitive";
                beamObj.transform.SetParent(beamPivot.transform, false);
                beamObj.transform.localPosition = Vector3.zero;
                beamObj.transform.localScale = new Vector3(2.4f, 0.1f, 0.35f);
                SetPrimitiveColor(beamObj, new Color(0.65f, 0.5f, 0.3f));
            }

            // --- Boulder (Load) ---
            GameObject boulderObj;
            if (boulderGlb != null)
            {
                boulderObj = PrefabUtility.InstantiatePrefab(boulderGlb) as GameObject;
                boulderObj.name = "Boulder_3D_Model";
                boulderObj.transform.SetParent(beamPivot.transform, false);
                boulderObj.transform.localPosition = new Vector3(-1.05f, 0.35f, 0f);
                boulderObj.transform.localScale = Vector3.one * 0.75f;
            }
            else
            {
                boulderObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                boulderObj.name = "Boulder_Primitive";
                boulderObj.transform.SetParent(beamPivot.transform, false);
                boulderObj.transform.localPosition = new Vector3(-1.05f, 0.35f, 0f);
                boulderObj.transform.localScale = Vector3.one * 0.65f;
                SetPrimitiveColor(boulderObj, new Color(0.85f, 0.35f, 0.35f));
            }

            // --- Effort Handle ---
            GameObject effortHandleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            effortHandleObj.name = "Effort_Handle_Point";
            effortHandleObj.transform.SetParent(beamPivot.transform, false);
            effortHandleObj.transform.localPosition = new Vector3(1.05f, 0.15f, 0f);
            effortHandleObj.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            SetPrimitiveColor(effortHandleObj, new Color(1f, 0.85f, 0.2f));

            // Force Lines
            GameObject loadLineObj = new GameObject("LoadForceLine");
            loadLineObj.transform.SetParent(lever3DRoot.transform, false);
            LineRenderer loadLine = loadLineObj.AddComponent<LineRenderer>();

            GameObject effortLineObj = new GameObject("EffortForceLine");
            effortLineObj.transform.SetParent(lever3DRoot.transform, false);
            LineRenderer effortLine = effortLineObj.AddComponent<LineRenderer>();

            // Particle System on Success
            GameObject partObj = new GameObject("SuccessParticles");
            partObj.transform.SetParent(boulderObj.transform, false);
            ParticleSystem ps = partObj.AddComponent<ParticleSystem>();
            var pMain = ps.main;
            pMain.startColor = new Color(1f, 0.85f, 0.2f);
            pMain.startSize = 0.12f;
            pMain.playOnAwake = false;
            pMain.loop = false;

            // Attach LeverPhysicsController
            var leverPhysics = lever3DRoot.AddComponent<SecretOfLostTemple.AR.LeverPhysicsController>();
            var lpSo = new SerializedObject(leverPhysics);
            lpSo.FindProperty("fulcrumRoot").objectReferenceValue = fulcrumObj.transform;
            lpSo.FindProperty("beamRoot").objectReferenceValue = beamPivot.transform;
            lpSo.FindProperty("boulderRoot").objectReferenceValue = boulderObj.transform;
            lpSo.FindProperty("effortHandle").objectReferenceValue = effortHandleObj.transform;
            lpSo.FindProperty("loadForceLine").objectReferenceValue = loadLine;
            lpSo.FindProperty("effortForceLine").objectReferenceValue = effortLine;
            lpSo.FindProperty("liftSuccessParticles").objectReferenceValue = ps;
            lpSo.ApplyModifiedProperties();

            // 5. Full 3D Physics Lab Canvas
            GameObject canvasObj = new GameObject("Physics Lab Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Load Sprites
            Sprite mayaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/profile_maya.png");
            Sprite senukSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/profile_senuk.png");
            Sprite backBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/back_button.png");
            Sprite leverTargetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Targets/lever_target_scaled.jpg");
            Sprite pulleyTargetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Targets/pulley_target_scaled.jpg");
            Sprite inclineTargetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Targets/inclined_plane_target_scaled.jpg");

            // --- TOP NAV BAR ---
            GameObject topBar = new GameObject("TopNavBar");
            topBar.transform.SetParent(canvasObj.transform, false);
            RectTransform tbRt = topBar.AddComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 0.88f);
            tbRt.anchorMax = new Vector2(1f, 0.99f);
            tbRt.sizeDelta = Vector2.zero;
            Image tbBg = topBar.AddComponent<Image>();
            tbBg.color = new Color(0.05f, 0.08f, 0.06f, 0.90f);

            // Back Button
            GameObject backBtnObj = new GameObject("Button_BackToMap");
            backBtnObj.transform.SetParent(topBar.transform, false);
            RectTransform bbRt = backBtnObj.AddComponent<RectTransform>();
            bbRt.anchorMin = new Vector2(0.015f, 0.1f);
            bbRt.anchorMax = new Vector2(0.12f, 0.9f);
            bbRt.sizeDelta = Vector2.zero;
            Button backBtn = backBtnObj.AddComponent<Button>();
            Image bbImg = backBtnObj.AddComponent<Image>();
            if (backBtnSprite != null) bbImg.sprite = backBtnSprite;
            bbImg.preserveAspect = true;

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(topBar.transform, false);
            RectTransform titRt = titleObj.AddComponent<RectTransform>();
            titRt.anchorMin = new Vector2(0.13f, 0.48f);
            titRt.anchorMax = new Vector2(0.68f, 0.95f);
            titRt.sizeDelta = Vector2.zero;
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.text = "🏛️ ANCIENT 3D PHYSICS LAB • LEVER MECHANISM";
            titleTxt.fontSize = 25;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1f, 0.88f, 0.4f);
            titleTxt.alignment = TextAnchor.MiddleLeft;

            GameObject subTitleObj = new GameObject("SubtitleText");
            subTitleObj.transform.SetParent(topBar.transform, false);
            RectTransform subRt = subTitleObj.AddComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.13f, 0.05f);
            subRt.anchorMax = new Vector2(0.68f, 0.48f);
            subRt.sizeDelta = Vector2.zero;
            Text subTxt = subTitleObj.AddComponent<Text>();
            subTxt.text = "Image-Tracked Simple Machine: Real-Time Torque Equilibrium & Mechanical Advantage";
            subTxt.fontSize = 17;
            subTxt.color = new Color(0.85f, 0.85f, 0.85f);
            subTxt.alignment = TextAnchor.MiddleLeft;

            // Character Info
            GameObject charAvObj = new GameObject("CharacterAvatar");
            charAvObj.transform.SetParent(topBar.transform, false);
            RectTransform caRt = charAvObj.AddComponent<RectTransform>();
            caRt.anchorMin = new Vector2(0.70f, 0.08f);
            caRt.anchorMax = new Vector2(0.76f, 0.92f);
            caRt.sizeDelta = Vector2.zero;
            Image charAvImg = charAvObj.AddComponent<Image>();
            if (mayaSprite != null) charAvImg.sprite = mayaSprite;
            charAvImg.preserveAspect = true;

            GameObject charTxtObj = new GameObject("CharacterText");
            charTxtObj.transform.SetParent(topBar.transform, false);
            RectTransform ctRt = charTxtObj.AddComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0.77f, 0.1f);
            ctRt.anchorMax = new Vector2(0.98f, 0.9f);
            ctRt.sizeDelta = Vector2.zero;
            Text charTxt = charTxtObj.AddComponent<Text>();
            charTxt.text = "<b>Explorer Maya</b>\n<size=16><color=#f5d77f>Tier 1 Scholar</color></size>";
            charTxt.fontSize = 19;
            charTxt.fontStyle = FontStyle.Bold;
            charTxt.color = Color.white;
            charTxt.alignment = TextAnchor.MiddleLeft;

            // --- IMAGE SCANNER OVERLAY ---
            GameObject scannerPanel = new GameObject("ScannerOverlayPanel");
            scannerPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform spRt = scannerPanel.AddComponent<RectTransform>();
            spRt.anchorMin = Vector2.zero;
            spRt.anchorMax = Vector2.one;
            spRt.sizeDelta = Vector2.zero;
            Image spBg = scannerPanel.AddComponent<Image>();
            spBg.color = new Color(0.04f, 0.07f, 0.05f, 0.92f);

            // Scanner Target Preview Card
            GameObject cardObj = new GameObject("TargetPreviewCard");
            cardObj.transform.SetParent(scannerPanel.transform, false);
            RectTransform cpRt = cardObj.AddComponent<RectTransform>();
            cpRt.anchorMin = new Vector2(0.35f, 0.35f);
            cpRt.anchorMax = new Vector2(0.65f, 0.78f);
            cpRt.sizeDelta = Vector2.zero;
            Image cpImg = cardObj.AddComponent<Image>();
            cpImg.color = new Color(0.12f, 0.18f, 0.14f, 0.95f);

            GameObject prevImgObj = new GameObject("TargetImage");
            prevImgObj.transform.SetParent(cardObj.transform, false);
            RectTransform piRt = prevImgObj.AddComponent<RectTransform>();
            piRt.anchorMin = new Vector2(0.05f, 0.05f);
            piRt.anchorMax = new Vector2(0.95f, 0.95f);
            piRt.sizeDelta = Vector2.zero;
            Image prevImg = prevImgObj.AddComponent<Image>();
            if (leverTargetSprite != null) prevImg.sprite = leverTargetSprite;
            prevImg.preserveAspect = true;

            // Scanner Instruction Text
            GameObject instObj = new GameObject("InstructionText");
            instObj.transform.SetParent(scannerPanel.transform, false);
            RectTransform instRt = instObj.AddComponent<RectTransform>();
            instRt.anchorMin = new Vector2(0.2f, 0.22f);
            instRt.anchorMax = new Vector2(0.8f, 0.33f);
            instRt.sizeDelta = Vector2.zero;
            Text instTxt = instObj.AddComponent<Text>();
            instTxt.text = "<b>POINT CAMERA AT 'LEVER TARGET'</b>\n<size=18><color=#d5c59f>Scan image to materialize ancient 3D physics mechanism or tap 'Simulate Scan'</color></size>";
            instTxt.fontSize = 24;
            instTxt.alignment = TextAnchor.MiddleCenter;
            instTxt.color = Color.white;

            // Simulate Scan Button
            GameObject simBtnObj = new GameObject("Button_SimulateScan");
            simBtnObj.transform.SetParent(scannerPanel.transform, false);
            RectTransform simRt = simBtnObj.AddComponent<RectTransform>();
            simRt.anchorMin = new Vector2(0.38f, 0.10f);
            simRt.anchorMax = new Vector2(0.62f, 0.20f);
            simRt.sizeDelta = Vector2.zero;
            Button simBtn = simBtnObj.AddComponent<Button>();
            Image simImg = simBtnObj.AddComponent<Image>();
            simImg.color = new Color(0.85f, 0.65f, 0.15f, 0.95f);

            GameObject simTxtObj = new GameObject("Text");
            simTxtObj.transform.SetParent(simBtnObj.transform, false);
            RectTransform simTRt = simTxtObj.AddComponent<RectTransform>();
            simTRt.anchorMin = Vector2.zero; simTRt.anchorMax = Vector2.one; simTRt.sizeDelta = Vector2.zero;
            Text simTxt = simTxtObj.AddComponent<Text>();
            simTxt.text = "⚡ <b>SIMULATE SCAN (TEST NOW)</b>";
            simTxt.alignment = TextAnchor.MiddleCenter;
            simTxt.fontSize = 20;
            simTxt.fontStyle = FontStyle.Bold;
            simTxt.color = Color.white;

            // --- BOTTOM DASHBOARD & SLIDERS ---
            GameObject btmPanel = new GameObject("BottomDashboardPanel");
            btmPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform bpRt = btmPanel.AddComponent<RectTransform>();
            bpRt.anchorMin = new Vector2(0.015f, 0.02f);
            bpRt.anchorMax = new Vector2(0.985f, 0.24f);
            bpRt.sizeDelta = Vector2.zero;
            Image bpBg = btmPanel.AddComponent<Image>();
            bpBg.color = new Color(0.05f, 0.08f, 0.06f, 0.92f);

            // Left Slider: Applied Force
            GameObject forceSldObj = CreateSliderWithLabel(btmPanel, "Slider_AppliedForce", 0.02f, 0.32f, 0.48f, 0.90f, "⚡ <b>Applied Effort Force (F₂):</b>", "0 N");
            Slider forceSld = forceSldObj.GetComponentInChildren<Slider>();
            Text forceValTxt = forceSldObj.transform.Find("ValueText").GetComponent<Text>();

            // Right Slider: Fulcrum Position
            GameObject fulcrumSldObj = CreateSliderWithLabel(btmPanel, "Slider_FulcrumPosition", 0.02f, 0.32f, 0.06f, 0.46f, "📐 <b>Fulcrum Pivot Distance (d₁):</b>", "0.80 m");
            Slider fulcrumSld = fulcrumSldObj.GetComponentInChildren<Slider>();
            Text fulcrumValTxt = fulcrumSldObj.transform.Find("ValueText").GetComponent<Text>();

            // Center: Live Physics Stats
            GameObject statsObj = new GameObject("LivePhysicsStats");
            statsObj.transform.SetParent(btmPanel.transform, false);
            RectTransform stRt = statsObj.AddComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.34f, 0.06f);
            stRt.anchorMax = new Vector2(0.66f, 0.94f);
            stRt.sizeDelta = Vector2.zero;

            GameObject maValObj = new GameObject("Text_MA");
            maValObj.transform.SetParent(statsObj.transform, false);
            RectTransform maRt = maValObj.AddComponent<RectTransform>();
            maRt.anchorMin = new Vector2(0f, 0.52f); maRt.anchorMax = new Vector2(1f, 0.98f); maRt.sizeDelta = Vector2.zero;
            Text maValTxt = maValObj.AddComponent<Text>();
            maValTxt.text = "<b>MA = 2.00x</b>\n<size=15><color=#d5c59f>Effort Arm (1.60m) / Load Arm (0.80m)</color></size>";
            maValTxt.fontSize = 22;
            maValTxt.color = new Color(1f, 0.88f, 0.4f);
            maValTxt.alignment = TextAnchor.MiddleCenter;

            GameObject reqValObj = new GameObject("Text_ReqForce");
            reqValObj.transform.SetParent(statsObj.transform, false);
            RectTransform reqRt = reqValObj.AddComponent<RectTransform>();
            reqRt.anchorMin = new Vector2(0f, 0.02f); reqRt.anchorMax = new Vector2(1f, 0.48f); reqRt.sizeDelta = Vector2.zero;
            Text reqValTxt = reqValObj.AddComponent<Text>();
            reqValTxt.text = "<b>Required Force: 500 N</b>\n<size=14><color=#f5d77f>Load / MA (1000N / 2.00x)</color></size>";
            reqValTxt.fontSize = 18;
            reqValTxt.color = Color.white;
            reqValTxt.alignment = TextAnchor.MiddleCenter;

            // Right: Status Badge & Preset Buttons
            GameObject statusBadgeObj = new GameObject("StatusBadge");
            statusBadgeObj.transform.SetParent(btmPanel.transform, false);
            RectTransform sbRt = statusBadgeObj.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0.68f, 0.45f);
            sbRt.anchorMax = new Vector2(0.98f, 0.92f);
            sbRt.sizeDelta = Vector2.zero;
            Text statusBadgeTxt = statusBadgeObj.AddComponent<Text>();
            statusBadgeTxt.text = "<color=#ffffff><b>DRAG EFFORT SLIDER TO APPLY FORCE</b>\nObserve how mechanical advantage changes the force needed.</color>";
            statusBadgeTxt.fontSize = 17;
            statusBadgeTxt.alignment = TextAnchor.MiddleLeft;

            // Preset Buttons Row
            Button preHighBtn = CreateSmallPresetButton(btmPanel, "Btn_HighMA", 0.68f, 0.77f, "High MA (3.0x)");
            Button preEqBtn = CreateSmallPresetButton(btmPanel, "Btn_EqualArms", 0.78f, 0.87f, "Equal (1.0x)");
            Button preLowBtn = CreateSmallPresetButton(btmPanel, "Btn_LowMA", 0.88f, 0.98f, "Low MA (0.5x)");

            // --- 6. ATTACH MASTER LAB CONTROLLER ---
            GameObject labManagerObj = new GameObject("Physics3DLabManager");
            var labCtrl = labManagerObj.AddComponent<SecretOfLostTemple.AR.Physics3DLabController>();
            var labSo = new SerializedObject(labCtrl);
            labSo.FindProperty("leverPhysics").objectReferenceValue = leverPhysics;
            labSo.FindProperty("lever3DRoot").objectReferenceValue = lever3DRoot;
            labSo.FindProperty("scannerOverlayPanel").objectReferenceValue = scannerPanel;
            labSo.FindProperty("targetPreviewImage").objectReferenceValue = prevImg;
            labSo.FindProperty("scannerInstructionText").objectReferenceValue = instTxt;
            labSo.FindProperty("simulateTargetFoundBtn").objectReferenceValue = simBtn;
            labSo.FindProperty("leverTargetSprite").objectReferenceValue = leverTargetSprite;
            labSo.FindProperty("pulleyTargetSprite").objectReferenceValue = pulleyTargetSprite;
            labSo.FindProperty("inclinedPlaneTargetSprite").objectReferenceValue = inclineTargetSprite;
            labSo.FindProperty("backToMapButton").objectReferenceValue = backBtn;
            labSo.FindProperty("characterInfoText").objectReferenceValue = charTxt;
            labSo.FindProperty("characterAvatarImage").objectReferenceValue = charAvImg;
            labSo.FindProperty("mayaAvatarSprite").objectReferenceValue = mayaSprite;
            labSo.FindProperty("senukAvatarSprite").objectReferenceValue = senukSprite;
            labSo.FindProperty("appliedForceSlider").objectReferenceValue = forceSld;
            labSo.FindProperty("appliedForceValueText").objectReferenceValue = forceValTxt;
            labSo.FindProperty("fulcrumPositionSlider").objectReferenceValue = fulcrumSld;
            labSo.FindProperty("fulcrumPositionValueText").objectReferenceValue = fulcrumValTxt;
            labSo.FindProperty("presetHighMAButton").objectReferenceValue = preHighBtn;
            labSo.FindProperty("presetEqualArmsButton").objectReferenceValue = preEqBtn;
            labSo.FindProperty("presetLowMAButton").objectReferenceValue = preLowBtn;
            labSo.FindProperty("mechanicalAdvantageValueText").objectReferenceValue = maValTxt;
            labSo.FindProperty("requiredForceValueText").objectReferenceValue = reqValTxt;
            labSo.FindProperty("simulationStatusBadge").objectReferenceValue = statusBadgeTxt;
            labSo.ApplyModifiedProperties();

            // Attach Singletons
            labManagerObj.AddComponent<SecretOfLostTemple.AR.CognitiveTelemetryLogger>();
            labManagerObj.AddComponent<SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine>();
            labManagerObj.AddComponent<SecretOfLostTemple.Adaptive.EngagementTracker>();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static GameObject CreateSliderWithLabel(GameObject parent, string name, float xMin, float xMax, float yMin, float yMax, string label, string defaultVal)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);
            RectTransform cRt = container.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(xMin, yMin);
            cRt.anchorMax = new Vector2(xMax, yMax);
            cRt.sizeDelta = Vector2.zero;

            GameObject lblObj = new GameObject("Label");
            lblObj.transform.SetParent(container.transform, false);
            RectTransform lRt = lblObj.AddComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0f, 0.55f); lRt.anchorMax = new Vector2(0.7f, 1f); lRt.sizeDelta = Vector2.zero;
            Text lblTxt = lblObj.AddComponent<Text>();
            lblTxt.text = label;
            lblTxt.fontSize = 17;
            lblTxt.color = Color.white;
            lblTxt.alignment = TextAnchor.MiddleLeft;

            GameObject valObj = new GameObject("ValueText");
            valObj.transform.SetParent(container.transform, false);
            RectTransform vRt = valObj.AddComponent<RectTransform>();
            vRt.anchorMin = new Vector2(0.72f, 0.55f); vRt.anchorMax = new Vector2(1f, 1f); vRt.sizeDelta = Vector2.zero;
            Text vTxt = valObj.AddComponent<Text>();
            vTxt.text = defaultVal;
            vTxt.fontSize = 18;
            vTxt.fontStyle = FontStyle.Bold;
            vTxt.color = new Color(1f, 0.85f, 0.35f);
            vTxt.alignment = TextAnchor.MiddleRight;

            GameObject sldObj = new GameObject("Slider");
            sldObj.transform.SetParent(container.transform, false);
            RectTransform sRt = sldObj.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0f, 0.05f); sRt.anchorMax = new Vector2(1f, 0.45f); sRt.sizeDelta = Vector2.zero;
            Slider sld = sldObj.AddComponent<Slider>();

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sldObj.transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.18f, 0.22f, 0.18f, 1f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sldObj.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one; faRt.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fRt = fill.AddComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero; fRt.anchorMax = Vector2.one; fRt.sizeDelta = Vector2.zero;
            Image fImg = fill.AddComponent<Image>();
            fImg.color = new Color(0.9f, 0.75f, 0.2f, 1f);

            sld.targetGraphic = fImg;
            sld.fillRect = fRt;

            return container;
        }

        private static Button CreateSmallPresetButton(GameObject parent, string name, float xMin, float xMax, string label)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, 0.08f);
            rt.anchorMax = new Vector2(xMax, 0.40f);
            rt.sizeDelta = Vector2.zero;
            Button btn = btnObj.AddComponent<Button>();
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.35f, 0.28f, 0.95f);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
            Text txt = txtObj.AddComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 15;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;

            return btn;
        }

        private static Button CreateStoneButton(GameObject parent, string name, float xMin, float xMax, string label)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, 0.12f);
            rt.anchorMax = new Vector2(xMax, 0.88f);
            rt.sizeDelta = Vector2.zero;
            Button btn = btnObj.AddComponent<Button>();
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.18f, 0.28f, 0.22f, 0.95f);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
            Text txt = txtObj.AddComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 18;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(1f, 0.95f, 0.8f, 1f);

            return btn;
        }

        private static void SetPrimitiveColor(GameObject obj, Color col)
        {
            if (obj == null) return;
            var ren = obj.GetComponent<Renderer>();
            if (ren == null) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.color = col;
                ren.sharedMaterial = mat;
            }
        }
    }
}
#endif
