using UnityEngine;

public class InteractableObjectInfo : MonoBehaviour
{
    [Header("Sync IDs (Matching JSON)")]
    public string id = "OBJ_001";
    public string floor_id = "FLR_001_01";
    public string section_id = "SEC_001_01_A";

    [Header("Basic Details")]
    public string name = "New Object";
    public string type = "asset";
    public float area_sqm = 0f;

    [Header("Statistics")]
    public int construction_year = 2024;
    public float construction_cost = 0f;
    public string condition = "good";
    public string last_inspection = "2025-01-01";

    [Header("Zoom View Point (X, Y, Z Controls)")]
    public Vector3 targetPosition;
    public Vector3 targetRotation;
}
