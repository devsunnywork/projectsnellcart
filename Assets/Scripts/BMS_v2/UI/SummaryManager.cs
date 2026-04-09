using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BMS_v2
{
    public class SummaryManager : MonoBehaviour
    {
        public static SummaryManager Instance;
        public static string LastSelectedId = "BLD_001";

        [Header("UI References")]
        public GameObject summaryPanel;
        public TextMeshProUGUI titleText;
        public Transform tableContent; 
        public Button closeBtn;

        [Header("Font Settings")]
        public TMP_FontAsset mulishFont;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            summaryPanel.SetActive(false);
            if (closeBtn != null) closeBtn.onClick.AddListener(CloseSummary);

            
            RectTransform rt = summaryPanel.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(1400, 450);
        }

        public void OpenSummary()
        {
            OpenSummary(LastSelectedId);
        }

        public void OpenSummary(string selectedId)
        {
            LastSelectedId = selectedId;
            summaryPanel.SetActive(true);
            PopulateVerticalTable(selectedId);
        }

        public void CloseSummary()
        {
            summaryPanel.SetActive(false);
        }

        private void PopulateVerticalTable(string id)
        {
            foreach (Transform child in tableContent) Destroy(child.gameObject);

            titleText.text = "TECHNICAL REPORT: " + id;
            titleText.fontSize = 32;

            
            string[] headers = { "S.N", "Bld. N", "Bld. ID", "Loc", "Des", "Qty", "Prio", "Cost", "Start", "End", "Remark" };
            AddTableRow(headers, true);

            
            string sno = "SN-01", bldName = "Main Complex", bldId = "BLD-001", loc = "Heritage Zone", desc = "Main Structure", qty = "1";
            string priority = "NORMAL", cost = "Rs. 0", startD = "2024-01-01", endD = "2025-01-01", remark = "Optimal Condition";

            if (id.StartsWith("BLD_"))
            {
                BuildingDocument doc = DataStore.Instance.GetBuilding(id);
                if (doc != null)
                {
                    sno = "PRIMARY-01";
                    bldName = !string.IsNullOrEmpty(doc.building_name) ? doc.building_name : "Temple Complex";
                    bldId = doc.building_id;
                    loc = !string.IsNullOrEmpty(doc.address) ? doc.address : "Main Entrance Gate";
                    desc = !string.IsNullOrEmpty(doc.description) ? doc.description : "Main Spiritual Centre Structure";
                    qty = doc.total_floors + " Floors / " + doc.total_area_sqm + " Sqm";
                    
                    int assetCount = 0; float totalVal = 0;
                    List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
                    foreach(var a in allAssets) if(a.location.building_id == id) { assetCount++; if(a.cost != null) totalVal += a.cost.total_invested; }
                    remark = $"Registered Assets: {assetCount}";
                    cost = $"Rs {totalVal:N0}";
                    startD = "2022-01-01"; endD = "2032-12-31"; 
                }
            }
            else if (DataStore.Instance.GetAsset(id) != null)
            {
                AssetData asset = DataStore.Instance.GetAsset(id);
                sno = !string.IsNullOrEmpty(asset.identity.serial_number) ? asset.identity.serial_number : "SR-" + asset.id.Split('_')[asset.id.Split('_').Length - 1];
                bldName = DataStore.Instance.GetBuilding(asset.location.building_id)?.building_name ?? "Divine Complex";
                bldId = asset.location.building_id;
                loc = $"{asset.location.floor_id}/{asset.location.room_id}";
                desc = !string.IsNullOrEmpty(asset.identity.name) ? asset.identity.name : "Facility Asset";
                
                int sceneCount = 0;
                ThinAssetInfo[] allSceneAssets = Object.FindObjectsByType<ThinAssetInfo>(FindObjectsSortMode.None);
                foreach(var sai in allSceneAssets) if(sai.assetId == asset.id) sceneCount++;
                if(sceneCount == 0) sceneCount = asset.identity.quantity > 0 ? asset.identity.quantity : 1;
                qty = sceneCount.ToString();

                startD = !string.IsNullOrEmpty(asset.lifecycle.purchase_date) ? asset.lifecycle.purchase_date : "2024-01-15";
                endD = !string.IsNullOrEmpty(asset.lifecycle.retirement_date) ? asset.lifecycle.retirement_date : "2029-01-15";

                if (asset.tasks != null && asset.tasks.Count > 0)
                {
                    var t = asset.tasks[0];
                    priority = t.priority.ToUpper();
                    cost = $"Rs {t.cost_breakdown.total_task_cost:N0}";
                    startD = !string.IsNullOrEmpty(t.schedule.start_date) ? t.schedule.start_date : startD;
                    endD = !string.IsNullOrEmpty(t.schedule.completion_date) ? t.schedule.completion_date : "2024-12-30";
                    remark = !string.IsNullOrEmpty(t.description) ? t.description : "No pending issues";
                }
                else
                {
                    remark = "Maintenance Up-to-date";
                }
            }
            else if (id.StartsWith("FLR_") || id.StartsWith("RM_"))
            {
                sno = id.StartsWith("FLR_") ? "FLR-STAT-01" : "RM-STAT-01";
                List<AssetData> assets = new List<AssetData>();
                List<AssetData> all = DataStore.Instance.GetAllAssets();
                foreach(var a in all) {
                    if(id.StartsWith("FLR_") && a.location.floor_id == id) assets.Add(a);
                    else if(id.StartsWith("RM_") && a.location.room_id == id) assets.Add(a);
                }

                if(assets.Count > 0) {
                    bldName = DataStore.Instance.GetBuilding(assets[0].location.building_id)?.building_name ?? "Main Temple";
                    bldId = assets[0].location.building_id;
                }
                loc = id;
                desc = id.StartsWith("FLR_") ? "Tier-Level Statistics" : "Room Inventory Overview";
                
                float totalVal = 0; string maxP = "NORMAL";
                foreach(var a in assets) {
                    if(a.cost != null) totalVal += a.cost.total_invested;
                    if(a.tasks != null && a.tasks.Count > 0 && a.tasks[0].priority == "high") maxP = "ULTRA";
                }
                qty = assets.Count + " Items";
                cost = $"Rs {totalVal:N0}";
                priority = maxP;
                remark = "System Metrics Synced";
                startD = "2024-01-01"; endD = "2024-12-31";
            }

            AddTableRow(new string[] { sno, bldName, bldId, loc, desc, qty, priority, cost, startD, endD, remark }, false);
        }

        private void AddTableRow(string[] values, bool isHeader)
        {
            GameObject row = new GameObject(isHeader ? "HeaderRow" : "DataRow");
            row.transform.SetParent(tableContent, false);
            
            if (isHeader)
            {
                Image bg = row.AddComponent<Image>();
                bg.color = new Color(0.12f, 0.22f, 0.45f, 0.95f);
                row.AddComponent<LayoutElement>().minHeight = 60;
            }
            else
            {
                GameObject line = new GameObject("Line");
                line.transform.SetParent(row.transform, false);
                line.AddComponent<Image>().color = new Color(1, 1, 1, 0.2f);
                RectTransform rtL = line.GetComponent<RectTransform>();
                rtL.anchorMin = new Vector2(0,0); rtL.anchorMax = new Vector2(1,0); 
                rtL.sizeDelta = new Vector2(0, 2); rtL.anchoredPosition = Vector2.zero;
            }

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 15, 15);
            hlg.spacing = 20;
            hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            for (int i = 0; i < values.Length; i++)
            {
                TextMeshProUGUI cell = CreateCell(row.transform, values[i], isHeader);
                cell.fontSize = isHeader ? 20 : 18; 
                cell.color = isHeader ? new Color(0.8f, 0.95f, 1.0f) : Color.white;
                cell.alignment = TextAlignmentOptions.Left;
                cell.enableWordWrapping = true; 
                cell.overflowMode = TextOverflowModes.Overflow; 

                LayoutElement le = cell.gameObject.AddComponent<LayoutElement>();
                switch (i)
                {
                    case 0: le.preferredWidth = 80; break; 
                    case 1: le.preferredWidth = 220; break; 
                    case 2: le.preferredWidth = 100; break; 
                    case 3: le.preferredWidth = 120; break; 
                    case 4: le.preferredWidth = 240; break; 
                    case 5: le.preferredWidth = 80; break; 
                    case 6: le.preferredWidth = 90; break; 
                    case 7: le.preferredWidth = 110; break; 
                    case 8: le.preferredWidth = 120; break; 
                    case 9: le.preferredWidth = 120; break; 
                    case 10: le.preferredWidth = 280; break; 
                }
            }
        }

        private TextMeshProUGUI CreateCell(Transform parent, string txt, bool bold)
        {
            GameObject g = new GameObject("CellText");
            g.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = g.AddComponent<TextMeshProUGUI>();
            tmp.text = txt;
            if (mulishFont != null) tmp.font = mulishFont;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            return tmp;
        }
    }
}
