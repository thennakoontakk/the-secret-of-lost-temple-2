using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using SecretOfLostTemple.AR;
using SecretOfLostTemple.Home;
using SecretOfLostTemple.Story;

namespace SecretOfLostTemple.Editor
{
    public static class LeverARUISetup
    {
        [MenuItem("LostTemple/Setup Lever AR Custom UI")]
        public static void BuildLeverARUI()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_App/Scenes/MainARScene.unity");
            var canvas = GameObject.Find("AR Main Canvas");
            if (canvas == null)
            {
                Debug.LogError("AR Main Canvas not found!");
                return;
            }

            // Hide old HUD panels
            string[] oldPanels = new string[] {
                "TopNavBar", "Component1_ModePanel", "BottomMachineBar", 
                "Text_MechanicalAdvantage", "Button_RequestHint", "Button_StartQuiz", 
                "TelemetryPanel", "StoryDialoguePanel"
            };
            foreach (var p in oldPanels)
            {
                var tr = canvas.transform.Find(p);
                if (tr != null) tr.gameObject.SetActive(false);
            }

            // Find or Create LeverAR_UIRoot
            var uiRootTr = canvas.transform.Find("LeverAR_UIRoot");
            GameObject uiRoot;
            if (uiRootTr == null)
            {
                uiRoot = new GameObject("LeverAR_UIRoot", typeof(RectTransform));
                uiRoot.transform.SetParent(canvas.transform, false);
            }
            else
            {
                uiRoot = uiRootTr.gameObject;
            }
            var rtRoot = uiRoot.GetComponent<RectTransform>() ?? uiRoot.AddComponent<RectTransform>();
            rtRoot.anchorMin = Vector2.zero;
            rtRoot.anchorMax = Vector2.one;
            rtRoot.sizeDelta = Vector2.zero;

            // 1. TOP BAR
            var topBarTr = uiRoot.transform.Find("TopBar");
            GameObject topBar = topBarTr != null ? topBarTr.gameObject : new GameObject("TopBar", typeof(RectTransform));
            topBar.transform.SetParent(uiRoot.transform, false);
            var rtTop = topBar.GetComponent<RectTransform>() ?? topBar.AddComponent<RectTransform>();
            rtTop.anchorMin = new Vector2(0f, 1f);
            rtTop.anchorMax = new Vector2(1f, 1f);
            rtTop.pivot = new Vector2(0.5f, 1f);
            rtTop.anchoredPosition = new Vector2(0f, -15f);
            rtTop.sizeDelta = new Vector2(-40f, 130f);

            // 1A. Back Button (Top Left - 2X Enlarger)
            var backBtnTr = topBar.transform.Find("Btn_Back");
            GameObject backBtnGo = backBtnTr != null ? backBtnTr.gameObject : new GameObject("Btn_Back", typeof(RectTransform));
            backBtnGo.transform.SetParent(topBar.transform, false);
            var rtBack = backBtnGo.GetComponent<RectTransform>() ?? backBtnGo.AddComponent<RectTransform>();
            rtBack.anchorMin = new Vector2(0f, 0.5f);
            rtBack.anchorMax = new Vector2(0f, 0.5f);
            rtBack.anchoredPosition = new Vector2(85f, 0f);
            rtBack.sizeDelta = new Vector2(140f, 140f);
            
            var imgBack = backBtnGo.GetComponent<Image>() ?? backBtnGo.AddComponent<Image>();
            var backSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/back_button.png");
            if (backSprite != null)
            {
                imgBack.sprite = backSprite;
                imgBack.color = Color.white;
                imgBack.preserveAspect = true;
            }
            else
            {
                imgBack.color = new Color(0.35f, 0.25f, 0.15f, 0.95f);
            }
            var btnBack = backBtnGo.GetComponent<Button>() ?? backBtnGo.AddComponent<Button>();

            // Remove any old text or icons from back button
            for (int i = backBtnGo.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(backBtnGo.transform.GetChild(i).gameObject);
            }

            // 1B. Center Carved Header Banner: "LEVER AR"
            var bannerTr = topBar.transform.Find("Banner_LeverAR");
            GameObject bannerGo = bannerTr != null ? bannerTr.gameObject : new GameObject("Banner_LeverAR", typeof(RectTransform));
            bannerGo.transform.SetParent(topBar.transform, false);
            var rtBanner = bannerGo.GetComponent<RectTransform>() ?? bannerGo.AddComponent<RectTransform>();
            rtBanner.anchorMin = new Vector2(0.5f, 0.5f);
            rtBanner.anchorMax = new Vector2(0.5f, 0.5f);
            rtBanner.anchoredPosition = new Vector2(0f, 0f);
            rtBanner.sizeDelta = new Vector2(460f, 95f);
            var imgBanner = bannerGo.GetComponent<Image>() ?? bannerGo.AddComponent<Image>();
            imgBanner.color = new Color(0.38f, 0.22f, 0.10f, 0.96f);

            var bannerTxtTr = bannerGo.transform.Find("Text");
            GameObject bannerTxtGo = bannerTxtTr != null ? bannerTxtTr.gameObject : new GameObject("Text", typeof(RectTransform));
            bannerTxtGo.transform.SetParent(bannerGo.transform, false);
            var txtBanner = bannerTxtGo.GetComponent<Text>() ?? bannerTxtGo.AddComponent<Text>();
            txtBanner.text = "<b>LEVER <size=30><color=#ffdd66>AR</color></size></b>";
            txtBanner.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtBanner.fontSize = 48;
            txtBanner.alignment = TextAnchor.MiddleCenter;
            txtBanner.color = new Color(1f, 0.88f, 0.45f);
            txtBanner.rectTransform.anchorMin = Vector2.zero;
            txtBanner.rectTransform.anchorMax = Vector2.one;
            txtBanner.rectTransform.sizeDelta = Vector2.zero;

            // 1C. Top Right: Mode Toggle Button (2X Enlarger)
            var modeBtnTr = topBar.transform.Find("Btn_ModeToggle");
            GameObject modeBtnGo = modeBtnTr != null ? modeBtnTr.gameObject : new GameObject("Btn_ModeToggle", typeof(RectTransform));
            modeBtnGo.transform.SetParent(topBar.transform, false);
            var rtMode = modeBtnGo.GetComponent<RectTransform>() ?? modeBtnGo.AddComponent<RectTransform>();
            rtMode.anchorMin = new Vector2(1f, 0.5f);
            rtMode.anchorMax = new Vector2(1f, 0.5f);
            rtMode.anchoredPosition = new Vector2(-195f, 0f);
            rtMode.sizeDelta = new Vector2(360f, 180f);
            var imgMode = modeBtnGo.GetComponent<Image>() ?? modeBtnGo.AddComponent<Image>();
            var interactiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_interactive_mode.png");
            if (interactiveSprite != null)
            {
                imgMode.sprite = interactiveSprite;
                imgMode.color = Color.white;
                imgMode.preserveAspect = true;
            }
            else
            {
                imgMode.color = new Color(0.2f, 0.65f, 0.3f, 0.95f);
            }
            var btnMode = modeBtnGo.GetComponent<Button>() ?? modeBtnGo.AddComponent<Button>();

            var modeTxtTr = modeBtnGo.transform.Find("Text");
            GameObject modeTxtGo = modeTxtTr != null ? modeTxtTr.gameObject : new GameObject("Text", typeof(RectTransform));
            modeTxtGo.transform.SetParent(modeBtnGo.transform, false);
            var txtMode = modeTxtGo.GetComponent<Text>() ?? modeTxtGo.AddComponent<Text>();
            txtMode.text = interactiveSprite != null ? "" : "<b>INTERACTIVE MODE</b>";
            txtMode.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtMode.fontSize = 20;
            txtMode.alignment = TextAnchor.MiddleCenter;
            txtMode.color = Color.white;
            txtMode.rectTransform.anchorMin = Vector2.zero;
            txtMode.rectTransform.anchorMax = Vector2.one;
            txtMode.rectTransform.sizeDelta = Vector2.zero;

            // 2. RIGHT SIDE PANEL: "How it works" Custom High-Res Tablet
            var howPanelTr = uiRoot.transform.Find("Panel_HowItWorks");
            GameObject howPanelGo = howPanelTr != null ? howPanelTr.gameObject : new GameObject("Panel_HowItWorks", typeof(RectTransform));
            howPanelGo.transform.SetParent(uiRoot.transform, false);
            var rtHow = howPanelGo.GetComponent<RectTransform>() ?? howPanelGo.AddComponent<RectTransform>();
            rtHow.anchorMin = new Vector2(1f, 0.5f);
            rtHow.anchorMax = new Vector2(1f, 0.5f);
            rtHow.pivot = new Vector2(1f, 0.5f);
            rtHow.anchoredPosition = new Vector2(-25f, 35f);
            rtHow.sizeDelta = new Vector2(340f, 510f);
            
            var imgHow = howPanelGo.GetComponent<Image>() ?? howPanelGo.AddComponent<Image>();
            var howItWorksSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/how_it_works_panel.png");
            if (howItWorksSprite != null)
            {
                imgHow.sprite = howItWorksSprite;
                imgHow.color = Color.white;
                imgHow.preserveAspect = true;
            }
            else
            {
                imgHow.color = new Color(0.30f, 0.18f, 0.08f, 0.92f);
            }

            // Remove any redundant child texts since artwork has carved illustrations & text
            for (int i = howPanelGo.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(howPanelGo.transform.GetChild(i).gameObject);
            }



            // 3. BOTTOM CARVED STONE CONSOLE (Active ONLY in Interactive Mode)
            var botBarTr = uiRoot.transform.Find("BottomConsole");
            GameObject botBarGo = botBarTr != null ? botBarTr.gameObject : new GameObject("BottomConsole", typeof(RectTransform));
            botBarGo.transform.SetParent(uiRoot.transform, false);
            var rtBot = botBarGo.GetComponent<RectTransform>() ?? botBarGo.AddComponent<RectTransform>();
            rtBot.anchorMin = new Vector2(0.5f, 0f);
            rtBot.anchorMax = new Vector2(0.5f, 0f);
            rtBot.pivot = new Vector2(0.5f, 0f);
            rtBot.anchoredPosition = new Vector2(0f, 25f);
            rtBot.sizeDelta = new Vector2(1580f, 195f);
            var imgBot = botBarGo.GetComponent<Image>() ?? botBarGo.AddComponent<Image>();
            imgBot.color = new Color(0f, 0f, 0f, 0f); // Completely Transparent

            // 3A. FORCE -/+ Custom Artwork Widget Container
            var forceBoxTr = botBarGo.transform.Find("Box_Force");
            GameObject forceBoxGo = forceBoxTr != null ? forceBoxTr.gameObject : new GameObject("Box_Force", typeof(RectTransform));
            forceBoxGo.transform.SetParent(botBarGo.transform, false);
            var rtForceBox = forceBoxGo.GetComponent<RectTransform>() ?? forceBoxGo.AddComponent<RectTransform>();
            rtForceBox.anchorMin = new Vector2(0f, 0.5f);
            rtForceBox.anchorMax = new Vector2(0f, 0.5f);
            rtForceBox.anchoredPosition = new Vector2(230f, 0f);
            rtForceBox.sizeDelta = new Vector2(380f, 200f);
            
            var imgForceBox = forceBoxGo.GetComponent<Image>() ?? forceBoxGo.AddComponent<Image>();
            var forceSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/force_adjust.png");
            if (forceSprite != null)
            {
                imgForceBox.sprite = forceSprite;
                imgForceBox.color = Color.white;
                imgForceBox.preserveAspect = true;
            }
            else
            {
                imgForceBox.color = new Color(0.28f, 0.22f, 0.16f, 1f);
            }

            var oldForceTitle = forceBoxGo.transform.Find("Title");
            if (oldForceTitle != null) GameObject.DestroyImmediate(oldForceTitle.gameObject);

            // Left [-] Interactive Click Region
            var forceDecBtnTr = forceBoxGo.transform.Find("Btn_Dec");
            GameObject forceDecBtnGo = forceDecBtnTr != null ? forceDecBtnTr.gameObject : new GameObject("Btn_Dec", typeof(RectTransform));
            forceDecBtnGo.transform.SetParent(forceBoxGo.transform, false);
            var rtDec = forceDecBtnGo.GetComponent<RectTransform>() ?? forceDecBtnGo.AddComponent<RectTransform>();
            rtDec.anchorMin = new Vector2(0.04f, 0.05f);
            rtDec.anchorMax = new Vector2(0.26f, 0.80f);
            rtDec.offsetMin = Vector2.zero;
            rtDec.offsetMax = Vector2.zero;
            var imgDec = forceDecBtnGo.GetComponent<Image>() ?? forceDecBtnGo.AddComponent<Image>();
            imgDec.color = new Color(0f, 0f, 0f, 0f);
            var btnDec = forceDecBtnGo.GetComponent<Button>() ?? forceDecBtnGo.AddComponent<Button>();
            for (int i = forceDecBtnGo.transform.childCount - 1; i >= 0; i--) GameObject.DestroyImmediate(forceDecBtnGo.transform.GetChild(i).gameObject);

            // Middle Empty Wooden Space: Force Value Text
            var forceValTr = forceBoxGo.transform.Find("ValText");
            GameObject forceValGo = forceValTr != null ? forceValTr.gameObject : new GameObject("ValText", typeof(RectTransform));
            forceValGo.transform.SetParent(forceBoxGo.transform, false);
            var txtForceVal = forceValGo.GetComponent<Text>() ?? forceValGo.AddComponent<Text>();
            txtForceVal.text = "<b>60 N</b>";
            txtForceVal.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtForceVal.fontSize = 38;
            txtForceVal.alignment = TextAnchor.MiddleCenter;
            txtForceVal.color = new Color(1f, 0.92f, 0.5f); // Glowing Gold
            txtForceVal.rectTransform.anchorMin = new Vector2(0.28f, 0.08f);
            txtForceVal.rectTransform.anchorMax = new Vector2(0.72f, 0.72f);
            txtForceVal.rectTransform.offsetMin = Vector2.zero;
            txtForceVal.rectTransform.offsetMax = Vector2.zero;

            // Right [+] Interactive Click Region
            var forceIncBtnTr = forceBoxGo.transform.Find("Btn_Inc");
            GameObject forceIncBtnGo = forceIncBtnTr != null ? forceIncBtnTr.gameObject : new GameObject("Btn_Inc", typeof(RectTransform));
            forceIncBtnGo.transform.SetParent(forceBoxGo.transform, false);
            var rtInc = forceIncBtnGo.GetComponent<RectTransform>() ?? forceIncBtnGo.AddComponent<RectTransform>();
            rtInc.anchorMin = new Vector2(0.74f, 0.05f);
            rtInc.anchorMax = new Vector2(0.96f, 0.80f);
            rtInc.offsetMin = Vector2.zero;
            rtInc.offsetMax = Vector2.zero;
            var imgInc = forceIncBtnGo.GetComponent<Image>() ?? forceIncBtnGo.AddComponent<Image>();
            imgInc.color = new Color(0f, 0f, 0f, 0f);
            var btnInc = forceIncBtnGo.GetComponent<Button>() ?? forceIncBtnGo.AddComponent<Button>();
            for (int i = forceIncBtnGo.transform.childCount - 1; i >= 0; i--) GameObject.DestroyImmediate(forceIncBtnGo.transform.GetChild(i).gameObject);

            // 3B. MOVE FULCRUM Custom Artwork Widget Container
            var fulcrumBoxTr = botBarGo.transform.Find("Box_Fulcrum");
            GameObject fulcrumBoxGo = fulcrumBoxTr != null ? fulcrumBoxTr.gameObject : new GameObject("Box_Fulcrum", typeof(RectTransform));
            fulcrumBoxGo.transform.SetParent(botBarGo.transform, false);
            var rtFulcBox = fulcrumBoxGo.GetComponent<RectTransform>() ?? fulcrumBoxGo.AddComponent<RectTransform>();
            rtFulcBox.anchorMin = new Vector2(0f, 0.5f);
            rtFulcBox.anchorMax = new Vector2(0f, 0.5f);
            rtFulcBox.anchoredPosition = new Vector2(640f, 0f);
            rtFulcBox.sizeDelta = new Vector2(350f, 210f);
            
            var imgFulcBox = fulcrumBoxGo.GetComponent<Image>() ?? fulcrumBoxGo.AddComponent<Image>();
            var fulcrumSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/fulcrum_adjust.png");
            if (fulcrumSprite != null)
            {
                imgFulcBox.sprite = fulcrumSprite;
                imgFulcBox.color = Color.white;
                imgFulcBox.preserveAspect = true;
            }
            else
            {
                imgFulcBox.color = new Color(0.28f, 0.22f, 0.16f, 1f);
            }

            var oldFulcTitle = fulcrumBoxGo.transform.Find("Title");
            if (oldFulcTitle != null) GameObject.DestroyImmediate(oldFulcTitle.gameObject);

            var oldFulcVal = fulcrumBoxGo.transform.Find("ValText");
            if (oldFulcVal != null) GameObject.DestroyImmediate(oldFulcVal.gameObject);

            // Left [ ⬅ ] Arrow Click Region
            var fulcLeftBtnTr = fulcrumBoxGo.transform.Find("Btn_FulcLeft");
            GameObject fulcLeftBtnGo = fulcLeftBtnTr != null ? fulcLeftBtnTr.gameObject : new GameObject("Btn_FulcLeft", typeof(RectTransform));
            fulcLeftBtnGo.transform.SetParent(fulcrumBoxGo.transform, false);
            var rtFulcLeft = fulcLeftBtnGo.GetComponent<RectTransform>() ?? fulcLeftBtnGo.AddComponent<RectTransform>();
            rtFulcLeft.anchorMin = new Vector2(0.12f, 0.10f);
            rtFulcLeft.anchorMax = new Vector2(0.48f, 0.76f);
            rtFulcLeft.offsetMin = Vector2.zero;
            rtFulcLeft.offsetMax = Vector2.zero;
            var imgFulcLeft = fulcLeftBtnGo.GetComponent<Image>() ?? fulcLeftBtnGo.AddComponent<Image>();
            imgFulcLeft.color = new Color(0f, 0f, 0f, 0f);
            var btnFulcLeft = fulcLeftBtnGo.GetComponent<Button>() ?? fulcLeftBtnGo.AddComponent<Button>();
            for (int i = fulcLeftBtnGo.transform.childCount - 1; i >= 0; i--) GameObject.DestroyImmediate(fulcLeftBtnGo.transform.GetChild(i).gameObject);

