using UnityEngine;

public class BuildingMarker3D : MonoBehaviour
{
    [Header("Arrow Position")]
    public float heightAboveBuilding = 10f;

    [Header("Arrow Size & Color")]
    public float coneRadius = 0.7f;
    public float coneHeight = 1.4f;
    public float tailWidth = 0.18f;
    public float tailLength = 2.2f;
    public Color arrowColor = new Color(0.15f, 0.5f, 1f, 1f);
    public Material markerMaterial; // [NEW] To fix visibility in builds

    private Transform arrowRoot;
    private float bobOffset;

    void Start()
    {
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        BuildArrow();
    }

    void BuildArrow()
    {
        GameObject rootGo = new GameObject("ArrowRoot");
        rootGo.transform.SetParent(transform);
        rootGo.transform.localPosition = Vector3.up * heightAboveBuilding;
        arrowRoot = rootGo.transform;

        // Determine material (fix for Builds)
        Material mat = markerMaterial;
        if (mat == null) {
            mat = new Material(Shader.Find("Standard")); 
            mat.color = arrowColor;
        }

        // --- Cone head (tip pointing DOWN = toward building) ---
        GameObject coneGo = new GameObject("ConeHead");
        coneGo.transform.SetParent(arrowRoot);
        coneGo.transform.localPosition = Vector3.zero;

        MeshFilter mf = coneGo.AddComponent<MeshFilter>();
        mf.mesh = BuildConeMesh(coneRadius, coneHeight, 12);

        MeshRenderer mr = coneGo.AddComponent<MeshRenderer>();
        mr.material = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        // --- Tail (cylinder going UP from cone base) ---
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tail.name = "Tail";
        tail.transform.SetParent(arrowRoot);
        tail.transform.localPosition = new Vector3(0, coneHeight + tailLength * 0.5f, 0);
        tail.transform.localRotation = Quaternion.identity;
        tail.transform.localScale = new Vector3(tailWidth, tailLength * 0.5f, tailWidth);
        Destroy(tail.GetComponent<CapsuleCollider>());
        Renderer tr = tail.GetComponent<Renderer>();
        tr.material = mat;
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tr.receiveShadows = false;
    }

    Mesh BuildConeMesh(float radius, float height, int segments)
    {
        Mesh mesh = new Mesh();

        Vector3[] verts = new Vector3[segments + 2];
        int[] tris = new int[segments * 6];

        // Tip at bottom (y = 0) — points toward building
        verts[0] = Vector3.zero;

        // Base circle at top of cone
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            verts[i + 1] = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
        }

        // Base center (for cap)
        verts[segments + 1] = new Vector3(0, height, 0);

        // Side triangles
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            // Side face
            tris[i * 6 + 0] = 0;
            tris[i * 6 + 1] = i + 1;
            tris[i * 6 + 2] = next + 1;
            // Cap face
            tris[i * 6 + 3] = segments + 1;
            tris[i * 6 + 4] = next + 1;
            tris[i * 6 + 5] = i + 1;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private bool isSelected = false;
    private float targetScale = 1f;
    private float currentScale = 1f;

    void Update()
    {
        if (arrowRoot == null || Camera.main == null) return;

        // Smooth scaling
        targetScale = isSelected ? 1.8f : 1f;
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * 6f);
        arrowRoot.localScale = Vector3.one * currentScale;

        // Billboard — face camera, keep upright
        Vector3 dirToCamera = Camera.main.transform.position - arrowRoot.position;
        arrowRoot.rotation = Quaternion.LookRotation(dirToCamera, Camera.main.transform.up);

        // Bob animation
        float bob = Mathf.Sin(Time.time * 1.8f + bobOffset) * 0.3f;
        arrowRoot.localPosition = Vector3.up * (heightAboveBuilding + bob);
    }

    public void SetVisible(bool visible)
    {
        if (arrowRoot != null)
            arrowRoot.gameObject.SetActive(visible);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }
}
