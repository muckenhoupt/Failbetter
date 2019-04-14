using UnityEngine;

public class Plotter : MonoBehaviour
{
    public GameObject PointPrefab;
    public GameObject BufferPrefab;
    public PolygonCollider2D Collider;
    public float BufferDistance = 40;

    public float pointSeparation = 8f;


    private void Plot(Vector3 point, GameObject prefab) {
        Instantiate(prefab, point, Quaternion.identity);
    }

    public void Clear()
    {

        int count = 0;

        foreach(GameObject o in FindObjectsOfType<GameObject>()) {

            // OK, this is kind of silly.
            //
            // My first thought was to use PrefabUtility.GetPrefabInstanceStatus
            // to filter for prefab instances.
            // But it turns out that things created via Instantiate() aren't
            // flagged as prefab instances, even in editor mode.
            //
            // My second thought was to use tags. But it wasn't clear from the
            // instructions if I was allowed to modify the prefabs, and besides,
            // the spec says to "remove all instantiated prefabs", not just
            // instances of the two prefabs given.
            // 
            // This is the only reliable way I have found to identify all prefab
            // instances created via script.

            if (o.name.Contains("(Clone)")) {
                Object.DestroyImmediate(o);
                count++;
            }
        }
        Debug.Log("Cleared "+count+" objects");
    }

    private void FillPoints(Vector2 pointA, Vector2 pointB, Collider2D asteroid) {
        Vector2 atob = (pointB - pointA).normalized;
        Vector2 normal = new Vector2(-atob.y, atob.x);

        float minSeparation = pointSeparation / 2;

        Vector2 lastPoint = pointA;
        while (Vector2.Distance(lastPoint, pointB) > minSeparation) {
            Vector2 raySource = lastPoint + (atob * minSeparation);
            while (asteroid.OverlapPoint(raySource))
                raySource += normal;
            RaycastHit2D hit = Physics2D.Raycast(raySource, -normal);

            Plot(hit.point, PointPrefab);
            if (Vector2.Distance(lastPoint, hit.point) > pointSeparation) {
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


        int nPoints = 8;

        // Cast rays inward from a circle around the object.
        // If this leaves too large a gap, fill in with more points.    
        float dtheta = 2f * Mathf.PI / nPoints;
        Vector2 firstPoint = Vector2.zero;
        Vector2 lastPoint = Vector2.zero;
        for (int i=0; i<nPoints; i++) {
            float theta = dtheta * i;
            Vector2 rayDirection = new Vector2(Mathf.Cos(theta), -Mathf.Sin(theta));
            Vector2 raySource = rayDirection * -radius + center;
            RaycastHit2D hit = Physics2D.Raycast(raySource, rayDirection);
            if (hit)
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

    private void FillBuffer(Vector2 pointA, Vector2 pointB, Collider2D asteroid) {
        Vector2 atob = (pointB - pointA).normalized;
        Vector2 normal = new Vector2(-atob.y, atob.x);

        float minSeparation = pointSeparation / 2;

        Vector2 lastPoint = pointA;
        while (Vector2.Distance(lastPoint, pointB) > minSeparation) {
            Vector2 raySource = lastPoint + (atob * minSeparation);
            RaycastHit2D hit = Physics2D.CircleCast(raySource, BufferDistance, -normal);
            while (hit.centroid == raySource) {
                raySource += (normal * BufferDistance);
                hit = Physics2D.CircleCast(raySource, BufferDistance, -normal);
            }

            Plot(hit.centroid, BufferPrefab);
            if (Vector2.Distance(lastPoint, hit.centroid) > pointSeparation) {
                //FillBuffer(lastPoint, hit.centroid, asteroid);
            }
            lastPoint = hit.centroid;
        }
    }

    public void PlotBuffer()
    {
        GameObject asteroid = GameObject.Find("Asteroid");
        Collider2D asteroidCollider = asteroid.GetComponent<Collider2D>();

        Bounds b = asteroidCollider.bounds;
        Vector2 center = new Vector2(b.center.x, b.center.y);
        float radius = b.max.x + b.max.y + BufferDistance;

        int nPoints = 8;

        // Cast rays inward from a circle around the object.
        // If this leaves too large a gap, fill in with more points.    
        float dtheta = 2f * Mathf.PI / nPoints;
        Vector2 firstPoint = Vector2.zero;
        Vector2 lastPoint = Vector2.zero;


        for (int i = 0; i < nPoints; i++) {
            float theta = dtheta * i;
            Vector2 rayDirection = new Vector2(Mathf.Cos(theta), -Mathf.Sin(theta));
            Vector2 raySource = rayDirection * -radius + center;
            RaycastHit2D hit = Physics2D.CircleCast(raySource, BufferDistance, rayDirection);
            if (hit)
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