            // Right [ ➡ ] Arrow Click Region
            var fulcRightBtnTr = fulcrumBoxGo.transform.Find("Btn_FulcRight");
            GameObject fulcRightBtnGo = fulcRightBtnTr != null ? fulcRightBtnTr.gameObject : new GameObject("Btn_FulcRight", typeof(RectTransform));
            fulcRightBtnGo.transform.SetParent(fulcrumBoxGo.transform, false);
            var rtFulcRight = fulcRightBtnGo.GetComponent<RectTransform>() ?? fulcRightBtnGo.AddComponent<RectTransform>();
            rtFulcRight.anchorMin = new Vector2(0.52f, 0.10f);
            rtFulcRight.anchorMax = new Vector2(0.88f, 0.76f);
            rtFulcRight.offsetMin = Vector2.zero;
            rtFulcRight.offsetMax = Vector2.zero;
            var imgFulcRight = fulcRightBtnGo.GetComponent<Image>() ?? fulcRightBtnGo.AddComponent<Image>();
            imgFulcRight.color = new Color(0f, 0f, 0f, 0f);
            var btnFulcRight = fulcRightBtnGo.GetComponent<Button>() ?? fulcRightBtnGo.AddComponent<Button>();
            for (int i = fulcRightBtnGo.transform.childCount - 1; i >= 0; i--) GameObject.DestroyImmediate(fulcRightBtnGo.transform.GetChild(i).gameObject);

            // 3C. RESET Button
            var resetBtnTr = botBarGo.transform.Find("Btn_Reset");
            GameObject resetBtnGo = resetBtnTr != null ? resetBtnTr.gameObject : new GameObject("Btn_Reset", typeof(RectTransform));
            resetBtnGo.transform.SetParent(botBarGo.transform, false);
            var rtReset = resetBtnGo.GetComponent<RectTransform>() ?? resetBtnGo.AddComponent<RectTransform>();
            rtReset.anchorMin = new Vector2(0f, 0.5f);
            rtReset.anchorMax = new Vector2(0f, 0.5f);
            rtReset.anchoredPosition = new Vector2(980f, 0f);
            rtReset.sizeDelta = new Vector2(175f, 175f);
            
            var imgReset = resetBtnGo.GetComponent<Image>() ?? resetBtnGo.AddComponent<Image>();
            var resetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_reset.png");
            if (resetSprite != null)
            {
                imgReset.sprite = resetSprite;
                imgReset.color = Color.white;
                imgReset.preserveAspect = true;
            }
            else
            {
                imgReset.color = new Color(0.35f, 0.28f, 0.22f, 1f);
            }
            var btnReset = resetBtnGo.GetComponent<Button>() ?? resetBtnGo.AddComponent<Button>();

            for (int i = resetBtnGo.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(resetBtnGo.transform.GetChild(i).gameObject);
            }

            // 3D. Big Glowing "TRY CHALLENGE" Button with Custom Artwork
            var tryBtnTr = botBarGo.transform.Find("Btn_TryChallenge");
            GameObject tryBtnGo = tryBtnTr != null ? tryBtnTr.gameObject : new GameObject("Btn_TryChallenge", typeof(RectTransform));
            tryBtnGo.transform.SetParent(botBarGo.transform, false);
            var rtTry = tryBtnGo.GetComponent<RectTransform>() ?? tryBtnGo.AddComponent<RectTransform>();
            rtTry.anchorMin = new Vector2(1f, 0.5f);
            rtTry.anchorMax = new Vector2(1f, 0.5f);
            rtTry.anchoredPosition = new Vector2(-190f, 0f);
            rtTry.sizeDelta = new Vector2(360f, 180f);
            var imgTry = tryBtnGo.GetComponent<Image>() ?? tryBtnGo.AddComponent<Image>();
            var trySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_try_challenge.png");
            if (trySprite != null)
            {
                imgTry.sprite = trySprite;
                imgTry.color = Color.white;
                imgTry.preserveAspect = true;
            }
            else
            {
                imgTry.color = new Color(0.40f, 0.75f, 0.15f, 1f);
            }
            var btnTry = tryBtnGo.GetComponent<Button>() ?? tryBtnGo.AddComponent<Button>();

            for (int i = tryBtnGo.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(tryBtnGo.transform.GetChild(i).gameObject);
            }

            // 4. VIEW-ONLY SUB-NAVIGATION BAR (Active in View-Only Mode!)
            var viewOnlyBarTr = uiRoot.transform.Find("ViewOnlyBottomBar");
            GameObject viewOnlyBarGo = viewOnlyBarTr != null ? viewOnlyBarTr.gameObject : new GameObject("ViewOnlyBottomBar", typeof(RectTransform));
            viewOnlyBarGo.transform.SetParent(uiRoot.transform, false);
            var rtVO = viewOnlyBarGo.GetComponent<RectTransform>() ?? viewOnlyBarGo.AddComponent<RectTransform>();
            rtVO.anchorMin = new Vector2(0.5f, 0f);
            rtVO.anchorMax = new Vector2(0.5f, 0f);
            rtVO.pivot = new Vector2(0.5f, 0f);
            rtVO.anchoredPosition = new Vector2(0f, 25f);
            rtVO.sizeDelta = new Vector2(1200f, 195f);
            var imgVO = viewOnlyBarGo.GetComponent<Image>() ?? viewOnlyBarGo.AddComponent<Image>();
            imgVO.color = new Color(0f, 0f, 0f, 0f); // Transparent container

            // 4A. Button 1: 360° Forces & Motion
            var forcesBtnTr = viewOnlyBarGo.transform.Find("Btn_ForcesAndMotion");
            GameObject forcesBtnGo = forcesBtnTr != null ? forcesBtnTr.gameObject : new GameObject("Btn_ForcesAndMotion", typeof(RectTransform));
            forcesBtnGo.transform.SetParent(viewOnlyBarGo.transform, false);
            var rtForces = forcesBtnGo.GetComponent<RectTransform>() ?? forcesBtnGo.AddComponent<RectTransform>();
            rtForces.anchorMin = new Vector2(0.5f, 0.5f);
            rtForces.anchorMax = new Vector2(0.5f, 0.5f);
            rtForces.anchoredPosition = new Vector2(-280f, 0f);
            rtForces.sizeDelta = new Vector2(500f, 170f);
            var imgForces = forcesBtnGo.GetComponent<Image>() ?? forcesBtnGo.AddComponent<Image>();
            var forcesSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_forces_motion.png");
            if (forcesSprite != null) imgForces.sprite = forcesSprite;
            imgForces.color = Color.white;
            imgForces.preserveAspect = true;
            var btnForces = forcesBtnGo.GetComponent<Button>() ?? forcesBtnGo.AddComponent<Button>();

            var forcesTxtTr = forcesBtnGo.transform.Find("Text");
            GameObject forcesTxtGo = forcesTxtTr != null ? forcesTxtTr.gameObject : new GameObject("Text", typeof(RectTransform));
            forcesTxtGo.transform.SetParent(forcesBtnGo.transform, false);
            var txtForces = forcesTxtGo.GetComponent<Text>() ?? forcesTxtGo.AddComponent<Text>();
            txtForces.text = "<b>🔄 360° FORCES & MOTION</b>\n<size=17><color=#ffe066>Vector Arrows & Rotation (බල සහ භ්‍රමණය)</color></size>";
            txtForces.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtForces.fontSize = 21;
            txtForces.alignment = TextAnchor.MiddleCenter;
            txtForces.color = Color.white;
            txtForces.rectTransform.anchorMin = Vector2.zero;
            txtForces.rectTransform.anchorMax = Vector2.one;
            txtForces.rectTransform.sizeDelta = new Vector2(-30f, -20f);

            // 4B. Button 2: Exploded Parts & Anatomy
            var explodedBtnTr = viewOnlyBarGo.transform.Find("Btn_ExplodedParts");
            GameObject explodedBtnGo = explodedBtnTr != null ? explodedBtnTr.gameObject : new GameObject("Btn_ExplodedParts", typeof(RectTransform));
            explodedBtnGo.transform.SetParent(viewOnlyBarGo.transform, false);
            var rtExploded = explodedBtnGo.GetComponent<RectTransform>() ?? explodedBtnGo.AddComponent<RectTransform>();
            rtExploded.anchorMin = new Vector2(0.5f, 0.5f);
            rtExploded.anchorMax = new Vector2(0.5f, 0.5f);
            rtExploded.anchoredPosition = new Vector2(280f, 0f);
            rtExploded.sizeDelta = new Vector2(500f, 170f);
            var imgExploded = explodedBtnGo.GetComponent<Image>() ?? explodedBtnGo.AddComponent<Image>();
            var explodedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_exploded_parts.png");
            if (explodedSprite != null) imgExploded.sprite = explodedSprite;
            imgExploded.color = Color.white;
            imgExploded.preserveAspect = true;
            var btnExploded = explodedBtnGo.GetComponent<Button>() ?? explodedBtnGo.AddComponent<Button>();

            var explodedTxtTr = explodedBtnGo.transform.Find("Text");
            GameObject explodedTxtGo = explodedTxtTr != null ? explodedTxtTr.gameObject : new GameObject("Text", typeof(RectTransform));
            explodedTxtGo.transform.SetParent(explodedBtnGo.transform, false);
            var txtExploded = explodedTxtGo.GetComponent<Text>() ?? explodedTxtGo.AddComponent<Text>();
            txtExploded.text = "<b>🔍 PARTS & ANATOMY</b>\n<size=17><color=#80e5ff>Exploded View & Lore (කොටස් සහ විස්තර)</color></size>";
            txtExploded.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtExploded.fontSize = 21;
            txtExploded.alignment = TextAnchor.MiddleCenter;
            txtExploded.color = Color.white;
            txtExploded.rectTransform.anchorMin = Vector2.zero;
            txtExploded.rectTransform.anchorMax = Vector2.one;
            txtExploded.rectTransform.sizeDelta = new Vector2(-30f, -20f);

            // 4C. Exploded Parts Selection Pills (Above the sub-bar)
            var pillsTr = uiRoot.transform.Find("Pills_ExplodedParts");
            GameObject pillsGo = pillsTr != null ? pillsTr.gameObject : new GameObject("Pills_ExplodedParts", typeof(RectTransform));
            pillsGo.transform.SetParent(uiRoot.transform, false);
            var rtPills = pillsGo.GetComponent<RectTransform>() ?? pillsGo.AddComponent<RectTransform>();
            rtPills.anchorMin = new Vector2(0.5f, 0f);
            rtPills.anchorMax = new Vector2(0.5f, 0f);
            rtPills.pivot = new Vector2(0.5f, 0f);
            rtPills.anchoredPosition = new Vector2(0f, 210f);
            rtPills.sizeDelta = new Vector2(1200f, 65f);

            // 4 Pills
            Button CreatePill(string name, string label, float xPos, Color color)
            {
                var pTr = pillsGo.transform.Find(name);
                GameObject pGo = pTr != null ? pTr.gameObject : new GameObject(name, typeof(RectTransform));
                pGo.transform.SetParent(pillsGo.transform, false);
                var pRt = pGo.GetComponent<RectTransform>() ?? pGo.AddComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 0.5f);
                pRt.anchorMax = new Vector2(0.5f, 0.5f);
                pRt.anchoredPosition = new Vector2(xPos, 0f);
                pRt.sizeDelta = new Vector2(275f, 55f);
                var pImg = pGo.GetComponent<Image>() ?? pGo.AddComponent<Image>();
                pImg.color = color;
                var pBtn = pGo.GetComponent<Button>() ?? pGo.AddComponent<Button>();

                var tTr = pGo.transform.Find("Text");
                GameObject tGo = tTr != null ? tTr.gameObject : new GameObject("Text", typeof(RectTransform));
                tGo.transform.SetParent(pGo.transform, false);
                var pTxt = tGo.GetComponent<Text>() ?? tGo.AddComponent<Text>();
                pTxt.text = label;
                pTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                pTxt.fontSize = 18;
                pTxt.fontStyle = FontStyle.Bold;
                pTxt.alignment = TextAnchor.MiddleCenter;
                pTxt.color = Color.white;
                pTxt.rectTransform.anchorMin = Vector2.zero;
                pTxt.rectTransform.anchorMax = Vector2.one;
                pTxt.rectTransform.sizeDelta = Vector2.zero;
                return pBtn;
            }

            var btnPillFulcrum = CreatePill("Pill_Fulcrum", "🏔️ 1. FULCRUM (ආධාරකය)", -420f, new Color(0.2f, 0.55f, 0.3f, 0.95f));
            var btnPillBeam = CreatePill("Pill_Beam", "🪵 2. LEVER BEAM (දණ්ඩ)", -140f, new Color(0.55f, 0.35f, 0.15f, 0.95f));
            var btnPillLoad = CreatePill("Pill_LoadStone", "🪨 3. LOAD STONE (භාරය)", 140f, new Color(0.65f, 0.25f, 0.2f, 0.95f));
            var btnPillEffort = CreatePill("Pill_Effort", "⚡ 4. EFFORT POINT (ආයාසය)", 420f, new Color(0.7f, 0.55f, 0.1f, 0.95f));

            pillsGo.SetActive(false);

            // 5. PART DETAIL KNOWLEDGE MODAL
            var partModalTr = uiRoot.transform.Find("Panel_PartDetailModal");
            GameObject partModalGo = partModalTr != null ? partModalTr.gameObject : new GameObject("Panel_PartDetailModal", typeof(RectTransform));
            partModalGo.transform.SetParent(uiRoot.transform, false);
            var rtPartModal = partModalGo.GetComponent<RectTransform>() ?? partModalGo.AddComponent<RectTransform>();
            rtPartModal.anchorMin = Vector2.zero;
            rtPartModal.anchorMax = Vector2.one;
            rtPartModal.sizeDelta = Vector2.zero;

            // Backdrop
            var partDimTr = partModalGo.transform.Find("BackdropDim");
            GameObject partDimGo = partDimTr != null ? partDimTr.gameObject : new GameObject("BackdropDim", typeof(RectTransform));
            partDimGo.transform.SetParent(partModalGo.transform, false);
            var rtPartDim = partDimGo.GetComponent<RectTransform>() ?? partDimGo.AddComponent<RectTransform>();
            rtPartDim.anchorMin = Vector2.zero;
            rtPartDim.anchorMax = Vector2.one;
            rtPartDim.sizeDelta = Vector2.zero;
            var imgPartDim = partDimGo.GetComponent<Image>() ?? partDimGo.AddComponent<Image>();
            imgPartDim.color = new Color(0f, 0f, 0f, 0.75f);
            var btnPartDim = partDimGo.GetComponent<Button>() ?? partDimGo.AddComponent<Button>();

            // Modal Card
            var partCardTr = partModalGo.transform.Find("ModalCard");
            GameObject partCardGo = partCardTr != null ? partCardTr.gameObject : new GameObject("ModalCard", typeof(RectTransform));
            partCardGo.transform.SetParent(partModalGo.transform, false);
            var rtPartCard = partCardGo.GetComponent<RectTransform>() ?? partCardGo.AddComponent<RectTransform>();
            rtPartCard.anchorMin = new Vector2(0.5f, 0.5f);
            rtPartCard.anchorMax = new Vector2(0.5f, 0.5f);
            rtPartCard.sizeDelta = new Vector2(980f, 620f);
            var imgPartCard = partCardGo.GetComponent<Image>() ?? partCardGo.AddComponent<Image>();
            imgPartCard.color = new Color(0.24f, 0.16f, 0.10f, 0.98f);

            // Title
            var pTitleTr = partCardGo.transform.Find("TitleText");
            GameObject pTitleGo = pTitleTr != null ? pTitleTr.gameObject : new GameObject("TitleText", typeof(RectTransform));
            pTitleGo.transform.SetParent(partCardGo.transform, false);
            var rtPTitle = pTitleGo.GetComponent<RectTransform>() ?? pTitleGo.AddComponent<RectTransform>();
            rtPTitle.anchorMin = new Vector2(0.05f, 0.85f);
            rtPTitle.anchorMax = new Vector2(0.95f, 0.96f);
            rtPTitle.offsetMin = Vector2.zero;
            rtPTitle.offsetMax = Vector2.zero;
            var txtPTitle = pTitleGo.GetComponent<Text>() ?? pTitleGo.AddComponent<Text>();
            txtPTitle.text = "🏔️ FULCRUM (ආධාරක ලක්ෂ්‍යය)";
            txtPTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtPTitle.fontSize = 32;
            txtPTitle.fontStyle = FontStyle.Bold;
            txtPTitle.color = new Color(1f, 0.88f, 0.45f);
            txtPTitle.alignment = TextAnchor.MiddleLeft;

            // Category
            var pCatTr = partCardGo.transform.Find("CategoryText");
            GameObject pCatGo = pCatTr != null ? pCatTr.gameObject : new GameObject("CategoryText", typeof(RectTransform));
            pCatGo.transform.SetParent(partCardGo.transform, false);
            var rtPCat = pCatGo.GetComponent<RectTransform>() ?? pCatGo.AddComponent<RectTransform>();
            rtPCat.anchorMin = new Vector2(0.05f, 0.77f);
            rtPCat.anchorMax = new Vector2(0.95f, 0.85f);
            rtPCat.offsetMin = Vector2.zero;
            rtPCat.offsetMax = Vector2.zero;
            var txtPCat = pCatGo.GetComponent<Text>() ?? pCatGo.AddComponent<Text>();
            txtPCat.text = "Class 1 Lever Central Pivot Point";
            txtPCat.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtPCat.fontSize = 20;
            txtPCat.fontStyle = FontStyle.Italic;
            txtPCat.color = new Color(0.55f, 0.88f, 0.75f);
            txtPCat.alignment = TextAnchor.MiddleLeft;

