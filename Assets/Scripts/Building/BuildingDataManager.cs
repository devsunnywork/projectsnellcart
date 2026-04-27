using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class RoomData
{
    public string id;
    public string floor_id;
    public string section_id;
    public string name;
    public string type;
    public float area_sqm;
    public int construction_year;
    public float construction_cost;
    public string condition;
    public string last_inspection;
}

[System.Serializable]
public class FloorData
{
    public string id;
    public int floor_number;
    public string label;
    public float area_sqm;
    public List<RoomData> rooms;
}

[System.Serializable]
public class BuildingData
{
    public string id;
    public string campus_id;
    public string name;
    public int year_built;
    public string last_renovation;
    public int total_floors;
    public float total_area_sqm;
    public float investment_cost_total;
    public List<FloorData> floors;
}

public class BuildingDataManager : MonoBehaviour
{
    [Header("Configuration")]
    public string jsonFileName = "data.json";

    [ContextMenu("Sync Data Now")]
    public void SyncData()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found at: " + path);
            return;
        }

        string jsonContent = File.ReadAllText(path);
        BuildingData data = JsonUtility.FromJson<BuildingData>(jsonContent);

        if (data == null)
        {
            Debug.LogError("Failed to parse JSON!");
            return;
        }

        Debug.Log("Syncing Building Data: " + data.name);

        // 1. Sync Building Info
        BuildingInfo bInfo = FindObjectOfType<BuildingInfo>();
        if (bInfo != null && bInfo.id == data.id)
        {
            bInfo.name = data.name;
            bInfo.year_built = data.year_built;
            bInfo.last_renovation = data.last_renovation;
            bInfo.total_area_sqm = data.total_area_sqm;
            bInfo.investment_cost_total = data.investment_cost_total;
            bInfo.total_floors = data.total_floors;
        }

        // 2. Sync Floors & Rooms
        FloorInfo[] allFloors = FindObjectsOfType<FloorInfo>();
        RoomInfo[] allRooms = FindObjectsOfType<RoomInfo>();

        foreach (var fData in data.floors)
        {
            // Sync Floor
            foreach (var fi in allFloors)
            {
                if (fi.id == fData.id)
                {
                    fi.floor_number = fData.floor_number;
                    fi.label = fData.label;
                    fi.area_sqm = fData.area_sqm;
                }
            }

            // Sync Rooms
            foreach (var rData in fData.rooms)
            {
                foreach (var ri in allRooms)
                {
                    if (ri.id == rData.id)
                    {
                        ri.floor_id = rData.floor_id;
                        ri.section_id = rData.section_id;
                        ri.name = rData.name;
                        ri.type = rData.type;
                        ri.area_sqm = rData.area_sqm;
                        ri.construction_year = rData.construction_year;
                        ri.construction_cost = rData.construction_cost;
                        ri.condition = rData.condition;
                        ri.last_inspection = rData.last_inspection;
                    }
                }
            }
        }

        Debug.Log("Sync Completed Successfully!");
    }
}
