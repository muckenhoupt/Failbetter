using UnityEngine;

public class Plotter : MonoBehaviour
{
    public GameObject PointPrefab;
    public GameObject BufferPrefab;
    public PolygonCollider2D Collider;
    public float BufferDistance = 40;

    public void Clear()
    {
        Debug.Log("Clear");
    }

    public void PlotPoints()
    {
        Debug.Log("PlotPoints");
    }

    public void PlotBuffer()
    {
        Debug.Log("PlotBuffer");
    }
}