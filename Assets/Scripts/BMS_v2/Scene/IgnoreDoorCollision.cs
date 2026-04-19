using UnityEngine;
using BMS_v2;

namespace BMS_v2
{
    /// <summary>
    /// Utility script attached to door objects to explicitly ignore and bypass physical collisions 
    /// with the SmartCameraController, allowing smooth passage through doorways.
    /// </summary>
    public class IgnoreDoorCollision : MonoBehaviour
    {
        private void Start()
        {
            ApplyIgnore();
        }

        public void ApplyIgnore()
        {
            // Find all colliders in this object and its children
            Collider[] allCols = GetComponentsInChildren<Collider>(true);
            
            // Also check parents just in case the blocker is a parent component
            Collider[] parentCols = GetComponentsInParent<Collider>(true);

            if (SmartCameraController.Instance != null)
            {
                CharacterController camPlayer = SmartCameraController.Instance.GetComponent<CharacterController>();
                if (camPlayer != null)
                {
                    foreach (var col in allCols)
                    {
                        Physics.IgnoreCollision(col, camPlayer, true);
                    }
                    foreach (var col in parentCols)
                    {
                        Physics.IgnoreCollision(col, camPlayer, true);
                    }
                    Debug.Log($"[PhysicsFix] Recursively ignoring {allCols.Length + parentCols.Length} colliders for {gameObject.name}");
                }
            }
        }
    }
}
