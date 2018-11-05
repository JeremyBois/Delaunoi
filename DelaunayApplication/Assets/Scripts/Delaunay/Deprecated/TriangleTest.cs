using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;


using DS = Delaunay.DataStructure;

public class TriangleTest : MonoBehaviour
{
    private Vector3[] points = new Vector3[3]
        {
            new Vector3(-13.0f, 14.0f, 0.0f),
            new Vector3(-10f, -2f, 0.0f),
            new Vector3(-3.0f, 4.0f, 0.0f)
        };

    // Show results as public
    public Vector3 circumcenter;
    public float squaredRadius;

    private DS.Triangle tri;
    private float epsilon = 0.00001f;

	// Use this for initialization
	void Start ()
    {

        // Test circumcenter calculation
        // https://www.geogebra.org/graphing
        tri = new DS.Triangle(points[0], points[1], points[2]);
        circumcenter = tri.Circumcenter;
        squaredRadius = tri.SquaredCircumcenterRadius;
        Debug.Log(Mathf.Abs(squaredRadius - 8.1634558757f * 8.1634558757f) < epsilon);

        var test = new HashSet<Vector3>();


        foreach (Vector3 vec in points)
        {
            test.Add(vec);
            Debug.Log(test.Contains(vec));
        }

        // Hash clash can occurs if a coordinate does note change and the other is very close
        // 1e10^-6
        var another = new Vector3(-13.000001f, 14.000001f, 0.0f);
        test.Add(another);
        Debug.Log(test.Count == 4);


        // Test for null contains
        Debug.Log("\n Triangle and HashSet");
        var hashTri = new HashSet<DS.Triangle>();
        hashTri.Add(tri);
        Debug.Log(hashTri.Contains(tri));
        Debug.Log(tri.GetAdjacent(0));
        Debug.Log(hashTri.Contains(tri.GetAdjacent(0)));

        // Hash always different even if value are the same
        var tri2 = new DS.Triangle(tri);
        hashTri.Add(tri2);
        Debug.Log(hashTri.Count);
        Debug.Log(hashTri.Contains(tri2));

        // Hash in different container
        var hashTri2 = new HashSet<DS.Triangle>();
        hashTri2.Add(tri);
        hashTri2.ExceptWith(hashTri);
        Debug.Log(hashTri2.Count == 0);
	}
}
