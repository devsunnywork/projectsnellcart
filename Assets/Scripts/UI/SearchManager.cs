using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SearchManager : MonoBehaviour
{
    [Header("1. Main Setup")]
    public AerialUIManager manager;          // Link your AerialUIManager here
    public TMP_InputField inputField;       // Link your search text box
    public Transform contentContainer;      // Link the Content of your Scroll View
    public GameObject resultBtnPrefab;      // Link your Button Prefab
    public GameObject fullPanelObject;      // Link the main Search Panel

    private List<InteractableObjectInfo> allItems = new List<InteractableObjectInfo>();

    void Start()
    {
        // Find all objects in scene once at start
        RefreshList();
        
        // Hide panel at start
        if (fullPanelObject != null) fullPanelObject.SetActive(false);

        // Add listener to text change
        if (inputField != null)
            inputField.onValueChanged.AddListener(delegate { Search(); });
    }

    public void RefreshList()
    {
        allItems.Clear();
        allItems.AddRange(FindObjectsByType<InteractableObjectInfo>(FindObjectsSortMode.None));
    }

    public void TogglePanel()
    {
        if (fullPanelObject == null) return;
        bool state = !fullPanelObject.activeSelf;
        fullPanelObject.SetActive(state);
        
        if (state) {
            RefreshList();
            inputField.text = ""; // Clear old search
            inputField.ActivateInputField();
        }
    }

    public void Search()
    {
        // Clear old buttons
        foreach (Transform child in contentContainer) Destroy(child.gameObject);

        string query = inputField.text.ToLower().Trim();
        if (string.IsNullOrEmpty(query)) return;

        foreach (var item in allItems)
        {
            if (item == null) continue;

            // Check if name matches (either display name or gameobject name)
            if (item.name.ToLower().Contains(query) || item.gameObject.name.ToLower().Contains(query))
            {
                CreateButton(item);
            }
        }
    }

    private void CreateButton(InteractableObjectInfo item)
    {
        GameObject btnGo = Instantiate(resultBtnPrefab, contentContainer);
        
        // Reset UI Offsets
        btnGo.transform.localPosition = Vector3.zero;
        btnGo.transform.localScale = Vector3.one;
        
        // Set Text - Prioritize the 'name' field from the script
        TMP_Text txt = btnGo.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            BuildingInfo b = item.GetComponentInParent<BuildingInfo>();
            string bLoc = (b != null) ? b.name : "Map";
            
            // Use script's name, fallback to GO name if it's default
            string displayName = (string.IsNullOrEmpty(item.name) || item.name == "New Object") 
                                 ? item.gameObject.name : item.name;

            txt.text = $"{displayName} <color=#999>({bLoc})</color>";
        }

        // Set Click
        Button bComp = btnGo.GetComponent<Button>();
        if (bComp != null)
        {
            bComp.onClick.AddListener(() => {
                manager.FocusOnSearchObject(item);
                // We keep panel open as requested
            });
        }
    }
}
