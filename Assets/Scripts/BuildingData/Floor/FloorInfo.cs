using UnityEngine;

public class FloorInfo : MonoBehaviour
{
    [Header("Floor Sync IDs")]
    public string id = "FLR_001_03";
    public string building_id = "BLD_001";

    [Header("Floor Specifics")]
    public int floor_number = 3;
    public string label = "Third Floor";
    public float area_sqm = 2100f;

    [Header("Hierarchy Stats")]
    public string sections_count = "1";

    [Header("UI & Navigation")]
    public string floorDescription = "Contains Academic Lab and Washrooms.";
    public Transform floorCameraPoint;
}
