using UnityEngine;

public class PersonInfo : MonoBehaviour
{
    [Header("Camera View Settings")]
    public Transform targetCameraPose; // Optional: Drag an Empty GameObject here for a custom camera view

    [Header("Professional Team Info")]
    public string architect_name = "Mr. Arjun Sharma";
    public string structural_engineer = "Mr. Vikram Singh";
    public string civil_contractor = "L&T Construction Ltd.";
    public string project_manager = "Mr. Rajesh Kumar";
    public string site_supervisor = "Mr. Amit Gupta";
    public string interior_designer = "Ms. Priya Kapoor";
    public string exterior_designer = "Ms. Ananya Iyer";
    public string electric_contractor = "Bajaj Electricals";
    public string plumbing_contractor = "Supreme Pipes & Fittings";
    public string roofing_specialist = "Tata BlueScope Steel";
    public string painter_finishing = "Asian Paints Team";
    public string material_supplier = "UltraTech Cement";

    [Header("Project Timeline & Cost")]
    public string construction_start_date = "15-Aug-2022";
    public string completion_date = "26-Jan-2024";
    public float total_cost = 50000000f;
}