            // Body
            var pBodyTr = partCardGo.transform.Find("BodyText");
            GameObject pBodyGo = pBodyTr != null ? pBodyTr.gameObject : new GameObject("BodyText", typeof(RectTransform));
            pBodyGo.transform.SetParent(partCardGo.transform, false);
            var rtPBody = pBodyGo.GetComponent<RectTransform>() ?? pBodyGo.AddComponent<RectTransform>();
            rtPBody.anchorMin = new Vector2(0.05f, 0.50f);
            rtPBody.anchorMax = new Vector2(0.95f, 0.75f);
            rtPBody.offsetMin = Vector2.zero;
            rtPBody.offsetMax = Vector2.zero;
            var txtPBody = pBodyGo.GetComponent<Text>() ?? pBodyGo.AddComponent<Text>();
            txtPBody.text = "The fixed point on which the lever beam balances and turns. It bears the entire weight of both the load and the applied effort forces.";
            txtPBody.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtPBody.fontSize = 21;
            txtPBody.color = Color.white;
            txtPBody.alignment = TextAnchor.UpperLeft;

            // Formula Box
            var pFormTr = partCardGo.transform.Find("FormulaText");
            GameObject pFormGo = pFormTr != null ? pFormTr.gameObject : new GameObject("FormulaText", typeof(RectTransform));
            pFormGo.transform.SetParent(partCardGo.transform, false);
            var rtPForm = pFormGo.GetComponent<RectTransform>() ?? pFormGo.AddComponent<RectTransform>();
            rtPForm.anchorMin = new Vector2(0.05f, 0.28f);
            rtPForm.anchorMax = new Vector2(0.95f, 0.48f);
            rtPForm.offsetMin = Vector2.zero;
            rtPForm.offsetMax = Vector2.zero;
            var txtPForm = pFormGo.GetComponent<Text>() ?? pFormGo.AddComponent<Text>();
            txtPForm.text = "<b>Mechanical Advantage:</b> MA = Effort Arm ÷ Load Arm\n<i>Moving fulcrum closer to load multiplies lifting strength!</i>";
            txtPForm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtPForm.fontSize = 20;
            txtPForm.color = new Color(1f, 0.92f, 0.5f);
            txtPForm.alignment = TextAnchor.UpperLeft;

            // Lore
            var pLoreTr = partCardGo.transform.Find("LoreText");
            GameObject pLoreGo = pLoreTr != null ? pLoreTr.gameObject : new GameObject("LoreText", typeof(RectTransform));
            pLoreGo.transform.SetParent(partCardGo.transform, false);
            var rtPLore = pLoreGo.GetComponent<RectTransform>() ?? pLoreGo.AddComponent<RectTransform>();
            rtPLore.anchorMin = new Vector2(0.05f, 0.05f);
            rtPLore.anchorMax = new Vector2(0.95f, 0.26f);
            rtPLore.offsetMin = Vector2.zero;
            rtPLore.offsetMax = Vector2.zero;
            var txtPLore = pLoreGo.GetComponent<Text>() ?? pLoreGo.AddComponent<Text>();
            txtPLore.text = "<b>Temple Architecture Lore:</b> Ancient stone masons carved triangular granite fulcrum wedges to pry massive megalithic gate lintels into place.";
            txtPLore.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtPLore.fontSize = 19;
            txtPLore.fontStyle = FontStyle.Italic;
            txtPLore.color = new Color(0.9f, 0.85f, 0.75f);
            txtPLore.alignment = TextAnchor.UpperLeft;

            // Close Button
            var pCloseTr = partCardGo.transform.Find("Btn_Close");
            GameObject pCloseGo = pCloseTr != null ? pCloseTr.gameObject : new GameObject("Btn_Close", typeof(RectTransform));
            pCloseGo.transform.SetParent(partCardGo.transform, false);
            var rtPClose = pCloseGo.GetComponent<RectTransform>() ?? pCloseGo.AddComponent<RectTransform>();
            rtPClose.anchorMin = new Vector2(1f, 1f);
            rtPClose.anchorMax = new Vector2(1f, 1f);
            rtPClose.anchoredPosition = new Vector2(-30f, -30f);
            rtPClose.sizeDelta = new Vector2(55f, 55f);
            var imgPClose = pCloseGo.GetComponent<Image>() ?? pCloseGo.AddComponent<Image>();
            imgPClose.color = new Color(0.85f, 0.25f, 0.25f, 1f);
            var btnPClose = pCloseGo.GetComponent<Button>() ?? pCloseGo.AddComponent<Button>();

            var pCloseTxtTr = pCloseGo.transform.Find("Text");
            GameObject pCloseTxtGo = pCloseTxtTr != null ? pCloseTxtTr.gameObject : new GameObject("Text", typeof(RectTransform));
            pCloseTxtGo.transform.SetParent(pCloseGo.transform, false);
            var txtPClose = pCloseTxtGo.GetComponent<Text>() ?? pCloseTxtGo.AddComponent<Text>();
            txtPClose.text = "<b>✕</b>";
            txtPClose.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtPClose.fontSize = 26;
            txtPClose.alignment = TextAnchor.MiddleCenter;
            txtPClose.color = Color.white;
            txtPClose.rectTransform.anchorMin = Vector2.zero;
            txtPClose.rectTransform.anchorMax = Vector2.one;
            txtPClose.rectTransform.sizeDelta = Vector2.zero;

            partModalGo.SetActive(false);

            // 6. Floating Live Lift Status Feedback Banner (Above Console - 2X Enlarger)
            var liftBannerTr = uiRoot.transform.Find("Banner_LiftStatus");
            GameObject liftBannerGo = liftBannerTr != null ? liftBannerTr.gameObject : new GameObject("Banner_LiftStatus", typeof(RectTransform));
            liftBannerGo.transform.SetParent(uiRoot.transform, false);
            var rtLiftBanner = liftBannerGo.GetComponent<RectTransform>() ?? liftBannerGo.AddComponent<RectTransform>();
            rtLiftBanner.anchorMin = new Vector2(0.5f, 0f);
            rtLiftBanner.anchorMax = new Vector2(0.5f, 0f);
            rtLiftBanner.pivot = new Vector2(0.5f, 0f);
            rtLiftBanner.anchoredPosition = new Vector2(0f, 280f);
            rtLiftBanner.sizeDelta = new Vector2(850f, 85f);
            var imgLiftBanner = liftBannerGo.GetComponent<Image>() ?? liftBannerGo.AddComponent<Image>();
            imgLiftBanner.color = new Color(0.18f, 0.14f, 0.10f, 0.90f);
            var liftTxtTr = liftBannerGo.transform.Find("Text");
            GameObject liftTxtGo = liftTxtTr != null ? liftTxtTr.gameObject : new GameObject("Text", typeof(RectTransform));
            liftTxtGo.transform.SetParent(liftBannerGo.transform, false);
            var txtLift = liftTxtGo.GetComponent<Text>() ?? liftTxtGo.AddComponent<Text>();
            txtLift.text = "<color=#ffdd44><b>🔄 360° FORCES & ROTATION ACTIVE</b></color>\n<size=17>Glowing arrows indicate input effort, gravity load, and fulcrum reaction force!</size>";
            txtLift.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtLift.fontSize = 21;
            txtLift.alignment = TextAnchor.MiddleCenter;
            txtLift.color = Color.white;
            txtLift.rectTransform.anchorMin = Vector2.zero;
            txtLift.rectTransform.anchorMax = Vector2.one;
            txtLift.rectTransform.sizeDelta = Vector2.zero;

            // 7. Character Companion (Left Side Appearance)
            var charTr = uiRoot.transform.Find("Companion_Character");
            GameObject charGo = charTr != null ? charTr.gameObject : new GameObject("Companion_Character", typeof(RectTransform));
            charGo.transform.SetParent(uiRoot.transform, false);
            var rtChar = charGo.GetComponent<RectTransform>() ?? charGo.AddComponent<RectTransform>();
            rtChar.anchorMin = new Vector2(0f, 0f);
            rtChar.anchorMax = new Vector2(0f, 0f);
            rtChar.pivot = new Vector2(0f, 0f);
            rtChar.anchoredPosition = new Vector2(25f, 0f);
            rtChar.sizeDelta = new Vector2(320f, 490f);
            var imgChar = charGo.GetComponent<Image>() ?? charGo.AddComponent<Image>();
            var mayaFull = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/char_maya_full.png");
            var senukFull = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/char_senuk_full.png");
            string selectedChar = PlayerPrefs.GetString("SelectedCharacter", "Maya");
            imgChar.sprite = (selectedChar == "Senuk" && senukFull != null) ? senukFull : mayaFull;
            imgChar.color = Color.white;
            imgChar.preserveAspect = true;

