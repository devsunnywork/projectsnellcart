using UnityEngine;

namespace BMS_v2
{
    public class ThinAssetInfo : MonoBehaviour
    {
        [Header("Unique ID from assets.json")]
        public string assetId = "AST_001_03_01_FAN_001";

        
        [HideInInspector] public AssetData Data;

        private void OnEnable()
        {
            
            DataLoader.OnDataReady += InitializeData;
        }

        private void OnDisable()
        {
            DataLoader.OnDataReady -= InitializeData;
        }

        private void Start()
        {
            
            if (DataStore.Instance != null && DataStore.Instance.IsDataLoaded)
            {
                InitializeData();
            }
        }

        private void InitializeData()
        {
            if (DataStore.Instance == null) return;

            Data = DataStore.Instance.GetAsset(assetId);

            if (Data != null)
            {
                ApplyHighlightColor();
            }
            else
            {
                Debug.LogWarning($"[ThinAssetInfo] AssetData not found for ID: {assetId} on {gameObject.name}");
            }
        }

        private void ApplyHighlightColor()
        {
            
            if (Data.location != null && !string.IsNullOrEmpty(Data.location.unity_highlight_color))
            {
                if (ColorUtility.TryParseHtmlString(Data.location.unity_highlight_color, out Color col))
                {
                    
                    
                    Renderer rend = GetComponent<Renderer>();
                    if (rend != null)
                    {
                        
                        Material mat = rend.material;
                        if (mat.HasProperty("_Color"))
                        {
                            mat.color = col;
                        }
                    }
                }
            }
        }
    }
}
