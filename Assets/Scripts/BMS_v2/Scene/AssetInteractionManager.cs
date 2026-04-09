using UnityEngine;

namespace BMS_v2
{
    public class AssetInteractionManager : MonoBehaviour
    {
        [Header("UI Reference")]
        public AssetDetailPanel detailPanel;

        private void Start()
        {
            if (detailPanel == null)
            {
                detailPanel = FindAnyObjectByType<AssetDetailPanel>(FindObjectsInactive.Include);
            }
        }

        private void Update()
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return; 
                }

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                
                
                RaycastHit[] hits = Physics.RaycastAll(ray, 5000f);
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                GameObject bestHit = null;
                string cleanId = "";
                
                float nearestInteractableDist = float.MaxValue;
                float nearestBlockerDist = float.MaxValue;
                RaycastHit blockerHit = default;

                
                
                foreach (var hit in hits)
                {
                    ThinAssetInfo aInfo = hit.collider.GetComponentInParent<ThinAssetInfo>();
                    BuildingZoneInfo zInfo = hit.collider.GetComponentInParent<BuildingZoneInfo>();

                    if (aInfo != null || zInfo != null)
                    {
                        if (hit.distance < nearestInteractableDist)
                        {
                            nearestInteractableDist = hit.distance;
                            bestHit = hit.collider.gameObject;
                            cleanId = (aInfo != null) ? aInfo.assetId : zInfo.zoneId;
                        }
                    }
                    else if (!hit.collider.isTrigger) 
                    {
                        if (hit.distance < nearestBlockerDist)
                        {
                            nearestBlockerDist = hit.distance;
                            blockerHit = hit;
                        }
                    }
                }

                
                
                if (!string.IsNullOrEmpty(cleanId))
                {
                    if (nearestBlockerDist < nearestInteractableDist - 0.3f)
                    {
                        Debug.Log($"[ClickBlocker] Click on '{cleanId}' blocked by '{blockerHit.collider.gameObject.name}'");
                        cleanId = "";
                        bestHit = null;
                    }
                }

                if (!string.IsNullOrEmpty(cleanId))
                {
                    cleanId = cleanId.Trim();
                    Debug.Log($"[ClickManager] Clicked on: '{cleanId}' via {bestHit.name}");
                    
                    if (detailPanel != null)
                    {
                        detailPanel.ShowAsset(cleanId);
                        if (TourStateManager.Instance != null) TourStateManager.Instance.ProcessInteraction(cleanId);
                        if (SmartCameraController.Instance != null) SmartCameraController.Instance.FocusOnAsset(cleanId);
                    }
                }
                else
                {
                    if (detailPanel != null && hits.Length == 0) detailPanel.HidePanel();
                }
            }
        }
    }
}
