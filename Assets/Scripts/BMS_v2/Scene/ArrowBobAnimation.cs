using UnityEngine;

namespace BMS_v2
{
    /// <summary>
    /// Animates a 3D arrow object by making it bob up and down or forward and back
    /// along its pointing direction to draw the user's attention to an interactable asset.
    /// </summary>
    public class ArrowBobAnimation : MonoBehaviour
    {
        [Header("Bob Animation")]
        [Tooltip("How far the arrow moves along its direction")]
        public float bobHeight = 2f;

        [Tooltip("How fast the arrow bobs")]
        public float bobSpeed = 5f;

        private Vector3 _originPos;
        private Vector3 _bobDirection;

        private void Start()
        {
            _originPos = transform.position;

            // Use the arrow's local DOWN direction (where the cone head points)
            // The arrow head is at local Y=0 (bottom), body goes up
            // So the "pointing" direction is local -Y = -transform.up
            _bobDirection = -transform.up;
        }

        private void Update()
        {
            // Bob along the arrow's pointing direction
            float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = _originPos + _bobDirection * offset;
        }
    }
}
