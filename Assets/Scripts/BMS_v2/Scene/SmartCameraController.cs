using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BMS_v2
{
    /// <summary>
    /// Human-eye-level first-person camera for BMS building tours.
    ///
    /// Design principles:
    ///   - Camera Y is ALWAYS locked to EYE_HEIGHT above the floor — no floating, no sinking.
    ///   - Movement is horizontal-only (WASD / arrow keys). No vertical drift.
    ///   - Mouse look is smooth with inertia, vertical pitch is clamped to ±75°.
    ///   - Scroll-wheel moves the camera forward/back along its flat ground plane.
    ///   - Asset focus teleports camera to eye-level in front of the asset, then
    ///     smoothly rotates to face it — no aerial swoop.
    ///   - Escape pops the history stack and returns to the previous position.
    /// </summary>
    public class SmartCameraController : MonoBehaviour
    {
        public static SmartCameraController Instance;

        // ─────────────────────────────────────────────────────────────────────────
        //  Inspector knobs
        // ─────────────────────────────────────────────────────────────────────────

        [Header("=== Drag & Drop Focus Points ===")]
        [Tooltip("Map building IDs to exact Front Point Transforms via Drag & Drop.")]
        public List<AssetPointMapping> customFocusPoints = new List<AssetPointMapping>();

        [System.Serializable]
        public struct AssetPointMapping
        {
            [Tooltip("ID of the building (e.g. 'Building1')")]
            public string targetId;
            [Tooltip("Drag the Front Point empty GameObject here")]
            public Transform frontPoint;
        }

        [Header("=== Horizontal Movement ===")]
        [Tooltip("Base walk speed in units/second")]
        public float moveSpeed = 5f;
        [Tooltip("Multiplier applied when Left-Shift is held")]
        public float sprintMultiplier = 2f;
        [Tooltip("Acceleration / deceleration smoothing (lower = silkier)")]
        public float moveSmoothTime = 0.12f;

        [Header("=== Mouse Look ===")]
        [Tooltip("Horizontal and vertical mouse sensitivity")]
        public float mouseSensitivity = 2.5f;
        [Tooltip("Rotation inertia (lower = snappier, higher = dreamier)")]
        public float rotationSmoothTime = 0.07f;
        [Tooltip("Maximum up/down head tilt in degrees")]
        [Range(10f, 85f)]
        public float maxPitchAngle = 75f;

        [Header("=== Scroll Zoom (forward/back) ===")]
        [Tooltip("Units per second at full scroll speed")]
        public float scrollZoomSpeed = 8f;
        [Tooltip("Scroll deceleration smoothing")]
        public float scrollSmoothTime = 0.18f;

        [Header("=== Asset Focus ===")]
        [Tooltip("How far in front of an asset the camera stops (metres)")]
        public float assetViewDistance = 2.5f;
        [Tooltip("Transition speed for focus animations (higher = faster)")]
        public float transitionSpeed = 3.5f;

        [Header("=== Intro Zoom (optional) ===")]
        public bool playIntroZoom = false;

        // ─────────────────────────────────────────────────────────────────────────
        //  Private state
        // ─────────────────────────────────────────────────────────────────────────

        // Rotation
        private float _targetPitch   = 0f;   // up/down  (X euler)
        private float _targetYaw     = 0f;   // left/right (Y euler)
        private float _currentPitch  = 0f;
        private float _currentYaw    = 0f;
        private float _pitchVelocity = 0f;
        private float _yawVelocity   = 0f;

        // Movement
        private Vector3 _smoothMoveVelocity = Vector3.zero;
        private Vector3 _currentMoveVelocity = Vector3.zero;

        // Scroll
        private float _scrollVelocity   = 0f;
        private float _smoothScrollRef  = 0f;

        // Animations / history
        private bool _isAnimating = false;
        private bool _isCrouchedForAsset = false;
        private float _normalWalkingHeight;
        private float _currentLerpTargetHeight = -1f;

        private class CamState
        {
            public Vector3 pos;
            public Quaternion rot;
            public TourStateManager.ViewState viewState;
        }
        private Stack<CamState> _history = new Stack<CamState>();

        private CharacterController _cc;

        // ─────────────────────────────────────────────────────────────────────────
        //  Unity lifecycle
        // ─────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(this); return; }

            transform.localScale = Vector3.one;

            _cc = GetComponent<CharacterController>();
            if (_cc == null) _cc = gameObject.AddComponent<CharacterController>();

            // Slim capsule — fits through doorways, still blocks walls
            _cc.radius     = 0.15f;
            _cc.height     = 0.3f;
            _cc.stepOffset = 0.05f;
            _cc.skinWidth  = 0.02f;
            _cc.center     = Vector3.zero;
            _cc.slopeLimit = 45f;

            Camera cam = GetComponent<Camera>();
            if (cam != null) cam.nearClipPlane = 0.05f;

            // Seed rotation from current transform so scene-placed orientation is respected
            Vector3 euler = transform.eulerAngles;
            _currentPitch = _targetPitch = NormalisePitch(euler.x);
            _currentYaw   = _targetYaw   = euler.y;
        }

        private void Start()
        {
            if (playIntroZoom) StartCoroutine(IntroZoomSequence());
        }

        private void Update()
        {
            if (_isAnimating) return;

            bool overUI = IsPointerOverUI();

            HandleEscapeBack();

            if (!overUI)
            {
                HandleMouseLook();
                HandleScrollZoom();
            }

            ApplySmoothRotation();
            HandleWASDMovement();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Input handlers
        // ─────────────────────────────────────────────────────────────────────────

        public void GoBack()
        {
            if (_history.Count > 0)
            {
                CamState prev = _history.Pop();
                if (TourStateManager.Instance != null)
                {
                    TourStateManager.Instance.ForceState(prev.viewState);
                }
                StopAllCoroutines();
                StartCoroutine(AnimateToState(prev.pos, prev.rot));
            }
        }

        private void HandleEscapeBack()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GoBack();
            }
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _targetYaw   += mouseX;
            _targetPitch -= mouseY;                                      // invert Y = natural head movement
            _targetPitch  = Mathf.Clamp(_targetPitch, -maxPitchAngle, maxPitchAngle);
        }

        private void ApplySmoothRotation()
        {
            _currentPitch = Mathf.SmoothDamp(_currentPitch, _targetPitch, ref _pitchVelocity, rotationSmoothTime);
            _currentYaw   = Mathf.SmoothDamp(_currentYaw,   _targetYaw,   ref _yawVelocity,   rotationSmoothTime);
            transform.rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        private void HandleWASDMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");   // A/D
            float v = Input.GetAxisRaw("Vertical");     // W/S

            // Flat forward/right — ignore camera pitch so walking always stays horizontal
            Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 flatRight   = new Vector3(transform.right.x,   0f, transform.right.z).normalized;

            Vector3 inputDir = (flatRight * h + flatForward * v);
            if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

            // Recover height if we crouched for an asset and are now walking manually
            if (_isCrouchedForAsset && inputDir.sqrMagnitude > 0.001f)
            {
                _isCrouchedForAsset = false;
                _currentLerpTargetHeight = _normalWalkingHeight;
            }

            // Smoothly stand back up to normal walking height
            if (_currentLerpTargetHeight > -1f)
            {
                if (Mathf.Abs(transform.position.y - _currentLerpTargetHeight) > 0.01f)
                {
                    _cc.enabled = false;
                    Vector3 pos = transform.position;
                    pos.y = Mathf.Lerp(pos.y, _currentLerpTargetHeight, Time.deltaTime * transitionSpeed);
                    transform.position = pos;
                    _cc.enabled = true;
                }
                else
                {
                    _currentLerpTargetHeight = -1f; // Done standing up
                }
            }

            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
            Vector3 targetVelocity = inputDir * speed;

            // Smooth acceleration
            _currentMoveVelocity = Vector3.SmoothDamp(
                _currentMoveVelocity, targetVelocity,
                ref _smoothMoveVelocity, moveSmoothTime);

            if (_currentMoveVelocity.sqrMagnitude > 0.0001f)
                _cc.Move(_currentMoveVelocity * Time.deltaTime);
        }

        private void HandleScrollZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            float targetScrollSpeed = scroll * scrollZoomSpeed;

            _scrollVelocity = Mathf.SmoothDamp(
                _scrollVelocity, targetScrollSpeed,
                ref _smoothScrollRef, scrollSmoothTime);

            if (Mathf.Abs(_scrollVelocity) > 0.01f)
            {
                // Move only along the flat ground plane (no vertical scroll drift)
                Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                _cc.Move(flatForward * _scrollVelocity * Time.deltaTime);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Focus system
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Public entry-point called by AssetInteractionManager when the user clicks
        /// an asset or zone. Routes to the correct focus sub-method.
        /// </summary>
        public void FocusOnAsset(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return;
            string id = targetId.Trim();

            Debug.Log($"[SmartCamera] FocusOnAsset: '{id}'");

            // --- 0. Try Drag & Drop custom points first ---
            if (customFocusPoints != null)
            {
                foreach (var mapping in customFocusPoints)
                {
                    if (!string.IsNullOrEmpty(mapping.targetId) &&
                        mapping.targetId.Trim().Equals(id, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (mapping.frontPoint != null)
                        {
                            Debug.Log($"[SmartCamera] Using Drag & Drop Front Point for '{id}'");
                            FocusOnSpecificPoint(mapping.frontPoint, null);
                            return;
                        }
                    }
                }
            }

            // --- Try BuildingZoneInfo first ---
            BuildingZoneInfo[] allZones = Object.FindObjectsByType<BuildingZoneInfo>(FindObjectsSortMode.None);
            foreach (var zone in allZones)
            {
                if (zone.zoneId != null &&
                    zone.zoneId.Trim().Equals(id, System.StringComparison.OrdinalIgnoreCase))
                {
                    // Prefer an explicit camera point authored by the designer
                    Transform customPt = zone.customCameraPoint;
                    if (customPt == null) customPt = zone.transform.Find("Front Point")
                                               ?? zone.transform.Find("FrontPoint")
                                               ?? zone.transform.Find("EntrancePoint");

                    if (customPt != null)
                        FocusOnSpecificPoint(customPt, zone.transform.position);
                    else
                        FocusInFrontOfTransform(zone.transform);

                    return;
                }
            }

            // --- Try ThinAssetInfo ---
            ThinAssetInfo[] allAssets = Object.FindObjectsByType<ThinAssetInfo>(FindObjectsSortMode.None);
            foreach (var asset in allAssets)
            {
                if (asset.assetId != null &&
                    asset.assetId.Trim().Equals(id, System.StringComparison.OrdinalIgnoreCase))
                {
                    FocusInFrontOfTransform(asset.transform);
                    return;
                }
            }

            // --- Name-based fallback ---
            GameObject go = GameObject.Find(id);
            if (go == null)
            {
                foreach (var g in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    if (g.name.Contains(id)) { go = g; break; }
                }
            }

            if (go != null)
                FocusInFrontOfTransform(go.transform);
            else
                Debug.LogWarning($"[SmartCamera] Cannot find any object with ID: '{id}'");
        }

        /// <summary>
        /// A direct helper for Unity UI Events (Buttons). You can drag and drop
        /// your Front Point Transform directly into the Button OnClick inspector.
        /// </summary>
        public void FocusDirectlyOnPoint(Transform draggedPoint)
        {
            if (draggedPoint != null)
                FocusOnSpecificPoint(draggedPoint, null);
        }

        /// <summary>
        /// Focus on a designer-placed camera point (e.g., "FrontPoint" child of a zone).
        /// Camera moves to that point at eye-height and looks toward lookAtPos.
        /// </summary>
        public void FocusOnSpecificPoint(Transform point, Vector3? lookAtPos = null)
        {
            PushCurrentState();
            StopAllCoroutines();

            // Stay EXACTLY at the height authored by the designer
            Vector3 targetPos = point.position;
            
            _isCrouchedForAsset = false; // Reset crouch state so we don't auto-stand up
            _currentLerpTargetHeight = -1f;

            Quaternion targetRot;
            if (lookAtPos.HasValue)
            {
                Vector3 toTarget = new Vector3(lookAtPos.Value.x - targetPos.x, 0f, lookAtPos.Value.z - targetPos.z);
                targetRot = toTarget.sqrMagnitude > 0.01f
                    ? Quaternion.LookRotation(toTarget)
                    : Quaternion.Euler(0f, _currentYaw, 0f);
            }
            else
            {
                targetRot = Quaternion.Euler(0f, _currentYaw, 0f);
            }

            StartCoroutine(AnimateToState(targetPos, targetRot));
        }

        /// <summary>
        /// Compute an eye-level stand-in position in front of a target object and
        /// animate to it, facing the asset at natural head height.
        /// </summary>
        public void FocusInFrontOfTransform(Transform target)
        {
            PushCurrentState();
            StopAllCoroutines();

            // Get the bounding-box centre & size of the target (works for large floor plates too)
            Vector3 assetCenter = target.position;
            float boundsRadius = 0f;
            Collider col = target.GetComponentInChildren<Collider>(true);
            if (col != null)
            {
                bool was = col.enabled;
                if (!was) col.enabled = true;
                assetCenter = col.bounds.center;
                boundsRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
                if (!was) col.enabled = false;
            }

            // Direction from current camera to asset (horizontal only)
            Vector3 toAsset = new Vector3(assetCenter.x - transform.position.x, 0f, assetCenter.z - transform.position.z);
            if (toAsset.sqrMagnitude < 0.01f) toAsset = transform.forward;
            toAsset.Normalize();

            float finalDist = assetViewDistance + boundsRadius;

            // Stand in front of the asset at a comfortable viewing distance (scaled by size)
            Vector3 targetPos = assetCenter - toAsset * finalDist;
            
            // Default: Keep human height so we don't fly into the air for large assets
            float humanHeight = transform.position.y;
            targetPos.y = humanHeight; 
            _isCrouchedForAsset = false;
            _currentLerpTargetHeight = -1f;

            // If the asset is small and on the floor, crouch down for a better angle
            if (assetCenter.y < humanHeight - 0.3f && boundsRadius < 3.0f)
            {
                targetPos.y = assetCenter.y + Mathf.Clamp(boundsRadius * 0.5f, 0.2f, 0.8f); 
                _isCrouchedForAsset = true;
                _normalWalkingHeight = humanHeight; // Save height to stand back up later
            }

            // Look horizontally toward the asset centre
            Vector3 lookDir = assetCenter - targetPos;
            Quaternion targetRot = Quaternion.LookRotation(lookDir);

            // Keep pitch gentle
            Vector3 angles = targetRot.eulerAngles;
            float pitch = NormalisePitch(angles.x);
            pitch = Mathf.Clamp(pitch, -45f, 25f); // Expanded clamp to allow looking down at lower assets
            targetRot = Quaternion.Euler(pitch, angles.y, 0f);

            StartCoroutine(AnimateToState(targetPos, targetRot));
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  History
        // ─────────────────────────────────────────────────────────────────────────

        private void PushCurrentState()
        {
            if (_isAnimating) return;

            TourStateManager.ViewState vs = TourStateManager.Instance != null
                ? TourStateManager.Instance.LastState
                : TourStateManager.ViewState.Global;

            _history.Push(new CamState
            {
                pos       = transform.position,
                rot       = transform.rotation,
                viewState = vs
            });
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Animation coroutine
        // ─────────────────────────────────────────────────────────────────────────

        private IEnumerator AnimateToState(Vector3 targetPos, Quaternion targetRot)
        {
            _isAnimating = true;

            // Kill residual velocity so inertia doesn't fight the animation
            _currentMoveVelocity = Vector3.zero;
            _smoothMoveVelocity  = Vector3.zero;
            _scrollVelocity      = 0f;
            _smoothScrollRef     = 0f;
            _pitchVelocity       = 0f;
            _yawVelocity         = 0f;

            Vector3    startPos = transform.position;
            Quaternion startRot = transform.rotation;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * transitionSpeed;
                float st = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));   // smooth-step easing

                _cc.enabled = false;
                transform.position = Vector3.Lerp(startPos, targetPos, st);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, st);
                _cc.enabled = true;

                yield return null;
            }

            _cc.enabled = false;
            transform.position = targetPos;
            transform.rotation = targetRot;
            _cc.enabled = true;

            // Re-seed rotation tracking from the final orientation
            Vector3 finalEuler = transform.eulerAngles;
            _currentPitch = _targetPitch = NormalisePitch(finalEuler.x);
            _currentYaw   = _targetYaw   = finalEuler.y;

            _isAnimating = false;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Intro zoom (optional, preserves existing feature)
        // ─────────────────────────────────────────────────────────────────────────

        private IEnumerator IntroZoomSequence()
        {
            Vector3    savedPos = transform.position;
            Quaternion savedRot = transform.rotation;

            // Start from a high bird's-eye position
            Vector3    highPos  = new Vector3(savedPos.x, savedPos.y + 40f, savedPos.z - 20f);
            Quaternion highRot  = Quaternion.Euler(45f, savedRot.eulerAngles.y, 0f);

            _cc.enabled = false;
            transform.position = highPos;
            transform.rotation = highRot;
            _cc.enabled = true;

            yield return new WaitForSeconds(0.3f);

            float duration = 3.5f;
            float elapsed  = 0f;
            while (elapsed < duration)
            {
                float st = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _cc.enabled = false;
                transform.position = Vector3.Lerp(highPos, savedPos, st);
                transform.rotation = Quaternion.Slerp(highRot, savedRot, st);
                _cc.enabled = true;
                elapsed += Time.deltaTime;
                yield return null;
            }

            _cc.enabled = false;
            transform.position = savedPos;
            transform.rotation = savedRot;
            _cc.enabled = true;

            _currentPitch = _targetPitch = NormalisePitch(savedRot.eulerAngles.x);
            _currentYaw   = _targetYaw   = savedRot.eulerAngles.y;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Unity stores euler.x in 0-360. Convert to -180..180 so pitch
        /// comparisons and clamping work correctly.</summary>
        private static float NormalisePitch(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private static bool IsPointerOverUI()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null) return false;
            if (es.IsPointerOverGameObject()) return true;
            var sel = es.currentSelectedGameObject;
            return sel != null && sel.GetComponent<TMPro.TMP_InputField>() != null;
        }
    }
}
