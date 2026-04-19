using System.Collections.Generic;
using UnityEngine;

namespace BMS_v2
{
    [System.Serializable]
    public class PatchEntry
    {
        public string patch_id;
        public string patch_type; 
        public string target_id;
        public AssetData payload; 
        public bool archive;
    }

    [System.Serializable]
    public class PatchListWrapper
    {
        public List<PatchEntry> patches;
    }

    /// <summary>
    /// PatchProcessor handles incoming data modifications or delta updates (patches), such as adding, updating, or archiving assets.
    /// It applies these changes directly to the in-memory DataStore.
    /// </summary>
    public class PatchProcessor : MonoBehaviour
    {
        public void ApplyPatches(string patchesJson)
        {
            if (string.IsNullOrEmpty(patchesJson)) return;

            try
            {
                PatchListWrapper patchData = JsonUtility.FromJson<PatchListWrapper>(patchesJson);
                
                if (patchData == null || patchData.patches == null) return;

                foreach (var patch in patchData.patches)
                {
                    switch (patch.patch_type)
                    {
                        case "asset_add":
                            if (patch.payload != null && !string.IsNullOrEmpty(patch.payload.id))
                            {
                                DataStore.Instance.AddOrUpdateAsset(patch.payload);
                                Debug.Log($"[PatchProcessor] Added new asset: {patch.payload.id}");
                            }
                            break;

                        case "asset_update":
                            if (DataStore.Instance.TryGetAsset(patch.target_id, out AssetData existingAsset))
                            {
                                
                                
                                
                                
                                
                                if (patch.payload.lifecycle != null && !string.IsNullOrEmpty(patch.payload.lifecycle.status))
                                {
                                    existingAsset.lifecycle.status = patch.payload.lifecycle.status;
                                }

                                Debug.Log($"[PatchProcessor] Updated asset: {patch.target_id}");
                            }
                            break;

                        case "asset_delete":
                            if (patch.archive && DataStore.Instance.TryGetAsset(patch.target_id, out AssetData toArchive))
                            {
                                
                                if (toArchive.lifecycle == null) 
                                    toArchive.lifecycle = new AssetLifecycleData();
                                    
                                toArchive.lifecycle.status = "archived";
                                Debug.Log($"[PatchProcessor] Archived asset: {patch.target_id}");
                            }
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PatchProcessor] Error parsing patches: {ex.Message}");
            }
        }
    }
}
