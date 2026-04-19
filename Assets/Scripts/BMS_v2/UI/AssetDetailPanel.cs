using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.IO;

namespace BMS_v2
{
    /// <summary>
    /// Controls the dynamic Right Panel UI that displays detailed info for the currently selected
    /// building, floor, room, or asset. Supports editing data and saving it back to the JSON store.
    /// </summary>
    public class AssetDetailPanel : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject infoRowPrefab;
        
        [Header("Containers (For Tabs)")]
        public Transform identityTabContent;
        public Transform lifecycleTabContent;
        public Transform costTabContent;
        public Transform warrantyTabContent;
        public Transform taskTabContent;
        
        [Header("Headers")]
        public TMP_Text headerTitleText;
        public TMP_Text headerSubtitleText;
        public TMP_Text editButtonText; 

        [Header("For Accordion/Tabs")]
        public GameObject[] allTabs; 

        private List<InfoRowUI> activeRows = new List<InfoRowUI>();
        private AssetData currentAssetData;
        private bool isEditMode = false;

        private void Awake()
        {
            
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null) rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
        }

        public void ToggleTab(GameObject tabToToggle)
        {
            if (allTabs == null || allTabs.Length == 0) return;
            bool wasActive = tabToToggle.activeSelf;
            foreach (var tab in allTabs) { if (tab != null) tab.SetActive(false); }
            if (!wasActive && tabToToggle != null) { tabToToggle.SetActive(true); }
        }

        public void ClearExistingRows()
        {
            foreach (var row in activeRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            activeRows.Clear();
        }

        public void ShowOutsideWarning(string assetId)
        {
            gameObject.SetActive(true);
            ClearExistingRows();
            
            if (headerTitleText != null) {
                headerTitleText.text = "NOT ACCESSIBLE";
                headerTitleText.color = new Color(1f, 0.4f, 0.4f);
            }
            if (headerSubtitleText != null) headerSubtitleText.text = assetId;

            SetTabTitle(identityTabContent, "NAVIGATION");
            AddRow(identityTabContent, "STATUS", "STATIONARY ASSET");
            AddRow(identityTabContent, "NOTICE", "PLEASE GO INSIDE THE BUILDING TO VIEW THIS ASSET", null);
            
            // Hide other tabs
            if (allTabs != null) {
                foreach(var t in allTabs) if(t != null) t.SetActive(false);
                if(allTabs.Length > 0 && allTabs[0] != null) allTabs[0].SetActive(true);
            }
        }

        public void ShowAsset(string id)
        {
            if (DataStore.Instance == null || !DataStore.Instance.IsDataLoaded) return;
            if (string.IsNullOrEmpty(id)) return;

            SummaryManager.LastSelectedId = id; 
            gameObject.SetActive(true);
            isEditMode = false;

            
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null) {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
                LayoutElement le = GetComponent<LayoutElement>();
                if (le != null) { le.minWidth = 300; le.preferredWidth = 300; }
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }

            ClearExistingRows();

            if (id.StartsWith("BLD_"))
            {
                currentAssetData = null; 
                RenderBuildingContext(id);
            }
            else if (id.StartsWith("FLR_"))
            {
                currentAssetData = null; 
                RenderFloorContext(id);
            }
            else if (id.StartsWith("RM_"))
            {
                currentAssetData = null; 
                RenderRoomContext(id);
            }
            else
            {
                RenderSingleAssetContext(id);
            }

            if (allTabs != null)
            {
                foreach (var tab in allTabs) { 
                    if (tab != null) {
                        tab.SetActive(false);
                        
                        if (tab.transform.parent != null) tab.transform.parent.gameObject.SetActive(true);
                    }
                }
                if (allTabs.Length > 0 && allTabs[0] != null) allTabs[0].SetActive(true); 
            }
        }

        private void RenderBuildingContext(string buildingId)
        {
            BuildingDocument doc = DataStore.Instance.GetBuilding(buildingId);
            List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
            
            float totalInv = 0; float totalVal = 0;
            Dictionary<string, int> deptStats = new Dictionary<string, int>();

            foreach(var a in allAssets) {
                if (a.location != null && a.location.building_id == buildingId) {
                    if (a.cost != null) { totalInv += a.cost.total_invested; totalVal += a.cost.current_market_value; }
                    if (a.identity != null && !string.IsNullOrEmpty(a.identity.department)) {
                        if(!deptStats.ContainsKey(a.identity.department)) deptStats[a.identity.department] = 0;
                        deptStats[a.identity.department]++;
                    }
                }
            }

            string bldName = (doc != null && !string.IsNullOrEmpty(doc.building_name)) ? doc.building_name : "Building Metrics";
            if (headerTitleText != null) { 
                headerTitleText.text = bldName; 
                headerTitleText.fontSize = 18f; 
                headerTitleText.color = new Color(0.9f, 0.7f, 0.2f); 
            }
            if (headerSubtitleText != null) {
                headerSubtitleText.text = "OFFICIAL RECORD";
                headerSubtitleText.fontSize = 11f; 
            }

            
            SetTabTitle(identityTabContent, "OVERVIEW");
            if (doc != null)
            {
                AddRow(identityTabContent, "Main Name", doc.building_name, (val) => doc.building_name = val);
                AddRow(identityTabContent, "Address", doc.address ?? "N/A", (val) => doc.address = val);
                AddRow(identityTabContent, "Year Built", doc.year_built > 0 ? doc.year_built.ToString() : "N/A", (val) => int.TryParse(val, out doc.year_built));
                AddRow(identityTabContent, "Currency", doc.currency ?? "INR", (val) => doc.currency = val);
            }
            else AddRow(identityTabContent, "Info", "No building data found");

            
            SetTabTitle(lifecycleTabContent, "ARCHITECTURE");
            if (doc != null)
            {
                AddRow(lifecycleTabContent, "Total Floors", doc.total_floors.ToString(), (val) => int.TryParse(val, out doc.total_floors));
                AddRow(lifecycleTabContent, "Total Area", $"{doc.total_area_sqm} Sqm", (val) => float.TryParse(val.Replace("Sqm","").Trim(), out doc.total_area_sqm));
            }
            else AddRow(lifecycleTabContent, "Notice", "Building record missing");

            
            SetTabTitle(costTabContent, "FINANCIALS");
            AddRow(costTabContent, "Portfolio Cost", $"{(doc != null ? doc.currency : "Rs")} {totalInv:N0}");
            AddRow(costTabContent, "Current Value", $"{(doc != null ? doc.currency : "Rs")} {totalVal:N0}");

            
            SetTabTitle(warrantyTabContent, "DEPARTMENTS");
            if (doc != null && doc.departments != null && doc.departments.Count > 0)
            {
                foreach(var d in doc.departments) AddRow(warrantyTabContent, "Active Dept", d);
            }
            else
            {
                foreach(var kv in deptStats) AddRow(warrantyTabContent, kv.Key, $"{kv.Value} Assets");
            }

            SetTabTitle(taskTabContent, "STATUS");
            AddRow(taskTabContent, "System Health", "All High-Priority Systems Optimal");
        }

        private void RenderRoomContext(string roomId)
        {
            List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
            int totalAssets = 0; float totalInv = 0;
            List<AssetData> roomAssets = new List<AssetData>();
            
            foreach(var a in allAssets) {
                if (a.location != null && a.location.room_id == roomId) {
                    totalAssets++;
                    roomAssets.Add(a);
                    if (a.cost != null) totalInv += a.cost.total_invested;
                }
            }

            if (headerTitleText != null) { 
                headerTitleText.text = "Room: " + roomId; 
                headerTitleText.fontSize = 18f;
                headerTitleText.color = new Color(0.1f, 0.7f, 0.9f); 
            }
            if (headerSubtitleText != null) {
                headerSubtitleText.text = "SPACE MANAGEMENT";
                headerSubtitleText.fontSize = 11f;
            }

            SetTabTitle(identityTabContent, "ROOM STATS");
            AddRow(identityTabContent, "Units Inside", totalAssets.ToString());
            AddRow(identityTabContent, "Total Val", $"Rs {totalInv:N0}");
            AddRow(identityTabContent, "Environment", "Stable (24.2°C)");

            SetTabTitle(lifecycleTabContent, "UTILITIES");
            AddRow(lifecycleTabContent, "HVAC Status", "Operational");
            AddRow(lifecycleTabContent, "Lighting", "Daylight Mode");

            SetTabTitle(costTabContent, "FINANCIALS");
            AddRow(costTabContent, "Asset Value", $"Rs {totalInv:N0}");
            AddRow(costTabContent, "Annual AMC", $"Rs {totalInv * 0.05f:N0}");
            
            SetTabTitle(warrantyTabContent, "INVENTORY LIST");
            foreach(var ra in roomAssets)
            {
                string name = (ra.identity != null) ? ra.identity.name : "Unknown Item";
                string cat = (ra.identity != null) ? ra.identity.category : "Misc";
                AddRow(warrantyTabContent, name, cat);
            }
            if(roomAssets.Count == 0) AddRow(warrantyTabContent, "Notice", "No items recorded in this room");

            SetTabTitle(taskTabContent, "RECENT LOGS");
            AddRow(taskTabContent, "Last Entry", "Routine Inspection (Mar 2024)");
        }

        private void RenderFloorContext(string floorId)
        {
            List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
            int totalAssets = 0; float totalInv = 0;
            HashSet<string> uniqueRooms = new HashSet<string>();
            
            foreach(var a in allAssets) {
                if (a.location != null && a.location.floor_id == floorId) {
                    totalAssets++;
                    if (!string.IsNullOrEmpty(a.location.room_id)) uniqueRooms.Add(a.location.room_id);
                    if (a.cost != null) totalInv += a.cost.total_invested;
                }
            }

            if (headerTitleText != null) { 
                headerTitleText.text = "Floor: " + floorId; 
                headerTitleText.fontSize = 18f;
                headerTitleText.color = new Color(0.1f, 0.9f, 0.4f); 
            }
            if (headerSubtitleText != null) {
                headerSubtitleText.text = "FLOOR INFRASTRUCTURE";
                headerSubtitleText.fontSize = 11f;
            }

            SetTabTitle(identityTabContent, "FLOOR DETAILS");
            AddRow(identityTabContent, "Total Units", totalAssets.ToString());
            AddRow(identityTabContent, "Total Rooms", uniqueRooms.Count.ToString());

            
            if (lifecycleTabContent != null && lifecycleTabContent.parent != null)
                lifecycleTabContent.parent.gameObject.SetActive(false);

            SetTabTitle(costTabContent, "FINANCIALS");
            AddRow(costTabContent, "Installed Cost", $"Rs {totalInv}");

            SetTabTitle(warrantyTabContent, "ROOM REGISTRY");
            foreach(var rid in uniqueRooms)
            {
                AddRow(warrantyTabContent, "Room", rid);
            }
            if(uniqueRooms.Count == 0) AddRow(warrantyTabContent, "Notice", "No rooms mapped on this floor");

            SetTabTitle(taskTabContent, "STATUS");
            AddRow(taskTabContent, "HVAC Status", "Operational");
            AddRow(taskTabContent, "Network", "Online");
        }

        private void RenderSingleAssetContext(string id)
        {
            currentAssetData = DataStore.Instance.GetAsset(id);
            if (currentAssetData == null) { gameObject.SetActive(false); return; }

            PopulateHeader(currentAssetData);
            if (headerTitleText != null) headerTitleText.color = new Color(0.2f, 0.8f, 1.0f);

            SetTabTitle(identityTabContent, "IDENTITY");
            SetTabTitle(lifecycleTabContent, "LIFE & QUALITY");
            SetTabTitle(costTabContent, "COST DATA");
            SetTabTitle(warrantyTabContent, "WARRANTY");
            SetTabTitle(taskTabContent, "MAINTENANCE TRACKER");

            PopulateAllTabs(currentAssetData);
        }

        private void SetTabTitle(Transform contentObj, string newTitle)
        {
            if (contentObj == null || contentObj.parent == null) return;
            Transform btnObj = contentObj.parent.GetChild(0); 
            if (btnObj != null) {
                TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = newTitle; 
            }
        }

        public void ToggleEditMode()
        {
            Debug.Log("[AssetDetailPanel] TOGGLE EDIT MODE CLICKED. Current Asset: " + (currentAssetData != null ? currentAssetData.id : "None"));

            isEditMode = !isEditMode;

            
            if (editButtonText != null)
            {
                editButtonText.text = isEditMode ? "SAVE" : "EDIT";
                editButtonText.color = isEditMode ? Color.green : Color.white;
            }

            foreach(var row in activeRows)
            {
                row.SetEditMode(isEditMode);
            }

            if (!isEditMode)
            {
                Debug.Log("[AssetDetailPanel] SAVING START...");
                SaveDataToFile();
            }
        }

        private void SaveDataToFile()
        {
            Debug.Log("[AssetDetailPanel] SAFE SAVE INITIATED...");
            string folderString = Path.Combine(Application.streamingAssetsPath, "BuildingData");
            if (!Directory.Exists(folderString)) Directory.CreateDirectory(folderString);
            
            
            

            if (currentAssetData != null)
            {
                if (currentAssetData.cost != null)
                {
                    currentAssetData.cost.total_invested = currentAssetData.cost.purchase_cost + currentAssetData.cost.installation_cost;
                }
                DataStore.Instance.AddOrUpdateAsset(currentAssetData);
            }

            
            List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
            Dictionary<string, List<AssetData>> buildingGroups = new Dictionary<string, List<AssetData>>();
            foreach (var a in allAssets)
            {
                string bld = (a.location != null && !string.IsNullOrEmpty(a.location.building_id)) ? a.location.building_id : "UnassignedZone";
                if (!buildingGroups.ContainsKey(bld)) buildingGroups[bld] = new List<AssetData>();
                buildingGroups[bld].Add(a);
            }

            foreach (var kvp in buildingGroups)
            {
                string bldId = kvp.Key;
                BuildingDocument doc = DataStore.Instance.GetBuilding(bldId);
                if (doc == null) doc = new BuildingDocument { building_id = bldId, building_name = bldId + " Name" };
                
                
                if (doc.floors == null) doc.floors = new List<FloorNode>();
                doc.floors.Clear();
                
                if (kvp.Value != null)
                {
                    foreach(var a in kvp.Value)
                    {
                        if (a == null) continue;
                        string fId = (a.location != null && !string.IsNullOrEmpty(a.location.floor_id)) ? a.location.floor_id : "UnassignedFloor";
                        string rId = (a.location != null && !string.IsNullOrEmpty(a.location.room_id)) ? a.location.room_id : "UnassignedRoom";
    
                        FloorNode fn = doc.floors.Find(f => f.floor_id == fId);
                        if(fn == null)
                        {
                            fn = new FloorNode { floor_id = fId, floor_name = fId + " Name", rooms = new List<RoomNode>() };
                            doc.floors.Add(fn);
                        }
                        if (fn.rooms == null) fn.rooms = new List<RoomNode>();
    
                        RoomNode rn = fn.rooms.Find(r => r.room_id == rId);
                        if(rn == null)
                        {
                            rn = new RoomNode { room_id = rId, room_name = rId + " Name", assets = new List<AssetData>() };
                            fn.rooms.Add(rn);
                        }
                        if (rn.assets == null) rn.assets = new List<AssetData>();
    
                        rn.assets.Add(a);
                    }
                }

                try {
                    string newJson = JsonUtility.ToJson(doc, true);
                    string path = Path.Combine(folderString, $"{bldId}.json");
                    File.WriteAllText(path, newJson);
                    Debug.Log($"[JSON Success] Written {newJson.Length} bytes to {path}");
                } catch (System.Exception ioEx) {
                    Debug.LogError($"[JSON Error] Failed writing {bldId}.json: {ioEx.Message}");
                }
            }
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif

            Debug.Log($"[AssetDetailPanel] Data Persistent Save Complete! Total Groups: {buildingGroups.Count}");
            if (currentAssetData != null) ShowAsset(currentAssetData.id);
            else if (headerSubtitleText != null) 
            {
                 
            }

            GlobalDashboardManager dashboard = FindAnyObjectByType<GlobalDashboardManager>(FindObjectsInactive.Include);
            if (dashboard != null) dashboard.BindDashboardData();
        }

        private void PopulateHeader(AssetData data)
        {
            if (data.identity != null)
            {
                if (headerTitleText != null) headerTitleText.text = data.identity.name;
                if (headerSubtitleText != null) headerSubtitleText.text = data.id;
            }
        }

        private void PopulateAllTabs(AssetData data)
        {
            
            if (data.identity != null && identityTabContent != null)
            {
                AddRow(identityTabContent, "Name", data.identity.name, (val) => data.identity.name = val);
                AddRow(identityTabContent, "Category", data.identity.category, (val) => data.identity.category = val);
                AddRow(identityTabContent, "Subcategory", data.identity.subcategory, (val) => data.identity.subcategory = val);
                AddRow(identityTabContent, "Department", data.identity.department, (val) => data.identity.department = val);
                AddRow(identityTabContent, "Brand", data.identity.brand, (val) => data.identity.brand = val);
                AddRow(identityTabContent, "Model", data.identity.model, (val) => data.identity.model = val);
                AddRow(identityTabContent, "Serial No.", data.identity.serial_number, (val) => data.identity.serial_number = val);
                AddRow(identityTabContent, "Quantity", data.identity.quantity.ToString(), (val) => int.TryParse(val, out data.identity.quantity));
            }

            
            if (data.lifecycle != null && data.quality != null && lifecycleTabContent != null)
            {
                AddRow(lifecycleTabContent, "Status", data.lifecycle.status, (val) => data.lifecycle.status = val);
                AddRow(lifecycleTabContent, "Type", data.lifecycle.type, (val) => data.lifecycle.type = val);
                AddRow(lifecycleTabContent, "Purchased On", data.lifecycle.purchase_date, (val) => data.lifecycle.purchase_date = val);
                AddRow(lifecycleTabContent, "Installed On", data.lifecycle.installation_date, (val) => data.lifecycle.installation_date = val);
                AddRow(lifecycleTabContent, "Last Service", data.lifecycle.last_service_date, (val) => data.lifecycle.last_service_date = val);
                AddRow(lifecycleTabContent, "Next Due", data.lifecycle.next_service_due, (val) => data.lifecycle.next_service_due = val);
                AddRow(lifecycleTabContent, "Consumable", data.lifecycle.is_consumable ? "Yes" : "No", null);
                AddRow(lifecycleTabContent, "Condition", data.quality.condition, (val) => data.quality.condition = val);
                AddRow(lifecycleTabContent, "Health Score", data.quality.current_score_percent.ToString() + "%", (val) => int.TryParse(val.Replace("%", ""), out data.quality.current_score_percent));
                AddRow(lifecycleTabContent, "Inspector", data.quality.inspected_by, (val) => data.quality.inspected_by = val);
            }

            
            if (data.cost != null && costTabContent != null)
            {
                AddRow(costTabContent, "Purchase", data.cost.purchase_cost.ToString(), (val) => float.TryParse(val.Replace("Rs", "").Trim(), out data.cost.purchase_cost));
                AddRow(costTabContent, "Installation", data.cost.installation_cost.ToString(), (val) => float.TryParse(val.Replace("Rs", "").Trim(), out data.cost.installation_cost));
                AddRow(costTabContent, "Total Invested", data.cost.total_invested.ToString(), null); 
                AddRow(costTabContent, "Market Value", data.cost.current_market_value.ToString(), (val) => float.TryParse(val.Replace("Rs", "").Trim(), out data.cost.current_market_value));
                AddRow(costTabContent, "Depreciation", data.cost.depreciation_per_year.ToString(), (val) => float.TryParse(val.Replace("Rs", "").Trim(), out data.cost.depreciation_per_year));
            }

            
            if (data.warranty != null && warrantyTabContent != null)
            {
                AddRow(warrantyTabContent, "Status", data.warranty.status, (val) => data.warranty.status = val);
                AddRow(warrantyTabContent, "Type", data.warranty.warranty_type, (val) => data.warranty.warranty_type = val);
                AddRow(warrantyTabContent, "Years", data.warranty.warranty_years.ToString(), (val) => int.TryParse(val, out data.warranty.warranty_years));
                AddRow(warrantyTabContent, "Provider", data.warranty.provider, (val) => data.warranty.provider = val);
                AddRow(warrantyTabContent, "Start Date", data.warranty.start_date, (val) => data.warranty.start_date = val);
                AddRow(warrantyTabContent, "Expiry Date", data.warranty.expiry_date, (val) => data.warranty.expiry_date = val);
                AddRow(warrantyTabContent, "AMC Expiry", data.warranty.amc_expiry, (val) => data.warranty.amc_expiry = val);
                AddRow(warrantyTabContent, "Contact", data.warranty.contact, (val) => data.warranty.contact = val);
                AddRow(warrantyTabContent, "Doc Ref", data.warranty.document_ref, (val) => data.warranty.document_ref = val);
            }
            
            if (data.tasks != null && taskTabContent != null)
            {
                if (data.tasks.Count == 0)
                {
                    AddRow(taskTabContent, "Operations", "No active work orders");
                }
                else
                {
                    foreach (var t in data.tasks)
                    {
                        string pStr = t.priority.ToUpper();
                        string romanPrio = "III";
                        if (pStr == "HIGH" || pStr == "ULTRA" || pStr == "URGENT") romanPrio = "I";
                        else if (pStr == "MEDIUM") romanPrio = "II";

                        string header = $"[{romanPrio}] {t.task_type}";
                        string scheduleStr = (t.schedule != null && !string.IsNullOrEmpty(t.schedule.start_date)) ? t.schedule.start_date : "No Date";
                        string workerStr = (t.worker != null && !string.IsNullOrEmpty(t.worker.name)) ? t.worker.name : "Unassigned";
                        
                        string body = $"{t.description}\n\nStatus: {(t.schedule != null ? t.schedule.status : "Pending")}\nWorker: {workerStr}\nDate: {scheduleStr}";
                        AddRow(taskTabContent, header, body, (newVal) => t.description = newVal);
                    }
                }
            }
        }

        private string FormatText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length == 1) return text.ToUpper();
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        private void AddRow(Transform container, string label, string value, System.Action<string> onSave = null)
        {
            if (container == null || string.IsNullOrEmpty(value)) return;

            GameObject rowGO = Instantiate(infoRowPrefab, container);
            InfoRowUI rowScript = rowGO.GetComponent<InfoRowUI>();
            
            if (rowScript != null)
            {
                rowScript.Setup(label, value, onSave); 
                activeRows.Add(rowScript);
            }
        }

        public void HidePanel()
        {
            gameObject.SetActive(false);
        }
    }
}
