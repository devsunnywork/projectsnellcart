using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace BMS_v2
{
    public class LeftPanelManager : MonoBehaviour
    {
        public GameObject[] allAccordions;
        public TMP_InputField searchInput;
        public Transform searchResultsContainer;
        public Transform taskResultsContainer;
        private List<GameObject> activeTaskBodies = new List<GameObject>();
        private List<GameObject> activeSubContents = new List<GameObject>();

        private void Awake()
        {
            
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null) rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
        }

        private void Start()
        {
            
            if (searchInput != null) 
            {
                searchInput.onValueChanged.RemoveAllListeners();
                searchInput.onValueChanged.AddListener(OnSearchChanged);
            }

            
            if (DataStore.Instance != null)
            {
                DataStore.Instance.OnDataReady += RefreshGlobalTasks;
                if (DataStore.Instance.IsDataLoaded) RefreshGlobalTasks();
            }

            
            if (allAccordions != null)
            {
                foreach (var acc in allAccordions)
                {
                    if (acc != null && acc.name.ToUpper().Contains("WORK"))
                    {
                        acc.SetActive(true);
                        RefreshGlobalTasks();
                    }
                    else if (acc != null)
                    {
                        acc.SetActive(false);
                    }
                }
            }

            
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null) {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
                LayoutElement le = GetComponent<LayoutElement>();
                if (le != null) { le.minWidth = 300; le.preferredWidth = 300; }
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        private void OnDestroy()
        {
            if (DataStore.Instance != null)
            {
                DataStore.Instance.OnDataReady -= RefreshGlobalTasks;
            }
        }

        public void OnSearchChanged(string query)
        {
            if (searchResultsContainer == null) return;
            foreach (Transform child in searchResultsContainer) Destroy(child.gameObject);

            if (string.IsNullOrEmpty(query)) 
            {
                RefreshGlobalTasks(); 
                return;
            }

            query = query.ToLower();
            if (DataStore.Instance == null) return;
            
            List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
            bool foundAsset = false;

            foreach (var asset in allAssets)
            {
                if (asset.identity != null && asset.identity.name != null && asset.identity.name.ToLower().Contains(query))
                {
                    foundAsset = true;
                    CreateSearchResultBtn(asset, searchResultsContainer);
                }
            }

            if (!foundAsset) CreateTextOnlyResult("No Assets Matches", searchResultsContainer);
            
            
            FilterTasks(query);
        }

        public void RefreshGlobalTasks()
        {
            if (taskResultsContainer == null) return;
            if (DataStore.Instance == null) return;

            foreach (Transform child in taskResultsContainer) Destroy(child.gameObject);
            activeTaskBodies.Clear();
            activeSubContents.Clear();

            List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
            int count = 0;
            foreach (var asset in allAssets)
            {
                if (asset.tasks == null) continue;
                foreach (var t in asset.tasks)
                {
                    CreateTaskResultBtn(t, asset.id, taskResultsContainer);
                    count++;
                }
            }

            if (count == 0) CreateTextOnlyResult("No Active Work Orders", taskResultsContainer);
        }

        private void FilterTasks(string query)
        {
            if (taskResultsContainer == null) return;
            foreach (Transform child in taskResultsContainer) Destroy(child.gameObject);

            List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
            bool found = false;
            foreach (var asset in allAssets)
            {
                if (asset.tasks == null) continue;
                foreach (var t in asset.tasks)
                {
                    if (t.description.ToLower().Contains(query) || t.task_type.ToLower().Contains(query))
                    {
                        CreateTaskResultBtn(t, asset.id, taskResultsContainer);
                        found = true;
                    }
                }
            }
            if (!found) CreateTextOnlyResult("No Tasks Matches", taskResultsContainer);
        }

        private void CreateSearchResultBtn(AssetData asset, Transform container)
        {
            GameObject btnObj = new GameObject("AssetResult_" + asset.id);
            btnObj.transform.SetParent(container, false);
            btnObj.AddComponent<Image>().color = new Color(0.1f, 0.4f, 0.7f, 0.4f); 
            Button b = btnObj.AddComponent<Button>();
            btnObj.AddComponent<LayoutElement>().minHeight = 45;

            TextMeshProUGUI tmp = CreateTextInResult(btnObj.transform, asset.identity.name);
            tmp.color = Color.white;
            b.onClick.AddListener(() => OnResultClicked(asset));
        }

        private void CreateTaskResultBtn(TaskData task, string assetId, Transform container)
        {
            
            GameObject cardObj = new GameObject("TaskCard_" + task.id);
            cardObj.transform.SetParent(container, false);
            VerticalLayoutGroup vlg = cardObj.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true; vlg.childControlHeight = true; vlg.childForceExpandHeight = false; vlg.spacing = 2;
            cardObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(cardObj.transform, false);
            headerObj.AddComponent<Image>().color = (task.priority == "high") ? new Color(0.7f, 0.15f, 0.15f, 0.6f) : new Color(0.1f, 0.25f, 0.45f, 0.6f);
            Button b = headerObj.AddComponent<Button>();
            headerObj.AddComponent<LayoutElement>().minHeight = 45;

            string label = $"[{task.task_type.ToUpper()}] {task.description}";
            TextMeshProUGUI tmp = CreateTextInResult(headerObj.transform, label);
            tmp.fontSize = 14; 
            tmp.fontStyle = FontStyles.Bold;

            
            GameObject bodyObj = new GameObject("DetailsBody");
            bodyObj.transform.SetParent(cardObj.transform, false);
            VerticalLayoutGroup vlgBody = bodyObj.AddComponent<VerticalLayoutGroup>();
            vlgBody.padding = new RectOffset(10, 10, 10, 10); vlgBody.spacing = 8;
            vlgBody.childControlWidth = true; vlgBody.childControlHeight = true; vlgBody.childForceExpandHeight = false;
            bodyObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            bodyObj.SetActive(false);
            activeTaskBodies.Add(bodyObj);

            
            Transform infoC = CreateSubSection(bodyObj.transform, "OVERVIEW", new Color(0.2f, 0.45f, 0.7f), true);
            string desc = !string.IsNullOrEmpty(task.description) ? task.description : "No description provided";
            string dept = !string.IsNullOrEmpty(task.department) ? task.department.ToUpper() : "GENERAL";
            
            AddTaskDetailRow(infoC, "Desc", desc);
            AddTaskDetailRow(infoC, "Dept", dept);

            if (task.worker != null) {
                Transform workC = CreateSubSection(bodyObj.transform, "PERSONNEL", new Color(0.5f, 0.35f, 0.1f));
                AddTaskDetailRow(workC, "Name", task.worker.name);
                AddTaskDetailRow(workC, "Role", task.worker.role);
                AddTaskDetailRow(workC, "Firm", task.worker.contractor_firm);
            }
            if (task.schedule != null) {
                Transform schedC = CreateSubSection(bodyObj.transform, "SCHEDULING", new Color(0.15f, 0.5f, 0.35f));
                AddTaskDetailRow(schedC, "From", task.schedule.start_date);
                AddTaskDetailRow(schedC, "To", task.schedule.end_date);
                AddTaskDetailRow(schedC, "Status", task.schedule.status.ToUpper());
            }
            if (task.cost_breakdown != null) {
                Transform costC = CreateSubSection(bodyObj.transform, "FINANCIALS", new Color(0.4f, 0.15f, 0.5f));
                AddTaskDetailRow(costC, "Total", $"Rs. {task.cost_breakdown.total_task_cost}");
                AddTaskDetailRow(costC, "Status", task.cost_breakdown.payment_status);
            }
            if (task.materials_used != null && task.materials_used.Count > 0) {
                Transform matC = CreateSubSection(bodyObj.transform, "MATERIALS", new Color(0.6f, 0.35f, 0.6f));
                foreach(var m in task.materials_used) AddTaskDetailRow(matC, m.name, $"{m.quantity} {m.unit}");
            }

            
            b.onClick.AddListener(() => {
                bool wasActive = bodyObj.activeSelf;
                foreach(var tb in activeTaskBodies) tb.SetActive(false);
                bodyObj.SetActive(!wasActive);
                OnTaskClicked(assetId, task.id);
            });
        }

        private Transform CreateSubSection(Transform parent, string title, Color col, bool startOpened = false)
        {
            GameObject wrap = new GameObject("Section_" + title);
            wrap.transform.SetParent(parent, false);
            VerticalLayoutGroup vlg = wrap.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true; vlg.childControlHeight = true; vlg.childForceExpandHeight = false;

            GameObject head = new GameObject("S_Head");
            head.transform.SetParent(wrap.transform, false);
            head.AddComponent<Image>().color = col;
            Button b = head.AddComponent<Button>();
            head.AddComponent<LayoutElement>().minHeight = 45;
            TextMeshProUGUI t = CreateTextInResult(head.transform, title);
            t.fontSize = 20; t.fontStyle = FontStyles.Bold;

            GameObject content = new GameObject("S_Content");
            content.transform.SetParent(wrap.transform, false);
            VerticalLayoutGroup vlgC = content.AddComponent<VerticalLayoutGroup>();
            vlgC.padding = new RectOffset(15, 15, 8, 8); vlgC.spacing = 6;
            vlgC.childControlWidth = true; vlgC.childControlHeight = true; vlgC.childForceExpandHeight = false;
            content.SetActive(startOpened);
            activeSubContents.Add(content);

            b.onClick.AddListener(() => {
                bool wasActive = content.activeSelf;
                
                
                foreach(var sc in activeSubContents) if(sc.transform.parent.parent == parent) sc.SetActive(false);
                content.SetActive(!wasActive);
            });
            return content.transform;
        }

        private void AddTaskDetailRow(Transform parent, string label, string value)
        {
            GameObject row = new GameObject("DetailRow");
            row.transform.SetParent(parent, false);
            RectTransform rowRt = row.AddComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(0, 30); 
            ContentSizeFitter csf = row.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.spacing = 20; 

            TextMeshProUGUI lTxt = CreateTextInResult(row.transform, label + ":");
            lTxt.fontSize = 12; lTxt.color = new Color(0.6f, 0.8f, 1f); 
            lTxt.alignment = TextAlignmentOptions.TopLeft;
            lTxt.fontStyle = FontStyles.Bold;
            LayoutElement leL = lTxt.gameObject.AddComponent<LayoutElement>();
            leL.minWidth = 90; leL.preferredWidth = 90; leL.flexibleWidth = 0; 
            
            TextMeshProUGUI vTxt = CreateTextInResult(row.transform, value);
            vTxt.fontSize = 12; vTxt.color = Color.white; 
            vTxt.alignment = TextAlignmentOptions.TopLeft; 
            vTxt.enableWordWrapping = true;
            LayoutElement leV = vTxt.gameObject.AddComponent<LayoutElement>();
            leV.flexibleWidth = 1;
        }

        private TextMeshProUGUI CreateTextInResult(Transform parent, string txt)
        {
            GameObject txtObj = new GameObject("Txt");
            txtObj.transform.SetParent(parent, false);
            RectTransform rt = txtObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(15, 0); 
            rt.offsetMax = new Vector2(-15, 0); 
            
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = txt;
            tmp.fontSize = 15;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left; 
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private void CreateTextOnlyResult(string msg, Transform container)
        {
            GameObject txtObj = new GameObject("EmptyTxt");
            txtObj.transform.SetParent(container, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = msg;
            tmp.fontSize = 14; tmp.color = Color.gray;
            tmp.alignment = TextAlignmentOptions.Center;
            txtObj.AddComponent<LayoutElement>().minHeight = 40;
        }

        private void OnTaskClicked(string assetId, string taskId)
        {
            AssetData asset = DataStore.Instance.GetAsset(assetId);
            if (asset != null) OnResultClicked(asset);
        }

        public void ToggleAccordion(GameObject targetAccordion)
        {
            if (allAccordions == null) return;
            bool wasOpen = targetAccordion.activeSelf;
            foreach (var acc in allAccordions) { if (acc != null) acc.SetActive(false); }
            if (!wasOpen) 
            {
                targetAccordion.SetActive(true);
                if (targetAccordion.name.Contains("WORK ORDERS")) RefreshGlobalTasks();
            }
        }

        private void OnResultClicked(AssetData asset)
        {
            
            if (TourStateManager.Instance != null)
            {
                TourStateManager.Instance.ProcessInteraction(asset.id);
            }

            
            if (SmartCameraController.Instance != null)
            {
                SmartCameraController.Instance.FocusOnAsset(asset.id);
            }

            
            AssetDetailPanel detailPanel = FindAnyObjectByType<AssetDetailPanel>(FindObjectsInactive.Include);
            if (detailPanel != null)
            {
                detailPanel.gameObject.SetActive(true);
                detailPanel.ShowAsset(asset.id);
            }
        }
    }
}
