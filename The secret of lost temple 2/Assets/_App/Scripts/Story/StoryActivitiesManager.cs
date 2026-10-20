using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SecretOfLostTemple.Story
{
    public class StoryActivitiesManager : MonoBehaviour
    {
        public static StoryActivitiesManager Instance { get; private set; }

        [Header("State Tracking")]
        public const string PREF_BACKPACK_DONE = "LostTemple_Story_Backpack_Done";
        public const string PREF_LEVER_DONE = "LostTemple_Story_LeverAssembly_Done";

        public static bool IsBackpackCompleted => PlayerPrefs.GetInt(PREF_BACKPACK_DONE, 0) == 1;
        public static bool IsLeverAssemblyCompleted => PlayerPrefs.GetInt(PREF_LEVER_DONE, 0) == 1;

        private GameObject activityRootCanvas;
        private GameObject backpackPanel;
        private GameObject leverPanel;
        private GameObject celebrationPanel;

        private AudioSource audioSource;
        private AudioClip snapClip;
        private AudioClip victoryClip;

        private Font storyFont;
        private Sprite backpackSprite;
        private Sprite torchSprite;
        private Sprite mapSprite;
        private Sprite compassSprite;
        private Sprite fulcrumSprite;
        private Sprite beamSprite;
        private Sprite boulderSprite;
        private Sprite mayaSprite;
        private Sprite senukSprite;
        private Sprite bubbleSprite;
        private Sprite modalBgSprite;

        private Action onActivityCompletedCallback;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadAssets();
            CreateProceduralAudioClips();
        }

        private void LoadAssets()
        {
            storyFont = Resources.Load<Font>("MontserratFont") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            backpackSprite = Resources.Load<Sprite>("backpack_explorer");
            torchSprite = Resources.Load<Sprite>("item_torch");
            mapSprite = Resources.Load<Sprite>("item_map");
            compassSprite = Resources.Load<Sprite>("item_compass");
            fulcrumSprite = Resources.Load<Sprite>("item_fulcrum_stone");
            beamSprite = Resources.Load<Sprite>("item_wood_beam");
            boulderSprite = Resources.Load<Sprite>("item_boulder_stone");

            mayaSprite = Resources.Load<Sprite>("char_maya_full");
            senukSprite = Resources.Load<Sprite>("char_senuk_full");
            bubbleSprite = Resources.Load<Sprite>("dialog_bubble_frame");
            modalBgSprite = Resources.Load<Sprite>("adventure_quest_modal_bg") ?? Resources.Load<Sprite>("bg_ancient_lever_storybook");

            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void CreateProceduralAudioClips()
        {
            // Crisp snap chime (ding)
            snapClip = CreateToneClip("SnapDing", new float[] { 587.33f, 880f }, 0.22f);
            // Rich victory fanfare
            victoryClip = CreateToneClip("VictoryFanfare", new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.55f);
        }

        private AudioClip CreateToneClip(string clipName, float[] frequencies, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] data = new float[totalSamples];
            int noteLength = totalSamples / frequencies.Length;

            for (int i = 0; i < totalSamples; i++)
            {
                int noteIndex = Mathf.Clamp(i / noteLength, 0, frequencies.Length - 1);
                float freq = frequencies[noteIndex];
                float t = (float)(i % noteLength) / sampleRate;
                float envelope = Mathf.Exp(-4.5f * ((float)(i % noteLength) / noteLength));
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.45f;
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        // ==========================================
        // ACTIVITY 1: PACK THE BACKPACK (PAGE 2 -> 3)
        // ==========================================
        public void OpenBackpackActivity(Action onCompleted = null)
        {
            onActivityCompletedCallback = onCompleted;
            EnsureCanvasCreated();

            if (leverPanel != null) leverPanel.SetActive(false);
            if (celebrationPanel != null) celebrationPanel.SetActive(false);

            if (backpackPanel == null)
            {
                backpackPanel = BuildBackpackPanel(activityRootCanvas.transform);
            }

            backpackPanel.SetActive(true);
            activityRootCanvas.SetActive(true);

            ResetItemsInPanel(backpackPanel);
        }

        private GameObject BuildBackpackPanel(Transform parent)
        {
            GameObject panel = CreateUIObject("BackpackActivityPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            StretchFull(rt);

            // Dark semi-transparent scrim
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.04f, 0.05f, 0.94f);

            // Header Container
            GameObject header = CreateUIObject("Header", panel.transform);
            RectTransform headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = new Vector2(0f, -25f);
            headerRt.sizeDelta = new Vector2(0f, 100f);

            Text titleTxt = CreateText(header.transform, "Title", "🎒 <b>MINI-QUEST: PACK THE BACKPACK</b>", 34, new Color(1f, 0.88f, 0.4f));
            titleTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform titleRt = titleTxt.GetComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 15f);
            titleRt.sizeDelta = new Vector2(1200f, 50f);

            Text subTxt = CreateText(header.transform, "Subtitle", "Drag the 3 essential exploration items into the backpack pockets!", 22, new Color(0.9f, 0.9f, 0.9f));
            subTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform subRt = subTxt.GetComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0f, -25f);
            subRt.sizeDelta = new Vector2(1200f, 40f);

            // Close Button in top right
            Button closeBtn = CreateButton(panel.transform, "CloseBtn", "✕", new Vector2(60f, 60f), () => CloseActivity());
            RectTransform closeRt = closeBtn.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-40f, -30f);

            // Center: Backpack Visual
            GameObject backpackCenter = CreateUIObject("BackpackCenter", panel.transform);
            RectTransform bpRt = backpackCenter.GetComponent<RectTransform>();
            bpRt.anchoredPosition = new Vector2(0f, 40f);
            bpRt.sizeDelta = new Vector2(280f, 280f);
            Image bpImg = backpackCenter.AddComponent<Image>();
            bpImg.sprite = backpackSprite;
            bpImg.preserveAspect = true;

            // Target Drop Slots: Left (Torch), Center-Top (Map), Right (Compass)
            StoryDropSlot slotTorch = CreateDropSlot(panel.transform, "Slot_Torch", "torch", new Vector2(-280f, 40f), new Vector2(140f, 140f), torchSprite, "1. TORCH\n(For Light)");
            StoryDropSlot slotMap = CreateDropSlot(panel.transform, "Slot_Map", "map", new Vector2(0f, 195f), new Vector2(140f, 140f), mapSprite, "2. TEMPLE MAP\n(For Path)");
            StoryDropSlot slotCompass = CreateDropSlot(panel.transform, "Slot_Compass", "compass", new Vector2(280f, 40f), new Vector2(140f, 140f), compassSprite, "3. COMPASS\n(For Direction)");

            // Bottom Shelf Tray
            GameObject shelf = CreateUIObject("ItemShelf", panel.transform);
            RectTransform shelfRt = shelf.GetComponent<RectTransform>();
            shelfRt.anchorMin = new Vector2(0.5f, 0f);
            shelfRt.anchorMax = new Vector2(0.5f, 0f);
            shelfRt.pivot = new Vector2(0.5f, 0.5f);
            shelfRt.anchoredPosition = new Vector2(0f, 105f);
            shelfRt.sizeDelta = new Vector2(980f, 150f);

            Image shelfImg = shelf.AddComponent<Image>();
            shelfImg.color = new Color(0.12f, 0.09f, 0.05f, 0.88f);
            var shelfOutline = shelf.AddComponent<Outline>();
            shelfOutline.effectColor = new Color(0.75f, 0.6f, 0.25f, 0.8f);
            shelfOutline.effectDistance = new Vector2(3f, -3f);

            Text shelfLabel = CreateText(shelf.transform, "ShelfLabel", "<b>EXPEDITION GEAR - DRAG TO PACK:</b>", 18, new Color(1f, 0.85f, 0.5f));
            shelfLabel.alignment = TextAnchor.UpperCenter;
            RectTransform slRt = shelfLabel.GetComponent<RectTransform>();
            slRt.anchorMin = new Vector2(0.5f, 1f);
            slRt.anchorMax = new Vector2(0.5f, 1f);
            slRt.pivot = new Vector2(0.5f, 1f);
            slRt.anchoredPosition = new Vector2(0f, -8f);
            slRt.sizeDelta = new Vector2(600f, 26f);

            // Draggable Items
            CreateDragItem(shelf.transform, "Drag_Torch", "torch", slotTorch, new Vector2(-280f, -8f), new Vector2(100f, 100f), torchSprite, "Torch");
            CreateDragItem(shelf.transform, "Drag_Map", "map", slotMap, new Vector2(0f, -8f), new Vector2(100f, 100f), mapSprite, "Map");
            CreateDragItem(shelf.transform, "Drag_Compass", "compass", slotCompass, new Vector2(280f, -8f), new Vector2(100f, 100f), compassSprite, "Compass");

            return panel;
        }

        // ==========================================
        // ACTIVITY 2: ASSEMBLE THE LEVER (PAGE 7 -> 8)
        // ==========================================
        public void OpenLeverAssemblyActivity(Action onCompleted = null)
        {
            onActivityCompletedCallback = onCompleted;
            EnsureCanvasCreated();

            if (backpackPanel != null) backpackPanel.SetActive(false);
            if (celebrationPanel != null) celebrationPanel.SetActive(false);

            if (leverPanel == null)
            {
                leverPanel = BuildLeverAssemblyPanel(activityRootCanvas.transform);
            }

            leverPanel.SetActive(true);
            activityRootCanvas.SetActive(true);

            ResetItemsInPanel(leverPanel);
        }

        private GameObject BuildLeverAssemblyPanel(Transform parent)
        {
            GameObject panel = CreateUIObject("LeverAssemblyPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            StretchFull(rt);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.04f, 0.05f, 0.94f);

            // Header Container
            GameObject header = CreateUIObject("Header", panel.transform);
            RectTransform headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = new Vector2(0f, -25f);
            headerRt.sizeDelta = new Vector2(0f, 100f);

            Text titleTxt = CreateText(header.transform, "Title", "⚙️ <b>MINI-QUEST: ASSEMBLE THE TEMPLE LEVER</b>", 34, new Color(1f, 0.88f, 0.4f));
            titleTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform titleRt = titleTxt.GetComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 15f);
            titleRt.sizeDelta = new Vector2(1200f, 50f);

            Text subTxt = CreateText(header.transform, "Subtitle", "Drag the 3 parts into the blueprint slots to build the ancient lever mechanism!", 22, new Color(0.9f, 0.9f, 0.9f));
            subTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform subRt = subTxt.GetComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0f, -25f);
            subRt.sizeDelta = new Vector2(1200f, 40f);

            Button closeBtn = CreateButton(panel.transform, "CloseBtn", "✕", new Vector2(60f, 60f), () => CloseActivity());
            RectTransform closeRt = closeBtn.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-40f, -30f);

            // Blueprint Schematic Area
            GameObject blueprint = CreateUIObject("BlueprintArea", panel.transform);
            RectTransform bpRt = blueprint.GetComponent<RectTransform>();
            bpRt.anchoredPosition = new Vector2(0f, 55f);
            bpRt.sizeDelta = new Vector2(950f, 320f);

            Image bpImg = blueprint.AddComponent<Image>();
            bpImg.color = new Color(0.08f, 0.12f, 0.16f, 0.85f);
            var bpOutline = blueprint.AddComponent<Outline>();
            bpOutline.effectColor = new Color(0.3f, 0.65f, 0.9f, 0.6f);
            bpOutline.effectDistance = new Vector2(2f, -2f);

            // Blueprint grid label
            Text bpLabel = CreateText(blueprint.transform, "BlueprintLabel", "📐 <b>CLASS 1 LEVER SCHEMATIC (LOAD — FULCRUM — EFFORT)</b>", 17, new Color(0.4f, 0.8f, 1f, 0.85f));
            bpLabel.alignment = TextAnchor.UpperCenter;
            RectTransform bplRt = bpLabel.GetComponent<RectTransform>();
            bplRt.anchorMin = new Vector2(0.5f, 1f);
            bplRt.anchorMax = new Vector2(0.5f, 1f);
            bplRt.pivot = new Vector2(0.5f, 1f);
            bplRt.anchoredPosition = new Vector2(0f, -12f);
            bplRt.sizeDelta = new Vector2(800f, 30f);

            // Effort arrow guide on the right side
            Text effortArrow = CreateText(blueprint.transform, "EffortGuide", "⬇️ <b>APPLY EFFORT HERE</b>", 16, new Color(1f, 0.9f, 0.3f, 0.85f));
            effortArrow.alignment = TextAnchor.MiddleCenter;
            RectTransform eaRt = effortArrow.GetComponent<RectTransform>();
            eaRt.anchoredPosition = new Vector2(270f, 75f);
            eaRt.sizeDelta = new Vector2(240f, 30f);

            // 3 Drop Slots on Blueprint
            // 1. Fulcrum Stone (Bottom center: 0, -45)
            StoryDropSlot slotFulcrum = CreateDropSlot(blueprint.transform, "Slot_Fulcrum", "fulcrum", new Vector2(0f, -50f), new Vector2(140f, 120f), fulcrumSprite, "1. FULCRUM\n(Pivot Stone)");
            // 2. Lever Beam (Horizontal: 0, 15)
            StoryDropSlot slotBeam = CreateDropSlot(blueprint.transform, "Slot_Beam", "beam", new Vector2(0f, 15f), new Vector2(560f, 65f), beamSprite, "2. LEVER BEAM (Timber)");
            // 3. Boulder Load (Left side: -240, 75)
            StoryDropSlot slotBoulder = CreateDropSlot(blueprint.transform, "Slot_Boulder", "boulder", new Vector2(-235f, 75f), new Vector2(130f, 130f), boulderSprite, "3. LOAD\n(Boulder)");

            // Bottom Shelf Tray
            GameObject shelf = CreateUIObject("ItemShelf", panel.transform);
            RectTransform shelfRt = shelf.GetComponent<RectTransform>();
            shelfRt.anchorMin = new Vector2(0.5f, 0f);
            shelfRt.anchorMax = new Vector2(0.5f, 0f);
            shelfRt.pivot = new Vector2(0.5f, 0.5f);
            shelfRt.anchoredPosition = new Vector2(0f, 105f);
            shelfRt.sizeDelta = new Vector2(980f, 150f);

            Image shelfImg = shelf.AddComponent<Image>();
            shelfImg.color = new Color(0.12f, 0.09f, 0.05f, 0.88f);
            var shelfOutline = shelf.AddComponent<Outline>();
            shelfOutline.effectColor = new Color(0.75f, 0.6f, 0.25f, 0.8f);
            shelfOutline.effectDistance = new Vector2(3f, -3f);

            Text shelfLabel = CreateText(shelf.transform, "ShelfLabel", "<b>LEVER PARTS - DRAG INTO PLACE:</b>", 18, new Color(1f, 0.85f, 0.5f));
            shelfLabel.alignment = TextAnchor.UpperCenter;
            RectTransform slRt = shelfLabel.GetComponent<RectTransform>();
            slRt.anchorMin = new Vector2(0.5f, 1f);
            slRt.anchorMax = new Vector2(0.5f, 1f);
            slRt.pivot = new Vector2(0.5f, 1f);
            slRt.anchoredPosition = new Vector2(0f, -8f);
            slRt.sizeDelta = new Vector2(600f, 26f);

            // Draggable Parts on shelf
            CreateDragItem(shelf.transform, "Drag_Fulcrum", "fulcrum", slotFulcrum, new Vector2(-290f, -8f), new Vector2(110f, 95f), fulcrumSprite, "Fulcrum Stone");
            CreateDragItem(shelf.transform, "Drag_Beam", "beam", slotBeam, new Vector2(0f, -8f), new Vector2(250f, 55f), beamSprite, "Wooden Beam");
            CreateDragItem(shelf.transform, "Drag_Boulder", "boulder", slotBoulder, new Vector2(290f, -8f), new Vector2(100f, 100f), boulderSprite, "Heavy Boulder");

            return panel;
        }

        // ==========================================
        // UI HELPERS
        // ==========================================
        private void EnsureCanvasCreated()
        {
            if (activityRootCanvas != null) return;

            Transform storyRoot = StoryBookViewerController.Instance != null ? StoryBookViewerController.Instance.transform : transform;
            activityRootCanvas = CreateUIObject("StoryActivityModalRoot", storyRoot);
            RectTransform rt = activityRootCanvas.GetComponent<RectTransform>();
            StretchFull(rt);

            Canvas canvas = activityRootCanvas.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500; // Above storybook pages and bars

            activityRootCanvas.AddComponent<GraphicRaycaster>();
        }

        private StoryDropSlot CreateDropSlot(Transform parent, string name, string id, Vector2 pos, Vector2 size, Sprite icon, string label)
        {
            GameObject slotObj = CreateUIObject(name, parent);
            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // Semi-transparent dark hollow slot background
            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = new Color(0.06f, 0.08f, 0.12f, 0.5f);

            Outline outline = slotObj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.3f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Icon Silhouette
            GameObject iconObj = CreateUIObject("IconSilhouette", slotObj.transform);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            StretchFull(iconRt);
            iconRt.offsetMin = new Vector2(6f, 6f);
            iconRt.offsetMax = new Vector2(-6f, -6f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.color = new Color(1f, 1f, 1f, 0.32f);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Text Label
            Text txt = null;
            if (!string.IsNullOrEmpty(label))
            {
                txt = CreateText(slotObj.transform, "Label", $"<b>{label}</b>", 13, new Color(1f, 0.95f, 0.8f));
                txt.alignment = TextAnchor.MiddleCenter;
                RectTransform txtRt = txt.GetComponent<RectTransform>();
                txtRt.anchorMin = new Vector2(0.5f, 0f);
                txtRt.anchorMax = new Vector2(0.5f, 0f);
                txtRt.pivot = new Vector2(0.5f, 1f);
                txtRt.anchoredPosition = new Vector2(0f, -5f);
                txtRt.sizeDelta = new Vector2(180f, 36f);
            }

            StoryDropSlot dropSlot = slotObj.AddComponent<StoryDropSlot>();
            dropSlot.slotId = id;
            dropSlot.slotBackground = slotBg;
            dropSlot.slotOutline = outline;
            dropSlot.slotIcon = iconImg;
            dropSlot.slotLabel = txt;

            return dropSlot;
        }

        private StoryDragItem CreateDragItem(Transform parent, string name, string id, StoryDropSlot slot, Vector2 pos, Vector2 size, Sprite sprite, string label)
        {
            GameObject itemObj = CreateUIObject(name, parent);
            RectTransform rt = itemObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = itemObj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = true;

            Shadow shadow = itemObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(3f, -3f);

            // Label under item
            if (!string.IsNullOrEmpty(label))
            {
                Text txt = CreateText(itemObj.transform, "Name", $"<b>{label}</b>", 14, new Color(1f, 0.92f, 0.7f));
                txt.alignment = TextAnchor.MiddleCenter;
                RectTransform txtRt = txt.GetComponent<RectTransform>();
                txtRt.anchorMin = new Vector2(0.5f, 0f);
                txtRt.anchorMax = new Vector2(0.5f, 0f);
                txtRt.pivot = new Vector2(0.5f, 1f);
                txtRt.anchoredPosition = new Vector2(0f, -3f);
                txtRt.sizeDelta = new Vector2(160f, 25f);
            }

            StoryDragItem dragItem = itemObj.AddComponent<StoryDragItem>();
            dragItem.itemId = id;
            dragItem.targetSlot = slot;
            dragItem.SetOriginalConfig(parent, pos, size);

            dragItem.OnItemSnapped += OnItemSnapped;

            return dragItem;
        }

        private void OnItemSnapped(StoryDragItem item, StoryDropSlot slot)
        {
            PlaySound(snapClip);
            CheckAllItemsSnapped();
        }

        private void CheckAllItemsSnapped()
        {
            GameObject currentActivePanel = (backpackPanel != null && backpackPanel.activeSelf) ? backpackPanel : leverPanel;
            if (currentActivePanel == null) return;

            StoryDropSlot[] slots = currentActivePanel.GetComponentsInChildren<StoryDropSlot>(true);
            if (slots.Length == 0) return;

            bool allOccupied = true;
            foreach (var s in slots)
            {
                if (!s.IsOccupied)
                {
                    allOccupied = false;
                    break;
                }
            }

            if (allOccupied)
            {
                PlaySound(victoryClip);
                if (currentActivePanel == backpackPanel)
                {
                    PlayerPrefs.SetInt(PREF_BACKPACK_DONE, 1);
                    PlayerPrefs.Save();
                    ShowCelebration(
                        "🎒 BACKPACK FULLY PACKED!",
                        "Awesome explorer! Our backpack is packed with a torch for light, a map for guidance, and a compass for direction! We are fully ready to enter the secret temple!",
                        "CONTINUE STORY (PAGE 3) ➔",
                        () =>
                        {
                            CloseActivity();
                            onActivityCompletedCallback?.Invoke();
                            if (StoryBookViewerController.Instance != null)
                            {
                                StoryBookViewerController.Instance.GoToPage(2); // Page 3 is index 2
                            }
                        }
                    );
                }
                else
                {
                    PlayerPrefs.SetInt(PREF_LEVER_DONE, 1);
                    PlayerPrefs.Save();
                    ShowCelebration(
                        "⚙️ LEVER ASSEMBLED!",
                        "Magnificent! The Class 1 Lever is fully assembled! Notice how placing the fulcrum near the load lets our small effort lift the giant boulder! Now turn to Page 9 to scan the printed marker and awaken the 3D machine!",
                        "CONTINUE TO PAGE 8 ➔",
                        () =>
                        {
                            CloseActivity();
                            onActivityCompletedCallback?.Invoke();
                            if (StoryBookViewerController.Instance != null)
                            {
                                StoryBookViewerController.Instance.GoToPage(7); // Page 8 is index 7
                            }
                        }
                    );
                }
            }
        }

        private void ShowCelebration(string title, string speech, string btnLabel, Action onContinue)
        {
            EnsureCanvasCreated();
            if (celebrationPanel == null)
            {
                celebrationPanel = BuildCelebrationPanel(activityRootCanvas.transform);
            }

            // Bring celebration panel to absolute top of hierarchy
            celebrationPanel.transform.SetAsLastSibling();

            // Update dialogue text
            Text titleText = celebrationPanel.transform.Find("Card/Title")?.GetComponent<Text>();
            if (titleText != null) titleText.text = $"<b>{title}</b>";

            Text speechText = celebrationPanel.transform.Find("Card/SpeechBubble/Text")?.GetComponent<Text>();
            if (speechText != null) speechText.text = speech;

            Button nextBtn = celebrationPanel.transform.Find("Card/ContinueBtn")?.GetComponent<Button>();
            if (nextBtn != null)
            {
                nextBtn.onClick.RemoveAllListeners();
                nextBtn.onClick.AddListener(() => onContinue?.Invoke());
                Text btnTxt = nextBtn.GetComponentInChildren<Text>();
                if (btnTxt != null) btnTxt.text = $"<b>{btnLabel}</b>";
            }

            // Set character portrait according to selected character
            Image charImg = celebrationPanel.transform.Find("Card/CharacterPortrait")?.GetComponent<Image>();
            if (charImg != null)
            {
                string charKey = PlayerPrefs.GetString("LostTemple_Selected_Character", "Maya");
                charImg.sprite = (charKey == "Senuk" && senukSprite != null) ? senukSprite : (mayaSprite ?? senukSprite);
            }

            celebrationPanel.SetActive(true);
        }

        private GameObject BuildCelebrationPanel(Transform parent)
        {
            GameObject overlay = CreateUIObject("CelebrationOverlay", parent);
            RectTransform ovRt = overlay.GetComponent<RectTransform>();
            StretchFull(ovRt);

            Image ovImg = overlay.AddComponent<Image>();
            ovImg.color = new Color(0f, 0f, 0f, 0.85f);

            // Card Panel
            GameObject card = CreateUIObject("Card", overlay.transform);
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchoredPosition = new Vector2(0f, 0f);
            cardRt.sizeDelta = new Vector2(850f, 440f);

            Image cardImg = card.AddComponent<Image>();
            if (modalBgSprite != null)
            {
                cardImg.sprite = modalBgSprite;
                cardImg.type = Image.Type.Sliced;
            }
            cardImg.color = new Color(0.12f, 0.1f, 0.08f, 0.98f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.95f, 0.78f, 0.3f, 1f);
            cardOutline.effectDistance = new Vector2(4f, -4f);

            // Title
            Text titleTxt = CreateText(card.transform, "Title", "<b>VICTORY!</b>", 28, new Color(1f, 0.88f, 0.4f));
            titleTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform titleRt = titleTxt.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -25f);
            titleRt.sizeDelta = new Vector2(800f, 40f);

            // Character Portrait on the left
            GameObject charObj = CreateUIObject("CharacterPortrait", card.transform);
            RectTransform charRt = charObj.GetComponent<RectTransform>();
            charRt.anchorMin = new Vector2(0f, 0.5f);
            charRt.anchorMax = new Vector2(0f, 0.5f);
            charRt.pivot = new Vector2(0.5f, 0.5f);
            charRt.anchoredPosition = new Vector2(145f, -15f);
            charRt.sizeDelta = new Vector2(220f, 290f);
            Image charImg = charObj.AddComponent<Image>();
            charImg.preserveAspect = true;

            // Speech Bubble Frame
            GameObject bubble = CreateUIObject("SpeechBubble", card.transform);
            RectTransform bubbleRt = bubble.GetComponent<RectTransform>();
            bubbleRt.anchorMin = new Vector2(1f, 0.5f);
            bubbleRt.anchorMax = new Vector2(1f, 0.5f);
            bubbleRt.pivot = new Vector2(1f, 0.5f);
            bubbleRt.anchoredPosition = new Vector2(-40f, 25f);
            bubbleRt.sizeDelta = new Vector2(530f, 200f);

            Image bubbleImg = bubble.AddComponent<Image>();
            if (bubbleSprite != null)
            {
                bubbleImg.sprite = bubbleSprite;
                bubbleImg.type = Image.Type.Sliced;
            }
            bubbleImg.color = new Color(0.18f, 0.15f, 0.1f, 0.95f);

            Outline bubbleOutline = bubble.AddComponent<Outline>();
            bubbleOutline.effectColor = new Color(0.85f, 0.7f, 0.3f, 0.8f);
            bubbleOutline.effectDistance = new Vector2(2f, -2f);

            // Speech Text inside Bubble
            Text speechTxt = CreateText(bubble.transform, "Text", "Great job!", 21, new Color(1f, 0.96f, 0.88f));
            speechTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform stRt = speechTxt.GetComponent<RectTransform>();
            StretchFull(stRt);
            stRt.offsetMin = new Vector2(25f, 15f);
            stRt.offsetMax = new Vector2(-25f, -15f);

            // Continue Button
            Button contBtn = CreateButton(card.transform, "ContinueBtn", "CONTINUE STORY ➔", new Vector2(340f, 65f), null);
            RectTransform cbRt = contBtn.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(1f, 0f);
            cbRt.anchorMax = new Vector2(1f, 0f);
            cbRt.pivot = new Vector2(1f, 0f);
            cbRt.anchoredPosition = new Vector2(-40f, 35f);

            Image contImg = contBtn.GetComponent<Image>();
            contImg.color = new Color(0.2f, 0.65f, 0.28f, 1f);
            Outline cbOutline = contBtn.gameObject.AddComponent<Outline>();
            cbOutline.effectColor = new Color(1f, 0.9f, 0.4f, 0.9f);
            cbOutline.effectDistance = new Vector2(2f, -2f);

            Text cbTxt = contBtn.GetComponentInChildren<Text>();
            cbTxt.fontSize = 20;
            cbTxt.color = Color.white;

            return overlay;
        }

        private void ResetItemsInPanel(GameObject panel)
        {
            StoryDropSlot[] slots = panel.GetComponentsInChildren<StoryDropSlot>(true);
            foreach (var s in slots) s.ResetSlot();

            StoryDragItem[] items = panel.GetComponentsInChildren<StoryDragItem>(true);
            foreach (var i in items) i.ResetItem();
        }

        public void CloseActivity()
        {
            if (activityRootCanvas != null)
            {
                activityRootCanvas.SetActive(false);
            }
            if (celebrationPanel != null)
            {
                celebrationPanel.SetActive(false);
            }
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, Color color)
        {
            GameObject obj = CreateUIObject(name, parent);
            Text txt = obj.AddComponent<Text>();
            txt.font = storyFont;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.text = content;
            txt.raycastTarget = false;

            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            return txt;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 size, Action onClick)
        {
            GameObject obj = CreateUIObject(name, parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.18f, 0.14f, 0.08f, 0.95f);

            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.7f, 0.3f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            Button btn = obj.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick.Invoke());

            Text txt = CreateText(obj.transform, "Label", $"<b>{label}</b>", 20, new Color(1f, 0.9f, 0.7f));
            txt.alignment = TextAnchor.MiddleCenter;
            RectTransform txtRt = txt.GetComponent<RectTransform>();
            StretchFull(txtRt);

            return btn;
        }
    }
}
