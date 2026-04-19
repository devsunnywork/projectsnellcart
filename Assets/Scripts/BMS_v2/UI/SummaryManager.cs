using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BMS_v2
{
    /// <summary>
    /// Controls the Enterprise Asset Report (Data Summary) overlay. 
    /// Dynamically populates a full-screen vertical table showing a comprehensive inventory 
    /// breakdown and cost sum for the currently selected building or area.
    /// </summary>
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
            
            Canvas.ForceUpdateCanvases();
            ScrollRect sr = summaryPanel.GetComponentInChildren<ScrollRect>();
            if (sr != null)
            {
                sr.verticalNormalizedPosition = 1f;
                LayoutRebuilder.ForceRebuildLayoutImmediate(sr.content);
            }
        }

        public void CloseSummary()
        {
            summaryPanel.SetActive(false);
        }

        private void PopulateVerticalTable(string id)
        {
            foreach (Transform child in tableContent) Destroy(child.gameObject);

            string buildingId = "BLD_001";
            if (!string.IsNullOrEmpty(id))
            {
                if (id.StartsWith("BLD_")) buildingId = id;
                else
                {
                    AssetData asset = DataStore.Instance.GetAsset(id);
                    if (asset != null) buildingId = asset.location.building_id;
                }
            }

            BuildingDocument doc = DataStore.Instance.GetBuilding(buildingId);
            titleText.text = "ENTERPRISE ASSET REPORT  —  " + (doc != null ? doc.building_name.ToUpper() : buildingId);
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(1f, 0.88f, 0.25f);
            titleText.characterSpacing = 2f;

            string[] headers = { "S.N", "ASSET ID", "NAME", "LOCATION", "DESCRIPTION", "QTY", "PRIORITY", "MAINT. COST", "REPAIR COST" };
            AddTableRow(headers, true, 0);

            List<AssetData> allAssets = DataStore.Instance.GetAllAssets();
            List<AssetData> buildingAssets = new List<AssetData>();
            foreach (var a in allAssets)
            {
                if (a.location.building_id == buildingId) buildingAssets.Add(a);
            }

            int index = 1;
            foreach (var asset in buildingAssets)
            {
                string sno = index.ToString("D2");
                string aid = asset.id;
                string name = asset.identity.name;
                string loc = $"{asset.location.floor_id}/{asset.location.room_id}";
                string desc = !string.IsNullOrEmpty(asset.identity.category) ? asset.identity.category : "Asset Item";
                string qty = asset.identity.quantity.ToString();
                
                string priority = "III";
                float repairCost = 0;
                if (asset.tasks != null && asset.tasks.Count > 0)
                {
                    string rawPrio = asset.tasks[0].priority.ToUpper();
                    if (rawPrio == "HIGH" || rawPrio == "ULTRA") priority = "I";
                    else if (rawPrio == "MEDIUM") priority = "II";
                    else priority = "III";
                    foreach(var t in asset.tasks) repairCost += t.cost_breakdown.total_task_cost;
                }

                float maintenanceCost = asset.cost != null ? asset.cost.purchase_cost : 0;

                string mCostStr = $"Rs {maintenanceCost:N0}";
                string rCostStr = $"Rs {repairCost:N0}";

                AddTableRow(new string[] { sno, aid, name, loc, desc, qty, priority, mCostStr, rCostStr }, false, index);
                index++;
            }

            // Add bottom summary row
            AddSummaryFooter(buildingAssets);
        }

        private void AddSummaryFooter(List<AssetData> assets)
        {
            float totalMaint = 0, totalRepair = 0;
            int totalQty = 0;
            foreach (var a in assets)
            {
                totalQty += a.identity.quantity;
                if (a.cost != null) totalMaint += a.cost.purchase_cost;
                if (a.tasks != null)
                    foreach (var t in a.tasks) totalRepair += t.cost_breakdown.total_task_cost;
            }

            GameObject footer = new GameObject("FooterRow");
            footer.transform.SetParent(tableContent, false);

            Image bg = footer.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.18f, 0.12f, 0.95f);
            footer.AddComponent<LayoutElement>().minHeight = 55;

            HorizontalLayoutGroup hlg = footer.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(25, 25, 8, 8);
            hlg.spacing = 10;
            hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            string[] footerVals = { "", "", "", "", "TOTAL", totalQty.ToString(), "", $"Rs {totalMaint:N0}", $"Rs {totalRepair:N0}" };
            for (int i = 0; i < footerVals.Length; i++)
            {
                TextMeshProUGUI cell = CreateCell(footer.transform, footerVals[i], true);
                cell.fontSize = 17;
                cell.color = new Color(0.3f, 1f, 0.5f);
                cell.alignment = TextAlignmentOptions.Left;
                cell.enableWordWrapping = false;

                LayoutElement le = cell.gameObject.AddComponent<LayoutElement>();
                switch (i)
                {
                    case 0: le.minWidth = 45; le.preferredWidth = 55; break;
                    case 1: le.minWidth = 160; le.flexibleWidth = 1; break;
                    case 2: le.minWidth = 180; le.flexibleWidth = 1.5f; break;
                    case 3: le.minWidth = 160; le.flexibleWidth = 1; break;
                    case 4: le.minWidth = 180; le.flexibleWidth = 1.5f; break;
                    case 5: le.minWidth = 45; le.preferredWidth = 55; break;
                    case 6: le.minWidth = 90; le.flexibleWidth = 0.7f; break;
                    case 7: le.minWidth = 120; le.flexibleWidth = 0.8f; break;
                    case 8: le.minWidth = 120; le.flexibleWidth = 0.8f; break;
                }
            }
        }

        private void AddTableRow(string[] values, bool isHeader, int rowIndex)
        {
            GameObject row = new GameObject(isHeader ? "HeaderRow" : $"DataRow_{rowIndex}");
            row.transform.SetParent(tableContent, false);
            
            Image bg = row.AddComponent<Image>();
            if (isHeader)
            {
                bg.color = new Color(0.06f, 0.12f, 0.22f, 0.98f);
                row.AddComponent<LayoutElement>().minHeight = 55;
            }
            else
            {
                bool isEven = rowIndex % 2 == 0;
                bg.color = isEven 
                    ? new Color(0.04f, 0.05f, 0.08f, 0.85f) 
                    : new Color(0.07f, 0.08f, 0.12f, 0.85f);
                row.AddComponent<LayoutElement>().minHeight = 50;
            }

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(25, 25, 6, 6);
            hlg.spacing = 10;
            hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            for (int i = 0; i < values.Length; i++)
            {
                TextMeshProUGUI cell = CreateCell(row.transform, values[i], isHeader);
                cell.fontSize = isHeader ? 18 : 17; 
                cell.characterSpacing = isHeader ? 1.5f : 0.3f;
                cell.alignment = TextAlignmentOptions.Left;
                cell.enableWordWrapping = false; 
                cell.overflowMode = TextOverflowModes.Ellipsis; 

                if (isHeader)
                {
                    cell.color = new Color(0.45f, 0.85f, 1.0f);
                }
                else
                {
                    cell.color = new Color(0.88f, 0.90f, 0.95f);
                }

                if (!isHeader && i == 6)
                {
                    if (values[i] == "I") 
                        cell.color = new Color(1f, 0.35f, 0.35f);
                    else if (values[i] == "II") 
                        cell.color = new Color(1f, 0.75f, 0.25f);
                    else 
                        cell.color = new Color(0.4f, 0.95f, 0.55f);
                }

                if (!isHeader && (i == 7 || i == 8))
                {
                    cell.color = new Color(0.7f, 0.85f, 1f);
                }

                LayoutElement le = cell.gameObject.AddComponent<LayoutElement>();
                switch (i)
                {
                    case 0: le.minWidth = 45; le.preferredWidth = 55; break;            // S.N
                    case 1: le.minWidth = 160; le.flexibleWidth = 1; break;              // ID
                    case 2: le.minWidth = 180; le.flexibleWidth = 1.5f; break;           // Name
                    case 3: le.minWidth = 160; le.flexibleWidth = 1; break;              // Location
                    case 4: le.minWidth = 180; le.flexibleWidth = 1.5f; break;           // Description
                    case 5: le.minWidth = 45; le.preferredWidth = 55; break;             // Qty
                    case 6: le.minWidth = 90; le.flexibleWidth = 0.7f; break;            // Priority
                    case 7: le.minWidth = 120; le.flexibleWidth = 0.8f; break;           // M. Cost
                    case 8: le.minWidth = 120; le.flexibleWidth = 0.8f; break;           // R. Cost
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
