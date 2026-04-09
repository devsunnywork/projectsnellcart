using UnityEngine;
using UnityEditor;
using TMPro;
using BMS_v2;

namespace BMS_v2.Editor
{
    public class BuildingLabelGenerator : EditorWindow
    {
        [MenuItem("BMS Tools/Generate Building Labels")]
        public static void GenerateLabels()
        {
            BuildingZoneInfo[] allZones = Object.FindObjectsByType<BuildingZoneInfo>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var zone in allZones)
            {
                if (string.IsNullOrEmpty(zone.zoneId)) continue;
                
                
                if (zone.zoneId.Trim().ToUpper().StartsWith("BLD"))
                {
                    
                    Transform existing = zone.transform.Find("BuildingNameLabel");
                    if (existing != null)
                    {
                        Undo.DestroyObjectImmediate(existing.gameObject);
                    }

                    
                    GameObject labelObj = new GameObject("BuildingNameLabel");
                    labelObj.transform.SetParent(zone.transform);
                    
                    
                    TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
                    tmp.text = zone.zoneId;
                    tmp.fontSize = 25;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;
                    tmp.outlineWidth = 0.2f;
                    tmp.outlineColor = Color.black;

                    
                    float topY = 10f; 
                    Collider col = zone.GetComponentInChildren<Collider>();
                    if (col != null)
                    {
                        topY = col.bounds.max.y - zone.transform.position.y + 5f;
                    }

                    labelObj.transform.localPosition = new Vector3(0, topY, 0);
                    labelObj.transform.rotation = Quaternion.Euler(0, 0, 0);

                    
                    Undo.RegisterCreatedObjectUndo(labelObj, "Gen Building Label");
                    count++;
                }
            }

            Debug.Log($"[BMS Tools] Successfully generated {count} Building Labels. You can now adjust their positions manually.");
        }
    }
}
