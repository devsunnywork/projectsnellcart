using System.Collections.Generic;
using UnityEngine;

public class BuildingInfo : MonoBehaviour
{
    [Header("Basic Building Info")]
    public string id = "BLD_001";
    public string campus_id = "CAMPUS_MAIN";
    public string name = "Building Name";
    public int year_built = 2023;
    public string last_renovation = "None";
    public int total_floors = 1;
    public float total_area_sqm = 1000f;
    public float investment_cost_total = 500000000f;

    [Header("Interactive Setup")]
    public Transform frontViewPoint; 
    public Transform entrancePoint; 
    public List<FloorInfo> floors = new List<FloorInfo>();
}