            // 8. Attach and Wire LeverARScreenController
            var controller = canvas.GetComponent<LeverARScreenController>();
            if (controller == null) controller = canvas.AddComponent<LeverARScreenController>();
            var audioSrc = canvas.GetComponent<AudioSource>();
            if (audioSrc == null) audioSrc = canvas.AddComponent<AudioSource>();
            if (audioSrc != null) audioSrc.playOnAwake = false;

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            typeof(LeverARScreenController).GetField("backOrSettingsButton", flags).SetValue(controller, btnBack);
            typeof(LeverARScreenController).GetField("modeToggleButton", flags).SetValue(controller, btnMode);
            typeof(LeverARScreenController).GetField("modeToggleText", flags).SetValue(controller, txtMode);
            typeof(LeverARScreenController).GetField("modeToggleBg", flags).SetValue(controller, imgMode);
            typeof(LeverARScreenController).GetField("viewOnlyModeSprite", flags).SetValue(controller, AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_view_only_mode.png"));
            typeof(LeverARScreenController).GetField("interactiveModeSprite", flags).SetValue(controller, AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_interactive_mode.png"));
            typeof(LeverARScreenController).GetField("howItWorksPanel", flags).SetValue(controller, howPanelGo);

            // View-Only SubBar references
            typeof(LeverARScreenController).GetField("viewOnlyBottomBar", flags).SetValue(controller, viewOnlyBarGo);
            typeof(LeverARScreenController).GetField("btnForcesAndMotion", flags).SetValue(controller, btnForces);
            typeof(LeverARScreenController).GetField("btnExplodedParts", flags).SetValue(controller, btnExploded);
            typeof(LeverARScreenController).GetField("imgForcesHighlight", flags).SetValue(controller, imgForces);
            typeof(LeverARScreenController).GetField("imgExplodedHighlight", flags).SetValue(controller, imgExploded);
            typeof(LeverARScreenController).GetField("explodedPartsPillsContainer", flags).SetValue(controller, pillsGo);
            typeof(LeverARScreenController).GetField("pillFulcrumBtn", flags).SetValue(controller, btnPillFulcrum);
            typeof(LeverARScreenController).GetField("pillBeamBtn", flags).SetValue(controller, btnPillBeam);
            typeof(LeverARScreenController).GetField("pillLoadStoneBtn", flags).SetValue(controller, btnPillLoad);
            typeof(LeverARScreenController).GetField("pillEffortBtn", flags).SetValue(controller, btnPillEffort);

            // Part Detail Modal references
            typeof(LeverARScreenController).GetField("partDetailModalPanel", flags).SetValue(controller, partModalGo);
            typeof(LeverARScreenController).GetField("partDetailTitleText", flags).SetValue(controller, txtPTitle);
            typeof(LeverARScreenController).GetField("partDetailCategoryText", flags).SetValue(controller, txtPCat);
            typeof(LeverARScreenController).GetField("partDetailBodyText", flags).SetValue(controller, txtPBody);
            typeof(LeverARScreenController).GetField("partDetailFormulaText", flags).SetValue(controller, txtPForm);
            typeof(LeverARScreenController).GetField("partDetailLoreText", flags).SetValue(controller, txtPLore);
            typeof(LeverARScreenController).GetField("partDetailCloseBtn", flags).SetValue(controller, btnPClose);
            typeof(LeverARScreenController).GetField("partDetailBgDismissBtn", flags).SetValue(controller, btnPartDim);

            // Interactive Bottom Console references
            typeof(LeverARScreenController).GetField("bottomConsole", flags).SetValue(controller, botBarGo);
            typeof(LeverARScreenController).GetField("rotateButton", flags).SetValue(controller, null);
            typeof(LeverARScreenController).GetField("forceDecreaseBtn", flags).SetValue(controller, btnDec);
            typeof(LeverARScreenController).GetField("forceIncreaseBtn", flags).SetValue(controller, btnInc);
            typeof(LeverARScreenController).GetField("forceValueText", flags).SetValue(controller, txtForceVal);
            typeof(LeverARScreenController).GetField("fulcrumLeftBtn", flags).SetValue(controller, btnFulcLeft);
            typeof(LeverARScreenController).GetField("fulcrumRightBtn", flags).SetValue(controller, btnFulcRight);
            typeof(LeverARScreenController).GetField("fulcrumStatusText", flags).SetValue(controller, null);
            typeof(LeverARScreenController).GetField("resetButton", flags).SetValue(controller, btnReset);
            typeof(LeverARScreenController).GetField("tryChallengeButton", flags).SetValue(controller, btnTry);
            typeof(LeverARScreenController).GetField("liftStatusBannerText", flags).SetValue(controller, txtLift);

            // Character Companion & Live Voice wiring
            typeof(LeverARScreenController).GetField("characterCompanionImage", flags).SetValue(controller, imgChar);
            typeof(LeverARScreenController).GetField("mayaFullSprite", flags).SetValue(controller, mayaFull);
            typeof(LeverARScreenController).GetField("senukFullSprite", flags).SetValue(controller, senukFull);
            typeof(LeverARScreenController).GetField("audioSource", flags).SetValue(controller, audioSrc);
            typeof(LeverARScreenController).GetField("mayaBoulderLiftedClip", flags).SetValue(controller, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Character Voices/maya_boulder_lifted.wav"));
            typeof(LeverARScreenController).GetField("mayaTooHeavyClip", flags).SetValue(controller, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Character Voices/maya_too_heavy.wav"));
            typeof(LeverARScreenController).GetField("mayaBalancedClip", flags).SetValue(controller, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Character Voices/maya_balanced.wav"));
            typeof(LeverARScreenController).GetField("senukBoulderLiftedClip", flags).SetValue(controller, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Character Voices/senuk_boulder_lifted.wav"));
            typeof(LeverARScreenController).GetField("senukTooHeavyClip", flags).SetValue(controller, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Character Voices/senuk_too_heavy.wav"));
            typeof(LeverARScreenController).GetField("senukBalancedClip", flags).SetValue(controller, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Character Voices/senuk_balanced.wav"));

            // 9. Wire 3D Object transforms & Dynamic Force Lines to controller
            var leverObj = GameObject.Find("SimpleMachine_Lever");
            if (leverObj != null)
            {
                typeof(LeverARScreenController).GetField("leverRoot", flags).SetValue(controller, leverObj.transform);
                var beam = leverObj.transform.Find("Lever_Beam");
                if (beam != null) typeof(LeverARScreenController).GetField("beamPivot", flags).SetValue(controller, beam);
                var fulcrum = leverObj.transform.Find("Fulcrum_Pivot");
                if (fulcrum != null) typeof(LeverARScreenController).GetField("fulcrumRoot", flags).SetValue(controller, fulcrum);
                var boulder = leverObj.transform.Find("Lever_Beam/Load_Stone_DoorWeight");
                if (boulder != null) typeof(LeverARScreenController).GetField("boulderRoot", flags).SetValue(controller, boulder);
                var effort = leverObj.transform.Find("Lever_Beam/Effort_Point_Handle");
                if (effort != null) typeof(LeverARScreenController).GetField("effortPoint", flags).SetValue(controller, effort);

                // Setup In-World LineRenderers for Force Vector Arrows
                var vectorsRootTr = leverObj.transform.Find("ForceVectorsRoot");
                GameObject vectorsRootGo = vectorsRootTr != null ? vectorsRootTr.gameObject : new GameObject("ForceVectorsRoot");
                vectorsRootGo.transform.SetParent(leverObj.transform, false);
                typeof(LeverARScreenController).GetField("forceVectorsRoot", flags).SetValue(controller, vectorsRootGo);

                // Effort LineRenderer
                var effortLineTr = vectorsRootGo.transform.Find("Line_EffortForce");
                GameObject effortLineGo = effortLineTr != null ? effortLineTr.gameObject : new GameObject("Line_EffortForce");
                effortLineGo.transform.SetParent(vectorsRootGo.transform, false);
                var lrEffort = effortLineGo.GetComponent<LineRenderer>();
                if (lrEffort == null) lrEffort = effortLineGo.AddComponent<LineRenderer>();
                lrEffort.material = new Material(Shader.Find("Sprites/Default"));
                typeof(LeverARScreenController).GetField("effortArrowLine", flags).SetValue(controller, lrEffort);

                // Load LineRenderer
                var loadLineTr = vectorsRootGo.transform.Find("Line_LoadWeight");
                GameObject loadLineGo = loadLineTr != null ? loadLineTr.gameObject : new GameObject("Line_LoadWeight");
                loadLineGo.transform.SetParent(vectorsRootGo.transform, false);
                var lrLoad = loadLineGo.GetComponent<LineRenderer>();
                if (lrLoad == null) lrLoad = loadLineGo.AddComponent<LineRenderer>();
                lrLoad.material = new Material(Shader.Find("Sprites/Default"));
                typeof(LeverARScreenController).GetField("loadArrowLine", flags).SetValue(controller, lrLoad);

                // Fulcrum Reaction LineRenderer
                var fulcLineTr = vectorsRootGo.transform.Find("Line_FulcrumReaction");
                GameObject fulcLineGo = fulcLineTr != null ? fulcLineTr.gameObject : new GameObject("Line_FulcrumReaction");
                fulcLineGo.transform.SetParent(vectorsRootGo.transform, false);
                var lrFulc = fulcLineGo.GetComponent<LineRenderer>();
                if (lrFulc == null) lrFulc = fulcLineGo.AddComponent<LineRenderer>();
                lrFulc.material = new Material(Shader.Find("Sprites/Default"));
                typeof(LeverARScreenController).GetField("fulcrumReactionArrowLine", flags).SetValue(controller, lrFulc);
            }

            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

            Debug.Log("[LeverARUISetup] Lever AR Custom UI successfully updated with View-Only Mode default, 2 SubBar buttons, Exploded Parts, and Detail Modal!");
        }

        [MenuItem("LostTemple/Build Home Map Quest & Story Book UI")]
        public static void BuildHomeMapQuestUI()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_App/Scenes/HomeMapScene.unity");
            UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(scene);

            var canvasGo = GameObject.Find("Map Canvas");
            if (canvasGo == null)
            {
                Debug.LogError("Map Canvas not found in HomeMapScene!");
                return;
            }

            var testObj = canvasGo.transform.Find("TestSavedObj");
            if (testObj != null) Object.DestroyImmediate(testObj.gameObject);

            var ctrlGo = GameObject.Find("Map Canvas/HomeMapController");
            var homeCtrl = ctrlGo.GetComponent<HomeMapController>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Modal_QuestAdventureSelection
            var questModalTr = canvasGo.transform.Find("Modal_QuestAdventureSelection");
            if (questModalTr != null) Object.DestroyImmediate(questModalTr.gameObject);

            GameObject questModalGo = new GameObject("Modal_QuestAdventureSelection", typeof(RectTransform));
            questModalGo.transform.SetParent(canvasGo.transform, false);
            var rtQModal = questModalGo.GetComponent<RectTransform>();
            rtQModal.anchorMin = Vector2.zero;
            rtQModal.anchorMax = Vector2.one;
            rtQModal.sizeDelta = Vector2.zero;

            GameObject bgDimGo = new GameObject("ModalBackgroundDim", typeof(RectTransform));
            bgDimGo.transform.SetParent(questModalGo.transform, false);
            var rtBgDim = bgDimGo.GetComponent<RectTransform>();
            rtBgDim.anchorMin = Vector2.zero;
            rtBgDim.anchorMax = Vector2.one;
            rtBgDim.sizeDelta = Vector2.zero;
            var imgBgDim = bgDimGo.AddComponent<Image>();
            imgBgDim.color = new Color(0f, 0f, 0f, 0.75f);
            var btnBgDim = bgDimGo.AddComponent<Button>();

            GameObject cardContGo = new GameObject("CardContainer", typeof(RectTransform));
            cardContGo.transform.SetParent(questModalGo.transform, false);
            var rtCardCont = cardContGo.GetComponent<RectTransform>();
            rtCardCont.anchorMin = new Vector2(0.5f, 0.5f);
            rtCardCont.anchorMax = new Vector2(0.5f, 0.5f);
            rtCardCont.sizeDelta = new Vector2(1100f, 620f);
            var imgCardCont = cardContGo.AddComponent<Image>();
            imgCardCont.color = new Color(0.24f, 0.16f, 0.10f, 0.98f);

            GameObject titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(cardContGo.transform, false);
            var rtTitle = titleGo.GetComponent<RectTransform>();
            rtTitle.anchorMin = new Vector2(0.5f, 1f);
            rtTitle.anchorMax = new Vector2(0.5f, 1f);
            rtTitle.pivot = new Vector2(0.5f, 1f);
            rtTitle.anchoredPosition = new Vector2(0f, -25f);
            rtTitle.sizeDelta = new Vector2(900f, 60f);
            var txtTitle = titleGo.AddComponent<Text>();
            txtTitle.text = "<b>ADVENTURE QUEST: SELECT CHAPTER</b>";
            txtTitle.font = defaultFont;
            txtTitle.fontSize = 32;
            txtTitle.alignment = TextAnchor.MiddleCenter;
            txtTitle.color = new Color(1f, 0.88f, 0.45f);

            GameObject subGo = new GameObject("Subtitle", typeof(RectTransform));
            subGo.transform.SetParent(cardContGo.transform, false);
            var rtSub = subGo.GetComponent<RectTransform>();
            rtSub.anchorMin = new Vector2(0.5f, 1f);
            rtSub.anchorMax = new Vector2(0.5f, 1f);
            rtSub.pivot = new Vector2(0.5f, 1f);
            rtSub.anchoredPosition = new Vector2(0f, -80f);
            rtSub.sizeDelta = new Vector2(900f, 35f);
            var txtSub = subGo.AddComponent<Text>();
            txtSub.text = "Read the illustrated story, scan the temple marker and solve the interactive challenges!";
            txtSub.font = defaultFont;
            txtSub.fontSize = 18;
            txtSub.alignment = TextAnchor.MiddleCenter;
            txtSub.color = new Color(0.9f, 0.85f, 0.75f);

            // Card 1: Lever
            GameObject cardLeverGo = new GameObject("Card_Lever", typeof(RectTransform));
            cardLeverGo.transform.SetParent(cardContGo.transform, false);
            var rtCL = cardLeverGo.GetComponent<RectTransform>();
            rtCL.anchorMin = new Vector2(0.5f, 0.5f);
            rtCL.anchorMax = new Vector2(0.5f, 0.5f);
            rtCL.anchoredPosition = new Vector2(-330f, -25f);
            rtCL.sizeDelta = new Vector2(300f, 380f);
            var imgCL = cardLeverGo.AddComponent<Image>();
            imgCL.color = new Color(0.35f, 0.25f, 0.15f, 1f);
            var btnCL = cardLeverGo.AddComponent<Button>();

            GameObject clTxtGo = new GameObject("Text", typeof(RectTransform));
            clTxtGo.transform.SetParent(cardLeverGo.transform, false);
            var txtCL = clTxtGo.AddComponent<Text>();
            txtCL.text = "<b>CHAPTER 1</b>\n<color=#ffdd66><b>THE LEVER</b></color>\n\n<size=15><color=#a8d5ba>UNLOCKED</color>\nPages 1 - 9\nAR Marker Inside</size>";
            txtCL.font = defaultFont;
            txtCL.fontSize = 20;
            txtCL.alignment = TextAnchor.MiddleCenter;
            txtCL.color = Color.white;
            txtCL.rectTransform.anchorMin = Vector2.zero;
            txtCL.rectTransform.anchorMax = Vector2.one;
            txtCL.rectTransform.sizeDelta = Vector2.zero;

            // Card 2: Pulley
            GameObject cardPulleyGo = new GameObject("Card_Pulley", typeof(RectTransform));
            cardPulleyGo.transform.SetParent(cardContGo.transform, false);
            var rtCP = cardPulleyGo.GetComponent<RectTransform>();
            rtCP.anchorMin = new Vector2(0.5f, 0.5f);
            rtCP.anchorMax = new Vector2(0.5f, 0.5f);
            rtCP.anchoredPosition = new Vector2(0f, -25f);
            rtCP.sizeDelta = new Vector2(300f, 380f);
            var imgCP = cardPulleyGo.AddComponent<Image>();
            imgCP.color = new Color(0.30f, 0.22f, 0.15f, 1f);
            var btnCP = cardPulleyGo.AddComponent<Button>();

            GameObject cpTxtGo = new GameObject("Text", typeof(RectTransform));
            cpTxtGo.transform.SetParent(cardPulleyGo.transform, false);
            var txtCP = cpTxtGo.AddComponent<Text>();
            txtCP.text = "<b>CHAPTER 2</b>\n<color=#ffdd66><b>THE PULLEY</b></color>\n\n<size=15>Pages 10 - 13\nAR Marker Inside</size>";
            txtCP.font = defaultFont;
            txtCP.fontSize = 20;
            txtCP.alignment = TextAnchor.MiddleCenter;
            txtCP.color = Color.white;
            txtCP.rectTransform.anchorMin = Vector2.zero;
            txtCP.rectTransform.anchorMax = Vector2.one;
            txtCP.rectTransform.sizeDelta = Vector2.zero;

            GameObject lockPGo = new GameObject("LockOverlay", typeof(RectTransform));
            lockPGo.transform.SetParent(cardPulleyGo.transform, false);
            var rtLockP = lockPGo.GetComponent<RectTransform>();
            rtLockP.anchorMin = Vector2.zero;
            rtLockP.anchorMax = Vector2.one;
            rtLockP.sizeDelta = Vector2.zero;
            var imgLockP = lockPGo.AddComponent<Image>();
            imgLockP.color = new Color(0.1f, 0.08f, 0.06f, 0.88f);
            GameObject lockTxtPGo = new GameObject("Text", typeof(RectTransform));
            lockTxtPGo.transform.SetParent(lockPGo.transform, false);
            var txtLockP = lockTxtPGo.AddComponent<Text>();
            txtLockP.text = "<b>LOCKED</b>\n<size=14><color=#f5d77f>Complete Chapter 1\nLever Activity to Unlock!</color></size>";
            txtLockP.font = defaultFont;
            txtLockP.fontSize = 18;
            txtLockP.alignment = TextAnchor.MiddleCenter;
            txtLockP.color = Color.white;
            txtLockP.rectTransform.anchorMin = Vector2.zero;
            txtLockP.rectTransform.anchorMax = Vector2.one;
            txtLockP.rectTransform.sizeDelta = Vector2.zero;

            // Card 3: Incline
            GameObject cardIncGo = new GameObject("Card_Incline", typeof(RectTransform));
            cardIncGo.transform.SetParent(cardContGo.transform, false);
            var rtCI = cardIncGo.GetComponent<RectTransform>();
            rtCI.anchorMin = new Vector2(0.5f, 0.5f);
            rtCI.anchorMax = new Vector2(0.5f, 0.5f);
            rtCI.anchoredPosition = new Vector2(330f, -25f);
            rtCI.sizeDelta = new Vector2(300f, 380f);
            var imgCI = cardIncGo.AddComponent<Image>();
            imgCI.color = new Color(0.30f, 0.22f, 0.15f, 1f);
            var btnCI = cardIncGo.AddComponent<Button>();

            GameObject ciTxtGo = new GameObject("Text", typeof(RectTransform));
            ciTxtGo.transform.SetParent(cardIncGo.transform, false);
            var txtCI = ciTxtGo.AddComponent<Text>();
            txtCI.text = "<b>CHAPTER 3</b>\n<color=#ffdd66><b>INCLINED PLANE</b></color>\n\n<size=15>Pages 14 - 18\nAR Marker Inside</size>";
            txtCI.font = defaultFont;
            txtCI.fontSize = 20;
            txtCI.alignment = TextAnchor.MiddleCenter;
            txtCI.color = Color.white;
            txtCI.rectTransform.anchorMin = Vector2.zero;
            txtCI.rectTransform.anchorMax = Vector2.one;
            txtCI.rectTransform.sizeDelta = Vector2.zero;

            GameObject lockIGo = new GameObject("LockOverlay", typeof(RectTransform));
            lockIGo.transform.SetParent(cardIncGo.transform, false);
            var rtLockI = lockIGo.GetComponent<RectTransform>();
            rtLockI.anchorMin = Vector2.zero;
            rtLockI.anchorMax = Vector2.one;
            rtLockI.sizeDelta = Vector2.zero;
            var imgLockI = lockIGo.AddComponent<Image>();
            imgLockI.color = new Color(0.1f, 0.08f, 0.06f, 0.88f);
            GameObject lockTxtIGo = new GameObject("Text", typeof(RectTransform));
            lockTxtIGo.transform.SetParent(lockIGo.transform, false);
            var txtLockI = lockTxtIGo.AddComponent<Text>();
            txtLockI.text = "<b>LOCKED</b>\n<size=14><color=#f5d77f>Complete Chapter 2\nPulley Activity to Unlock!</color></size>";
            txtLockI.font = defaultFont;
            txtLockI.fontSize = 18;
            txtLockI.alignment = TextAnchor.MiddleCenter;
            txtLockI.color = Color.white;
            txtLockI.rectTransform.anchorMin = Vector2.zero;
            txtLockI.rectTransform.anchorMax = Vector2.one;
            txtLockI.rectTransform.sizeDelta = Vector2.zero;

            // Close Button
            GameObject closeQBtnGo = new GameObject("Btn_Close", typeof(RectTransform));
            closeQBtnGo.transform.SetParent(cardContGo.transform, false);
            var rtCloseQ = closeQBtnGo.GetComponent<RectTransform>();
            rtCloseQ.anchorMin = new Vector2(1f, 1f);
            rtCloseQ.anchorMax = new Vector2(1f, 1f);
            rtCloseQ.anchoredPosition = new Vector2(-25f, -25f);
            rtCloseQ.sizeDelta = new Vector2(50f, 50f);
            var imgCloseQ = closeQBtnGo.AddComponent<Image>();
            imgCloseQ.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            var btnCloseQ = closeQBtnGo.AddComponent<Button>();
            GameObject closeQTxtGo = new GameObject("Text", typeof(RectTransform));
            closeQTxtGo.transform.SetParent(closeQBtnGo.transform, false);
            var txtCloseQ = closeQTxtGo.AddComponent<Text>();
            txtCloseQ.text = "<b>X</b>";
            txtCloseQ.font = defaultFont;
            txtCloseQ.fontSize = 24;
            txtCloseQ.alignment = TextAnchor.MiddleCenter;
            txtCloseQ.color = Color.white;
            txtCloseQ.rectTransform.anchorMin = Vector2.zero;
            txtCloseQ.rectTransform.anchorMax = Vector2.one;
            txtCloseQ.rectTransform.sizeDelta = Vector2.zero;

            questModalGo.SetActive(false);

            // 2. StoryBookViewerModal
            var storyRootTr = canvasGo.transform.Find("StoryBookViewerModal");
            if (storyRootTr != null) Object.DestroyImmediate(storyRootTr.gameObject);

            GameObject storyRootGo = new GameObject("StoryBookViewerModal", typeof(RectTransform));
            storyRootGo.transform.SetParent(canvasGo.transform, false);
            var rtStoryRoot = storyRootGo.GetComponent<RectTransform>();
            rtStoryRoot.anchorMin = Vector2.zero;
            rtStoryRoot.anchorMax = Vector2.one;
            rtStoryRoot.sizeDelta = Vector2.zero;

            var imgStoryBg = storyRootGo.AddComponent<Image>();
            imgStoryBg.color = new Color(0.10f, 0.08f, 0.06f, 0.98f);

            // Top Header Bar
            GameObject topBarStoryGo = new GameObject("TopBar", typeof(RectTransform));
            topBarStoryGo.transform.SetParent(storyRootGo.transform, false);
            var rtTopStory = topBarStoryGo.GetComponent<RectTransform>();
            rtTopStory.anchorMin = new Vector2(0f, 1f);
            rtTopStory.anchorMax = new Vector2(1f, 1f);
            rtTopStory.pivot = new Vector2(0.5f, 1f);
            rtTopStory.anchoredPosition = Vector2.zero;
            rtTopStory.sizeDelta = new Vector2(0f, 85f);
            var imgTopStory = topBarStoryGo.AddComponent<Image>();
            imgTopStory.color = new Color(0.24f, 0.16f, 0.10f, 0.95f);

            // Back to Map Button
            GameObject backMapBtnGo = new GameObject("Btn_BackMap", typeof(RectTransform));
            backMapBtnGo.transform.SetParent(topBarStoryGo.transform, false);
            var rtBackMap = backMapBtnGo.GetComponent<RectTransform>();
            rtBackMap.anchorMin = new Vector2(0f, 0.5f);
            rtBackMap.anchorMax = new Vector2(0f, 0.5f);
            rtBackMap.anchoredPosition = new Vector2(100f, 0f);
            rtBackMap.sizeDelta = new Vector2(160f, 55f);
            var imgBackMap = backMapBtnGo.AddComponent<Image>();
            imgBackMap.color = new Color(0.40f, 0.28f, 0.18f, 1f);
            var btnBackMap = backMapBtnGo.AddComponent<Button>();

            GameObject backMapTxtGo = new GameObject("Text", typeof(RectTransform));
            backMapTxtGo.transform.SetParent(backMapBtnGo.transform, false);
            var txtBackMap = backMapTxtGo.AddComponent<Text>();
            txtBackMap.text = "<b>MAP</b>";
            txtBackMap.font = defaultFont;
            txtBackMap.fontSize = 20;
            txtBackMap.alignment = TextAnchor.MiddleCenter;
            txtBackMap.color = Color.white;
            txtBackMap.rectTransform.anchorMin = Vector2.zero;
            txtBackMap.rectTransform.anchorMax = Vector2.one;
            txtBackMap.rectTransform.sizeDelta = Vector2.zero;

            // Chapter Title
            GameObject chapTitleGo = new GameObject("ChapterTitle", typeof(RectTransform));
            chapTitleGo.transform.SetParent(topBarStoryGo.transform, false);
            var rtChapTitle = chapTitleGo.GetComponent<RectTransform>();
            rtChapTitle.anchorMin = new Vector2(0.5f, 0.5f);
            rtChapTitle.anchorMax = new Vector2(0.5f, 0.5f);
            rtChapTitle.anchoredPosition = Vector2.zero;
            rtChapTitle.sizeDelta = new Vector2(700f, 60f);
            var txtChapTitle = chapTitleGo.AddComponent<Text>();
            txtChapTitle.text = "<b>CHAPTER 1: THE ANCIENT LEVER</b>";
            txtChapTitle.font = defaultFont;
            txtChapTitle.fontSize = 26;
            txtChapTitle.alignment = TextAnchor.MiddleCenter;
            txtChapTitle.color = new Color(1f, 0.88f, 0.45f);

            // Page Display Area
            GameObject pageContGo = new GameObject("PageDisplayArea", typeof(RectTransform));
            pageContGo.transform.SetParent(storyRootGo.transform, false);
            var rtPageCont = pageContGo.GetComponent<RectTransform>();
            rtPageCont.anchorMin = new Vector2(0.5f, 0.5f);
            rtPageCont.anchorMax = new Vector2(0.5f, 0.5f);
            rtPageCont.anchoredPosition = new Vector2(0f, 15f);
            rtPageCont.sizeDelta = new Vector2(1200f, 845f);

            GameObject pageImgGo = new GameObject("PageImage", typeof(RectTransform));
            pageImgGo.transform.SetParent(pageContGo.transform, false);
            var rtPageImg = pageImgGo.GetComponent<RectTransform>();
            rtPageImg.anchorMin = Vector2.zero;
            rtPageImg.anchorMax = Vector2.one;
            rtPageImg.sizeDelta = Vector2.zero;
            var imgPage = pageImgGo.AddComponent<Image>();
            imgPage.color = Color.white;
            imgPage.preserveAspect = true;

            // Bottom Navigation Bar
            GameObject botNavGo = new GameObject("BottomNavBar", typeof(RectTransform));
            botNavGo.transform.SetParent(storyRootGo.transform, false);
            var rtBotNav = botNavGo.GetComponent<RectTransform>();
            rtBotNav.anchorMin = new Vector2(0.5f, 0f);
            rtBotNav.anchorMax = new Vector2(0.5f, 0f);
            rtBotNav.pivot = new Vector2(0.5f, 0f);
            rtBotNav.anchoredPosition = new Vector2(0f, 20f);
            rtBotNav.sizeDelta = new Vector2(900f, 85f);

            // Previous Button
            GameObject prevBtnGo = new GameObject("Btn_PrevPage", typeof(RectTransform));
            prevBtnGo.transform.SetParent(botNavGo.transform, false);
            var rtPrev = prevBtnGo.GetComponent<RectTransform>();
            rtPrev.anchorMin = new Vector2(0f, 0.5f);
            rtPrev.anchorMax = new Vector2(0f, 0.5f);
            rtPrev.anchoredPosition = new Vector2(100f, 0f);
            rtPrev.sizeDelta = new Vector2(180f, 65f);
            var imgPrev = prevBtnGo.AddComponent<Image>();
            imgPrev.color = new Color(0.40f, 0.28f, 0.18f, 1f);
            var btnPrev = prevBtnGo.AddComponent<Button>();
            GameObject prevTxtGo = new GameObject("Text", typeof(RectTransform));
            prevTxtGo.transform.SetParent(prevBtnGo.transform, false);
            var txtPrev = prevTxtGo.AddComponent<Text>();
            txtPrev.text = "<b>PREVIOUS</b>";
            txtPrev.font = defaultFont;
            txtPrev.fontSize = 20;
            txtPrev.alignment = TextAnchor.MiddleCenter;
            txtPrev.color = Color.white;
            txtPrev.rectTransform.anchorMin = Vector2.zero;
            txtPrev.rectTransform.anchorMax = Vector2.one;
            txtPrev.rectTransform.sizeDelta = Vector2.zero;

            // Page Indicator Text
            GameObject indTxtGo = new GameObject("Text_PageIndicator", typeof(RectTransform));
            indTxtGo.transform.SetParent(botNavGo.transform, false);
            var rtInd = indTxtGo.GetComponent<RectTransform>();
            rtInd.anchorMin = new Vector2(0.5f, 0.5f);
            rtInd.anchorMax = new Vector2(0.5f, 0.5f);
            rtInd.anchoredPosition = Vector2.zero;
            rtInd.sizeDelta = new Vector2(220f, 50f);
            var txtInd = indTxtGo.AddComponent<Text>();
            txtInd.text = "<b>PAGE 1 / 9</b>";
            txtInd.font = defaultFont;
            txtInd.fontSize = 24;
            txtInd.alignment = TextAnchor.MiddleCenter;
            txtInd.color = new Color(1f, 0.92f, 0.6f);

            // Next Button
            GameObject nextBtnGo = new GameObject("Btn_NextPage", typeof(RectTransform));
            nextBtnGo.transform.SetParent(botNavGo.transform, false);
            var rtNext = nextBtnGo.GetComponent<RectTransform>();
            rtNext.anchorMin = new Vector2(1f, 0.5f);
            rtNext.anchorMax = new Vector2(1f, 0.5f);
            rtNext.anchoredPosition = new Vector2(-100f, 0f);
            rtNext.sizeDelta = new Vector2(180f, 65f);
            var imgNext = nextBtnGo.AddComponent<Image>();
            imgNext.color = new Color(0.40f, 0.28f, 0.18f, 1f);
            var btnNext = nextBtnGo.AddComponent<Button>();
            GameObject nextTxtGo = new GameObject("Text", typeof(RectTransform));
            nextTxtGo.transform.SetParent(nextBtnGo.transform, false);
            var txtNext = nextTxtGo.AddComponent<Text>();
            txtNext.text = "<b>NEXT</b>";
            txtNext.font = defaultFont;
            txtNext.fontSize = 20;
            txtNext.alignment = TextAnchor.MiddleCenter;
            txtNext.color = Color.white;
            txtNext.rectTransform.anchorMin = Vector2.zero;
            txtNext.rectTransform.anchorMax = Vector2.one;
            txtNext.rectTransform.sizeDelta = Vector2.zero;

            // Start AR Activity CTA Button
            GameObject startARBtnGo = new GameObject("Btn_StartARActivity", typeof(RectTransform));
            startARBtnGo.transform.SetParent(storyRootGo.transform, false);
            var rtStartAR = startARBtnGo.GetComponent<RectTransform>();
            rtStartAR.anchorMin = new Vector2(0.5f, 0f);
            rtStartAR.anchorMax = new Vector2(0.5f, 0f);
            rtStartAR.pivot = new Vector2(0.5f, 0f);
            rtStartAR.anchoredPosition = new Vector2(0f, 120f);
            rtStartAR.sizeDelta = new Vector2(440f, 85f);
            var imgStartAR = startARBtnGo.AddComponent<Image>();
            imgStartAR.color = new Color(0.20f, 0.70f, 0.25f, 1f);
            var btnStartAR = startARBtnGo.AddComponent<Button>();
            GameObject startARTxtGo = new GameObject("Text", typeof(RectTransform));
            startARTxtGo.transform.SetParent(startARBtnGo.transform, false);
            var txtStartAR = startARTxtGo.AddComponent<Text>();
            txtStartAR.text = "<b>START LEVER AR EXPEDITION</b>";
            txtStartAR.font = defaultFont;
            txtStartAR.fontSize = 22;
            txtStartAR.alignment = TextAnchor.MiddleCenter;
            txtStartAR.color = Color.white;
            txtStartAR.rectTransform.anchorMin = Vector2.zero;
            txtStartAR.rectTransform.anchorMax = Vector2.one;
            txtStartAR.rectTransform.sizeDelta = Vector2.zero;

            startARBtnGo.SetActive(false);
            storyRootGo.SetActive(false);

            // 3. Attach StoryBookViewerController
            var storyCtrl = storyRootGo.AddComponent<StoryBookViewerController>();
            var sFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            typeof(StoryBookViewerController).GetField("storyBookRootPanel", sFlags).SetValue(storyCtrl, storyRootGo);
            typeof(StoryBookViewerController).GetField("pageDisplayImage", sFlags).SetValue(storyCtrl, imgPage);
            typeof(StoryBookViewerController).GetField("pageIndicatorText", sFlags).SetValue(storyCtrl, txtInd);
            typeof(StoryBookViewerController).GetField("chapterTitleText", sFlags).SetValue(storyCtrl, txtChapTitle);
            typeof(StoryBookViewerController).GetField("prevPageButton", sFlags).SetValue(storyCtrl, btnPrev);
            typeof(StoryBookViewerController).GetField("nextPageButton", sFlags).SetValue(storyCtrl, btnNext);
            typeof(StoryBookViewerController).GetField("closeBookButton", sFlags).SetValue(storyCtrl, btnBackMap);
            typeof(StoryBookViewerController).GetField("startARActivityButton", sFlags).SetValue(storyCtrl, btnStartAR);
            typeof(StoryBookViewerController).GetField("startARActivityButtonText", sFlags).SetValue(storyCtrl, txtStartAR);

            var pagesList = new List<Sprite>();
            for (int i = 1; i <= 18; i++)
            {
                string p = string.Format("Assets/Art/StoryBook/page_{0:D2}.png", i);
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (sp != null) pagesList.Add(sp);
            }
            typeof(StoryBookViewerController).GetField("allStoryPages", sFlags).SetValue(storyCtrl, pagesList);

            // 4. Wire HomeMapController
            var hFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(HomeMapController).GetField("questButton", hFlags).SetValue(homeCtrl, GameObject.Find("Map Canvas/Button_QuestStart")?.GetComponent<Button>());
            typeof(HomeMapController).GetField("questModalPanel", hFlags).SetValue(homeCtrl, questModalGo);
            typeof(HomeMapController).GetField("selectLeverQuestButton", hFlags).SetValue(homeCtrl, btnCL);
            typeof(HomeMapController).GetField("selectPulleyQuestButton", hFlags).SetValue(homeCtrl, btnCP);
            typeof(HomeMapController).GetField("selectInclineQuestButton", hFlags).SetValue(homeCtrl, btnCI);
            typeof(HomeMapController).GetField("closeQuestModalButton", hFlags).SetValue(homeCtrl, btnCloseQ);
            typeof(HomeMapController).GetField("dismissQuestModalButton", hFlags).SetValue(homeCtrl, btnBgDim);
            typeof(HomeMapController).GetField("pulleyLockOverlay", hFlags).SetValue(homeCtrl, lockPGo);
            typeof(HomeMapController).GetField("inclineLockOverlay", hFlags).SetValue(homeCtrl, lockIGo);
            typeof(HomeMapController).GetField("storyBookViewer", hFlags).SetValue(homeCtrl, storyCtrl);

            EditorUtility.SetDirty(homeCtrl);
            EditorUtility.SetDirty(storyCtrl);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

            Debug.Log("[BuildHomeMapQuest] Successfully built Quest Modal and Story Book Reader with " + pagesList.Count + " pages!");
        }

        [MenuItem("LostTemple/Setup AR Image Tracking in MainARScene")]
        public static void SetupARImageTrackingInMainARScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_App/Scenes/MainARScene.unity");
            UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(scene);

            var xrOrigin = GameObject.Find("XR Origin (Mobile AR)");
            if (xrOrigin == null)
            {
                Debug.LogError("XR Origin (Mobile AR) not found in MainARScene!");
                return;
            }

            var canvas = GameObject.Find("AR Main Canvas");
            var leverObj = GameObject.Find("SimpleMachine_Lever");
            var pulleyObj = GameObject.Find("SimpleMachine_Pulley");
            var inclineObj = GameObject.Find("SimpleMachine_InclinedPlane");
            var screenCtrl = GameObject.Find("AR Main Canvas/LeverAR_UIRoot")?.GetComponent<LeverARScreenController>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Configure ARTrackedImageManager on XR Origin
            var imageManager = xrOrigin.GetComponent<UnityEngine.XR.ARFoundation.ARTrackedImageManager>();
            if (imageManager == null) imageManager = xrOrigin.AddComponent<UnityEngine.XR.ARFoundation.ARTrackedImageManager>();

            var lib = AssetDatabase.LoadAssetAtPath<UnityEngine.XR.ARSubsystems.XRReferenceImageLibrary>("Assets/XR/ReferenceImageLibraries/SimpleMachinesReferenceImageLibrary.asset");
            imageManager.referenceLibrary = lib;
            imageManager.requestedMaxNumberOfMovingImages = 1;

            // 2. Setup Live WebCam Background directly in AR Main Canvas at Sibling Index 0
            var camGo = GameObject.Find("XR Origin (Mobile AR)/Camera Offset/Main Camera");
            var webCamBg = camGo != null ? (camGo.GetComponent<LiveWebCamBackground>() ?? camGo.AddComponent<LiveWebCamBackground>()) : null;

            var bgCanvasTr = xrOrigin.transform.Find("CameraBackgroundCanvas");
            if (bgCanvasTr != null) Object.DestroyImmediate(bgCanvasTr.gameObject);

            var oldRawTr = canvas.transform.Find("WebCamRawImage");
            if (oldRawTr != null) Object.DestroyImmediate(oldRawTr.gameObject);

            GameObject rawImgGo = new GameObject("WebCamRawImage", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            rawImgGo.transform.SetParent(canvas.transform, false);
            rawImgGo.transform.SetSiblingIndex(0); // Ensure it's rendered behind all UI HUD

            var rtRaw = rawImgGo.GetComponent<RectTransform>();
            rtRaw.anchorMin = Vector2.zero;
            rtRaw.anchorMax = Vector2.one;
            rtRaw.sizeDelta = Vector2.zero;
            var rawImg = rawImgGo.GetComponent<RawImage>();
            rawImg.color = Color.clear;
            rawImgGo.SetActive(false); // Only activated by LiveWebCamBackground once actual video frames arrive!
            var fitter = rawImgGo.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;

            if (webCamBg != null)
            {
                var wFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                typeof(LiveWebCamBackground).GetField("webCamDisplayImage", wFlags).SetValue(webCamBg, rawImg);
                typeof(LiveWebCamBackground).GetField("aspectRatioFitter", wFlags).SetValue(webCamBg, fitter);
                EditorUtility.SetDirty(webCamBg);
            }

            // 3. Build ScanningMarkerOverlay inside AR Main Canvas
            var scanOverlayTr = canvas.transform.Find("ScanningMarkerOverlay");
            if (scanOverlayTr != null) Object.DestroyImmediate(scanOverlayTr.gameObject);

            GameObject scanOverlayGo = new GameObject("ScanningMarkerOverlay", typeof(RectTransform));
            scanOverlayGo.transform.SetParent(canvas.transform, false);

            var rtScan = scanOverlayGo.GetComponent<RectTransform>();
            rtScan.anchorMin = Vector2.zero;
            rtScan.anchorMax = Vector2.one;
            rtScan.sizeDelta = Vector2.zero;

            // Semi-transparent frame (not solid)
            var imgScan = scanOverlayGo.AddComponent<Image>();
            imgScan.color = new Color(0f, 0f, 0f, 0.25f);

            // Header Guidance Card
            GameObject guideCardGo = new GameObject("GuidanceCard", typeof(RectTransform));
            guideCardGo.transform.SetParent(scanOverlayGo.transform, false);
            var rtCard = guideCardGo.GetComponent<RectTransform>();
            rtCard.anchorMin = new Vector2(0.5f, 0.88f);
            rtCard.anchorMax = new Vector2(0.5f, 0.88f);
            rtCard.pivot = new Vector2(0.5f, 0.5f);
            rtCard.sizeDelta = new Vector2(800f, 110f);
            var imgCard = guideCardGo.AddComponent<Image>();
            imgCard.color = new Color(0.20f, 0.14f, 0.08f, 0.95f);

            GameObject statusTxtGo = new GameObject("StatusText", typeof(RectTransform));
            statusTxtGo.transform.SetParent(guideCardGo.transform, false);
            var txtStatus = statusTxtGo.AddComponent<Text>();
            txtStatus.text = "<b>🔍 POINT CAMERA AT STORY BOOK MARKER</b>\n<size=18><color=#ffdd66>Scan Page 9 to awaken the Ancient Lever on your table!</color></size>";
            txtStatus.font = defaultFont;
            txtStatus.fontSize = 22;
            txtStatus.alignment = TextAnchor.MiddleCenter;
            txtStatus.color = Color.white;
            txtStatus.rectTransform.anchorMin = Vector2.zero;
            txtStatus.rectTransform.anchorMax = Vector2.one;
            txtStatus.rectTransform.sizeDelta = Vector2.zero;

            // Reticle Border Frame (Transparent with 4 boundary lines)
            GameObject reticleGo = new GameObject("ReticleFrame", typeof(RectTransform));
            reticleGo.transform.SetParent(scanOverlayGo.transform, false);
            var rtReticle = reticleGo.GetComponent<RectTransform>();
            rtReticle.anchorMin = new Vector2(0.5f, 0.5f);
            rtReticle.anchorMax = new Vector2(0.5f, 0.5f);
            rtReticle.sizeDelta = new Vector2(500f, 500f);

            // 4 corner border lines
            string[] borders = new string[] { "Top", "Bottom", "Left", "Right" };
            Vector2[] anchors = new Vector2[] {
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0f),
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f)
            };
            Vector2[] sizes = new Vector2[] {
                new Vector2(500f, 4f), new Vector2(500f, 4f),
                new Vector2(4f, 500f), new Vector2(4f, 500f)
            };
            for (int i = 0; i < 4; i++)
            {
                var borderGo = new GameObject("Border_" + borders[i], typeof(RectTransform), typeof(Image));
                borderGo.transform.SetParent(reticleGo.transform, false);
                var rtB = borderGo.GetComponent<RectTransform>();
                rtB.anchorMin = anchors[i];
                rtB.anchorMax = anchors[i];
                rtB.pivot = anchors[i];
                rtB.sizeDelta = sizes[i];
                var imgB = borderGo.GetComponent<Image>();
                imgB.color = new Color(1f, 0.88f, 0.45f, 0.9f);
            }

            scanOverlayGo.SetActive(false);

            // 4. Attach ARTrackedImageMachineAnchor
            var anchor = xrOrigin.GetComponent<ARTrackedImageMachineAnchor>();
            if (anchor == null) anchor = xrOrigin.AddComponent<ARTrackedImageMachineAnchor>();

            var uiRootGo = canvas.transform.Find("LeverAR_UIRoot")?.gameObject;
            var masterCtrl = canvas.GetComponent<LeverARScreenController>();

            var aFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(ARTrackedImageMachineAnchor).GetField("trackedImageManager", aFlags).SetValue(anchor, imageManager);
            typeof(ARTrackedImageMachineAnchor).GetField("referenceLibrary", aFlags).SetValue(anchor, lib);
            typeof(ARTrackedImageMachineAnchor).GetField("leverMachineRoot", aFlags).SetValue(anchor, leverObj);
            typeof(ARTrackedImageMachineAnchor).GetField("pulleyMachineRoot", aFlags).SetValue(anchor, pulleyObj);
            typeof(ARTrackedImageMachineAnchor).GetField("inclineMachineRoot", aFlags).SetValue(anchor, inclineObj);
            typeof(ARTrackedImageMachineAnchor).GetField("scanningMarkerOverlay", aFlags).SetValue(anchor, scanOverlayGo);
            typeof(ARTrackedImageMachineAnchor).GetField("scanStatusText", aFlags).SetValue(anchor, txtStatus);
            typeof(ARTrackedImageMachineAnchor).GetField("leverARUIRoot", aFlags).SetValue(anchor, uiRootGo);
            typeof(ARTrackedImageMachineAnchor).GetField("leverARScreenController", aFlags).SetValue(anchor, masterCtrl);

            // Attach WebCamMarkerDetector for Editor live camera scanning
            var detector = xrOrigin.GetComponent<WebCamMarkerDetector>();
            if (detector == null) detector = xrOrigin.AddComponent<WebCamMarkerDetector>();
            typeof(WebCamMarkerDetector).GetField("liveWebCam", aFlags).SetValue(detector, webCamBg);
            typeof(WebCamMarkerDetector).GetField("machineAnchor", aFlags).SetValue(detector, anchor);

            EditorUtility.SetDirty(xrOrigin);
            EditorUtility.SetDirty(canvas);
            EditorUtility.SetDirty(anchor);
            EditorUtility.SetDirty(detector);
            EditorUtility.SetDirty(scanOverlayGo);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

            Debug.Log("[SetupARImageTracking] Successfully configured Live Camera AR Image Tracking in MainARScene!");
        }

        [MenuItem("LostTemple/Setup Pathirana & Abeywickrama UI")]
        public static void BuildPathiranaAndAbeywickramaUI()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_App/Scenes/MainARScene.unity");
            var canvas = GameObject.Find("AR Main Canvas");
            if (canvas == null)
            {
                Debug.LogError("AR Main Canvas not found!");
                return;
            }

            var uiRoot = canvas.transform.Find("LeverAR_UIRoot");
            if (uiRoot == null)
            {
                Debug.LogError("LeverAR_UIRoot not found!");
                return;
            }

            var topBar = uiRoot.Find("TopBar");
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. TOP BAR ADDITIONS
            // A. Badge Adaptive Tier
            var oldBadge = topBar.Find("Badge_AdaptiveTier");
            if (oldBadge != null) GameObject.DestroyImmediate(oldBadge.gameObject);

            var tierBadgeGo = new GameObject("Badge_AdaptiveTier");
            tierBadgeGo.transform.SetParent(topBar, false);
            var tbImg = tierBadgeGo.AddComponent<Image>();
            tbImg.color = new Color(0.12f, 0.18f, 0.28f, 0.90f);
            var tbRt = tierBadgeGo.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 0.5f);
            tbRt.anchorMax = new Vector2(0f, 0.5f);
            tbRt.sizeDelta = new Vector2(185, 38);
            tbRt.anchoredPosition = new Vector2(155, 0);

            var tbTxtGo = new GameObject("Text");
            tbTxtGo.transform.SetParent(tierBadgeGo.transform, false);
            var tbTxt = tbTxtGo.AddComponent<Text>();
            tbTxt.text = "🎓 <b>APPRENTICE (Tier 1)</b>";
            tbTxt.font = defaultFont;
            tbTxt.fontSize = 14;
            tbTxt.color = new Color(0.95f, 0.85f, 0.3f);
            tbTxt.alignment = TextAnchor.MiddleCenter;
            var tbtRt = tbTxtGo.GetComponent<RectTransform>();
            tbtRt.anchorMin = Vector2.zero;
            tbtRt.anchorMax = Vector2.one;
            tbtRt.sizeDelta = Vector2.zero;

            // B. Btn Smart Hint
            Sprite spriteHint = null;
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/UI/btn_smart_hint.png"))
            {
                if (a is Sprite s) { spriteHint = s; break; }
            }
            Sprite spriteQuiz = null;
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/UI/btn_ar_quiz.png"))
            {
                if (a is Sprite s) { spriteQuiz = s; break; }
            }

            var oldHint = topBar.Find("Btn_SmartHint");
            if (oldHint != null) GameObject.DestroyImmediate(oldHint.gameObject);

            var hintBtnGo = new GameObject("Btn_SmartHint");
            hintBtnGo.transform.SetParent(topBar, false);
            var hImg = hintBtnGo.AddComponent<Image>();
            if (spriteHint != null)
            {
                hImg.sprite = spriteHint;
                hImg.color = Color.white;
                hImg.preserveAspect = true;
            }
            else
            {
                hImg.color = new Color(0.25f, 0.20f, 0.05f, 0.95f);
            }
            hintBtnGo.AddComponent<Button>();
            var hRt = hintBtnGo.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(1f, 0.5f);
            hRt.anchorMax = new Vector2(1f, 0.5f);
            hRt.sizeDelta = new Vector2(110, 42);
            hRt.anchoredPosition = new Vector2(-340, 0);

            if (spriteHint == null)
            {
                var hTxtGo = new GameObject("Text");
                hTxtGo.transform.SetParent(hintBtnGo.transform, false);
                var hTxt = hTxtGo.AddComponent<Text>();
                hTxt.text = "💡 <b>HINT</b>";
                hTxt.font = defaultFont;
                hTxt.fontSize = 16;
                hTxt.color = new Color(1f, 0.9f, 0.2f);
                hTxt.alignment = TextAnchor.MiddleCenter;
                var htRt = hTxtGo.GetComponent<RectTransform>();
                htRt.anchorMin = Vector2.zero;
                htRt.anchorMax = Vector2.one;
                htRt.sizeDelta = Vector2.zero;
            }

            // C. Btn AR Quiz
            var oldQuizBtn = topBar.Find("Btn_ARQuiz");
            if (oldQuizBtn != null) GameObject.DestroyImmediate(oldQuizBtn.gameObject);

            var quizBtnGo = new GameObject("Btn_ARQuiz");
            quizBtnGo.transform.SetParent(topBar, false);
            var qbImg = quizBtnGo.AddComponent<Image>();
            if (spriteQuiz != null)
            {
                qbImg.sprite = spriteQuiz;
                qbImg.color = Color.white;
                qbImg.preserveAspect = true;
            }
            else
            {
                qbImg.color = new Color(0.15f, 0.35f, 0.22f, 0.95f);
            }
            quizBtnGo.AddComponent<Button>();
            var qbRt = quizBtnGo.GetComponent<RectTransform>();
            qbRt.anchorMin = new Vector2(1f, 0.5f);
            qbRt.anchorMax = new Vector2(1f, 0.5f);
            qbRt.sizeDelta = new Vector2(110, 42);
            qbRt.anchoredPosition = new Vector2(-225, 0);

            if (spriteQuiz == null)
            {
                var qbTxtGo = new GameObject("Text");
                qbTxtGo.transform.SetParent(quizBtnGo.transform, false);
                var qbTxt = qbTxtGo.AddComponent<Text>();
                qbTxt.text = "📝 <b>AR QUIZ</b>";
                qbTxt.font = defaultFont;
                qbTxt.fontSize = 17;
                qbTxt.color = Color.white;
                qbTxt.alignment = TextAnchor.MiddleCenter;
                var qbtRt = qbTxtGo.GetComponent<RectTransform>();
                qbtRt.anchorMin = Vector2.zero;
                qbtRt.anchorMax = Vector2.one;
                qbtRt.sizeDelta = Vector2.zero;
            }

            // 2. PANEL AR QUIZ MODAL
            var oldQuizModal = uiRoot.Find("Panel_ARQuizModal");
            if (oldQuizModal != null) GameObject.DestroyImmediate(oldQuizModal.gameObject);

            var quizModalGo = new GameObject("Panel_ARQuizModal");
            quizModalGo.transform.SetParent(uiRoot, false);
            var qmRt = quizModalGo.AddComponent<RectTransform>();
            qmRt.anchorMin = Vector2.zero;
            qmRt.anchorMax = Vector2.one;
            qmRt.sizeDelta = Vector2.zero;

            // Backdrop Dim
            var qmDim = new GameObject("BackdropDim");
            qmDim.transform.SetParent(quizModalGo.transform, false);
            var qmdImg = qmDim.AddComponent<Image>();
            qmdImg.color = new Color(0f, 0f, 0f, 0.75f);
            var qmdRt = qmDim.GetComponent<RectTransform>();
            qmdRt.anchorMin = Vector2.zero;
            qmdRt.anchorMax = Vector2.one;
            qmdRt.sizeDelta = Vector2.zero;

            // Modal Card
            var cardGo = new GameObject("ModalCard");
            cardGo.transform.SetParent(quizModalGo.transform, false);
            var cardImg = cardGo.AddComponent<Image>();
            cardImg.color = new Color(0.08f, 0.12f, 0.18f, 0.98f);
            var cardRt = cardGo.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(620, 520);
            cardRt.anchoredPosition = Vector2.zero;

            // Title
            var qTitleGo = new GameObject("TitleText");
            qTitleGo.transform.SetParent(cardGo.transform, false);
            var qTitleTxt = qTitleGo.AddComponent<Text>();
            qTitleTxt.font = defaultFont;
            qTitleTxt.fontSize = 22;
            qTitleTxt.fontStyle = FontStyle.Bold;
            qTitleTxt.alignment = TextAnchor.MiddleCenter;
            qTitleTxt.color = new Color(1f, 0.88f, 0.3f);
            var qtr = qTitleGo.GetComponent<RectTransform>();
            qtr.anchorMin = new Vector2(0f, 1f);
            qtr.anchorMax = new Vector2(1f, 1f);
            qtr.sizeDelta = new Vector2(-40, 45);
            qtr.anchoredPosition = new Vector2(0, -30);
            qTitleTxt.text = "📝 IN-SITU AR ASSESSMENT (1/4)";

            // Category
            var qCatGo = new GameObject("CategoryText");
            qCatGo.transform.SetParent(cardGo.transform, false);
            var qCatTxt = qCatGo.AddComponent<Text>();
            qCatTxt.font = defaultFont;
            qCatTxt.fontSize = 16;
            qCatTxt.alignment = TextAnchor.MiddleCenter;
            qCatTxt.color = new Color(0.6f, 0.85f, 1f);
            var qcr = qCatGo.GetComponent<RectTransform>();
            qcr.anchorMin = new Vector2(0f, 1f);
            qcr.anchorMax = new Vector2(1f, 1f);
            qcr.sizeDelta = new Vector2(-40, 25);
            qcr.anchoredPosition = new Vector2(0, -62);
            qCatTxt.text = "Topic: Mechanical Advantage & Force";

            // Question Text
            var qQGo = new GameObject("QuestionText");
            qQGo.transform.SetParent(cardGo.transform, false);
            var qQTxt = qQGo.AddComponent<Text>();
            qQTxt.font = defaultFont;
            qQTxt.fontSize = 18;
            qQTxt.alignment = TextAnchor.UpperLeft;
            qQTxt.color = Color.white;
            var qqr = qQGo.GetComponent<RectTransform>();
            qqr.anchorMin = new Vector2(0f, 1f);
            qqr.anchorMax = new Vector2(1f, 1f);
            qqr.sizeDelta = new Vector2(-60, 75);
            qqr.anchoredPosition = new Vector2(0, -115);
            qQTxt.text = "Question";

            // 4 Option Buttons
            for (int i = 0; i < 4; i++)
            {
                var optGo = new GameObject("Btn_Option_" + (i + 1));
                optGo.transform.SetParent(cardGo.transform, false);
                var oi = optGo.AddComponent<Image>();
                oi.color = new Color(0.16f, 0.22f, 0.32f, 0.95f);
                optGo.AddComponent<Button>();
                var ort = optGo.GetComponent<RectTransform>();
                ort.anchorMin = new Vector2(0.5f, 1f);
                ort.anchorMax = new Vector2(0.5f, 1f);
                ort.sizeDelta = new Vector2(540, 42);
                ort.anchoredPosition = new Vector2(0, -185 - (i * 48));

                var otxtGo = new GameObject("Text");
                otxtGo.transform.SetParent(optGo.transform, false);
                var otxt = otxtGo.AddComponent<Text>();
                otxt.text = "Option " + (i + 1);
                otxt.font = defaultFont;
                otxt.fontSize = 16;
                otxt.color = Color.white;
                otxt.alignment = TextAnchor.MiddleLeft;
                var otr = otxtGo.GetComponent<RectTransform>();
                otr.anchorMin = Vector2.zero;
                otr.anchorMax = Vector2.one;
                otr.anchoredPosition = new Vector2(15, 0);
                otr.sizeDelta = new Vector2(-30, 0);
            }

            // Panel Feedback
            var fbGo = new GameObject("Panel_Feedback");
            fbGo.transform.SetParent(cardGo.transform, false);
            var fbImg = fbGo.AddComponent<Image>();
            fbImg.color = new Color(0.08f, 0.18f, 0.12f, 0.95f);
            var fbRt = fbGo.GetComponent<RectTransform>();
            fbRt.anchorMin = new Vector2(0.5f, 0f);
            fbRt.anchorMax = new Vector2(0.5f, 0f);
            fbRt.sizeDelta = new Vector2(540, 65);
            fbRt.anchoredPosition = new Vector2(0, 80);

            var fbtGo = new GameObject("FeedbackText");
            fbtGo.transform.SetParent(fbGo.transform, false);
            var fbTxt = fbtGo.AddComponent<Text>();
            fbTxt.font = defaultFont;
            fbTxt.fontSize = 15;
            fbTxt.alignment = TextAnchor.MiddleCenter;
            fbTxt.color = Color.white;
            var fbtr = fbtGo.GetComponent<RectTransform>();
            fbtr.anchorMin = Vector2.zero;
            fbtr.anchorMax = Vector2.one;
            fbtr.sizeDelta = Vector2.zero;
            fbGo.SetActive(false);

            // Btn Next Question
            var nextGo = new GameObject("Btn_NextQuestion");
            nextGo.transform.SetParent(cardGo.transform, false);
            var nImg = nextGo.AddComponent<Image>();
            nImg.color = new Color(0.18f, 0.65f, 0.35f, 1f);
            nextGo.AddComponent<Button>();
            var nRt = nextGo.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(1f, 0f);
            nRt.anchorMax = new Vector2(1f, 0f);
            nRt.sizeDelta = new Vector2(160, 42);
            nRt.anchoredPosition = new Vector2(-40, 25);

            var ntxtGo = new GameObject("Text");
            ntxtGo.transform.SetParent(nextGo.transform, false);
            var ntxt = ntxtGo.AddComponent<Text>();
            ntxt.text = "Next ➔";
            ntxt.font = defaultFont;
            ntxt.fontSize = 17;
            ntxt.fontStyle = FontStyle.Bold;
            ntxt.color = Color.white;
            ntxt.alignment = TextAnchor.MiddleCenter;
            var ntr = ntxtGo.GetComponent<RectTransform>();
            ntr.anchorMin = Vector2.zero;
            ntr.anchorMax = Vector2.one;
            ntr.sizeDelta = Vector2.zero;

            // Btn Review Mode
            var revGo = new GameObject("Btn_ReviewMode");
            revGo.transform.SetParent(cardGo.transform, false);
            var rvi = revGo.AddComponent<Image>();
            rvi.color = new Color(0.2f, 0.5f, 0.85f, 1f);
            revGo.AddComponent<Button>();
            var rvRt = revGo.GetComponent<RectTransform>();
            rvRt.anchorMin = new Vector2(0f, 0f);
            rvRt.anchorMax = new Vector2(0f, 0f);
            rvRt.sizeDelta = new Vector2(180, 42);
            rvRt.anchoredPosition = new Vector2(40, 25);

            var rvtxtGo = new GameObject("Text");
            rvtxtGo.transform.SetParent(revGo.transform, false);
            var rvtxt = rvtxtGo.AddComponent<Text>();
            rvtxt.text = "🔍 Review in AR";
            rvtxt.font = defaultFont;
            rvtxt.fontSize = 16;
            rvtxt.fontStyle = FontStyle.Bold;
            rvtxt.color = Color.white;
            rvtxt.alignment = TextAnchor.MiddleCenter;
            var rvtr = rvtxtGo.GetComponent<RectTransform>();
            rvtr.anchorMin = Vector2.zero;
            rvtr.anchorMax = Vector2.one;
            rvtr.sizeDelta = Vector2.zero;

            // Btn Retention Report
            var retGo = new GameObject("Btn_RetentionReport");
            retGo.transform.SetParent(cardGo.transform, false);
            var retImg = retGo.AddComponent<Image>();
            retImg.color = new Color(0.7f, 0.45f, 0.15f, 1f);
            retGo.AddComponent<Button>();
            var retRt = retGo.GetComponent<RectTransform>();
            retRt.anchorMin = new Vector2(0.5f, 0f);
            retRt.anchorMax = new Vector2(0.5f, 0f);
            retRt.sizeDelta = new Vector2(180, 42);
            retRt.anchoredPosition = new Vector2(0, 25);

            var rettxtGo = new GameObject("Text");
            rettxtGo.transform.SetParent(retGo.transform, false);
            var rettxt = rettxtGo.AddComponent<Text>();
            rettxt.text = "📈 Retention Report";
            rettxt.font = defaultFont;
            rettxt.fontSize = 16;
            rettxt.fontStyle = FontStyle.Bold;
            rettxt.color = Color.white;
            rettxt.alignment = TextAnchor.MiddleCenter;
            var rettr = rettxtGo.GetComponent<RectTransform>();
            rettr.anchorMin = Vector2.zero;
            rettr.anchorMax = Vector2.one;
            rettr.sizeDelta = Vector2.zero;

            // Btn Close Quiz
            var cBtnGo = new GameObject("Btn_Close");
            cBtnGo.transform.SetParent(cardGo.transform, false);
            var cbImg = cBtnGo.AddComponent<Image>();
            cbImg.color = new Color(0.7f, 0.2f, 0.2f, 0.9f);
            cBtnGo.AddComponent<Button>();
            var cbRt = cBtnGo.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(1f, 1f);
            cbRt.anchorMax = new Vector2(1f, 1f);
            cbRt.sizeDelta = new Vector2(36, 36);
            cbRt.anchoredPosition = new Vector2(-22, -22);
            var cbtGo = new GameObject("Text");
            cbtGo.transform.SetParent(cBtnGo.transform, false);
            var cbt = cbtGo.AddComponent<Text>();
            cbt.text = "✕";
            cbt.font = defaultFont;
            cbt.fontSize = 20;
            cbt.color = Color.white;
            cbt.alignment = TextAnchor.MiddleCenter;
            var cbr = cbtGo.GetComponent<RectTransform>();
            cbr.anchorMin = Vector2.zero;
            cbr.anchorMax = Vector2.one;
            cbr.sizeDelta = Vector2.zero;

            quizModalGo.SetActive(false);

            // 3. PANEL RETENTION MODAL
            var oldRetModal = uiRoot.Find("Panel_RetentionModal");
            if (oldRetModal != null) GameObject.DestroyImmediate(oldRetModal.gameObject);

            var retModalGo = new GameObject("Panel_RetentionModal");
            retModalGo.transform.SetParent(uiRoot, false);
            var rmRt = retModalGo.AddComponent<RectTransform>();
            rmRt.anchorMin = Vector2.zero;
            rmRt.anchorMax = Vector2.one;
            rmRt.sizeDelta = Vector2.zero;

            var rmDim = new GameObject("BackdropDim");
            rmDim.transform.SetParent(retModalGo.transform, false);
            var rmdImg = rmDim.AddComponent<Image>();
            rmdImg.color = new Color(0f, 0f, 0f, 0.75f);
            var rmdRt = rmDim.GetComponent<RectTransform>();
            rmdRt.anchorMin = Vector2.zero;
            rmdRt.anchorMax = Vector2.one;
            rmdRt.sizeDelta = Vector2.zero;

            var rCardGo = new GameObject("ModalCard");
            rCardGo.transform.SetParent(retModalGo.transform, false);
            var rcImg = rCardGo.AddComponent<Image>();
            rcImg.color = new Color(0.08f, 0.12f, 0.20f, 0.98f);
            var rcRt = rCardGo.GetComponent<RectTransform>();
            rcRt.anchorMin = new Vector2(0.5f, 0.5f);
            rcRt.anchorMax = new Vector2(0.5f, 0.5f);
            rcRt.sizeDelta = new Vector2(600, 480);
            rcRt.anchoredPosition = Vector2.zero;

            var rTitleGo = new GameObject("TitleText");
            rTitleGo.transform.SetParent(rCardGo.transform, false);
            var rTitleTxt = rTitleGo.AddComponent<Text>();
            rTitleTxt.font = defaultFont;
            rTitleTxt.fontSize = 21;
            rTitleTxt.fontStyle = FontStyle.Bold;
            rTitleTxt.alignment = TextAnchor.MiddleCenter;
            rTitleTxt.color = new Color(0.4f, 0.9f, 1f);
            var rtr = rTitleGo.GetComponent<RectTransform>();
            rtr.anchorMin = new Vector2(0f, 1f);
            rtr.anchorMax = new Vector2(1f, 1f);
            rtr.sizeDelta = new Vector2(-40, 40);
            rtr.anchoredPosition = new Vector2(0, -25);
            rTitleTxt.text = "📊 KNOWLEDGE RETENTION & RESEARCH ANALYTICS";

            var rSumGo = new GameObject("SummaryText");
            rSumGo.transform.SetParent(rCardGo.transform, false);
            var rSumTxt = rSumGo.AddComponent<Text>();
            rSumTxt.font = defaultFont;
            rSumTxt.fontSize = 16;
            rSumTxt.alignment = TextAnchor.UpperLeft;
            rSumTxt.color = Color.white;
            var rsr = rSumGo.GetComponent<RectTransform>();
            rsr.anchorMin = new Vector2(0f, 0f);
            rsr.anchorMax = new Vector2(1f, 1f);
            rsr.sizeDelta = new Vector2(-60, -140);
            rsr.anchoredPosition = new Vector2(0, 10);

            var rExpBtnGo = new GameObject("Btn_ExportCSV");
            rExpBtnGo.transform.SetParent(rCardGo.transform, false);
            var reImg = rExpBtnGo.AddComponent<Image>();
            reImg.color = new Color(0.18f, 0.55f, 0.28f, 1f);
            rExpBtnGo.AddComponent<Button>();
            var reRt = rExpBtnGo.GetComponent<RectTransform>();
            reRt.anchorMin = new Vector2(0.5f, 0f);
            reRt.anchorMax = new Vector2(0.5f, 0f);
            reRt.sizeDelta = new Vector2(240, 44);
            reRt.anchoredPosition = new Vector2(0, 30);
            var retxtGo = new GameObject("Text");
            retxtGo.transform.SetParent(rExpBtnGo.transform, false);
            var retxt = retxtGo.AddComponent<Text>();
            retxt.text = "📁 Export Research CSV";
            retxt.font = defaultFont;
            retxt.fontSize = 17;
            retxt.color = Color.white;
            retxt.alignment = TextAnchor.MiddleCenter;
            var retr = retxtGo.GetComponent<RectTransform>();
            retr.anchorMin = Vector2.zero;
            retr.anchorMax = Vector2.one;
            retr.sizeDelta = Vector2.zero;

            var rCloseBtnGo = new GameObject("Btn_Close");
            rCloseBtnGo.transform.SetParent(rCardGo.transform, false);
            var rcbImg = rCloseBtnGo.AddComponent<Image>();
            rcbImg.color = new Color(0.7f, 0.2f, 0.2f, 0.9f);
            rCloseBtnGo.AddComponent<Button>();
            var rcbRt = rCloseBtnGo.GetComponent<RectTransform>();
            rcbRt.anchorMin = new Vector2(1f, 1f);
            rcbRt.anchorMax = new Vector2(1f, 1f);
            rcbRt.sizeDelta = new Vector2(36, 36);
            rcbRt.anchoredPosition = new Vector2(-22, -22);
            var rcbtGo = new GameObject("Text");
            rcbtGo.transform.SetParent(rCloseBtnGo.transform, false);
            var rcbt = rcbtGo.AddComponent<Text>();
            rcbt.text = "✕";
            rcbt.font = defaultFont;
            rcbt.fontSize = 20;
            rcbt.color = Color.white;
            rcbt.alignment = TextAnchor.MiddleCenter;
            var rcbtr = rcbtGo.GetComponent<RectTransform>();
            rcbtr.anchorMin = Vector2.zero;
            rcbtr.anchorMax = Vector2.one;
            rcbtr.sizeDelta = Vector2.zero;

            retModalGo.SetActive(false);

            // 4. PANEL SMART HINT SPEECH BUBBLE (Appears right next to Companion Character)
            var oldHintModal = uiRoot.Find("Panel_SmartHintModal");
            if (oldHintModal != null) GameObject.DestroyImmediate(oldHintModal.gameObject);

            var hintModalGo = new GameObject("Panel_SmartHintModal");
            hintModalGo.transform.SetParent(uiRoot, false);
            var hmRt = hintModalGo.AddComponent<RectTransform>();
            hmRt.anchorMin = Vector2.zero;
            hmRt.anchorMax = Vector2.one;
            hmRt.sizeDelta = Vector2.zero;

            // Transparent dismiss area (tap outside bubble to dismiss)
            var hmDim = new GameObject("BackdropDim");
            hmDim.transform.SetParent(hintModalGo.transform, false);
            var hmdImg = hmDim.AddComponent<Image>();
            hmdImg.color = new Color(0f, 0f, 0f, 0.05f); // Subtle non-blocking dismiss backdrop
            var hmdBtn = hmDim.AddComponent<Button>();
            var hmdRt = hmDim.GetComponent<RectTransform>();
            hmdRt.anchorMin = Vector2.zero;
            hmdRt.anchorMax = Vector2.one;
            hmdRt.sizeDelta = Vector2.zero;

            // Speech Bubble Card beside Companion Character (Maya/Senuk at bottom-left)
            var hCardGo = new GameObject("ModalCard");
            hCardGo.transform.SetParent(hintModalGo.transform, false);
            var hcImg = hCardGo.AddComponent<Image>();
            hcImg.color = new Color(0.10f, 0.14f, 0.22f, 0.98f);
            var hcRt = hCardGo.GetComponent<RectTransform>();
            hcRt.anchorMin = new Vector2(0f, 0f);
            hcRt.anchorMax = new Vector2(0f, 0f);
            hcRt.pivot = new Vector2(0f, 0.5f); // Grow outward from character
            hcRt.sizeDelta = new Vector2(560, 270);
            hcRt.anchoredPosition = new Vector2(220, 320);

            // Speech Bubble Pointer / Callout Arrow
            var pointerGo = new GameObject("BubblePointer");
            pointerGo.transform.SetParent(hCardGo.transform, false);
            var ptImg = pointerGo.AddComponent<Image>();
            ptImg.color = new Color(0.10f, 0.14f, 0.22f, 0.98f);
            var ptRt = pointerGo.GetComponent<RectTransform>();
            ptRt.anchorMin = new Vector2(0f, 0.5f);
            ptRt.anchorMax = new Vector2(0f, 0.5f);
            ptRt.pivot = new Vector2(0.5f, 0.5f);
            ptRt.sizeDelta = new Vector2(32, 32);
            ptRt.anchoredPosition = new Vector2(-8, 0);
            ptRt.localEulerAngles = new Vector3(0, 0, 45); // Rotated square acting as speech arrow

            // Title (e.g. "💬 MAYA'S HINT:")
            var hTitleGo = new GameObject("TitleText");
            hTitleGo.transform.SetParent(hCardGo.transform, false);
            var hTitleTxt = hTitleGo.AddComponent<Text>();
            hTitleTxt.font = defaultFont;
            hTitleTxt.fontSize = 24;
            hTitleTxt.fontStyle = FontStyle.Bold;
            hTitleTxt.alignment = TextAnchor.MiddleLeft;
            hTitleTxt.color = new Color(1f, 0.90f, 0.25f);
            var htr = hTitleGo.GetComponent<RectTransform>();
            htr.anchorMin = new Vector2(0f, 1f);
            htr.anchorMax = new Vector2(1f, 1f);
            htr.sizeDelta = new Vector2(-40, 38);
            htr.anchoredPosition = new Vector2(25, -20);
            hTitleTxt.text = "💬 <b>MAYA'S ENGINEERING TIP:</b>";

            // Body Text
            var hBodyGo = new GameObject("BodyText");
            hBodyGo.transform.SetParent(hCardGo.transform, false);
            var hBodyTxt = hBodyGo.AddComponent<Text>();
            hBodyTxt.font = defaultFont;
            hBodyTxt.fontSize = 21;
            hBodyTxt.lineSpacing = 1.15f;
            hBodyTxt.alignment = TextAnchor.UpperLeft;
            hBodyTxt.color = Color.white;
            var hbr = hBodyGo.GetComponent<RectTransform>();
            hbr.anchorMin = new Vector2(0f, 0f);
            hbr.anchorMax = new Vector2(1f, 1f);
            hbr.sizeDelta = new Vector2(-40, -90);
            hbr.anchoredPosition = new Vector2(22, 10);

            // Got It / Close Button
            var hCloseBtnGo = new GameObject("Btn_Close");
            hCloseBtnGo.transform.SetParent(hCardGo.transform, false);
            var hcbImg = hCloseBtnGo.AddComponent<Image>();
            hcbImg.color = new Color(0.85f, 0.65f, 0.15f, 1f);
            hCloseBtnGo.AddComponent<Button>();
            var hcbRt = hCloseBtnGo.GetComponent<RectTransform>();
            hcbRt.anchorMin = new Vector2(1f, 0f);
            hcbRt.anchorMax = new Vector2(1f, 0f);
            hcbRt.pivot = new Vector2(1f, 0f);
            hcbRt.sizeDelta = new Vector2(140, 44);
            hcbRt.anchoredPosition = new Vector2(-20, 16);
            var hcbtGo = new GameObject("Text");
            hcbtGo.transform.SetParent(hCloseBtnGo.transform, false);
            var hcbt = hcbtGo.AddComponent<Text>();
            hcbt.text = "Got It! 👍";
            hcbt.font = defaultFont;
            hcbt.fontSize = 18;
            hcbt.color = Color.black;
            hcbt.fontStyle = FontStyle.Bold;
            hcbt.alignment = TextAnchor.MiddleCenter;
            var hcbtr = hcbtGo.GetComponent<RectTransform>();
            hcbtr.anchorMin = Vector2.zero;
            hcbtr.anchorMax = Vector2.one;
            hcbtr.sizeDelta = Vector2.zero;

            hintModalGo.SetActive(false);

            // Re-bind controller
            var controller = canvas.GetComponent<LeverARScreenController>();
            if (controller != null)
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                controller.GetType().GetMethod("AutoBindAllReferences", flags)?.Invoke(controller, null);
                controller.GetType().GetMethod("SetupButtonListeners", flags)?.Invoke(controller, null);
                EditorUtility.SetDirty(controller);
            }

            EditorUtility.SetDirty(canvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

            Debug.Log("[BuildPathiranaAndAbeywickramaUI] Successfully built Quiz, Retention, and Smart Hint UI in MainARScene!");
        }

        [MenuItem("LostTemple/Setup HomeMap Research Dashboard UI")]
        public static void BuildHomeMapResearchDashboard()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_App/Scenes/HomeMapScene.unity");
            var canvas = GameObject.Find("Map Canvas");
            if (canvas == null)
            {
                Debug.LogError("Map Canvas not found in HomeMapScene!");
                return;
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Add Button_ResearchDashboard to TopHUD
            var topHUD = canvas.transform.Find("TopHUD");
            var oldBtn = topHUD != null ? topHUD.Find("Button_ResearchDashboard") : canvas.transform.Find("Button_ResearchDashboard");
            if (oldBtn != null) GameObject.DestroyImmediate(oldBtn.gameObject);

            var dashBtnGo = new GameObject("Button_ResearchDashboard");
            dashBtnGo.transform.SetParent(topHUD != null ? topHUD : canvas.transform, false);
            var dbImg = dashBtnGo.AddComponent<UnityEngine.UI.Image>();
            dbImg.color = new Color(0.12f, 0.28f, 0.48f, 0.95f);
            dashBtnGo.AddComponent<UnityEngine.UI.Button>();
            var dbRt = dashBtnGo.GetComponent<RectTransform>();
            dbRt.anchorMin = new Vector2(0.5f, 0.5f);
            dbRt.anchorMax = new Vector2(0.5f, 0.5f);
            dbRt.sizeDelta = new Vector2(230, 46);
            dbRt.anchoredPosition = new Vector2(40, 0);

            var dbtGo = new GameObject("Text");
            dbtGo.transform.SetParent(dashBtnGo.transform, false);
            var dbtTxt = dbtGo.AddComponent<UnityEngine.UI.Text>();
            dbtTxt.text = "📊 <b>RESEARCH DASHBOARD</b>";
            dbtTxt.font = defaultFont;
            dbtTxt.fontSize = 15;
            dbtTxt.color = new Color(0.4f, 0.95f, 1f);
            dbtTxt.alignment = TextAnchor.MiddleCenter;
            var dbtr = dbtGo.GetComponent<RectTransform>();
            dbtr.anchorMin = Vector2.zero;
            dbtr.anchorMax = Vector2.one;
            dbtr.sizeDelta = Vector2.zero;

            // 2. Build Modal_ResearchDashboard
            var oldModal = canvas.transform.Find("Modal_ResearchDashboard");
            if (oldModal != null) GameObject.DestroyImmediate(oldModal.gameObject);

            var modalGo = new GameObject("Modal_ResearchDashboard");
            modalGo.transform.SetParent(canvas.transform, false);
            var mRt = modalGo.AddComponent<RectTransform>();
            mRt.anchorMin = Vector2.zero;
            mRt.anchorMax = Vector2.one;
            mRt.sizeDelta = Vector2.zero;

            var dimGo = new GameObject("BackdropDim");
            dimGo.transform.SetParent(modalGo.transform, false);
            var dimImg = dimGo.AddComponent<UnityEngine.UI.Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.85f);
            var dimBtn = dimGo.AddComponent<UnityEngine.UI.Button>();
            var dimRt = dimGo.GetComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.sizeDelta = Vector2.zero;

            // Modal Card
            var cardGo = new GameObject("ModalCard");
            cardGo.transform.SetParent(modalGo.transform, false);
            var cardImg = cardGo.AddComponent<UnityEngine.UI.Image>();
            cardImg.color = new Color(0.06f, 0.09f, 0.15f, 0.98f);
            var cardRt = cardGo.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(740, 540);
            cardRt.anchoredPosition = Vector2.zero;

            // Header Title
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleTxt = titleGo.AddComponent<UnityEngine.UI.Text>();
            titleTxt.text = "📊 <b>RESEARCH & EVALUATION ANALYTICS DASHBOARD</b>";
            titleTxt.font = defaultFont;
            titleTxt.fontSize = 20;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1f, 0.88f, 0.3f);
            titleTxt.alignment = TextAnchor.MiddleCenter;
            var trt = titleGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 1f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.sizeDelta = new Vector2(-40, 36);
            trt.anchoredPosition = new Vector2(0, -22);

            // Header Info / Participant Line
            var infoGo = new GameObject("HeaderInfoText");
            infoGo.transform.SetParent(cardGo.transform, false);
            var infoTxt = infoGo.AddComponent<UnityEngine.UI.Text>();
            infoTxt.font = defaultFont;
            infoTxt.fontSize = 14;
            infoTxt.alignment = TextAnchor.MiddleCenter;
            infoTxt.color = Color.white;
            var irt = infoGo.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0f, 1f);
            irt.anchorMax = new Vector2(1f, 1f);
            irt.sizeDelta = new Vector2(-40, 32);
            irt.anchoredPosition = new Vector2(0, -56);

