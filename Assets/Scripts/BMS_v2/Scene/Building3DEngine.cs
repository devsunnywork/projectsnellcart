using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BMS_v2
{
    /// <summary>
    /// Responsible for fetching live OpenStreetMap (OSM) data and dynamically generating 3D meshes 
    /// representing surrounding buildings. Enhances the visual context of the real-world map.
    /// </summary>
    public class Building3DEngine : MonoBehaviour
    {
        public static Building3DEngine Instance;

        [Header("Status Colors")]
        public Color inProgressColor = new Color(1f, 0.9f, 0.2f, 0.8f); 
        public Color completedColor = new Color(0.2f, 0.9f, 0.3f, 0.8f); 
        public Color criticalColor = new Color(0.9f, 0.2f, 0.2f, 0.8f); 
        public Color operationalColor = new Color(0.2f, 0.6f, 1f, 0.8f); 
        
        [Header("3D Map Settings")]
        public float scaleFactor = 100000f; 
        public Material buildingMaterial;
        public float defaultBuildingHeight = 15f;

        [Header("Query Bounds")]
        public float searchRadiusKm = 0.5f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            if (buildingMaterial == null)
            {
                
                buildingMaterial = new Material(Shader.Find("Standard"));
                buildingMaterial.SetFloat("_Glossiness", 0f);
                buildingMaterial.SetFloat("_Metallic", 0f);
            }
        }

        public void Load3DBuildings(double lat, double lon)
        {
            StartCoroutine(FetchOSMBuildingData(lat, lon));
        }

        private IEnumerator FetchOSMBuildingData(double lat, double lon)
        {
            
            string query = $"[out:json];way[\"building\"](around:{searchRadiusKm * 1000},{lat},{lon});out body;>;out skel qt;";
            string url = "https://overpass-api.de/api/interpreter?data=" + UnityWebRequest.EscapeURL(query);

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    ParseOSMData(www.downloadHandler.text, lat, lon);
                }
                else
                {
                    Debug.LogWarning("[Building3D] API Error: " + www.error);
                }
            }
        }

        private void ParseOSMData(string json, double centerLat, double centerLon)
        {
            
            var response = JsonUtility.FromJson<OSMResponse>(json);
            if (response == null || response.elements == null) return;

            Dictionary<long, Vector3> nodeMap = new Dictionary<long, Vector3>();
            foreach (var el in response.elements)
            {
                if (el.type == "node")
                {
                    
                    float x = (float)((el.lon - centerLon) * 111320f); 
                    float z = (float)((el.lat - centerLat) * 111320f);
                    nodeMap[el.id] = new Vector3(x, 0, z);
                }
            }

            foreach (var el in response.elements)
            {
                if (el.type == "way" && el.nodes != null && el.nodes.Length > 2)
                {
                    List<Vector3> verts = new List<Vector3>();
                    foreach (var nid in el.nodes)
                    {
                        if (nodeMap.ContainsKey(nid)) verts.Add(nodeMap[nid]);
                    }
                    if (verts.Count > 2) CreateBuildingMesh(verts, el.id);
                }
            }
        }

        private void CreateBuildingMesh(List<Vector3> polygon, long id)
        {
            GameObject bld = new GameObject("OSM_Building_" + id);
            bld.transform.SetParent(transform);
            
            MeshFilter mf = bld.AddComponent<MeshFilter>();
            MeshRenderer mr = bld.AddComponent<MeshRenderer>();
            
            Color[] statusPool = { inProgressColor, completedColor, criticalColor, operationalColor };
            Color baseColor = statusPool[UnityEngine.Random.Range(0, statusPool.Length)];
            
            Material bldMat = new Material(Shader.Find("Sprites/Default"));
            if (Application.isPlaying) mr.material = bldMat;
            else mr.sharedMaterial = bldMat;

            Mesh mesh = new Mesh();
            int vCount = polygon.Count;
            Vector3[] vertices = new Vector3[vCount * 2];
            Color[] colors = new Color[vCount * 2];
            
            int[] triangles = new int[(vCount * 6) + ((vCount - 2) * 3)];
            float height = UnityEngine.Random.Range(30f, 80f); 

            Color wallColor = baseColor * 0.5f; 
            Color roofColor = baseColor; 

            for (int i = 0; i < vCount; i++)
            {
                vertices[i] = polygon[i];
                vertices[i + vCount] = polygon[i] + Vector3.up * height;
                colors[i] = wallColor;
                colors[i + vCount] = roofColor;
            }

            int tIdx = 0;
            for (int i = 0; i < vCount; i++)
            {
                int next = (i + 1) % vCount;
                triangles[tIdx++] = i; triangles[tIdx++] = i + vCount; triangles[tIdx++] = next;
                triangles[tIdx++] = next; triangles[tIdx++] = i + vCount; triangles[tIdx++] = next + vCount;
            }

            int roofOffset = vCount;
            for (int i = 1; i < vCount - 1; i++)
            {
                triangles[tIdx++] = roofOffset;
                triangles[tIdx++] = roofOffset + i;
                triangles[tIdx++] = roofOffset + i + 1;
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mf.mesh = mesh;
            
            // Add collider so camera/player doesn't go through walls
            MeshCollider mc = bld.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;

            
            GameObject outline = new GameObject("Outline");
            outline.transform.SetParent(bld.transform, false);
            LineRenderer lr = outline.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.widthMultiplier = 2.5f;
            Material outlineMat = new Material(Shader.Find("Sprites/Default"));
            if (Application.isPlaying) lr.material = outlineMat;
            else lr.sharedMaterial = outlineMat;
            lr.startColor = lr.endColor = Color.white; 
            
            Vector3[] outlinePoints = new Vector3[vCount];
            for (int i = 0; i < vCount; i++) outlinePoints[i] = vertices[i + vCount] + Vector3.up * 0.1f;
            lr.positionCount = vCount;
            lr.SetPositions(outlinePoints);
        }

        [Serializable]
        public class OSMResponse { public List<OSMElement> elements; }
        [Serializable]
        public class OSMElement { public string type; public long id; public double lat; public double lon; public long[] nodes; }
    }
}
