using System.Collections.Generic;
using UnityEngine;

namespace BMS_v2
{
    /// <summary>
    /// DataStore acts as a centralized, persistent repository for building and asset data throughout the application's lifecycle.
    /// It provides methods to query, update, and manage in-memory collections of buildings and assets.
    /// </summary>
    public class DataStore : MonoBehaviour
    {
        public static DataStore Instance { get; private set; }

        private Dictionary<string, AssetData> assetDB = new Dictionary<string, AssetData>();
        private Dictionary<string, BuildingDocument> buildingDB = new Dictionary<string, BuildingDocument>();
        
        public bool IsDataLoaded { get; private set; } = false;
        public event System.Action OnDataReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterBuilding(BuildingDocument doc)
        {
            if (doc != null && !string.IsNullOrEmpty(doc.building_id))
            {
                buildingDB[doc.building_id] = doc;
            }
        }

        public BuildingDocument GetBuilding(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            buildingDB.TryGetValue(id, out var doc);
            return doc;
        }

        public List<string> GetAllBuildings()
        {
            return new List<string>(buildingDB.Keys);
        }

        public void ClearAndLoadAssets(List<AssetData> assets)
        {
            assetDB.Clear();
            foreach (var asset in assets)
            {
                if (!string.IsNullOrEmpty(asset.id))
                {
                    assetDB[asset.id] = asset;
                }
            }
            IsDataLoaded = true;
            Debug.Log($"[DataStore] Loaded {assetDB.Count} assets into memory.");
            OnDataReady?.Invoke(); 
        }

        public AssetData GetAsset(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (assetDB.TryGetValue(id, out AssetData data))
            {
                return data;
            }
            return null;
        }

        public bool TryGetAsset(string id, out AssetData data)
        {
            data = null;
            if (string.IsNullOrEmpty(id)) return false;
            return assetDB.TryGetValue(id, out data);
        }

        public List<AssetData> GetAllAssets()
        {
            return new List<AssetData>(assetDB.Values);
        }

        public void AddOrUpdateAsset(AssetData updatedData)
        {
            if (updatedData == null || string.IsNullOrEmpty(updatedData.id)) return;
            assetDB[updatedData.id] = updatedData;
        }

        

        public List<AssetData> GetAssetsByRoom(string roomId)
        {
            List<AssetData> result = new List<AssetData>();
            foreach (var asset in assetDB.Values)
            {
                if (asset.location != null && asset.location.room_id == roomId)
                {
                    result.Add(asset);
                }
            }
            return result;
        }

        public List<AssetData> GetAssetsByDepartment(string dept)
        {
            List<AssetData> result = new List<AssetData>();
            foreach (var asset in assetDB.Values)
            {
                if (asset.identity != null && asset.identity.department == dept)
                {
                    result.Add(asset);
                }
            }
            return result;
        }
    }
}
