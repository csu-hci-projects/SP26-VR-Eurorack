using UnityEngine;

[ExecuteAlways]
public class StretchableWire : MonoBehaviour
{
    public Transform jackA;
    public Transform jackB;
    public float sag = 0.5f;
    public int segments = 20;
    public float width = 0.05f;

    private LineRenderer lineRenderer;

    void Update()
    {
        if (jackA == null || jackB == null) return;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.numCornerVertices = 10;
        lineRenderer.numCapVertices = 10;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 point = Vector3.Lerp(jackA.position, jackB.position, t);
            point.y -= sag * Mathf.Sin(t * Mathf.PI);
            lineRenderer.SetPosition(i, point);
        }
    }
}