            // Helper to build a metric card box
            Text txtCognitive = BuildMetricCardBox(cardGo, "Card_Cognitive", "🎯 1. JAYASINGHE: AR SPATIAL & COGNITIVE (RQ1)", new Vector2(-175, 80), new Vector2(335, 155), defaultFont);
            Text txtAdaptive = BuildMetricCardBox(cardGo, "Card_Adaptive", "💡 2. ABEYWICKRAMA: ADAPTIVE & SCAFFOLDING (RQ2)", new Vector2(175, 80), new Vector2(335, 155), defaultFont);
            Text txtAssessment = BuildMetricCardBox(cardGo, "Card_Assessment", "📝 3. PATHIRANA: ASSESSMENT & LEARNING GAIN (RQ3)", new Vector2(-175, -85), new Vector2(335, 155), defaultFont);
            Text txtRetention = BuildMetricCardBox(cardGo, "Card_Retention", "🧠 4. PATHIRANA: EBBINGHAUS RETENTION (RQ3)", new Vector2(175, -85), new Vector2(335, 155), defaultFont);

            // Bottom Action Bar: Export CSV Button
            var expBtnGo = new GameObject("Btn_ExportResearchCSV");
            expBtnGo.transform.SetParent(cardGo.transform, false);
            var expImg = expBtnGo.AddComponent<UnityEngine.UI.Image>();
            expImg.color = new Color(0.18f, 0.58f, 0.28f, 1f);
            expBtnGo.AddComponent<UnityEngine.UI.Button>();
            var expRt = expBtnGo.GetComponent<RectTransform>();
            expRt.anchorMin = new Vector2(0.5f, 0f);
            expRt.anchorMax = new Vector2(0.5f, 0f);
            expRt.sizeDelta = new Vector2(320, 44);
            expRt.anchoredPosition = new Vector2(-120, 32);

