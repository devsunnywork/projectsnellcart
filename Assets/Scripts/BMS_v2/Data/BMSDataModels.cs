using System;
using System.Collections.Generic;
using UnityEngine;

namespace BMS_v2
{
    /// <summary>
    /// This file contains all the serializable data models used to represent the building management system's data.
    /// It includes structures defining asset details like location, identity, lifecycle, cost, warranty, and quality,
    /// as well as the hierarchical structure of buildings, floors, and rooms.
    /// </summary>
    [Serializable]
    public class Vector3Data { public float x, y, z; }

    [Serializable]
    public class AssetLocationData
    {
        public string building_id, floor_id, room_id, position_label, unity_object_ref, unity_highlight_color;
        public Vector3Data unity_coordinates;
    }

    [Serializable]
    public class AssetIdentityData
    {
        public string name, category, subcategory, department, brand, model, serial_number, unit;
        public int quantity;
    }

    [Serializable]
    public class AssetLifecycleData
    {
        public string status, type, purchase_date, installation_date, last_service_date, next_service_due, retirement_date, replaced_asset_id, replaced_by_asset_id;
        public bool is_consumable, is_old, is_replaced;
        public int expected_lifespan_years;
    }

    [Serializable]
    public class AssetCostData
    {
        public float purchase_cost, installation_cost, total_invested, current_market_value, depreciation_per_year;
    }

    [Serializable]
    public class AssetWarrantyData
    {
        public string status, provider, start_date, expiry_date, warranty_type, amc_expiry, contact, document_ref;
        public int warranty_years;
        public bool amc_available;
    }

    [Serializable]
    public class AssetQualityData
    {
        public int current_score_percent;
        public string condition, last_inspection_date, inspected_by, inspection_notes, next_inspection_due;
    }

    [Serializable]
    public class WorkerData
    {
        public string id, name, role, contact, contractor_firm;
    }

    [Serializable]
    public class ScheduleData
    {
        public string start_date, end_date, status, completion_date;
        public int working_days, actual_days_taken;
    }

    [Serializable]
    public class CostBreakdownData
    {
        public float labour_cost, material_cost, other_charges, total_task_cost;
        public string payment_status, payment_date, payment_mode, invoice_ref;
    }

    [Serializable]
    public class QualityCheckData
    {
        public int score_percent;
        public string checked_by, check_date, remarks;
    }

    [Serializable]
    public class MaterialData
    {
        public string id, name, unit, supplier;
        public int quantity;
        public float unit_cost, total_cost;
        public bool is_consumable;
    }

    [Serializable]
    public class TaskData
    {
        public string id, asset_id, task_type, department, description, priority;
        public WorkerData worker;
        public ScheduleData schedule;
        public CostBreakdownData cost_breakdown;
        public QualityCheckData quality_check;
        public List<MaterialData> materials_used;
        public string created_at, updated_at;
    }

    [Serializable]
    public class AssetData
    {
        public string id;
        public AssetLocationData location;
        public AssetIdentityData identity;
        public AssetLifecycleData lifecycle;
        public AssetCostData cost;
        public AssetWarrantyData warranty;
        public AssetQualityData quality;
        public List<TaskData> tasks;
        public List<string> tags;
        public string notes, created_at, updated_at;
    }

    [Serializable]
    public class AssetListWrapper
    {
        public List<AssetData> assets;
    }

    
    [Serializable]
    public class RoomNode
    {
        public string room_id;
        public string room_name;
        public List<AssetData> assets = new List<AssetData>();
    }

    [Serializable]
    public class FloorNode
    {
        public string floor_id;
        public string floor_name;
        public List<RoomNode> rooms = new List<RoomNode>();
    }

    [Serializable]
    public class BuildingDocument
    {
        public string building_id;
        public string building_name;
        public string address;
        public int year_built;
        public int total_floors;
        public float total_area_sqm;
        public List<string> departments = new List<string>();
        public string currency;
        public string unity_scene_ref;
        public string description;
        public BuildingCostData cost;

        [Serializable]
        public class BuildingCostData
        {
            public float total_building_value;
            public float maintenance_budget;
        }
        
        
        public float latitude;
        public float longitude;
        public int map_zoom = 18;

        public List<FloorNode> floors = new List<FloorNode>();
    }
}
