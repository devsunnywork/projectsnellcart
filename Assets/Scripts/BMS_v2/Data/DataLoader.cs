using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace BMS_v2
{
    public class DataLoader : MonoBehaviour
    {
        public string assetsFileName = "assets.json";
        
        
        public delegate void OnDataReadyHandler();
        public static event OnDataReadyHandler OnDataReady;

        private void Start()
        {
            StartCoroutine(LoadAllDataRoutine());
        }

        private IEnumerator LoadAllDataRoutine()
        {
            string folderPath = Path.Combine(Application.streamingAssetsPath, "BuildingData");
            string[] jsonFiles = new string[0];

            if (Directory.Exists(folderPath))
            {
                jsonFiles = Directory.GetFiles(folderPath, "*.json");
            }
            else
            {
                Debug.LogError($"[DataLoader] Folder not found: {folderPath}");
                yield break;
            }

            int loadedFileCount = 0;
            List<AssetData> masterList = new List<AssetData>();

            foreach (string filePath in jsonFiles)
            {
                string assetsJson = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(assetsJson))
                {
                    try
                    {
                        
                        BuildingDocument bldDoc = JsonUtility.FromJson<BuildingDocument>(assetsJson);
                        if (bldDoc != null && bldDoc.floors != null)
                        {
                            if (DataStore.Instance != null) DataStore.Instance.RegisterBuilding(bldDoc);
                            foreach (var floor in bldDoc.floors)
                            {
                                if (floor.rooms != null)
                                {
                                    foreach (var room in floor.rooms)
                                    {
                                        if (room.assets != null)
                                        {
                                            masterList.AddRange(room.assets);
                                        }
                                    }
                                }
                            }
                            loadedFileCount++;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[DataLoader] Error parsing JSON {filePath}: {ex.Message}");
                    }
                }
            }

            if (masterList.Count > 0 && DataStore.Instance != null)
            {
                DataStore.Instance.ClearAndLoadAssets(masterList);
                
                
                yield return null;
                    
                
                OnDataReady?.Invoke();
                Debug.Log($"[DataLoader] System Initialized. Loaded {masterList.Count} assets from {loadedFileCount} building files.");

                if (RealWorldMapEngine.Instance != null && RealWorldMapEngine.Instance.autoLoadMap)
                {
                    List<string> blds = DataStore.Instance.GetAllBuildings();
                    if (blds.Count > 0)
                    {
                        var b = DataStore.Instance.GetBuilding(blds[0]);
                        RealWorldMapEngine.Instance.LoadMapForBuilding(b.latitude, b.longitude, b.map_zoom);
                        
                        
                        if (Building3DEngine.Instance != null)
                        {
                            Building3DEngine.Instance.Load3DBuildings(b.latitude, b.longitude);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("[DataLoader] Failed to load any valid asset data from BuildingData folder.");
            }
        }
    }
}
