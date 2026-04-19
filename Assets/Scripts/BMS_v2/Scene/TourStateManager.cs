using UnityEngine;
using System.Collections.Generic;

namespace BMS_v2
{
    /// <summary>
    /// Manages the logical state of the user's tour (Global, Building, Floor, Asset).
    /// Dynamically toggles interaction layers so users only click on appropriate scale items based on their current view.
    /// </summary>
    public class TourStateManager : MonoBehaviour
    {
        public static TourStateManager Instance;

        public enum ViewState { Global, BuildingFront, InternalFloor, AssetFocus }
        public ViewState CurrentState { get; private set; } = ViewState.Global;
        public ViewState LastState { get; private set; } = ViewState.Global;

        private List<MonoBehaviour> buildings = new List<MonoBehaviour>();
        private List<MonoBehaviour> floors = new List<MonoBehaviour>();
        private List<MonoBehaviour> rooms = new List<MonoBehaviour>();
        private List<MonoBehaviour> assets = new List<MonoBehaviour>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            CategorizeAllInteractables();
            ForceState(ViewState.Global);
        }

        public void CategorizeAllInteractables()
        {
            buildings.Clear(); floors.Clear(); rooms.Clear(); assets.Clear();
            
            
            BuildingZoneInfo[] allZones = Object.FindObjectsByType<BuildingZoneInfo>(FindObjectsSortMode.None);
            foreach (var z in allZones)
            {
                if (string.IsNullOrEmpty(z.zoneId)) continue;
                string id = z.zoneId.Trim().ToUpper();
                if (id.StartsWith("BLD")) buildings.Add(z);
                else if (id.StartsWith("FLR")) floors.Add(z);
                else if (id.StartsWith("RM")) rooms.Add(z);
            }

            
            ThinAssetInfo[] allAssets = Object.FindObjectsByType<ThinAssetInfo>(FindObjectsSortMode.None);
            foreach (var a in allAssets)
            {
                if (string.IsNullOrEmpty(a.assetId)) continue;
                assets.Add(a);
            }
        }

        
        public void ProcessInteraction(string clickedId)
        {
            if (string.IsNullOrEmpty(clickedId)) return;
            string upperId = clickedId.ToUpper();

            if (upperId.StartsWith("BLD")) ForceState(ViewState.BuildingFront);
            else if (upperId.StartsWith("FLR")) ForceState(ViewState.InternalFloor);
            else if (upperId.StartsWith("RM")) ForceState(ViewState.InternalFloor); 
            else ForceState(ViewState.AssetFocus);
        }

        
        public void ForceState(ViewState newState)
        {
            if (CurrentState != newState) LastState = CurrentState;
            CurrentState = newState;
            Debug.Log($"[TourStateManager] Phase Shift: {newState}");

            switch (newState)
            {
                case ViewState.Global:
                    SetInteractionState(buildings, true);
                    SetInteractionState(floors, false);
                    SetInteractionState(rooms, false);
                    SetInteractionState(assets, false);
                    break;

                case ViewState.BuildingFront:
                    SetInteractionState(buildings, false); 
                    SetInteractionState(floors, true);
                    SetInteractionState(rooms, false);
                    SetInteractionState(assets, false);
                    break;

                case ViewState.InternalFloor:
                    
                    SetInteractionState(buildings, false);
                    SetInteractionState(floors, false); 
                    SetInteractionState(rooms, true);
                    SetInteractionState(assets, true);
                    break;

                case ViewState.AssetFocus:
                    
                    SetInteractionState(buildings, false);
                    SetInteractionState(floors, false); 
                    SetInteractionState(rooms, false);
                    SetInteractionState(assets, true);
                    break;
            }
        }

        private void SetInteractionState(List<MonoBehaviour> group, bool interactable)
        {
            foreach (var item in group)
            {
                if (item != null)
                {
                    Collider[] cols = item.GetComponentsInChildren<Collider>();
                    foreach (Collider c in cols)
                    {
                        // We keep the collider enabled so the SmartCameraController (Physics) still hits it.
                        // We just change the layer so the AssetInteractionManager (Raycast) ignores it if not interactable.
                        c.enabled = true;
                        
                        // Layer 2 is the built-in "Ignore Raycast" layer in Unity
                        c.gameObject.layer = interactable ? 0 : 2; 
                    }
                }
            }
        }
    }
}