            var expTxtGo = new GameObject("Text");
            expTxtGo.transform.SetParent(expBtnGo.transform, false);
            var expTxt = expTxtGo.AddComponent<UnityEngine.UI.Text>();
            expTxt.text = "📁 <b>EXPORT RESEARCH CSV DATASET</b>";
            expTxt.font = defaultFont;
            expTxt.fontSize = 14;
            expTxt.color = Color.white;
            expTxt.alignment = TextAnchor.MiddleCenter;
            var exprt = expTxtGo.GetComponent<RectTransform>();
            exprt.anchorMin = Vector2.zero;
            exprt.anchorMax = Vector2.one;
            exprt.sizeDelta = Vector2.zero;

            // Close Button
            var clsBtnGo = new GameObject("Btn_CloseDashboard");
            clsBtnGo.transform.SetParent(cardGo.transform, false);
            var clsImg = clsBtnGo.AddComponent<UnityEngine.UI.Image>();
            clsImg.color = new Color(0.75f, 0.22f, 0.22f, 1f);
            clsBtnGo.AddComponent<UnityEngine.UI.Button>();
            var clsRt = clsBtnGo.GetComponent<RectTransform>();
            clsRt.anchorMin = new Vector2(0.5f, 0f);
            clsRt.anchorMax = new Vector2(0.5f, 0f);
            clsRt.sizeDelta = new Vector2(140, 44);
            clsRt.anchoredPosition = new Vector2(140, 32);

