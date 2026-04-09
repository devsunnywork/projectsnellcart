using UnityEngine;

namespace BMS_v2
{
    public class BuildingZoneInfo : MonoBehaviour
    {
        [Header("Zone ID (e.g., BLD_001 or FLR_001)")]
        public string zoneId = "BLD_001";

        [Header("Custom Camera View (Drag empty GameObject here)")]
        public Transform customCameraPoint;
    }
}
