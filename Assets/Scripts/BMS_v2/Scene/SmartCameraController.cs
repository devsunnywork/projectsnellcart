using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BMS_v2
{
    /// <summary>
    /// Advanced camera system for touring the building map. 
    /// Supports smooth WASD+QE movement, drag rotation, scroll zoom, and dynamically focusing on specific models or zones.
    /// </summary>
    public class SmartCameraController : MonoBehaviour
    {
        public static SmartCameraController Instance;

        [Header("=== Smooth Movement ===")]
        public float moveSpeed = 35f;
        public float sprintMultiplier = 2f;
        [Tooltip("How quickly the camera accelerates to target speed (lower = smoother)")]
        public float moveSmoothTime = 0.15f;

        [Header("=== Smooth Mouse Rotation ===")]
        public float mouseSensitivity = 3f;
        [Tooltip("How quickly rotation catches up to the target (lower = smoother)")]
        public float rotationSmoothTime = 0.08f;

        [Header("=== Smooth Scroll Zoom ===")]
        public float scrollZoomSpeed = 600f;
        [Tooltip("How quickly scroll zoom accelerates/decelerates")]
        public float scrollSmoothTime = 0.2f;

        [Header("Focus Smoothing")]
        public float transitionSpeed = 4f;
        public float defaultFocusDistance = 3f;

        // --- Smooth rotation state ---
        private float targetRotX = 0f;
        private float targetRotY = 0f;
        private float currentRotX = 0f;
        private float currentRotY = 0f;
        private float rotVelocityX = 0f;
        private float rotVelocityY = 0f;

        // --- Smooth movement state ---
        private Vector3 currentMoveVelocity = Vector3.zero;
        private Vector3 smoothMoveVelocity = Vector3.zero; // ref for SmoothDamp

        // --- Smooth scroll state ---
        private float currentScrollVelocity = 0f;
        private float smoothScrollRef = 0f;

        private CharacterController _characterController;

        private class CamState
        {
            public Vector3 pos;
            public Quaternion rot;
            public TourStateManager.ViewState viewState;
        }

        private Stack<CamState> history = new Stack<CamState>();
        private bool isAnimating = false;

        public bool playIntroZoom = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            // Force scale to 1 so the collision radius stays accurate in world space
            transform.localScale = Vector3.one;

            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
            {
                _characterController = gameObject.AddComponent<CharacterController>();
            }

            // Balanced collision size: stops before entering the wall but fits through doors
            _characterController.radius = 0.15f;
            _characterController.height = 0.2f;
            _characterController.stepOffset = 0.1f;
            _characterController.skinWidth = 0.02f;
            _characterController.center = Vector3.zero;
            _characterController.slopeLimit = 90f;

            // Set camera near clip to be smaller than the radius to prevent visual clipping
            Camera cam = GetComponent<Camera>();
            if (cam != null) cam.nearClipPlane = 0.05f;

            Vector3 euler = transform.eulerAngles;
            currentRotX = targetRotX = (euler.x > 180) ? euler.x - 360 : euler.x;
            currentRotY = targetRotY = euler.y;
        }

        private void Start()
        {

        }

        private IEnumerator StartZoomSequence()
        {
            Vector3 startPos = new Vector3(transform.position.x, 2000f, transform.position.z + 500f);
            Quaternion startRot = Quaternion.Euler(65, transform.eulerAngles.y, 0);

            Vector3 targetPos = transform.position;
            Quaternion targetRot = transform.rotation;

            transform.position = startPos;
            transform.rotation = startRot;

            yield return new WaitForSeconds(0.5f);

            float duration = 4.0f;
            float elapsed = 0;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float smoothT = Mathf.SmoothStep(0, 1, t);

                transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;

            targetRotX = currentRotX = transform.eulerAngles.x;
            targetRotY = currentRotY = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (isAnimating) return;

            bool isOverUI = false;
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    isOverUI = true;
                }

                var cur = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                if (cur != null && cur.GetComponent<TMPro.TMP_InputField>() != null)
                {
                    isOverUI = true;
                }
            }

            // ========== ESCAPE → GO BACK ==========
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (history.Count > 0)
                {
                    CamState prev = history.Pop();
                    if (TourStateManager.Instance != null) TourStateManager.Instance.ForceState(prev.viewState);
                    StopAllCoroutines();
                    StartCoroutine(AnimateCamera(prev.pos, prev.rot));
                }
            }

            // ========== SMOOTH MOUSE ROTATION (cursor visible, no button needed) ==========
            // Rotates camera when mouse is NOT over any UI element
            if (!isOverUI)
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                targetRotY += mouseX;
                targetRotX -= mouseY;
                targetRotX = Mathf.Clamp(targetRotX, -85f, 85f);
            }

            // SmoothDamp rotation towards target (runs every frame for smooth deceleration)
            currentRotX = Mathf.SmoothDamp(currentRotX, targetRotX, ref rotVelocityX, rotationSmoothTime);
            currentRotY = Mathf.SmoothDamp(currentRotY, targetRotY, ref rotVelocityY, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(currentRotX, currentRotY, 0f);

            // ========== SMOOTH WASD MOVEMENT (works in both locked/unlocked) ==========
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");

                Vector3 flatForward = transform.forward;
                flatForward.y = 0;
                flatForward = flatForward.normalized;

                Vector3 flatRight = transform.right;
                flatRight.y = 0;
                flatRight = flatRight.normalized;

                Vector3 inputDir = flatRight * h + flatForward * v;

                // Vertical movement (Q/E)
                if (Input.GetKey(KeyCode.E)) inputDir += Vector3.up;
                if (Input.GetKey(KeyCode.Q)) inputDir -= Vector3.up;

                if (inputDir.sqrMagnitude > 1f) inputDir = inputDir.normalized;

                // Sprint with Left Shift
                float currentSpeed = moveSpeed;
                if (Input.GetKey(KeyCode.LeftShift)) currentSpeed *= sprintMultiplier;

                Vector3 targetVelocity = inputDir * currentSpeed;

                // SmoothDamp velocity for buttery smooth acceleration/deceleration
                currentMoveVelocity = Vector3.SmoothDamp(
                    currentMoveVelocity,
                    targetVelocity,
                    ref smoothMoveVelocity,
                    moveSmoothTime
                );

                if (currentMoveVelocity.sqrMagnitude > 0.001f)
                {
                    _characterController.Move(currentMoveVelocity * Time.deltaTime);
                }

                // ========== SMOOTH SCROLL ZOOM ==========
                float scrollInput = Input.mouseScrollDelta.y;
                float targetScrollSpeed = scrollInput * scrollZoomSpeed;

                currentScrollVelocity = Mathf.SmoothDamp(
                    currentScrollVelocity,
                    targetScrollSpeed,
                    ref smoothScrollRef,
                    scrollSmoothTime
                );

                if (Mathf.Abs(currentScrollVelocity) > 0.01f)
                {
                    _characterController.Move(transform.forward * currentScrollVelocity * Time.deltaTime);
                }
            }

            // Minimum height clamp
            float minHeight = 0.5f;
            if (transform.position.y < minHeight)
            {
                transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
            }
        }

        public void FocusOnAsset(string targetAssetId)
        {
            if (string.IsNullOrEmpty(targetAssetId)) return;
            string cleanTarget = targetAssetId.Trim();

            Debug.Log($"[SmartCamera] Focusing on ID: {cleanTarget}");

            BuildingZoneInfo[] allZones = Object.FindObjectsByType<BuildingZoneInfo>(FindObjectsSortMode.None);
            BuildingZoneInfo bestZone = null;

            foreach (var zone in allZones)
            {
                if (zone.zoneId != null && zone.zoneId.Trim().Equals(cleanTarget, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (bestZone == null || (bestZone.customCameraPoint == null && zone.customCameraPoint != null))
                    {
                        bestZone = zone;
                    }
                }
            }

            if (bestZone != null)
            {
                Transform autoFront = bestZone.transform.Find("Front Point");
                if (autoFront == null) autoFront = bestZone.transform.Find("FrontPoint");
                if (autoFront == null) autoFront = bestZone.transform.Find("EntrancePoint");

                Transform finalTarget = (bestZone.customCameraPoint != null) ? bestZone.customCameraPoint : autoFront;

                if (finalTarget != null)
                {
                    Debug.Log($"[SmartCamera] Finalizing focus on '{finalTarget.name}' for {cleanTarget}. Forced Front View Active.");
                    FocusOnSpecificPoint(finalTarget, bestZone.transform.position);
                }
                else
                {
                    Debug.LogWarning($"[SmartCamera] Found {cleanTarget} but absolutely no Presentation point. Falling back to center.");
                    FocusOnTransform(bestZone.transform);
                }
                return;
            }

            ThinAssetInfo[] allAssetsInScene = Object.FindObjectsByType<ThinAssetInfo>(FindObjectsSortMode.None);
            ThinAssetInfo targetAssetInfo = null;

            foreach (var assetInfo in allAssetsInScene)
            {
                if (assetInfo.assetId != null && assetInfo.assetId.Trim().Equals(cleanTarget, System.StringComparison.OrdinalIgnoreCase))
                {
                    targetAssetInfo = assetInfo;
                    break;
                }
            }

            if (targetAssetInfo != null)
            {
                Debug.Log($"[SmartCamera] Found Asset {cleanTarget} via Metadata, using Transform focus.");
                FocusOnTransform(targetAssetInfo.transform);
            }
            else
            {
                GameObject fallbackObj = GameObject.Find(cleanTarget);
                if (fallbackObj == null) {
                    GameObject[] allGOs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                    foreach(var go in allGOs) {
                        if (go.name.Contains(cleanTarget)) {
                            fallbackObj = go;
                            break;
                        }
                    }
                }

                if (fallbackObj != null) {
                    Debug.Log($"[SmartCamera] Found Asset {cleanTarget} via Name Fallback, using Transform focus.");
                    FocusOnTransform(fallbackObj.transform);
                } else {
                    Debug.LogWarning($"[SmartCamera] Could not find any Zone, Metadata or GameObject with ID: {cleanTarget}");
                }
            }
        }

        public void FocusOnSpecificPoint(Transform customPoint, Vector3? lookAtPos = null)
        {
            PushCurrentState();
            StopAllCoroutines();

            Quaternion targetRot;
            if (lookAtPos.HasValue)
            {
                Vector3 toTarget = (lookAtPos.Value - customPoint.position);
                toTarget.y = 0;
                targetRot = (toTarget.sqrMagnitude > 0.01f) ? Quaternion.LookRotation(toTarget) : transform.rotation;
            }
            else
            {
                targetRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }

            StartCoroutine(AnimateCamera(customPoint.position, targetRot));
        }

        public void FocusOnTransform(Transform target)
        {
            PushCurrentState();

            Vector3 centerReference = target.position;
            float dynamicDistance = defaultFocusDistance;

            Collider col = target.GetComponentInChildren<Collider>(true);
            if (col != null)
            {
                bool wasEnabled = col.enabled;
                if (!wasEnabled) col.enabled = true;
                centerReference = col.bounds.center;

                dynamicDistance = Mathf.Max(defaultFocusDistance, col.bounds.extents.magnitude * 1.5f);
                if (!wasEnabled) col.enabled = false;
            }

            Vector3 viewDirection = centerReference - transform.position;
            Vector3 flatDirection = new Vector3(viewDirection.x, 0, viewDirection.z).normalized;
            if (flatDirection.sqrMagnitude < 0.1f) flatDirection = transform.forward;

            Vector3 targetPos = centerReference - (flatDirection * dynamicDistance);
            targetPos.y = centerReference.y + (dynamicDistance * 0.3f);

            Quaternion targetRot = Quaternion.LookRotation(centerReference - targetPos);

            StopAllCoroutines();
            StartCoroutine(AnimateCamera(targetPos, targetRot));
        }

        private void PushCurrentState()
        {
            if (!isAnimating)
            {
                TourStateManager.ViewState historicalVs = TourStateManager.Instance != null
                    ? TourStateManager.Instance.LastState
                    : TourStateManager.ViewState.Global;

                history.Push(new CamState { pos = transform.position, rot = transform.rotation, viewState = historicalVs });
            }
        }

        private IEnumerator AnimateCamera(Vector3 targetPos, Quaternion targetRot)
        {
            isAnimating = true;

            // Kill any residual velocity so the camera doesn't drift after animation
            currentMoveVelocity = Vector3.zero;
            smoothMoveVelocity = Vector3.zero;
            currentScrollVelocity = 0f;
            smoothScrollRef = 0f;
            rotVelocityX = 0f;
            rotVelocityY = 0f;

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * transitionSpeed;
                float smoothT = t * t * (3f - 2f * t);

                Vector3 newPos = Vector3.Lerp(startPos, targetPos, smoothT);

                // Use field directly to avoid collision checks during focus animation
                _characterController.enabled = false;
                transform.position = newPos;
                transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
                _characterController.enabled = true;

                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;

            Vector3 finalAngles = transform.eulerAngles;
            currentRotX = targetRotX = (finalAngles.x > 180) ? finalAngles.x - 360 : finalAngles.x;
            currentRotY = targetRotY = finalAngles.y;

            isAnimating = false;
        }
    }
}

