using UnityEngine;

public class AssetInfo : MonoBehaviour
{
    [Header("Asset Sync IDs (JSON)")]
    public string id = "AST_000_00_00_000";
    public string floor_id = "FLR_001_01";
    public string section_id = "SEC_001_01_A";

    [Header("Asset Details (JSON)")]
    public string name = "New Asset";
    public string type = "equipment";
    public float area_sqm = 0f;

    [Header("Statistics")]
    public int construction_year = 2024;
    public float construction_cost = 0f;
    public string condition = "good";
    public string last_inspection = "2025-01-01";

    [Header("Interior Interaction (X, Y, Z Controls)")]
    public Vector3 targetPosition;
    public Vector3 targetRotation;
}
