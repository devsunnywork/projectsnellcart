using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BMS_v2
{
    /// <summary>
    /// Connects to a tile server (e.g., Google Maps) to download and render a 2D satellite map
    /// underneath the 3D models. Supports local caching to avoid redundant downloads.
    /// </summary>
    public class RealWorldMapEngine : MonoBehaviour
    {
        public static RealWorldMapEngine Instance;

        [Header("Offline Storage Settings")]
        public string cacheSubFolder = "MapCache";
        public bool autoLoadMap = false; 
        public bool saveToCache = true; 
        public bool useCacheOnly = false; 

        [Header("Map Settings")]
        public string tileServerUrl = "https://mt1.google.com/vt/lyrs=m&x={x}&y={y}&z={z}"; 
        public int gridSize = 10; 
        public float tileSize = 800f; 
        
        [Header("Current View")]
        public double lastLat = 19.1176;
        public double lastLon = 72.9145;
        public int lastZoom = 18;

        private Dictionary<string, GameObject> activeTiles = new Dictionary<string, GameObject>();
        private string cachePath;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (string.IsNullOrEmpty(cachePath))
            {
                cachePath = Path.Combine(Application.streamingAssetsPath, cacheSubFolder);
                if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);
            }
        }

        public void LoadMapForBuilding(double lat, double lon, int zoom)
        {
            EnsureInitialized();
            if (Mathf.Abs((float)(lat - lastLat)) < 0.0001 && Mathf.Abs((float)(lon - lastLon)) < 0.0001 && zoom == lastZoom) return;
            lastLat = lat; lastLon = lon; lastZoom = zoom;
            RefreshMap();
        }

        public void RefreshMap()
        {
            EnsureInitialized();
            ClearTiles();
            int centerX, centerY;
            WorldToTile(lastLat, lastLon, lastZoom, out centerX, out centerY);

            int halfDist = gridSize / 2;
            for (int xOffset = -halfDist; xOffset < halfDist; xOffset++)
            {
                for (int yOffset = -halfDist; yOffset < halfDist; yOffset++)
                {
                    int tx = centerX + xOffset;
                    int ty = centerY + yOffset;
                    CreateTile(tx, ty, lastZoom, xOffset, yOffset);
                }
            }
        }

        private void CreateTile(int x, int y, int z, int xOff, int yOff)
        {
            string tileKey = $"{z}_{y}_{x}";
            string localFilePath = Path.Combine(cachePath, tileKey + ".png");

            GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tileObj.name = "MapTile_" + tileKey;
            tileObj.transform.SetParent(transform);
            tileObj.transform.localPosition = new Vector3(xOff * tileSize, -0.6f, -yOff * tileSize);
            tileObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            tileObj.transform.localScale = new Vector3(tileSize, tileSize, 1);
            if (Application.isPlaying) tileObj.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
            else tileObj.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Unlit/Texture"));

            activeTiles[tileKey] = tileObj;

            
            if (File.Exists(localFilePath))
            {
                LoadLocalTile(localFilePath, tileObj.GetComponent<Renderer>());
            }
            else if (!useCacheOnly)
            {
                StartCoroutine(DownloadTileRoutine(x, y, z, tileKey, localFilePath, tileObj.GetComponent<Renderer>()));
            }
        }

        private void LoadLocalTile(string path, Renderer rend)
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(256, 256);
            if (tex.LoadImage(fileData)) 
            {
                if (Application.isPlaying) rend.material.mainTexture = tex;
                else rend.sharedMaterial.mainTexture = tex;
            }
        }

        private IEnumerator DownloadTileRoutine(int x, int y, int z, string key, string path, Renderer rend)
        {
            string url = tileServerUrl.Replace("{z}", z.ToString())
                                      .Replace("{y}", y.ToString())
                                      .Replace("{x}", x.ToString());

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                www.SetRequestHeader("User-Agent", "BuildingTrackSatellite_v4");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(www);
                    if (rend != null) 
                    {
                        if (Application.isPlaying) rend.material.mainTexture = tex;
                        else rend.sharedMaterial.mainTexture = tex;
                    }

                    if (saveToCache)
                    {
                        byte[] bytes = tex.EncodeToPNG();
                        File.WriteAllBytes(path, bytes);
                        Debug.Log("[MapCache] Saved tile: " + key);
                    }
                }
            }
        }

        public void ClearTiles()
        {
            if (activeTiles == null) return;
            foreach (var t in activeTiles.Values) if (t != null) 
            {
                if (Application.isPlaying) Destroy(t);
                else DestroyImmediate(t);
            }
            activeTiles.Clear();
        }

        public static void WorldToTile(double lat, double lon, int zoom, out int x, out int y)
        {
            x = (int)((lon + 180.0) / 360.0 * (1 << zoom));
            y = (int)((1.0 - System.Math.Log(System.Math.Tan(lat * System.Math.PI / 180.0) + 
                1.0 / System.Math.Cos(lat * System.Math.PI / 180.0)) / System.Math.PI) / 2.0 * (1 << zoom));
        }
    }
}
