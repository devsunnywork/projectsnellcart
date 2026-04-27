using UnityEngine;

public class RoomInfo : MonoBehaviour
{
    [Header("Room Sync IDs (Matching JSON)")]
    public string id = "RM_001_03_01";
    public string floor_id = "FLR_001_03";
    public string section_id = "SEC_001_03_A";

    [Header("Room Basic Details")]
    public string name = "Lab 301";
    public string type = "laboratory";
    public float area_sqm = 65f;

    [Header("Room Statistics")]
    public string condition = "good"; 
    public string last_inspection = "2024-11-15";
    public int construction_year = 2024;
    public float construction_cost = 280000f;

    [Header("Interior View Point (X, Y, Z Controls)")]
    public Vector3 targetPosition; // Focus here on CLICK (PHASE 3)
    public Vector3 targetRotation; 

    [Header("Indicator Settings (Blue Dot)")]
    public float indicatorHeight = 1.0f;
    public float dotSize = 0.12f;
    public Color dotColor = new Color(0.1f, 0.45f, 0.95f, 0.8f);

    private GameObject dotRoot;
    private Material dotMat;

    void Start()
    {
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<MeshCollider>();

        BuildSimpleDot();
    }

    void BuildSimpleDot()
    {
        dotRoot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dotRoot.name = "RoomIndicator";
        dotRoot.transform.SetParent(transform);
        dotRoot.transform.localPosition = Vector3.up * indicatorHeight;
        dotRoot.transform.localScale = Vector3.one * dotSize;

        DestroyImmediate(dotRoot.GetComponent<SphereCollider>());
        
        dotMat = new Material(Shader.Find("Unlit/Color"));
        dotMat.color = dotColor;
        dotRoot.GetComponent<Renderer>().material = dotMat;

        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.radius = 0.5f; 
        sc.center = Vector3.up * indicatorHeight;
        sc.isTrigger = true;
    }

    void Update()
    {
        if (dotRoot == null || Camera.main == null) return;
        var camCtrl = Camera.main.GetComponent<CameraController>();
        bool active = camCtrl != null && camCtrl.currentPhase != CameraController.CameraPhase.Aerial;
        dotRoot.SetActive(active);

        if (active)
        {
            float s = dotSize * (1f + Mathf.Sin(Time.time * 3f) * 0.2f);
            dotRoot.transform.localScale = Vector3.one * s;
        }
    }

    public void SetHover(bool hover)
    {
        if (dotMat != null)
            dotMat.color = hover ? Color.orange : dotColor;
    }
}
