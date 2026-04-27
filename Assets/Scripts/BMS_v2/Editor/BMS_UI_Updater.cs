using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using BMS_v2;

public class BMS_UI_Updater : EditorWindow
{
    private static Canvas GetMainCanvas()
    {
        AssetDetailPanel panel = Object.FindAnyObjectByType<AssetDetailPanel>(FindObjectsInactive.Include);
        if (panel != null)
        {
            Canvas parentCanvas = panel.GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.isRootCanvas) 
                return parentCanvas;
            else if (parentCanvas != null)
                return parentCanvas.rootCanvas;
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.gameObject.activeInHierarchy)
                return c;
        }
        return canvases.Length > 0 ? canvases[0] : null;
    }

    [MenuItem("BMS Tools/Update Right Panel (Gallery UI)")]
    public static void UpdateRightPanelUI()
    {
        AssetDetailPanel panel = Object.FindAnyObjectByType<AssetDetailPanel>(FindObjectsInactive.Include);
        if (panel == null)
        {
            Debug.LogError("AssetDetailPanel not found in the scene! Please open the scene containing it.");
            return;
        }

        Undo.RecordObject(panel, "Update AssetDetailPanel UI");

        // 1. DUPICATE TASK TAB to MAKE GALLERY TAB
        if (panel.galleryTabContent == null && panel.taskTabContent != null)
        {
            GameObject taskTabGroup = panel.taskTabContent.parent.gameObject;
            
            // Try maintaining prefab connection if it exists, otherwise just instantiate
            GameObject galleryTabGroup;
#if UNITY_2018_3_OR_NEWER
            if (PrefabUtility.IsPartOfAnyPrefab(taskTabGroup))
            {
                var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(taskTabGroup);
                galleryTabGroup = (GameObject)PrefabUtility.InstantiatePrefab(prefabSource, taskTabGroup.transform.parent);
            }
            else
#endif
            {
                galleryTabGroup = Instantiate(taskTabGroup, taskTabGroup.transform.parent);
            }

            galleryTabGroup.name = "GalleryTabGroup";
            
            // Fix ordering: Place immediately after the Task tab so it stays ABOVE the Edit button
            galleryTabGroup.transform.SetSiblingIndex(taskTabGroup.transform.GetSiblingIndex() + 1);
            
            // Force the button text to be 'GALLERY'
            Transform buttonObj = galleryTabGroup.transform.GetChild(0);
            if (buttonObj != null)
            {
                TextMeshProUGUI btnTxt = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null) btnTxt.text = "GALLERY";
            }
            
            Transform newContent = galleryTabGroup.transform.GetChild(1);
            while (newContent.childCount > 0)
            {
                DestroyImmediate(newContent.GetChild(0).gameObject);
            }
            
            panel.galleryTabContent = newContent;
            
            // Add to allTabs array
            var temp = new System.Collections.Generic.List<GameObject>(panel.allTabs);
            if (!temp.Contains(newContent.gameObject))
            {
                temp.Add(newContent.gameObject);
                panel.allTabs = temp.ToArray();
            }
        } // <--- Added missing closing brace
        
        // Ensure ordering and naming are correct EVEN IF it was already created previously
        if (panel.galleryTabContent != null)
        {
            GameObject galleryTabGroup = panel.galleryTabContent.parent.gameObject;
            galleryTabGroup.name = "GalleryTabGroup";
            
            if (panel.taskTabContent != null)
            {
                // Push it above Edit
                galleryTabGroup.transform.SetSiblingIndex(panel.taskTabContent.parent.GetSiblingIndex() + 1);
            }
            
            Transform buttonObj = galleryTabGroup.transform.GetChild(0);
            if (buttonObj != null)
            {
                TextMeshProUGUI btnTxt = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null) btnTxt.text = "GALLERY";
            }
        }

        // 2. CREATE CENTER POPUP UI
        if (panel.centerGalleryPanel == null)
        {
            Canvas canvas = GetMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("AssetDetailPanel must be inside a Canvas!");
                return;
            }

            GameObject popupObj = new GameObject("CenterGalleryPopup_Fixed");
            popupObj.transform.SetParent(canvas.transform, false);
            popupObj.layer = LayerMask.NameToLayer("UI");
            
            RectTransform rt = popupObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(700, 800);
            
            Image bg = popupObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.98f);
            
            Outline outline = popupObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.8f, 1f, 0.5f);
            outline.effectDistance = new Vector2(2, -2);
            
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(rt, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.anchoredPosition = new Vector2(0, -30);
            titleRt.sizeDelta = new Vector2(-40, 50);
            TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = "GALLERY DETAILS";
            titleTxt.fontSize = 28;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = new Color(0.3f, 0.85f, 1.0f);
            
            GameObject photoObj = new GameObject("PhotoRawImage");
            photoObj.transform.SetParent(rt, false);
            RectTransform photoRt = photoObj.AddComponent<RectTransform>();
            photoRt.anchorMin = new Vector2(0.5f, 1);
            photoRt.anchorMax = new Vector2(0.5f, 1);
            photoRt.pivot = new Vector2(0.5f, 1);
            photoRt.anchoredPosition = new Vector2(0, -100);
            photoRt.sizeDelta = new Vector2(600, 350);
            RawImage rawImg = photoObj.AddComponent<RawImage>();
            rawImg.color = Color.white;
            AspectRatioFitter fitter = photoObj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1.6f;
            
            GameObject detailsObj = new GameObject("DetailsText");
            detailsObj.transform.SetParent(rt, false);
            RectTransform detailsRt = detailsObj.AddComponent<RectTransform>();
            detailsRt.anchorMin = new Vector2(0, 0);
            detailsRt.anchorMax = new Vector2(1, 1);
            detailsRt.pivot = new Vector2(0.5f, 1);
            detailsRt.anchoredPosition = new Vector2(0, -480);
            detailsRt.sizeDelta = new Vector2(-100, 280);
            TextMeshProUGUI detailsTxt = detailsObj.AddComponent<TextMeshProUGUI>();
            detailsTxt.fontSize = 20;
            detailsTxt.alignment = TextAlignmentOptions.TopLeft;
            detailsTxt.color = new Color(0.9f, 0.9f, 0.9f);
            detailsTxt.richText = true;
            
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(rt, false);
            RectTransform closeRt = closeBtnObj.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.5f, 0);
            closeRt.anchorMax = new Vector2(0.5f, 0);
            closeRt.pivot = new Vector2(0.5f, 0);
            closeRt.anchoredPosition = new Vector2(0, 50);
            closeRt.sizeDelta = new Vector2(250, 55);
            Image closeBg = closeBtnObj.AddComponent<Image>();
            closeBg.color = new Color(0.85f, 0.25f, 0.2f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            
            GameObject closeTxtObj = new GameObject("Text");
            closeTxtObj.transform.SetParent(closeRt, false);
            RectTransform closeTxtRt = closeTxtObj.AddComponent<RectTransform>();
            closeTxtRt.anchorMin = Vector2.zero;
            closeTxtRt.anchorMax = Vector2.one;
            closeTxtRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
            closeTxt.text = "CLOSE";
            closeTxt.fontSize = 22;
            closeTxt.fontStyle = FontStyles.Bold;
            closeTxt.alignment = TextAlignmentOptions.Center;
            closeTxt.color = Color.white;

            popupObj.SetActive(false);

            panel.centerGalleryPanel = popupObj;
            panel.centerPopupImage = rawImg;
            panel.centerPopupDetails = detailsTxt;
        }

        EditorUtility.SetDirty(panel);
        Debug.Log("Gallery UI generated into the Scene successfully! Panel assignments updated.");
    }

    [MenuItem("BMS Tools/Update Top Panel (Back Button)")]
    public static void UpdateTopPanelUI()
    {
        SmartCameraController camCtrl = Object.FindAnyObjectByType<SmartCameraController>(FindObjectsInactive.Include);
        if (camCtrl == null)
        {
            Debug.LogError("SmartCameraController not found in the scene! Cannot wire up the Back Button.");
            return;
        }

        Canvas canvas = GetMainCanvas();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene to add the UI!");
            return;
        }

        Undo.RecordObject(canvas.gameObject, "Create Back Button");

        Transform topPanel = null;
        for(int i = 0; i < canvas.transform.childCount; i++)
        {
            Transform child = canvas.transform.GetChild(i);
            string n = child.name.ToLower();
            if ((n.Contains("top") && n.Contains("panel")) || n.Contains("header") || n.Contains("dash") || n.Contains("topbar"))
            {
                topPanel = child;
                break;
            }
        }

        Transform parent = topPanel != null ? topPanel : canvas.transform;

        Transform existingBtn = parent.Find("Back_Button_TopLeft");
        if (existingBtn != null)
        {
            DestroyImmediate(existingBtn.gameObject);
        }

        GameObject btnObj = new GameObject("Back_Button_TopLeft");
        btnObj.transform.SetParent(parent, false);
        btnObj.layer = LayerMask.NameToLayer("UI");

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Top Left
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        
        rt.anchoredPosition = new Vector2(15, -15);
        rt.sizeDelta = new Vector2(140, 45);

        // FORCE SIZES so layout groups don't squash it!
        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.minWidth = 140;
        le.preferredWidth = 140;
        le.minHeight = 45;
        le.preferredHeight = 45;

        // FORCE it to be the VERY FIRST item in the Horizontal layout (Top Left)
        btnObj.transform.SetAsFirstSibling();

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.8f, 1f, 0.8f);
        outline.effectDistance = new Vector2(1, -1);

        Button btn = btnObj.AddComponent<Button>();

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(rt, false);
        RectTransform txtRt = txtObj.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = "< BACK";
        txt.fontSize = 22;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        
        // Fix the weird "vertical stack" issue by disabling word wrap
        txt.enableWordWrapping = false;
        txt.overflowMode = TextOverflowModes.Overflow;

        // Wire it up persistently!
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(camCtrl.GoBack));

        EditorUtility.SetDirty(parent.gameObject);
        Debug.Log("Back Button perfectly generated and wired to SmartCameraController!");
    }
}
