using UnityEngine;

namespace BMS_v2
{
    /// <summary>
    /// Attach to physical 3D asset objects in the scene. Handles visual indicators (like floating dots) 
    /// and stores the unique assetId for interaction systems to fetch data from the DataStore.
    /// </summary>
    public class ThinAssetInfo : MonoBehaviour
    {
        [Header("Unique ID from assets.json")]
        public string assetId = "AST_001_03_01_FAN_001";

        [Header("Clickable Dot Indicator")]
        [Tooltip("Size of the floating dot in the asset center")]
        public float dotSize = 1.5f;
        [Tooltip("Vertical offset from the asset's center")]
        public float dotHeightOffset = 0f;
        [Tooltip("Color of the glowing dot")]
        public Color dotColor = new Color(0f, 0.9f, 1f, 1f); // Cyan glow
        [Tooltip("Pulse speed (how fast it breathes)")]
        public float pulseSpeed = 2f;
        [Tooltip("How much the dot scales when pulsing (0-1)")]
        public float pulseAmount = 0.3f;

        [HideInInspector] public AssetData Data;

        private GameObject _dotIndicator;
        private Vector3 _dotBaseScale;
        private float _pulseTimer;

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

            CreateDotIndicator();
        }

        private void Update()
        {
            if (_dotIndicator == null || !_dotIndicator.activeSelf) return;

            // --- Pulse animation (gentle scale breathing) ---
            _pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(_pulseTimer) * pulseAmount;
            _dotIndicator.transform.localScale = _dotBaseScale * pulse;

            // --- Billboard: always face the camera ---
            if (Camera.main != null)
            {
                _dotIndicator.transform.LookAt(Camera.main.transform);
                // Flip so front faces camera
                _dotIndicator.transform.Rotate(0, 180, 0);
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

        /// <summary>
        /// Creates a small glowing sphere dot above this asset at runtime.
        /// </summary>
        private void CreateDotIndicator()
        {
            // Calculate the top of the asset using its bounds
            Vector3 topPoint = transform.position;

            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                topPoint = rend.bounds.center;
            }
            else
            {
                Collider col = GetComponentInChildren<Collider>();
                if (col != null)
                {
                    topPoint = col.bounds.center;
                }
            }

            topPoint.y += dotHeightOffset;

            // Create the dot sphere
            _dotIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _dotIndicator.name = $"DotIndicator_{assetId}";
            _dotIndicator.transform.SetParent(transform);
            _dotIndicator.transform.position = topPoint;

            // Compensate for parent's world scale so all dots are the same size
            Vector3 parentScale = transform.lossyScale;
            _dotBaseScale = new Vector3(
                dotSize / Mathf.Max(parentScale.x, 0.001f),
                dotSize / Mathf.Max(parentScale.y, 0.001f),
                dotSize / Mathf.Max(parentScale.z, 0.001f)
            );
            _dotIndicator.transform.localScale = _dotBaseScale;

            // Remove collider so the dot doesn't block raycasts to the actual asset
            Collider dotCol = _dotIndicator.GetComponent<Collider>();
            if (dotCol != null) Object.Destroy(dotCol);

            // Create unlit glowing material
            Renderer dotRend = _dotIndicator.GetComponent<Renderer>();
            if (dotRend != null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = dotColor;
                dotRend.material = mat;

                // Make it render on top of everything (overlay feel)
                mat.renderQueue = 4000;
            }

            // Put dot on Ignore Raycast layer so it never blocks clicks
            _dotIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        private void OnDestroy()
        {
            if (_dotIndicator != null)
            {
                Destroy(_dotIndicator);
            }
        }
    }
}

