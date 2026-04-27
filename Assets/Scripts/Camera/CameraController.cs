using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public enum CameraPhase { Aerial, Transitioning, Interior }
    public CameraPhase currentPhase = CameraPhase.Aerial;

    [System.Serializable]
    public struct CameraState
    {
        public Vector3 position;
        public Quaternion rotation;
        public CameraPhase phase;
    }

    private Stack<CameraState> history = new Stack<CameraState>();

    [Header("Aerial Settings")]
    public Vector3 startPosition = new Vector3(0f, 80f, -60f);
    public Vector3 startRotation = new Vector3(50f, 0f, 0f);
    public float panSpeed = 0.2f;
    public float zoomSpeed = 35f;
    public float minHeight = 5f;
    public float maxHeight = 400f;

    [Header("Interior - Movement")]
    public float moveSpeed = 6f;
    public float scrollStep = 3.5f; 
    public float positionSmoothing = 12f;
    public float rotationSmoothing = 14f;

    [Header("Interior - Look Sensitivity")]
    [Range(0.01f, 3f)] public float mouseSensitivityX = 0.5f;
    [Range(0.01f, 3f)] public float mouseSensitivityY = 0.5f;

    [Header("Transition")]
    public float flyDuration = 1.0f;

    [Header("UI References (For ESC Optimization)")]
    public AerialUIManager aerialUIManager;
    public SearchManager searchManager;

    private Vector2 lastMousePos;
    private float rotX, rotY;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isFlying = false;

    private Vector2 mouseDownPos;
    public bool isDragging = false;
    private float dragThreshold = 5f;

    void Start()
    {
        targetPosition = startPosition;
        targetRotation = Quaternion.Euler(startRotation);
        transform.position = startPosition;
        transform.rotation = targetRotation;
    }

    void Update()
    {
        // Check if ANY panel is active (Building/Floor/Object or Search)
        bool uiIsShowing = (aerialUIManager != null && aerialUIManager.AnyPanelActive()) ||
                           (searchManager != null && searchManager.fullPanelObject != null && searchManager.fullPanelObject.activeSelf);

        // Handle ESC key for Back logic
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !isFlying)
        {
            // Only fly back if NO UI is showing. 
            // If UI is showing, the individual scripts will close them on ESC.
            if (!uiIsShowing) 
            {
                FlyBack();
            }
        }

        if (currentPhase == CameraPhase.Aerial) 
        {
            if (!uiIsShowing) AerialUpdate();
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
        else if (currentPhase == CameraPhase.Interior)
        {
            if (!isFlying && !uiIsShowing) InteriorUpdate();
            
            transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothing * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
        }
    }

    void AerialUpdate()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            float nextY = targetPosition.y - Mathf.Sign(scroll) * zoomSpeed * Time.deltaTime * 30f;
            targetPosition.y = Mathf.Clamp(nextY, minHeight, maxHeight);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
            lastMousePos = Mouse.current.position.ReadValue();

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = (Vector2)Mouse.current.position.ReadValue() - lastMousePos;
            targetPosition -= new Vector3(delta.x, 0, delta.y) * panSpeed * Time.deltaTime * 15f;
            lastMousePos = Mouse.current.position.ReadValue();
        }
    }

    void InteriorUpdate()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseDownPos = Mouse.current.position.ReadValue();
            lastMousePos = mouseDownPos;
            isDragging = false;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 currentPos = Mouse.current.position.ReadValue();
            float dist = Vector2.Distance(currentPos, mouseDownPos);
            
            if (dist > dragThreshold)
            {
                isDragging = true;
                Vector2 delta = currentPos - lastMousePos;
                rotY += delta.x * mouseSensitivityX * 0.18f;
                rotX -= delta.y * mouseSensitivityY * 0.18f;
                rotX = Mathf.Clamp(rotX, -70f, 70f);
                targetRotation = Quaternion.Euler(rotX, rotY, 0f);
                lastMousePos = currentPos;
            }
        }
        else { isDragging = false; }

        Vector3 camForward = transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = transform.right; camRight.y = 0; camRight.Normalize();

        Vector3 moveInput = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) moveInput += camForward;
        if (Keyboard.current.sKey.isPressed) moveInput -= camForward;
        if (Keyboard.current.aKey.isPressed) moveInput -= camRight;
        if (Keyboard.current.dKey.isPressed) moveInput += camRight;
        
        if (Keyboard.current.eKey.isPressed) targetPosition += Vector3.up * moveSpeed * Time.deltaTime;
        if (Keyboard.current.qKey.isPressed) targetPosition -= Vector3.up * moveSpeed * Time.deltaTime;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0) targetPosition += camForward * Mathf.Sign(scroll) * scrollStep;

        targetPosition += moveInput.normalized * moveSpeed * Time.deltaTime;
    }

    public bool IsUserDragging() => isDragging;

    public void SaveCurrentState()
    {
        history.Push(new CameraState { 
            position = transform.position, 
            rotation = transform.rotation, 
            phase = currentPhase 
        });
    }

    public void FlyToDirect(Vector3 pos, Quaternion rot)
    {
        SaveCurrentState();
        StartCoroutine(FlyToFrontWithInit(pos, rot));
    }

    public void FlyToBuilding(Transform marker) 
    { 
        if(marker) 
        {
            SaveCurrentState();
            StartCoroutine(FlyToFrontWithInit(marker.position, marker.rotation)); 
        }
    }

    public void FlyBack()
    {
        if (history.Count > 0)
        {
            CameraState prevState = history.Pop();
            StartCoroutine(FlyBackTransition(prevState));
        }
        else if (currentPhase != CameraPhase.Aerial)
        {
            FlyBackToAerial();
        }
    }

    IEnumerator FlyBackTransition(CameraState state)
    {
        currentPhase = CameraPhase.Transitioning;
        isFlying = true;
        yield return StartCoroutine(SmoothMoveTransition(state.position, state.rotation, flyDuration));
        targetPosition = state.position;
        targetRotation = state.rotation;
        rotY = transform.eulerAngles.y; rotX = transform.eulerAngles.x;
        if (rotX > 180f) rotX -= 360f;
        currentPhase = state.phase;
        isFlying = false;
    }

    IEnumerator FlyToFrontWithInit(Vector3 pos, Quaternion rot)
    {
        currentPhase = CameraPhase.Transitioning;
        isFlying = true;
        yield return StartCoroutine(SmoothMoveTransition(pos, rot, flyDuration));
        targetPosition = pos; targetRotation = rot;
        rotY = transform.eulerAngles.y; rotX = transform.eulerAngles.x;
        if (rotX > 180f) rotX -= 360f;
        currentPhase = CameraPhase.Interior;
        isFlying = false;
    }

    IEnumerator SmoothMoveTransition(Vector3 toPos, Quaternion toRot, float duration)
    {
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float s = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fromPos, toPos, s);
            transform.rotation = Quaternion.Slerp(fromRot, toRot, s);
            yield return null;
        }
        transform.position = toPos;
        transform.rotation = toRot;
    }

    public void FlyBackToAerial() { StartCoroutine(BackToAerial()); }

    IEnumerator BackToAerial()
    {
        currentPhase = CameraPhase.Transitioning;
        isFlying = true;
        yield return StartCoroutine(SmoothMoveTransition(startPosition, Quaternion.Euler(startRotation), 1.5f));
        targetPosition = startPosition;
        targetRotation = Quaternion.Euler(startRotation);
        currentPhase = CameraPhase.Aerial;
        isFlying = false;
        history.Clear(); // Reset history when going back to start
    }
}
