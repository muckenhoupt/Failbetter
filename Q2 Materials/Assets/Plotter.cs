using UnityEngine;

public class Plotter : MonoBehaviour
{
    public GameObject PointPrefab;
    public GameObject BufferPrefab;
    public PolygonCollider2D Collider;
    public float BufferDistance = 40;

    public float MarkerDistance = 10f;


    private void Plot(Vector3 point, GameObject prefab) {
        GameObject o = Instantiate(prefab, point, Quaternion.identity);
        o.tag = "Marker";
    }

    public void Clear()
    {

        int count = 0;

        foreach(GameObject o in FindObjectsOfType<GameObject>()) {
            if (o.tag == "Marker") {
                Object.DestroyImmediate(o);
                count++;
            }
        }
        Debug.Log("Cleared "+count+" objects");
    }

    // Given two points on the perimeter, try to fill the space between them with
    // instances of PointPrefab, spaced no more than MarkerDistance apart.
    private void FillPoints(Vector2 pointA, Vector2 pointB, Collider2D asteroid) {
        Vector2 atob = (pointB - pointA).normalized;
        Vector2 normal = new Vector2(-atob.y, atob.x);

        float minSeparation = MarkerDistance / 2;

        Vector2 lastPoint = pointA;
        while (Vector2.Distance(lastPoint, pointB) > minSeparation) {

            // Take a step towards point B
            Vector2 raySource = lastPoint + (atob * minSeparation);

            // Step outward if we're on the collider
            while (asteroid.OverlapPoint(raySource))
                raySource += normal;

            RaycastHit2D hit = Physics2D.Raycast(raySource, -normal);

            if (!hit) {
                print("Warning: Missed collider while trying to plot points");
                return;
            }

            Plot(hit.point, PointPrefab);

            // The curvature of the collider can cause the points plotted this
            // way to be too far apart. If they are, fill in the space between
            // them recursively.
            if (Vector2.Distance(lastPoint, hit.point) > MarkerDistance) {
                FillPoints(lastPoint, hit.point, asteroid);
            }
            lastPoint = hit.point;
        }
    }

    public void PlotPoints()
    {
        GameObject asteroid = GameObject.Find("Asteroid");
        Collider2D asteroidCollider = asteroid.GetComponent<Collider2D>();

        Bounds b = asteroidCollider.bounds;
        Vector2 center = new Vector2(b.center.x, b.center.y);
        float radius = b.max.x + b.max.y;


        int nPoints = 20;

        // Cast rays inward from a circle around the object.
        // Fill in the space between the points found this way with more points.    
        float dtheta = 2f * Mathf.PI / nPoints;
        Vector2 firstPoint = Vector2.zero;
        Vector2 lastPoint = Vector2.zero;
        for (int i=0; i<nPoints; i++) {
            float theta = dtheta * i;
            Vector2 rayDirection = new Vector2(Mathf.Cos(theta), -Mathf.Sin(theta));
            Vector2 raySource = rayDirection * -radius + center;
            RaycastHit2D hit = Physics2D.Raycast(raySource, rayDirection);
            if (!hit) continue; 

            Plot(hit.point, PointPrefab);
            if (i == 0) {
                firstPoint = hit.point;
            } else {
                FillPoints(lastPoint, hit.point, asteroidCollider);
            }
            lastPoint = hit.point;
        }
        FillPoints(lastPoint, firstPoint, asteroidCollider);


    }

    // FillBuffer and PlotBuffer are much like FillPoints and PlotPoints,
    // except that they use CircleCast instead of Raycast, to find points
    // exactly BufferDistance away from the collider.

    private void FillBuffer(Vector2 pointA, Vector2 pointB, Collider2D asteroid) {
        Vector2 atob = (pointB - pointA).normalized;
        Vector2 normal = new Vector2(-atob.y, atob.x);

        float minSeparation = MarkerDistance / 2;

        Vector2 lastPoint = pointA;
        while (Vector2.Distance(lastPoint, pointB) > minSeparation) {
            Vector2 raySource = lastPoint + (atob * minSeparation);
            RaycastHit2D hit = Physics2D.CircleCast(raySource, BufferDistance, -normal);

            if (!hit) {
                print("Warning: Missed collider while trying to plot buffer");
                return;
            }

            while (hit.centroid == raySource) {
                raySource += (normal * BufferDistance);
                hit = Physics2D.CircleCast(raySource, BufferDistance, -normal);
            }

            Plot(hit.centroid, BufferPrefab);
            lastPoint = hit.centroid;

            // Unlike FillPoints, I'm finding that the the smoothing effect of the
            // buffer distance makes recursive filling unnecessary.
        }
    }

    public void PlotBuffer()
    {
        GameObject asteroid = GameObject.Find("Asteroid");
        Collider2D asteroidCollider = asteroid.GetComponent<Collider2D>();

        Bounds b = asteroidCollider.bounds;
        Vector2 center = new Vector2(b.center.x, b.center.y);
        float radius = b.max.x + b.max.y + BufferDistance;

        int nPoints = 20;

        // Cast rays inward from a circle around the object.
        // Fill in the space between the points found this way with more points.    
        float dtheta = 2f * Mathf.PI / nPoints;
        Vector2 firstPoint = Vector2.zero;
        Vector2 lastPoint = Vector2.zero;


        for (int i = 0; i < nPoints; i++) {
            float theta = dtheta * i;
            Vector2 rayDirection = new Vector2(Mathf.Cos(theta), -Mathf.Sin(theta));
            Vector2 raySource = rayDirection * -radius + center;
            RaycastHit2D hit = Physics2D.CircleCast(raySource, BufferDistance, rayDirection);
            if (!hit) continue; 
            Plot(hit.centroid, BufferPrefab);
            if (i == 0) {
                firstPoint = hit.centroid;
            } else {
                FillBuffer(lastPoint, hit.centroid, asteroidCollider);
            }
            lastPoint = hit.centroid;
        }
        FillBuffer(lastPoint, firstPoint, asteroidCollider);


    }
}