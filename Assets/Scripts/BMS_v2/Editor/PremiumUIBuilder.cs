using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using BMS_v2;
using UnityEngine.Events;

namespace BMS_v2.Editor
{
    public class PremiumUIBuilder : EditorWindow
    {
        [MenuItem("BMS_v2 / 🚀 Generate FINAL Enterprise HUD (Side-by-Side)")]
        public static void GenerateUI()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Premium_Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            GameObject prefabBase = new GameObject("PremiumInfoRow");
            RectTransform prefabRect = prefabBase.AddComponent<RectTransform>();
            prefabRect.sizeDelta = new Vector2(280, 40); 
            HorizontalLayoutGroup hlg = prefabBase.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = false;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(prefabBase.transform, false);
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.fontSize = 14; labelText.color = new Color(0.75f, 0.75f, 0.78f); 
            labelText.alignment = TextAlignmentOptions.Left;

            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(prefabBase.transform, false);
            TextMeshProUGUI valText = valueObj.AddComponent<TextMeshProUGUI>();
            valText.fontSize = 14; valText.color = Color.white; 
            valText.fontStyle = FontStyles.Bold; valText.alignment = TextAlignmentOptions.Right;

            GameObject valueInputObj = new GameObject("Value Input");
            valueInputObj.transform.SetParent(prefabBase.transform, false);
            TMP_InputField inputField = valueInputObj.AddComponent<TMP_InputField>();
            valueInputObj.AddComponent<Image>().color = new Color(0.12f, 0.15f, 0.2f);
            
            LayoutElement vLay = valueInputObj.AddComponent<LayoutElement>();
            vLay.minWidth = 160; vLay.flexibleWidth = 1;

            GameObject inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(valueInputObj.transform, false);
            RectTransform txtRt = inputTextObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(10, 0); txtRt.offsetMax = new Vector2(-10, 0); 
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 14; inputText.color = new Color(0.2f, 0.9f, 1.0f); inputText.alignment = TextAlignmentOptions.Right;
            inputText.enableWordWrapping = false;
            inputField.textComponent = inputText; inputField.fontAsset = inputText.font;

            InfoRowUI rowScript = prefabBase.AddComponent<InfoRowUI>();
            rowScript.labelText = labelText; 
            rowScript.valueText = valText;
            rowScript.valueInput = inputField;

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabBase, "Assets/Prefabs/PremiumInfoRow.prefab");
            DestroyImmediate(prefabBase);

