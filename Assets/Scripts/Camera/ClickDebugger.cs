using UnityEngine;
using UnityEngine.InputSystem;

public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 2f);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("HIT: " + hit.collider.gameObject.name + " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
            }
            else
            {
                Debug.Log("MISS - Koi bhi cheez hit nahi hui. Collider missing hai!");
            }
        }
    }
}
