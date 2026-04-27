using UnityEngine;

namespace BMS_v2
{
    /// <summary>
    /// Attach to building zones, floors, or rooms to assign an identifier. 
    /// Optionally references a custom transform for SmartCameraController to focus on.
    /// </summary>
    public class BuildingZoneInfo : MonoBehaviour
    {
        [Header("Zone ID (e.g., BLD_001 or FLR_001)")]
        public string zoneId = "BLD_001";

        [Header("Custom Camera View (Drag empty GameObject here)")]
        public Transform customCameraPoint;

        private void OnTriggerEnter(Collider other)
        {
            // Check if the object entering is the player (has SmartCameraController)
            if (other.GetComponent<SmartCameraController>() != null)
            {
                if (!string.IsNullOrEmpty(zoneId) && zoneId.ToUpper().StartsWith("FLR"))
                {
                    // Only trigger if we aren't already in this state or focus
                    if (TourStateManager.Instance != null && 
                        TourStateManager.Instance.CurrentState != TourStateManager.ViewState.InternalFloor &&
                        TourStateManager.Instance.CurrentState != TourStateManager.ViewState.AssetFocus)
                    {
                        Debug.Log($"[BuildingZoneInfo] Player walked into {zoneId}. Activating floor context.");
                        
                        // 1. Update State (Enables asset interaction/search)
                        TourStateManager.Instance.ProcessInteraction(zoneId);
                        
                        // 2. Update UI (Shows floor info in the detail panel)
                        AssetDetailPanel detailPanel = Object.FindAnyObjectByType<AssetDetailPanel>(FindObjectsInactive.Include);
                        if (detailPanel != null)
                        {
                            detailPanel.ShowAsset(zoneId);
                        }
                    }
                }
            }
        }
    }
}
