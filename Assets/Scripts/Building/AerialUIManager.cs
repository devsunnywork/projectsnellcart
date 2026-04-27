using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class AerialUIManager : MonoBehaviour
{
    [Header("General Reference")]
    public CameraController cameraController;
    
    [Header("Layers")]
    public LayerMask buildingLayer;
    public LayerMask floorLayer;
    public LayerMask objectLayer;

    [Header("--- MAIN BUILDING PANEL ---")]
    public GameObject panel_Building;
    public TextMeshProUGUI building_Id, building_CampusId, building_Name, building_YearBuilt, building_LastRenovation, building_TotalFloors, building_TotalArea, building_InvestmentCost;
    
    [Header("--- PERSONNEL & PROJECT PANEL (SEPARATE) ---")]
    public GameObject panel_Personnel; // [NEW] Separate GameObject for personnel details
    public TextMeshProUGUI personnelPanelTitle; 
    public TextMeshProUGUI architect;
    public TextMeshProUGUI structuralEngineer;
    public TextMeshProUGUI civilContractor;
    public TextMeshProUGUI projectManager;
    public TextMeshProUGUI siteSupervisor;
    public TextMeshProUGUI interiorDesigner;
    public TextMeshProUGUI exteriorDesigner;
    public TextMeshProUGUI electricContractor;
    public TextMeshProUGUI plumbingContractor;
    public TextMeshProUGUI roofingSpecialist;
    public TextMeshProUGUI painterFinishing;
    public TextMeshProUGUI materialSupplier;
    public TextMeshProUGUI startDate;
    public TextMeshProUGUI completionDate;
    public TextMeshProUGUI totalProjectCost;

    [Header("--- FLOOR PANEL ---")]
    public GameObject panel_Floor;
    public TextMeshProUGUI floor_Id, floor_BuildingId, floor_Number, floor_Label, floor_Area;

    [Header("--- OBJECT PANEL ---")]
    public GameObject panel_Object;
    public TextMeshProUGUI object_Id, object_FloorId, object_SectionId, object_Name, object_Type, object_Area, object_Year, object_ConditionCost, object_Inspection;

    [HideInInspector] public BuildingInfo activeBuilding; 
    private bool isLocked = false; 
    private bool isPanelDismissed = false; 
    private BuildingInfo highlightedBuilding; 

    void Start() { HideAllPanels(); }

    void Update() {
        if (cameraController == null) return;
        bool isAerial = cameraController.currentPhase == CameraController.CameraPhase.Aerial;
        bool isInterior = cameraController.currentPhase == CameraController.CameraPhase.Interior;

        if (isAerial && activeBuilding != null) {
            SetExteriorColliders(true, true); 
            SetMarkerState(activeBuilding, true, false);
            activeBuilding = null; isLocked = false; isPanelDismissed = false;
        }

        if (isInterior && activeBuilding != null) {
            SetExteriorColliders(false, true); 
            SetMarkerState(activeBuilding, false, false);
            float distToEntrance = 100f;
            if (activeBuilding.entrancePoint != null) distToEntrance = Vector3.Distance(cameraController.transform.position, activeBuilding.entrancePoint.position);
            if ((distToEntrance < 5f || UserIsWalking()) && panel_Floor != null && panel_Floor.activeSelf) { isPanelDismissed = true; panel_Floor.SetActive(false); }
            else if (!isLocked && (panel_Object != null && !panel_Object.activeSelf) && !isPanelDismissed) { if (activeBuilding.floors.Count > 0 && (panel_Floor != null && !panel_Floor.activeSelf)) UpdateUI(null, activeBuilding.floors[0], null); }
        }

        bool buildingActive = panel_Building != null && panel_Building.activeSelf;
        bool floorActive = panel_Floor != null && panel_Floor.activeSelf;
        bool objectActive = panel_Object != null && panel_Object.activeSelf;
        bool personnelActive = panel_Personnel != null && panel_Personnel.activeSelf;

        if ((isLocked || buildingActive || floorActive || objectActive || personnelActive) && UserIsWalking() && !isLocked) {
            isLocked = false; isPanelDismissed = true; HideAllPanels();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame) { isPanelDismissed = true; HideAllPanels(); isLocked = false; }
        HandleUnifiedControl(isAerial, isInterior);
    }

    void SetMarkerState(BuildingInfo building, bool visible, bool selected) {
        if (building == null) return;
        BuildingMarker3D marker = building.GetComponent<BuildingMarker3D>() ?? building.GetComponentInChildren<BuildingMarker3D>();
        if (marker != null) { marker.SetVisible(visible); marker.SetSelected(selected); }
    }

    bool UserIsWalking() { return Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed || Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed; }

    void HandleUnifiedControl(bool isAerial, bool isInterior) {
        if (Mouse.current == null || cameraController.isDragging || isLocked) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        LayerMask currentMask = isAerial ? buildingLayer : (floorLayer | objectLayer);

        if (Physics.Raycast(ray, out hit, 100000f, currentMask, QueryTriggerInteraction.Collide)) {
            BuildingInfo bi = (isAerial) ? (hit.collider.GetComponentInParent<BuildingInfo>() ?? hit.collider.GetComponent<BuildingInfo>()) : null;
            InteractableObjectInfo obj = isInterior ? (hit.collider.GetComponentInParent<InteractableObjectInfo>() ?? hit.collider.GetComponent<InteractableObjectInfo>()) : null;
            RoomInfo ri = isInterior ? (hit.collider.GetComponentInParent<RoomInfo>() ?? hit.collider.GetComponent<RoomInfo>()) : null;
            FloorInfo fi = isInterior ? (hit.collider.GetComponentInParent<FloorInfo>() ?? hit.collider.GetComponent<FloorInfo>()) : null;
            PersonInfo pi = hit.collider.GetComponentInParent<PersonInfo>() ?? hit.collider.GetComponent<PersonInfo>();

            if (isInterior) {
                bool rightClick = Mouse.current.rightButton.wasPressedThisFrame;
                bool leftClickRelease = Mouse.current.leftButton.wasReleasedThisFrame && !cameraController.isDragging;

                if (leftClickRelease || rightClick) {
                    if (obj != null) {
                        isLocked = true; isPanelDismissed = false;
                        BuildingInfo pb = obj.GetComponentInParent<BuildingInfo>();
                        if (pb != null) { activeBuilding = pb; SetExteriorColliders(false, true); }

                        Vector3 fPos; Quaternion fRot;
                        if (obj.targetPosition == Vector3.zero) {
                            Vector3 target = hit.collider.bounds.center;
                            Vector3 dir = (target - cameraController.transform.position).normalized;
                            fPos = target - (dir * 3.2f); fPos.y += 1.35f; 
                            fRot = Quaternion.LookRotation(target - fPos);
                        } else {
                            fPos = obj.transform.TransformPoint(obj.targetPosition);
                            fRot = (obj.targetRotation == Vector3.zero) ? cameraController.transform.rotation : Quaternion.Euler(obj.targetRotation);
                        }
                        cameraController.FlyToDirect(fPos, fRot);
                        UpdateUI(null, null, obj);
                    }
                    else if (pi != null) {
                        // [NEW] CLICK ON PERSON MODEL - Custom or Auto Zoom
                        Debug.Log("[PERSONNEL] Clicking on Person Model");
                        isLocked = true; isPanelDismissed = false;

                        Vector3 finalPos;
                        Quaternion finalRot;

                        if (pi.targetCameraPose != null) {
                            // PRIORITY 1: Use specific target camera pose (Empty GameObject)
                            finalPos = pi.targetCameraPose.position;
                            finalRot = pi.targetCameraPose.rotation;
                        } else {
                            // PRIORITY 2: Auto-focus on the model (3.2m framing)
                            Vector3 focusTarget = hit.collider.bounds.center;
                            Vector3 dirFromCam = (focusTarget - cameraController.transform.position).normalized;
                            finalPos = focusTarget - (dirFromCam * 3.2f);
                            finalPos.y += 1.35f; 
                            finalRot = Quaternion.LookRotation(focusTarget - finalPos);
                        }

                        cameraController.FlyToDirect(finalPos, finalRot);
                        UpdateUI(null, null, null, null, pi);
                    }
                    else if (ri != null) {
                        Vector3 target = ri.transform.TransformPoint(ri.targetPosition);
                        cameraController.FlyToDirect(target, cameraController.transform.rotation);
                        UpdateUI(null, null, null, ri);
                        isLocked = true;
                    }
                }
                return;
            }
            if (bi != null) {
                UpdateUI(bi, null, null); 
                if (Mouse.current.leftButton.wasPressedThisFrame && bi.frontViewPoint != null) {
                    activeBuilding = bi; HideAllPanels(); cameraController.FlyToBuilding(bi.frontViewPoint);
                }
                return;
            }
        }
        HideAllPanels();
    }

    void UpdateUI(BuildingInfo bi, FloorInfo fi, InteractableObjectInfo obj, RoomInfo ri = null, PersonInfo pi = null) {
        HideAllPanels();
        
        // ... (Object/Room/Floor logic same) ...
        if (obj != null && panel_Object != null) {
            panel_Object.SetActive(true);
            if(object_Id) object_Id.text = obj.id; if(object_FloorId) object_FloorId.text = obj.floor_id; if(object_SectionId) object_SectionId.text = obj.section_id;
            if(object_Name) object_Name.text = obj.name; if(object_Type) object_Type.text = obj.type; if(object_Area) object_Area.text = obj.area_sqm.ToString() + " sqm";
            if(object_Year) object_Year.text = obj.construction_year.ToString();
            if(object_ConditionCost) object_ConditionCost.text = obj.condition + " (₹ " + obj.construction_cost.ToString("N0") + ")"; 
            if(object_Inspection) object_Inspection.text = obj.last_inspection;
        }
        else if (ri != null && panel_Object != null) {
            panel_Object.SetActive(true);
            if(object_Id) object_Id.text = ri.id; if(object_FloorId) object_FloorId.text = ri.floor_id; if(object_SectionId) object_SectionId.text = ri.section_id;
            if(object_Name) object_Name.text = ri.name; if(object_Type) object_Type.text = ri.type; if(object_Area) object_Area.text = ri.area_sqm.ToString() + " sqm";
            if(object_Year) object_Year.text = ri.construction_year.ToString();
            if(object_ConditionCost) object_ConditionCost.text = ri.condition + " (₹ " + ri.construction_cost.ToString("N0") + ")";
            if(object_Inspection) object_Inspection.text = ri.last_inspection;
        }
        else if (fi != null && panel_Floor != null) {
            panel_Floor.SetActive(true);
            if(floor_Id) floor_Id.text = fi.id; if(floor_BuildingId) floor_BuildingId.text = fi.building_id; if(floor_Number) floor_Number.text = fi.floor_number.ToString();
            if(floor_Label) floor_Label.text = fi.label; if(floor_Area) floor_Area.text = fi.area_sqm.ToString();
        }
        else if (pi != null && panel_Personnel != null) {
             // [NEW] Separate Person Model Click
             panel_Personnel.SetActive(true);
             if(personnelPanelTitle) personnelPanelTitle.text = "PERSONNEL & PROJECT DETAILS";
             if(architect) architect.text = pi.architect_name;
             if(structuralEngineer) structuralEngineer.text = pi.structural_engineer;
             if(civilContractor) civilContractor.text = pi.civil_contractor;
             if(projectManager) projectManager.text = pi.project_manager;
             if(siteSupervisor) siteSupervisor.text = pi.site_supervisor;
             if(interiorDesigner) interiorDesigner.text = pi.interior_designer;
             if(exteriorDesigner) exteriorDesigner.text = pi.exterior_designer;
             if(electricContractor) electricContractor.text = pi.electric_contractor;
             if(plumbingContractor) plumbingContractor.text = pi.plumbing_contractor;
             if(roofingSpecialist) roofingSpecialist.text = pi.roofing_specialist;
             if(painterFinishing) painterFinishing.text = pi.painter_finishing;
             if(materialSupplier) materialSupplier.text = pi.material_supplier;
             if(startDate) startDate.text = pi.construction_start_date;
             if(completionDate) completionDate.text = pi.completion_date;
             if(totalProjectCost) totalProjectCost.text = "₹ " + pi.total_cost.ToString("N0");
        }
        else if (bi != null && panel_Building != null) {
            panel_Building.SetActive(true);
            // BUILDING STATS ONLY
            if(building_Id) building_Id.text = bi.id; 
            if(building_CampusId) building_CampusId.text = bi.campus_id; 
            if(building_Name) building_Name.text = bi.name;
            if(building_YearBuilt) building_YearBuilt.text = bi.year_built.ToString(); 
            if(building_LastRenovation) building_LastRenovation.text = bi.last_renovation;
            if(building_TotalFloors) building_TotalFloors.text = bi.total_floors.ToString(); 
            if(building_TotalArea) building_TotalArea.text = bi.total_area_sqm.ToString("N0") + " sqm";
            if(building_InvestmentCost) building_InvestmentCost.text = "₹ " + bi.investment_cost_total.ToString("N0");
            
            if (highlightedBuilding != bi) { if (highlightedBuilding != null) SetMarkerState(highlightedBuilding, true, false); highlightedBuilding = bi; SetMarkerState(bi, true, true); }
        }
    }

    void SetExteriorColliders(bool sState, bool fState) {
        if (activeBuilding == null) return;
        Collider bCol = activeBuilding.GetComponent<Collider>(); if (bCol != null) bCol.enabled = sState;
        if (activeBuilding.floors != null) { foreach (var f in activeBuilding.floors) if (f != null) { Collider fc = f.GetComponent<Collider>(); if (fc) fc.enabled = fState; } }
    }

    public void HideAllPanels() {
        if (panel_Building != null) panel_Building.SetActive(false);
        if (panel_Personnel != null) panel_Personnel.SetActive(false); // [NEW] Hide separate panel
        if (panel_Floor != null) panel_Floor.SetActive(false);
        if (panel_Object != null) panel_Object.SetActive(false);
        if (highlightedBuilding != null) { SetMarkerState(highlightedBuilding, true, false); highlightedBuilding = null; }
    }

    public void FocusOnSearchObject(InteractableObjectInfo obj) {
        if (obj == null) return;
        isLocked = true; isPanelDismissed = false;
        BuildingInfo pb = obj.GetComponentInParent<BuildingInfo>();
        if (pb != null) { activeBuilding = pb; SetExteriorColliders(false, true); }
        Vector3 target = (obj.GetComponent<Collider>() != null) ? obj.GetComponent<Collider>().bounds.center : obj.transform.position;
        Vector3 dir = (target - cameraController.transform.position).normalized;
        Vector3 fPos = target - (dir * 3.2f); fPos.y += 1.35f; Quaternion fRot = Quaternion.LookRotation(target - fPos);
        cameraController.FlyToDirect(fPos, fRot); UpdateUI(null, null, obj);
    }

    public bool AnyPanelActive() {
        return (panel_Building != null && panel_Building.activeSelf) || (panel_Personnel != null && panel_Personnel.activeSelf) || (panel_Floor != null && panel_Floor.activeSelf) || (panel_Object != null && panel_Object.activeSelf);
    }
}