            GameObject topPanel = CreateSolidPanel("2. Top_DashboardPanel", canvas.transform, 
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -75), new Vector2(0, 0)); 
            
            GameObject borderLine = new GameObject("BottomBorder");
            borderLine.transform.SetParent(topPanel.transform, false);
            RectTransform borderRt = borderLine.AddComponent<RectTransform>();
            borderRt.anchorMin = new Vector2(0, 0); borderRt.anchorMax = new Vector2(1, 0);
            borderRt.anchoredPosition = new Vector2(0, 1);
            borderRt.sizeDelta = new Vector2(0, 2);
            borderLine.AddComponent<Image>().color = new Color(0.2f, 0.6f, 1.0f, 0.4f);

            HorizontalLayoutGroup topHlg = topPanel.AddComponent<HorizontalLayoutGroup>();
            topHlg.padding = new RectOffset(40, 40, 0, 0); 
            topHlg.spacing = 80;
            topHlg.childControlWidth = true; topHlg.childControlHeight = true;
            topHlg.childForceExpandWidth = false; topHlg.childForceExpandHeight = false;
            topHlg.childAlignment = TextAnchor.MiddleLeft; 

            CreateTopStatHorizontal(topPanel.transform, "INVESTMENT:", "0", new Color(0.2f, 1.0f, 0.6f));
            CreateFlexibleSpacer(topPanel.transform);
            CreateTopStatHorizontal(topPanel.transform, "UNITS:", "0", Color.white);
            CreateFlexibleSpacer(topPanel.transform);
            CreateTopStatHorizontal(topPanel.transform, "WARRANTY:", "0", new Color(1.0f, 0.4f, 0.4f));
            CreateFlexibleSpacer(topPanel.transform);
            CreateFlexibleSpacer(topPanel.transform); 
            
            GlobalDashboardManager dashScript = topPanel.AddComponent<GlobalDashboardManager>();
            TextMeshProUGUI[] allTexts = topPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach(var t in allTexts) {
                if(t.name.Contains("INVESTMENT:")) dashScript.totalInvestmentText = t;
                if(t.name.Contains("UNITS:")) dashScript.activeAssetsText = t;
                if(t.name.Contains("WARRANTY:")) dashScript.warrantyAlertsText = t;
            }
            
            topPanel.SetActive(true); 

            GameObject setBtnObj = new GameObject("Settings_Btn");
            setBtnObj.transform.SetParent(topPanel.transform, false);
            Image invisibleBg = setBtnObj.AddComponent<Image>();
            invisibleBg.color = new Color(0, 0, 0, 0); 
            LayoutElement setLay = setBtnObj.AddComponent<LayoutElement>();
            setLay.minWidth = 150; setLay.preferredWidth = 200;
            Button setBtn = setBtnObj.AddComponent<Button>();
            CreateText(setBtnObj.transform, "SYSTEM MENU", 18, new Color(0.85f, 0.9f, 1.0f)).alignment = TextAlignmentOptions.Center;
            Sprite btnRounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/RoundedCorner.png");
            if (btnRounded != null) { setBtnObj.GetComponent<Image>().sprite = btnRounded; setBtnObj.GetComponent<Image>().type = Image.Type.Sliced; }

            GameObject sumBtnObj = new GameObject("Summary_Btn");
            sumBtnObj.transform.SetParent(topPanel.transform, false);
            Image sumInvisibleBg = sumBtnObj.AddComponent<Image>();
            sumInvisibleBg.color = new Color(0, 0, 0, 0); 
            LayoutElement sumLay = sumBtnObj.AddComponent<LayoutElement>();
            sumLay.minWidth = 150; sumLay.preferredWidth = 200;
            Button sumBtn = sumBtnObj.AddComponent<Button>();
            CreateText(sumBtnObj.transform, "DATA SUMMARY", 18, new Color(0.9f, 0.7f, 0.2f)).alignment = TextAlignmentOptions.Center;
            if (btnRounded != null) { sumBtnObj.GetComponent<Image>().sprite = btnRounded; sumBtnObj.GetComponent<Image>().type = Image.Type.Sliced; }

            GameObject helpBtnObj = new GameObject("Help_Btn");
            helpBtnObj.transform.SetParent(topPanel.transform, false);
            Image hlpInvisibleBg = helpBtnObj.AddComponent<Image>();
            hlpInvisibleBg.color = new Color(0, 0, 0, 0); 
            LayoutElement hlpLay = helpBtnObj.AddComponent<LayoutElement>();
            hlpLay.minWidth = 150; hlpLay.preferredWidth = 180;
            Button helpBtn = helpBtnObj.AddComponent<Button>();
            CreateText(helpBtnObj.transform, "HELP/FAQ", 18, new Color(0.2f, 0.8f, 1.0f)).alignment = TextAlignmentOptions.Center;

            GameObject rightPanel = CreateSolidPanel("1. Right_AssetPanel", canvas.transform, 
                new Vector2(1, 0), new Vector2(1, 1), new Vector2(-300, 0), new Vector2(0, -75)); 
            AddVerticalLayout(rightPanel, 20, 12);
            
            GameObject closeBtnObj = new GameObject("Close Button");
            closeBtnObj.transform.SetParent(rightPanel.transform, false);
            LayoutElement closeLay = closeBtnObj.AddComponent<LayoutElement>();
            closeLay.ignoreLayout = true; 
            RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1); closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-15, -15);
            closeRect.sizeDelta = new Vector2(40, 40);
            Image xbg = closeBtnObj.AddComponent<Image>();
            xbg.color = new Color(0.85f, 0.2f, 0.25f, 1f); 
            if (btnRounded != null) { xbg.sprite = btnRounded; xbg.type = Image.Type.Sliced; }
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            CreateText(closeBtnObj.transform, "X", 22, Color.white).alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI titleTxt = CreateText(rightPanel.transform, "Appliance", 24, new Color(0.2f, 0.8f, 1.0f));
            TextMeshProUGUI subTxt = CreateText(rightPanel.transform, "ID: XXXX", 14, new Color(0.5f, 0.6f, 0.7f), false);
            AssetDetailPanel panelScript = rightPanel.AddComponent<AssetDetailPanel>();
            panelScript.infoRowPrefab = savedPrefab;
            panelScript.headerTitleText = titleTxt;
            panelScript.headerSubtitleText = subTxt;
            UnityAction closeAction = new UnityAction(panelScript.HidePanel);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(closeBtn.onClick, closeAction);
            
            CreateAccordionSection("IDENTITY", rightPanel.transform, panelScript, out Transform tab1);
            CreateAccordionSection("LIFECYCLE", rightPanel.transform, panelScript, out Transform tab5);
            CreateAccordionSection("FINANCIALS", rightPanel.transform, panelScript, out Transform tab2);
            CreateAccordionSection("WARRANTY", rightPanel.transform, panelScript, out Transform tab3);
            CreateAccordionSection("OPERATIONS", rightPanel.transform, panelScript, out Transform tab4);
            panelScript.allTabs = new GameObject[] { tab1.gameObject, tab5.gameObject, tab2.gameObject, tab3.gameObject, tab4.gameObject };
            panelScript.identityTabContent = tab1;
            panelScript.lifecycleTabContent = tab5; 
            panelScript.costTabContent = tab2;
            panelScript.warrantyTabContent = tab3;
            panelScript.taskTabContent = tab4; 

            GameObject editBtnObj = new GameObject("Edit Save Button");
            editBtnObj.transform.SetParent(rightPanel.transform, false);
            Image editImg = editBtnObj.AddComponent<Image>();
            editImg.color = new Color(0.12f, 0.55f, 0.95f, 1f); 
            if (btnRounded != null) { editImg.sprite = btnRounded; editImg.type = Image.Type.Sliced; }
            Button editBtn = editBtnObj.AddComponent<Button>();
            editBtnObj.AddComponent<LayoutElement>().minHeight = 60;
            TextMeshProUGUI editBtnTxt = CreateText(editBtnObj.transform, "EDIT", 15, Color.white);
            editBtnTxt.alignment = TextAlignmentOptions.Center;
            panelScript.editButtonText = editBtnTxt;
            editBtn.onClick.AddListener(panelScript.ToggleEditMode);
            UnityAction editAction = new UnityAction(panelScript.ToggleEditMode);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(editBtn.onClick, editAction);
            rightPanel.SetActive(false);

            GameObject leftPanelRoot = CreateSolidPanel("3. Left_OperationsPanel", canvas.transform, 
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(300, -75)); 
            GameObject scrollObj = new GameObject("SideScrollArea");
            scrollObj.transform.SetParent(leftPanelRoot.transform, false);
            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.vertical = true;
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            RectTransform vpRt = viewport.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one; vpRt.sizeDelta = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            sr.viewport = vpRt;
            GameObject leftPanelContent = new GameObject("Content");
            leftPanelContent.transform.SetParent(viewport.transform, false);
            RectTransform cntRt = leftPanelContent.AddComponent<RectTransform>();
            cntRt.anchorMin = new Vector2(0, 1); cntRt.anchorMax = Vector2.one;
            cntRt.pivot = new Vector2(0.5f, 1);
            leftPanelContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            AddVerticalLayout(leftPanelContent, 20, 15);
            sr.content = cntRt;
            LeftPanelManager leftManager = leftPanelRoot.AddComponent<LeftPanelManager>();
            CreateLeftAccordion("LOCAL SEARCH", leftPanelContent.transform, leftManager, out Transform searchTabContent);
            GameObject searchBoxObj = new GameObject("Search Input Field");
            searchBoxObj.transform.SetParent(searchTabContent, false);
            Image sBg = searchBoxObj.AddComponent<Image>();
            sBg.color = new Color(0.08f, 0.1f, 0.15f, 1f);
            if (btnRounded != null) { sBg.sprite = btnRounded; sBg.type = Image.Type.Sliced; }
            searchBoxObj.AddComponent<LayoutElement>().minHeight = 45;
            GameObject textAreaBox = new GameObject("Text Area");
            textAreaBox.transform.SetParent(searchBoxObj.transform, false);
            RectTransform textAreaRect = textAreaBox.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero; textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(15, 0); textAreaRect.offsetMax = new Vector2(-15, 0);
            TextMeshProUGUI placeholder = CreateText(textAreaBox.transform, "Search Model...", 14, new Color(0.4f, 0.4f, 0.5f), false);
            placeholder.alignment = TextAlignmentOptions.Left;
            TextMeshProUGUI inputTxt = CreateText(textAreaBox.transform, "", 14, Color.white, false);
            inputTxt.alignment = TextAlignmentOptions.Left;
            TMP_InputField searchInputField = searchBoxObj.AddComponent<TMP_InputField>();
            searchInputField.textViewport = textAreaRect;
            searchInputField.textComponent = inputTxt;
            searchInputField.placeholder = placeholder;
            leftManager.searchInput = searchInputField;
            GameObject resultsContainer = new GameObject("Search Results Container");
            resultsContainer.transform.SetParent(searchTabContent, false);
            VerticalLayoutGroup vlgRes = resultsContainer.AddComponent<VerticalLayoutGroup>();
            vlgRes.childControlWidth = true; vlgRes.childControlHeight = true; vlgRes.childForceExpandHeight = false; vlgRes.spacing = 8;
            leftManager.searchResultsContainer = resultsContainer.transform;
            CreateLeftAccordion("TASKS", leftPanelContent.transform, leftManager, out Transform taskTabContent);
            leftManager.taskResultsContainer = taskTabContent;
            leftManager.allAccordions = new GameObject[] { searchTabContent.gameObject, taskTabContent.gameObject };
            leftPanelRoot.SetActive(true);

            GameObject setPanel = CreateSolidPanel("4. Settings_Popup", canvas.transform, 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-280, -180), new Vector2(280, 180)); 
            AddVerticalLayout(setPanel, 35, 20);
            CreateText(setPanel.transform, "SYSTEM MENU", 32, new Color(1f, 0.9f, 0.2f)).alignment = TextAlignmentOptions.Center;
            GameObject resBtnObj = new GameObject("Resume_Btn");
            resBtnObj.transform.SetParent(setPanel.transform, false);
            Image resImg = resBtnObj.AddComponent<Image>();
            resImg.color = new Color(0.1f, 0.6f, 0.3f, 1f); if (btnRounded != null) { resImg.sprite = btnRounded; resImg.type = Image.Type.Sliced; }
            resBtnObj.AddComponent<LayoutElement>().minHeight = 65;
            Button resumeBtn = resBtnObj.AddComponent<Button>();
            CreateText(resBtnObj.transform, "RESUME OPERATIONS", 20, Color.white).alignment = TextAlignmentOptions.Center;
            GameObject quitBtnObj = new GameObject("Exit_Btn");
            quitBtnObj.transform.SetParent(setPanel.transform, false);
            Image qImg = quitBtnObj.AddComponent<Image>();
            qImg.color = new Color(0.8f, 0.25f, 0.25f, 1f); if (btnRounded != null) { qImg.sprite = btnRounded; qImg.type = Image.Type.Sliced; }
            quitBtnObj.AddComponent<LayoutElement>().minHeight = 65;
            Button quitBtn = quitBtnObj.AddComponent<Button>();
            CreateText(quitBtnObj.transform, "QUIT APPLICATION", 20, Color.white).alignment = TextAlignmentOptions.Center;
            SettingsManager setScript = setPanel.AddComponent<SettingsManager>();
            setScript.resumeBtn = resumeBtn; setScript.exitBtn = quitBtn;
            UnityAction openSets = new UnityAction(setScript.OpenSettings);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(setBtn.onClick, openSets);
            setPanel.SetActive(false);

            GameObject helpPanel = CreateSolidPanel("5. Help_Popup", canvas.transform, 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350, -250), new Vector2(350, 250)); 
            AddVerticalLayout(helpPanel, 30, 15);
            CreateText(helpPanel.transform, "SYSTEM CONTROLS (A-Z)", 28, new Color(1f, 0.9f, 0.2f)).alignment = TextAlignmentOptions.Center;
            GameObject scrollBoxHelp = new GameObject("FAQ_Content");
            scrollBoxHelp.transform.SetParent(helpPanel.transform, false);
            VerticalLayoutGroup svlg = scrollBoxHelp.AddComponent<VerticalLayoutGroup>();
            svlg.spacing = 12; svlg.childControlWidth = true; svlg.childControlHeight = true; svlg.childForceExpandHeight = false;
            CreateHelpItem(scrollBoxHelp.transform, "[ WASD ]", "Fly around and explore the map");
            CreateHelpItem(scrollBoxHelp.transform, "[ E / Q ]", "Move camera UP or DOWN physically");
            CreateHelpItem(scrollBoxHelp.transform, "[ RIGHT CLICK ]", "Hold and Drag to look around (360°)");
            GameObject hCloseBtnObj = new GameObject("HelpClose_Btn");
            hCloseBtnObj.transform.SetParent(helpPanel.transform, false);
            Image hImg = hCloseBtnObj.AddComponent<Image>();
            hImg.color = new Color(0.12f, 0.55f, 0.95f, 1f); if (btnRounded != null) { hImg.sprite = btnRounded; hImg.type = Image.Type.Sliced; }
            hCloseBtnObj.AddComponent<LayoutElement>().minHeight = 55;
            Button hCloseBtn = hCloseBtnObj.AddComponent<Button>();
            CreateText(hCloseBtnObj.transform, "GOT IT!", 18, Color.white).alignment = TextAlignmentOptions.Center;
            HelpManager helpScript = helpPanel.AddComponent<HelpManager>();
            helpScript.closeBtn = hCloseBtn;
            UnityAction openHelp = new UnityAction(helpScript.OpenHelp);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(helpBtn.onClick, openHelp);
            helpPanel.SetActive(false);

            GameObject summaryPopup = CreateSolidPanel("6. Summary_Popup", canvas.transform, 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-275, -350), new Vector2(275, 350)); 
            AddVerticalLayout(summaryPopup, 30, 10);
            CreateText(summaryPopup.transform, "TECHNICAL REPORT", 26, new Color(1f, 0.9f, 0.2f)).alignment = TextAlignmentOptions.Center;
            GameObject tableScroll = new GameObject("ReportScroll");
            tableScroll.transform.SetParent(summaryPopup.transform, false);
            ScrollRect smSr = tableScroll.AddComponent<ScrollRect>();
            smSr.horizontal = false; smSr.vertical = true;
            tableScroll.AddComponent<LayoutElement>().flexibleHeight = 1;
            GameObject smViewport = new GameObject("Viewport");
            smViewport.transform.SetParent(tableScroll.transform, false);
            RectTransform smVpRt = smViewport.AddComponent<RectTransform>();
            smVpRt.anchorMin = Vector2.zero; smVpRt.anchorMax = Vector2.one; smVpRt.sizeDelta = Vector2.zero;
            smViewport.AddComponent<Mask>().showMaskGraphic = false;
            smSr.viewport = smVpRt;
            GameObject smContent = new GameObject("ReportContent");
            smContent.transform.SetParent(smViewport.transform, false);
            RectTransform smCntRt = smContent.AddComponent<RectTransform>();
            smCntRt.anchorMin = new Vector2(0, 1); smCntRt.anchorMax = Vector2.one;
            smCntRt.pivot = new Vector2(0.5f, 1);
            smContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            AddVerticalLayout(smContent, 5, 2); 
            smSr.content = smCntRt;
            GameObject sCloseBtnObj = new GameObject("SummaryClose_Btn");
            sCloseBtnObj.transform.SetParent(summaryPopup.transform, false);
            Image sImg = sCloseBtnObj.AddComponent<Image>();
            sImg.color = new Color(0.12f, 0.55f, 0.95f, 1f); if (btnRounded != null) { sImg.sprite = btnRounded; sImg.type = Image.Type.Sliced; }
            sCloseBtnObj.AddComponent<LayoutElement>().minHeight = 55;
            Button sCloseBtn = sCloseBtnObj.AddComponent<Button>();
            CreateText(sCloseBtnObj.transform, "CLOSE REPORT", 16, Color.white).alignment = TextAlignmentOptions.Center;
            SummaryManager sumScript = summaryPopup.AddComponent<SummaryManager>();
            sumScript.summaryPanel = summaryPopup;
            sumScript.titleText = summaryPopup.GetComponentInChildren<TextMeshProUGUI>(); 
            sumScript.tableContent = smContent.transform;
            sumScript.closeBtn = sCloseBtn;
            string[] fontGuids = AssetDatabase.FindAssets("Mulish t:TMP_FontAsset");
            if (fontGuids.Length > 0) sumScript.mulishFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));
            sumBtn.onClick.AddListener(() => { sumScript.OpenSummary(); });
            UnityAction openSum = new UnityAction(sumScript.OpenSummary); 
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(sumBtn.onClick, openSum); 
            summaryPopup.SetActive(false);

            Selection.activeGameObject = canvas.gameObject;
        }

        [MenuItem("BMS_v2 / 🏷️ Create In-Game Room Label Template")]
        public static void CreateRoomLabelTemplate()
        {
            GameObject labelRoot = new GameObject("Room_Label_Template");
            Canvas canvas = labelRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform rt = labelRoot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 100);
            rt.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            labelRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;
            labelRoot.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Background");
            panel.transform.SetParent(labelRoot.transform, false);
            RectTransform pRt = panel.AddComponent<RectTransform>();
            pRt.anchorMin = Vector2.zero; pRt.anchorMax = Vector2.one; pRt.sizeDelta = Vector2.zero;
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.12f, 0.15f, 0.85f);
            Sprite rnd = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/RoundedCorner.png");
            if (rnd != null) { img.sprite = rnd; img.type = Image.Type.Sliced; }

            GameObject line = new GameObject("AccentLine");
            line.transform.SetParent(panel.transform, false);
            RectTransform lRt = line.AddComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0, 0); lRt.anchorMax = new Vector2(1, 0);
            lRt.anchoredPosition = new Vector2(0, 5); lRt.sizeDelta = new Vector2(-40, 4);
            line.AddComponent<Image>().color = new Color(0.2f, 0.8f, 1.0f, 1f);

            TextMeshProUGUI txt = CreateText(panel.transform, "ROOM NAME", 45, Color.white);
            txt.alignment = TextAlignmentOptions.Center;
            txt.rectTransform.offsetMin = new Vector2(20, 15);

            RoomLabel rl = labelRoot.AddComponent<RoomLabel>();
            rl.roomNameText = txt;
            Selection.activeGameObject = labelRoot;
            Debug.Log("🏷️ Room Label Template Created! Move it in 3D space and duplicate.");
        }

        [MenuItem("BMS_v2 / 🏷️ Auto-Spawn All Room Labels (Scene Wide)")]
        public static void AutoSpawnLabels()
        {
            int count = 0;

            
            BuildingZoneInfo[] zoneObjects = Object.FindObjectsByType<BuildingZoneInfo>(FindObjectsSortMode.None);
            foreach(var zo in zoneObjects)
            {
                if(zo.zoneId.ToUpper().Contains("RM") || zo.gameObject.name.ToUpper().Contains("ROOM"))
                {
                    GameObject lbl = CreateLabelInstance(zo.transform.position + Vector3.up * 2.5f, zo.zoneId);
                    lbl.transform.rotation = Quaternion.identity; 
                    count++;
                }
            }

            
            ThinAssetInfo[] assetObjects = Object.FindObjectsByType<ThinAssetInfo>(FindObjectsSortMode.None);
            foreach (var so in assetObjects)
            {
                string id = so.assetId.ToUpper();
                string objName = so.gameObject.name.ToUpper();
                if (id.Contains("RM") || id.Contains("ROOM") || objName.Contains("ROOM") || (objName.Contains("RM") && !objName.Contains("ARM")))
                {
                    GameObject lbl = CreateLabelInstance(so.transform.position + Vector3.up * 2.5f, so.assetId);
                    lbl.transform.rotation = Quaternion.identity; 
                    count++;
                }
            }

            if(count == 0)
            {
                Debug.LogWarning("⚠️ No Room objects detected. Ensure your ID contains 'RM' and the script is attached.");
            }

            Debug.Log($"✅ Successfully spawned {count} Room Labels above room points!");
        }

        private static GameObject CreateLabelInstance(Vector3 worldPos, string roomId)
        {
            GameObject labelRoot = new GameObject("RoomLabel_" + roomId);
            labelRoot.transform.position = worldPos;
            Canvas canvas = labelRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform rt = labelRoot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 100);
            rt.localScale = new Vector3(0.015f, 0.015f, 0.015f); 

            labelRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;
            labelRoot.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Background");
            panel.transform.SetParent(labelRoot.transform, false);
            RectTransform pRt = panel.AddComponent<RectTransform>();
            pRt.anchorMin = Vector2.zero; pRt.anchorMax = Vector2.one; pRt.sizeDelta = Vector2.zero;
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            Sprite rnd = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/RoundedCorner.png");
            if (rnd != null) { img.sprite = rnd; img.type = Image.Type.Sliced; }

            GameObject line = new GameObject("Accent");
            line.transform.SetParent(panel.transform, false);
            RectTransform lRt = line.AddComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.anchoredPosition = new Vector2(0, 5); lRt.sizeDelta = new Vector2(-40, 5);
            line.AddComponent<Image>().color = new Color(0.2f, 0.8f, 1.0f, 1f);

            TextMeshProUGUI txt = CreateText(panel.transform, roomId, 45, Color.white);
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableWordWrapping = false;
            txt.enableAutoSizing = true;
            txt.fontSizeMax = 45;
            txt.rectTransform.offsetMin = new Vector2(25, 20);

            labelRoot.AddComponent<RoomLabel>().roomNameText = txt;
            return labelRoot;
        }

        private static GameObject CreateSolidPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 posMin, Vector2 posMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = posMin; rt.offsetMax = posMax;
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.08f, 0.09f, 0.12f, 0.96f); 
            Sprite rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/RoundedCorner.png");
            if (rounded != null) { img.sprite = rounded; img.type = Image.Type.Sliced; }
            return obj;
        }

        private static TextMeshProUGUI CreateTopStatHorizontal(Transform parent, string title, string val, Color valColor)
        {
            GameObject statGrp = new GameObject("Stat_" + title);
            statGrp.transform.SetParent(parent, false);
            statGrp.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            HorizontalLayoutGroup hlg = statGrp.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft; hlg.spacing = 20; 
            TextMeshProUGUI tLbl = CreateText(statGrp.transform, title, 16, new Color(0.7f, 0.75f, 0.8f), true);
            TextMeshProUGUI tVal = CreateText(statGrp.transform, val, 20, valColor, true); 
            tVal.name = title;
            return tVal;
        }

        private static void CreateFlexibleSpacer(Transform parent)
        {
            GameObject spacer = new GameObject("Flexible_Spacer");
            spacer.transform.SetParent(parent, false);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1;
        }

        private static void CreateLeftAccordion(string name, Transform parent, LeftPanelManager panelScript, out Transform contentContainer)
        {
            GameObject wrapper = new GameObject(name + " Section");
            wrapper.transform.SetParent(parent, false);
            wrapper.AddComponent<VerticalLayoutGroup>().childControlWidth = true;
            GameObject btnObj = new GameObject(name + " Button");
            btnObj.transform.SetParent(wrapper.transform, false);
            Image bImg = btnObj.AddComponent<Image>();
            bImg.color = new Color(0.12f, 0.15f, 0.20f, 1f);
            Sprite rnd = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/RoundedCorner.png");
            if (rnd != null) { bImg.sprite = rnd; bImg.type = Image.Type.Sliced; }
            btnObj.AddComponent<Outline>().effectColor = new Color(1, 1, 1, 0.05f);
            Button btn = btnObj.AddComponent<Button>();
            btnObj.AddComponent<LayoutElement>().minHeight = 50;
            CreateText(btnObj.transform, name, 15, Color.white).alignment = TextAlignmentOptions.Center;
            GameObject contentObj = new GameObject(name + " Content Box");
            contentObj.transform.SetParent(wrapper.transform, false);
            contentObj.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 1f); 
            VerticalLayoutGroup vlgContent = contentObj.AddComponent<VerticalLayoutGroup>();
            vlgContent.padding = new RectOffset(15, 15, 15, 15); vlgContent.spacing = 10;
            contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UnityAction<GameObject> action = new UnityAction<GameObject>(panelScript.ToggleAccordion);
            btn.onClick.AddListener(() => { panelScript.ToggleAccordion(contentObj); });
            contentContainer = contentObj.transform;
            contentObj.SetActive(true);
        }

        private static void AddVerticalLayout(GameObject panel, int pad, int space)
        {
            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(pad, pad, pad, pad); vlg.spacing = space;
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string txt, int size, Color c, bool bold = true)
        {
            GameObject obj = new GameObject("Txt_" + txt);
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI t = obj.AddComponent<TextMeshProUGUI>();
            t.text = txt; t.fontSize = size; t.color = c;
            if (bold) t.fontStyle = FontStyles.Bold;
            string[] guids = AssetDatabase.FindAssets("Mulish t:TMP_FontAsset");
            if (guids.Length > 0) t.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return t;
        }

        private static void CreateAccordionSection(string name, Transform parent, AssetDetailPanel panelScript, out Transform contentContainer)
        {
            GameObject wrapper = new GameObject(name + " Section");
            wrapper.transform.SetParent(parent, false);
            wrapper.AddComponent<VerticalLayoutGroup>().childControlWidth = true;
            GameObject btnObj = new GameObject(name + " Button");
            btnObj.transform.SetParent(wrapper.transform, false);
            Image bImg = btnObj.AddComponent<Image>();
            bImg.color = new Color(0.15f, 0.18f, 0.22f, 1f);
            Sprite rnd = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/RoundedCorner.png");
            if (rnd != null) { bImg.sprite = rnd; bImg.type = Image.Type.Sliced; }
            btnObj.AddComponent<Outline>().effectColor = new Color(1, 1, 1, 0.05f);
            Button btn = btnObj.AddComponent<Button>();
            btnObj.AddComponent<LayoutElement>().minHeight = 45;
            CreateText(btnObj.transform, name, 15, Color.white).alignment = TextAlignmentOptions.Center;
            GameObject contentObj = new GameObject(name + " Content Box");
            contentObj.transform.SetParent(wrapper.transform, false);
            contentObj.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.12f, 1f); 
            VerticalLayoutGroup vlgContent = contentObj.AddComponent<VerticalLayoutGroup>();
            vlgContent.padding = new RectOffset(15, 15, 12, 12); vlgContent.spacing = 10;
            contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            btn.onClick.AddListener(() => { panelScript.ToggleTab(contentObj); });
            contentContainer = contentObj.transform;
        }

        private static void CreateHelpItem(Transform parent, string key, string desc)
        {
            GameObject g = new GameObject("HelpItem");
            g.transform.SetParent(parent, false);
            HorizontalLayoutGroup h = g.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 20; h.childAlignment = TextAnchor.MiddleLeft; h.childControlWidth = true;
            TextMeshProUGUI kt = CreateText(g.transform, key, 16, new Color(0.2f, 0.8f, 1.0f));
            kt.gameObject.AddComponent<LayoutElement>().minWidth = 150;
            CreateText(g.transform, desc, 14, Color.white, false);
        }
    }
}
