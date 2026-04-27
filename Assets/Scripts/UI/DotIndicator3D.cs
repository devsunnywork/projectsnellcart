using UnityEngine;

public class DotIndicator3D : MonoBehaviour
{
    [Header("Indicator Settings")]
    public float height = 1.0f;
    public float size = 0.15f;
    public Color color = new Color(0.1f, 0.45f, 0.95f, 0.8f);

    private GameObject dot;
    private Material mat;

    void Start()
    {
        // Simple sphere as dot
        dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = "DotIcon";
        dot.transform.SetParent(transform);
        dot.transform.localPosition = Vector3.up * height;
        dot.transform.localScale = Vector3.one * size;

        // Remove sphere collider from icon (we use parental hit)
        Destroy(dot.GetComponent<SphereCollider>());

        mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = color;
        dot.GetComponent<Renderer>().material = mat;
    }

    void Update()
    {
        if (dot == null) return;
        
        // Pulsing animation
        float s = size * (1f + Mathf.Sin(Time.time * 4f) * 0.2f);
        dot.transform.localScale = Vector3.one * s;
    }

    public void SetHover(bool hover)
    {
        if (mat != null) mat.color = hover ? Color.orange : color;
    }
}
