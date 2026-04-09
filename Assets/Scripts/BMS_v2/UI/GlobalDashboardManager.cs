using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BMS_v2
{
    public class GlobalDashboardManager : MonoBehaviour
    {
        [Header("Top Panel Elements")]
        public TextMeshProUGUI totalInvestmentText;
        public TextMeshProUGUI activeAssetsText;
        public TextMeshProUGUI warrantyAlertsText;

        private void Start()
        {
            
            if (DataStore.Instance != null)
            {
                DataStore.Instance.OnDataReady += BindDashboardData;
                
                
                if (DataStore.Instance.IsDataLoaded)
                {
                    BindDashboardData();
                }
            }
        }

        private void OnDestroy()
        {
            if (DataStore.Instance != null)
            {
                DataStore.Instance.OnDataReady -= BindDashboardData;
            }
        }

        
        public void BindDashboardData()
        {
            float totalSystemCost = 0;
            int totalActiveUnits = 0;
            int warrantyCount = 0;

            List<AssetData> allAssets = DataStore.Instance.GetAllAssets(); 

            foreach (var asset in allAssets) 
            {
                
                if (asset.cost != null)
                {
                    totalSystemCost += asset.cost.total_invested;
                }

                
                if (asset.lifecycle != null && asset.lifecycle.status == "active")
                {
                    totalActiveUnits++;
                }

                
                if (asset.warranty != null && asset.warranty.status == "under_warranty")
                {
                    warrantyCount++;
                }
            }

            
            if (totalInvestmentText != null)
            {
                totalInvestmentText.text = $"Rs {totalSystemCost:N0}";
            }
            if (activeAssetsText != null)
            {
                activeAssetsText.text = $"{totalActiveUnits} Units";
            }
            if (warrantyAlertsText != null)
            {
                warrantyAlertsText.text = $"{warrantyCount} Covered";
            }

            Debug.Log($"[Dashboard] Calculated Total Cost: {totalSystemCost}, Active: {totalActiveUnits}");
        }
    }
}
