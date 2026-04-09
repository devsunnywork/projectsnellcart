using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BMS_v2
{
    public class SmartCameraController : MonoBehaviour
    {
        public static SmartCameraController Instance;

        [Header("WASD Movement & View")]
        public float moveSpeed = 35f;
        public float lookSpeed = 5f; 
        public float rotationSmoothing = 15f; 
        public float scrollZoomSpeed = 600f; 
        
        private float rotationX = 0;
        private float rotationY = 0;
        private float targetRotX = 0;
        private float targetRotY = 0;

        [Header("Focus Smoothing")]
        public float transitionSpeed = 4f; 
        public float defaultFocusDistance = 3f; 

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

            Vector3 euler = transform.eulerAngles;
            rotationX = targetRotX = euler.x;
            rotationY = targetRotY = euler.y;
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
            
            targetRotX = rotationX = transform.eulerAngles.x;
            targetRotY = rotationY = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (isAnimating) return; 

            
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) 
                {
                    
                    ApplySmoothing();
                    return;
                }
                
                var cur = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                if (cur != null && cur.GetComponent<TMPro.TMP_InputField>() != null) 
                {
                    ApplySmoothing();
                    return; 
                }
            }

            
            float h = Input.GetAxisRaw("Horizontal"); 
            float v = Input.GetAxisRaw("Vertical"); 

            Vector3 flatForward = transform.forward;
            flatForward.y = 0;
            flatForward = flatForward.normalized;

            Vector3 flatRight = transform.right;
            flatRight.y = 0;
            flatRight = flatRight.normalized;

            Vector3 move = flatRight * h + flatForward * v;
            
            if (Input.GetKey(KeyCode.E)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;
            
            if (move.magnitude > 0)
            {
                transform.position += move.normalized * moveSpeed * Time.deltaTime;
            }

            
            float minHeight = 0.5f;
            if (transform.position.y < minHeight)
            {
                transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
            }

            
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0)
            {
                transform.position += transform.forward * scroll * scrollZoomSpeed * Time.deltaTime;
            }

            
            if (Input.GetMouseButton(1))
            {
                targetRotX -= Input.GetAxisRaw("Mouse Y") * lookSpeed;
                targetRotY += Input.GetAxisRaw("Mouse X") * lookSpeed;
                
                
                targetRotX = Mathf.Clamp(targetRotX, -85f, 85f);
            }

            ApplySmoothing();

            
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
        }

        private void ApplySmoothing()
        {
            
            rotationX = Mathf.Lerp(rotationX, targetRotX, Time.deltaTime * rotationSmoothing);
            rotationY = Mathf.Lerp(rotationY, targetRotY, Time.deltaTime * rotationSmoothing);
            
            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
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
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * transitionSpeed;
                
                float smoothT = t * t * (3f - 2f * t); 
                
                transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
                yield return null;
            }
            
            transform.position = targetPos;
            transform.rotation = targetRot;

            
            Vector3 finalAngles = transform.eulerAngles;
            rotationX = targetRotX = (finalAngles.x > 180) ? finalAngles.x - 360 : finalAngles.x;
            rotationY = targetRotY = finalAngles.y;
            
            isAnimating = false;
        }
    }
}
