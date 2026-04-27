using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class UIMarker : MonoBehaviour
{
    [Header("Circle Settings")]
    public float radius = 0.5f;
    public float lineWidth = 0.05f;
    public int segments = 36;
    public Color markerColor = new Color(0.15f, 0.5f, 1f, 0.5f); // Semi-transparent blue

    [Header("Placement")]
    public Vector3 offset = new Vector3(0, 0.01f, 0); // Slightly above surface to avoid Z-fighting
    public bool faceCamera = false;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        SetupLine();
        DrawCircle();
    }

    void OnValidate()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        SetupLine();
        DrawCircle();
    }

    void SetupLine()
    {
        if (line == null) return;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = segments;
        
        // Simple unlit-colored material for transparency
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = markerColor;
        line.endColor = markerColor;
    }

    void DrawCircle()
    {
        if (line == null) return;

        float angle = 0f;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, 0, z) + offset);
            angle += (360f / segments);
        }
    }

    void Update()
    {
        if (faceCamera && Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}
