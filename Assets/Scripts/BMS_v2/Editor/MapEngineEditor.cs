using UnityEngine;
using UnityEditor;
using BMS_v2;

namespace BMS_v2.Editor
{
    /// <summary>
    /// Custom editor script for the RealWorldMapEngine. Provides a custom inspector with buttons 
    /// to manually build the map or clear tiles during development, as well as an accessible menu option.
    /// </summary>
    [CustomEditor(typeof(RealWorldMapEngine))]
    public class MapEngineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            RealWorldMapEngine engine = (RealWorldMapEngine)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Manual Build Map", GUILayout.Height(30)))
            {
                
                
                
                engine.RefreshMap();
            }
            
            if (GUILayout.Button("Clear All Tiles", GUILayout.Height(30)))
            {
                engine.ClearTiles();
            }
        }

        [MenuItem("BMS Tools/Build Map Engine")]
        public static void BuildMapMenu()
        {
            var engine = GameObject.FindObjectOfType<RealWorldMapEngine>();
            if (engine == null)
            {
                Debug.Log("[BMS Tools] Creating MapEngine object...");
                GameObject go = new GameObject("MapEngine");
                engine = go.AddComponent<RealWorldMapEngine>();
            }

            var bld3d = GameObject.FindObjectOfType<Building3DEngine>();
            if (bld3d == null)
            {
                Debug.Log("[BMS Tools] Creating 3D Building Engine object...");
                GameObject go = new GameObject("3D_BuildingEngine");
                bld3d = go.AddComponent<Building3DEngine>();
            }

            if (engine != null)
            {
                engine.RefreshMap();
                Debug.Log("[BMS Tools] Map Engine triggered manually.");

                
                if (bld3d != null)
                {
                    bld3d.Load3DBuildings(engine.lastLat, engine.lastLon);
                    Debug.Log("[BMS Tools] 3D Buildings triggered.");
                }
            }
        }
    }
}