            var clsTxtGo = new GameObject("Text");
            clsTxtGo.transform.SetParent(clsBtnGo.transform, false);
            var clsTxt = clsTxtGo.AddComponent<UnityEngine.UI.Text>();
            clsTxt.text = "✕ <b>CLOSE</b>";
            clsTxt.font = defaultFont;
            clsTxt.fontSize = 15;
            clsTxt.color = Color.white;
            clsTxt.alignment = TextAnchor.MiddleCenter;
            var clsrt = clsTxtGo.GetComponent<RectTransform>();
            clsrt.anchorMin = Vector2.zero;
            clsrt.anchorMax = Vector2.one;
            clsrt.sizeDelta = Vector2.zero;

            // Top Corner X Button
            var xBtnGo = new GameObject("Btn_CornerClose");
            xBtnGo.transform.SetParent(cardGo.transform, false);
            var xImg = xBtnGo.AddComponent<UnityEngine.UI.Image>();
            xImg.color = new Color(0.7f, 0.2f, 0.2f, 0.9f);
            xBtnGo.AddComponent<UnityEngine.UI.Button>();
            var xRt = xBtnGo.GetComponent<RectTransform>();
            xRt.anchorMin = new Vector2(1f, 1f);
            xRt.anchorMax = new Vector2(1f, 1f);
            xRt.sizeDelta = new Vector2(34, 34);
            xRt.anchoredPosition = new Vector2(-18, -18);
            var xtGo = new GameObject("Text");
            xtGo.transform.SetParent(xBtnGo.transform, false);
            var xt = xtGo.AddComponent<UnityEngine.UI.Text>();
            xt.text = "✕";
            xt.font = defaultFont;
            xt.fontSize = 20;
            xt.color = Color.white;
            xt.alignment = TextAnchor.MiddleCenter;
            var xtr = xtGo.GetComponent<RectTransform>();
            xtr.anchorMin = Vector2.zero;
            xtr.anchorMax = Vector2.one;
            xtr.sizeDelta = Vector2.zero;

            // Feedback label
            var fbLabelGo = new GameObject("ExportFeedbackText");
            fbLabelGo.transform.SetParent(cardGo.transform, false);
            var fbLabel = fbLabelGo.AddComponent<UnityEngine.UI.Text>();
            fbLabel.font = defaultFont;
            fbLabel.fontSize = 13;
            fbLabel.color = new Color(0.4f, 1f, 0.5f);
            fbLabel.alignment = TextAnchor.MiddleCenter;
            var fbrt = fbLabelGo.GetComponent<RectTransform>();
            fbrt.anchorMin = new Vector2(0.5f, 0f);
            fbrt.anchorMax = new Vector2(0.5f, 0f);
            fbrt.sizeDelta = new Vector2(700, 24);
            fbrt.anchoredPosition = new Vector2(0, 68);

            modalGo.SetActive(false);

            // Bind references on HomeMapController
            var homeCtrl = canvas.GetComponentInChildren<SecretOfLostTemple.Home.HomeMapController>(true);
            if (homeCtrl != null)
            {
                var so = new UnityEditor.SerializedObject(homeCtrl);
                so.FindProperty("researchDashboardButton").objectReferenceValue = dashBtnGo.GetComponent<UnityEngine.UI.Button>();
                so.FindProperty("dashboardModalPanel").objectReferenceValue = modalGo;
                so.FindProperty("dashboardHeaderInfoText").objectReferenceValue = infoTxt;
                so.FindProperty("dashboardCognitiveStatsText").objectReferenceValue = txtCognitive;
                so.FindProperty("dashboardAdaptiveStatsText").objectReferenceValue = txtAdaptive;
                so.FindProperty("dashboardAssessmentStatsText").objectReferenceValue = txtAssessment;
                so.FindProperty("dashboardRetentionStatsText").objectReferenceValue = txtRetention;
                so.FindProperty("dashboardExportCsvButton").objectReferenceValue = expBtnGo.GetComponent<UnityEngine.UI.Button>();
                so.FindProperty("dashboardCloseButton").objectReferenceValue = clsBtnGo.GetComponent<UnityEngine.UI.Button>();
                so.FindProperty("dashboardDismissButton").objectReferenceValue = dimBtn;
                so.FindProperty("dashboardExportFeedbackText").objectReferenceValue = fbLabel;
                so.ApplyModifiedProperties();
                UnityEditor.EditorUtility.SetDirty(homeCtrl);
            }

            UnityEditor.EditorUtility.SetDirty(canvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

            Debug.Log("[BuildHomeMapResearchDashboard] Successfully built Research Analytics Dashboard in HomeMapScene!");
        }

        private static Text BuildMetricCardBox(GameObject parent, string name, string header, Vector2 pos, Vector2 size, Font font)
        {
            var boxGo = new GameObject(name);
            boxGo.transform.SetParent(parent.transform, false);
            var bi = boxGo.AddComponent<UnityEngine.UI.Image>();
            bi.color = new Color(0.10f, 0.15f, 0.24f, 0.95f);
            var brt = boxGo.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.5f);
            brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = size;
            brt.anchoredPosition = pos;

            var bTitleGo = new GameObject("Header");
            bTitleGo.transform.SetParent(boxGo.transform, false);
            var bt = bTitleGo.AddComponent<UnityEngine.UI.Text>();
            bt.text = header;
            bt.font = font;
            bt.fontSize = 13;
            bt.fontStyle = FontStyle.Bold;
            bt.color = new Color(0.4f, 0.9f, 1f);
            var btrt = bTitleGo.GetComponent<RectTransform>();
            btrt.anchorMin = new Vector2(0f, 1f);
            btrt.anchorMax = new Vector2(1f, 1f);
            btrt.sizeDelta = new Vector2(-20, 24);
            btrt.anchoredPosition = new Vector2(10, -12);

            var bBodyGo = new GameObject("ContentText");
            bBodyGo.transform.SetParent(boxGo.transform, false);
            var outTxt = bBodyGo.AddComponent<UnityEngine.UI.Text>();
            outTxt.font = font;
            outTxt.fontSize = 12;
            outTxt.color = Color.white;
            outTxt.alignment = TextAnchor.UpperLeft;
            var bbrt = bBodyGo.GetComponent<RectTransform>();
            bbrt.anchorMin = Vector2.zero;
            bbrt.anchorMax = Vector2.one;
            bbrt.sizeDelta = new Vector2(-20, -36);
            bbrt.anchoredPosition = new Vector2(10, 6);

            return outTxt;
        }

        [MenuItem("LostTemple/Setup HomeMap Profile Modal UI")]
        public static void BuildHomeMapProfileModal()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_App/Scenes/HomeMapScene.unity");
            var canvas = GameObject.Find("Map Canvas");
            if (canvas == null) return;

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Ensure ProfileCard Button is configured
            var topHUD = canvas.transform.Find("TopHUD");
            var profileCard = topHUD != null ? topHUD.Find("ProfileCard") : canvas.transform.Find("ProfileCard");
            if (profileCard != null)
            {
                var btn = profileCard.GetComponent<UnityEngine.UI.Button>();
                if (btn == null) btn = profileCard.gameObject.AddComponent<UnityEngine.UI.Button>();
                var img = profileCard.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = true;
            }

            // Also make Card_PlayerRank clickable
            var rankCard = canvas.transform.Find("Card_PlayerRank");
            if (rankCard != null)
            {
                var rankBtn = rankCard.GetComponent<UnityEngine.UI.Button>();
                if (rankBtn == null) rankBtn = rankCard.gameObject.AddComponent<UnityEngine.UI.Button>();
            }

            // 2. Build Modal_ProfileDetails
            var oldModal = canvas.transform.Find("Modal_ProfileDetails");
            if (oldModal != null) GameObject.DestroyImmediate(oldModal.gameObject);

            var modalGo = new GameObject("Modal_ProfileDetails");
            modalGo.transform.SetParent(canvas.transform, false);
            var mRt = modalGo.AddComponent<RectTransform>();
            mRt.anchorMin = Vector2.zero;
            mRt.anchorMax = Vector2.one;
            mRt.sizeDelta = Vector2.zero;

            // Background Dim
            var dimGo = new GameObject("BackdropDim");
            dimGo.transform.SetParent(modalGo.transform, false);
            var dimImg = dimGo.AddComponent<UnityEngine.UI.Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.85f);
            var dimBtn = dimGo.AddComponent<UnityEngine.UI.Button>();
            var dimRt = dimGo.GetComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.sizeDelta = Vector2.zero;

            // Modal Card
            var cardGo = new GameObject("ModalCard");
            cardGo.transform.SetParent(modalGo.transform, false);
            var cardImg = cardGo.AddComponent<UnityEngine.UI.Image>();
            cardImg.color = new Color(0.08f, 0.12f, 0.20f, 0.98f);
            var cardRt = cardGo.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(620, 480);
            cardRt.anchoredPosition = Vector2.zero;

            // Header Title
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleTxt = titleGo.AddComponent<UnityEngine.UI.Text>();
            titleTxt.text = "📜 <b>EXPLORER PROFILE & STATS</b>";
            titleTxt.font = defaultFont;
            titleTxt.fontSize = 22;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1f, 0.88f, 0.3f);
            titleTxt.alignment = TextAnchor.MiddleCenter;
            var trt = titleGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 1f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.sizeDelta = new Vector2(-40, 40);
            trt.anchoredPosition = new Vector2(0, -20);

            // Avatar Image Frame
            var avatarFrameGo = new GameObject("AvatarFrame");
            avatarFrameGo.transform.SetParent(cardGo.transform, false);
            var afImg = avatarFrameGo.AddComponent<UnityEngine.UI.Image>();
            afImg.color = new Color(0.15f, 0.22f, 0.35f, 1f);
            var afrt = avatarFrameGo.GetComponent<RectTransform>();
            afrt.anchorMin = new Vector2(0f, 1f);
            afrt.anchorMax = new Vector2(0f, 1f);
            afrt.sizeDelta = new Vector2(110, 110);
            afrt.anchoredPosition = new Vector2(85, -135);

            var avatarImgGo = new GameObject("AvatarImage");
            avatarImgGo.transform.SetParent(avatarFrameGo.transform, false);
            var avatarImg = avatarImgGo.AddComponent<UnityEngine.UI.Image>();
            avatarImg.preserveAspect = true;
            var airt = avatarImgGo.GetComponent<RectTransform>();
            airt.anchorMin = Vector2.zero;
            airt.anchorMax = Vector2.one;
            airt.sizeDelta = new Vector2(-10, -10);

            // Player Name Text
            var nameGo = new GameObject("PlayerNameText");
            nameGo.transform.SetParent(cardGo.transform, false);
            var nameTxt = nameGo.AddComponent<UnityEngine.UI.Text>();
            nameTxt.text = "Brave Explorer";
            nameTxt.font = defaultFont;
            nameTxt.fontSize = 24;
            nameTxt.fontStyle = FontStyle.Bold;
            nameTxt.color = Color.white;
            nameTxt.alignment = TextAnchor.MiddleLeft;
            var nrt = nameGo.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0f, 1f);
            nrt.anchorMax = new Vector2(1f, 1f);
            nrt.sizeDelta = new Vector2(-220, 32);
            nrt.anchoredPosition = new Vector2(170, -95);

            // Player Email Text
            var emailGo = new GameObject("PlayerEmailText");
            emailGo.transform.SetParent(cardGo.transform, false);
            var emailTxt = emailGo.AddComponent<UnityEngine.UI.Text>();
            emailTxt.text = "explorer@losttemple.app";
            emailTxt.font = defaultFont;
            emailTxt.fontSize = 14;
            emailTxt.color = new Color(0.4f, 0.9f, 1f);
            emailTxt.alignment = TextAnchor.MiddleLeft;
            var ert = emailGo.GetComponent<RectTransform>();
            ert.anchorMin = new Vector2(0f, 1f);
            ert.anchorMax = new Vector2(1f, 1f);
            ert.sizeDelta = new Vector2(-220, 24);
            ert.anchoredPosition = new Vector2(170, -125);

            // Character Role Text
            var roleGo = new GameObject("CharacterRoleText");
            roleGo.transform.SetParent(cardGo.transform, false);
            var roleTxt = roleGo.AddComponent<UnityEngine.UI.Text>();
            roleTxt.text = "<b>MAYA</b> - Ancient Lore Scholar\n<color=#f5d77f>Specialty: Inscription Decoding & Spatial Archaeology</color>";
            roleTxt.font = defaultFont;
            roleTxt.fontSize = 14;
            roleTxt.color = new Color(0.95f, 0.85f, 0.4f);
            roleTxt.alignment = TextAnchor.MiddleLeft;
            var rrt = roleGo.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0f, 1f);
            rrt.anchorMax = new Vector2(1f, 1f);
            rrt.sizeDelta = new Vector2(-220, 38);
            rrt.anchoredPosition = new Vector2(170, -158);

            // Stats Box
            var statsBoxGo = new GameObject("StatsBox");
            statsBoxGo.transform.SetParent(cardGo.transform, false);
            var sbImg = statsBoxGo.AddComponent<UnityEngine.UI.Image>();
            sbImg.color = new Color(0.12f, 0.18f, 0.28f, 0.95f);
            var sbrt = statsBoxGo.GetComponent<RectTransform>();
            sbrt.anchorMin = new Vector2(0.5f, 0.5f);
            sbrt.anchorMax = new Vector2(0.5f, 0.5f);
            sbrt.sizeDelta = new Vector2(560, 160);
            sbrt.anchoredPosition = new Vector2(0, -50);

            var statsGo = new GameObject("StatsText");
            statsGo.transform.SetParent(statsBoxGo.transform, false);
            var statsTxt = statsGo.AddComponent<UnityEngine.UI.Text>();
            statsTxt.text = "• 🏛️ <b>AR Spatial Accuracy:</b> 98.4% (Temple Gates)\n• 🧠 <b>Adaptive Skill Tier:</b> 🎓 Apprentice (Tier 1)\n• 📊 <b>Assessment Mastery:</b> 100% (Level 1 Readiness)\n• 🪙 <b>Ancient Coins:</b> 240  |  🎒 <b>Relics:</b> 1/12";
            statsTxt.font = defaultFont;
            statsTxt.fontSize = 16;
            statsTxt.lineSpacing = 1.25f;
            statsTxt.color = Color.white;
            statsTxt.alignment = TextAnchor.UpperLeft;
            var stbrt = statsGo.GetComponent<RectTransform>();
            stbrt.anchorMin = Vector2.zero;
            stbrt.anchorMax = Vector2.one;
            stbrt.sizeDelta = new Vector2(-30, -20);
            stbrt.anchoredPosition = new Vector2(15, 0);

            // Close Button
            var clsBtnGo = new GameObject("Btn_Close");
            clsBtnGo.transform.SetParent(cardGo.transform, false);
            var clsImg = clsBtnGo.AddComponent<UnityEngine.UI.Image>();
            clsImg.color = new Color(0.85f, 0.65f, 0.15f, 1f);
            var clsBtn = clsBtnGo.AddComponent<UnityEngine.UI.Button>();
            var clsRt = clsBtnGo.GetComponent<RectTransform>();
            clsRt.anchorMin = new Vector2(0.5f, 0f);
            clsRt.anchorMax = new Vector2(0.5f, 0f);
            clsRt.sizeDelta = new Vector2(180, 46);
            clsRt.anchoredPosition = new Vector2(0, 32);

            var clstGo = new GameObject("Text");
            clstGo.transform.SetParent(clsBtnGo.transform, false);
            var clst = clstGo.AddComponent<UnityEngine.UI.Text>();
            clst.text = "Got It! 👍";
            clst.font = defaultFont;
            clst.fontSize = 18;
            clst.color = Color.black;
            clst.fontStyle = FontStyle.Bold;
            clst.alignment = TextAnchor.MiddleCenter;
            var clsTrt = clstGo.GetComponent<RectTransform>();
            clsTrt.anchorMin = Vector2.zero;
            clsTrt.anchorMax = Vector2.one;
            clsTrt.sizeDelta = Vector2.zero;

            modalGo.SetActive(false);

            // Bind references on HomeMapController
            var homeCtrl = canvas.GetComponentInChildren<SecretOfLostTemple.Home.HomeMapController>(true);
            if (homeCtrl != null)
            {
                var so = new UnityEditor.SerializedObject(homeCtrl);
                if (profileCard != null) so.FindProperty("profileCardButton").objectReferenceValue = profileCard.GetComponent<UnityEngine.UI.Button>();
                so.FindProperty("profileModalPanel").objectReferenceValue = modalGo;
                so.FindProperty("modalAvatarImage").objectReferenceValue = avatarImg;
                so.FindProperty("modalPlayerName").objectReferenceValue = nameTxt;
                so.FindProperty("modalPlayerEmail").objectReferenceValue = emailTxt;
                so.FindProperty("modalCharacterRole").objectReferenceValue = roleTxt;
                so.FindProperty("modalStatsText").objectReferenceValue = statsTxt;
                so.FindProperty("modalCloseButton").objectReferenceValue = clsBtn;
                so.FindProperty("modalBackgroundDismissButton").objectReferenceValue = dimBtn;
                so.ApplyModifiedProperties();
                UnityEditor.EditorUtility.SetDirty(homeCtrl);
            }

            UnityEditor.EditorUtility.SetDirty(canvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

            Debug.Log("[BuildHomeMapProfileModal] Successfully built Profile Details Modal in HomeMapScene!");
        }
    }
}

