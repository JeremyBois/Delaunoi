using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Delaunay.Generators;

public class TriangleFuncTest : MonoBehaviour
{
    [Tooltip("Seed used for point generation.")]
    [SerializeField]
    int sequenceInit = 100;

    [Tooltip("GameObject used to represent a point.")]
    [SerializeField]
    private GameObject shape;

    [Tooltip("Scale coefficient for the shape.")]
    [SerializeField]
    private float ShapeScale = 2.0f;

    [Tooltip("Material used for Line Renderer.")]
    [SerializeField]
    private Material material;

    [Tooltip("Colors used for points.")]
    [SerializeField]
    private Color[] colors;
    private int colorsLength;

    private Vector3[] points;
    private GameObject[] pointsObject;

	// Use this for initialization
	void Start ()
    {
        colorsLength = colors.Length;

        // Create random points
        int n = sequenceInit;
        points = new Vector3[3];
        pointsObject = new GameObject[3];

        points[0] = new Vector3(transform.position.x + 10.0f,
                                transform.position.y + 5.0f);
        points[1] = new Vector3(transform.position.x + 60.0f,
                                transform.position.y + 7.0f);
        points[2] = new Vector3(transform.position.x + 50.0f,
                                transform.position.y + 50.0f);


        for (int i = 0; i < points.Length; i++)
        {
            var newShape = GameObject.Instantiate(shape);
            newShape.name = i.ToString();
            newShape.transform.SetParent(transform);
            newShape.transform.position = points[i];
            newShape.transform.localScale = new Vector3(ShapeScale,
                                                        ShapeScale,
                                                        ShapeScale
                                                        );
            // Color
            var meshR = newShape.GetComponent<MeshRenderer>();
            if (meshR != null)
            {
                meshR.materials[0].color = colors[i];
            }

            pointsObject[i] = newShape;
        }

        // Fill area
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        var filter =gameObject.GetComponent<MeshFilter>();
        var renderer =gameObject.GetComponent<MeshRenderer>();
        renderer.material = material;
        renderer.materials[0].color = Color.yellow;
        filter.mesh.vertices =  points;
        filter.mesh.triangles = new []{2, 1, 0};

        // Circle
        var center = ComputeCircumcenter(points[0], points[1], points[2]);
        var radius = Radius(points[0], center);
        Vector3 result = CircumcenterAndRadius(points[0], points[1], points[2]);
        // Debug.Log(radius + "---" + result.z);
        Debug.Log(center.x + "---" + center.y);
        Debug.Log(result.x + "---" + result.y);

        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        lr.material = material;
        lr.positionCount = 100;
        lr.loop = true;
        lr.material.color = Color.black;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.startWidth = 2.0f;
        lr.endWidth = 2.0f;

        int segments = lr.positionCount;
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < segments; i++)
        {
            x = center.x + Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
            y = center.y + Mathf.Cos (Mathf.Deg2Rad * angle) * radius;

            lr.SetPosition (i, new Vector3(x, y, z) );

            angle += (360f / segments);
        }
	}

	// Update is called once per frame
	void Update ()
    {
        // Assume CCW order
        var pos = Input.mousePosition;
        var mousePos = Camera.main.ScreenToWorldPoint(pos);
        Color color = Color.yellow;

        if (Ccw(mousePos, points[1], points[0]))
        {
            color = Color.red;
        }
        // double result = Determinant(mousePos, points[0], points[1], points[2]);
        // Debug.Log("Pos --> " + mousePos.x + ", " + mousePos.y);
        // Debug.Log("Determinant --> " + result);
        // if (result == 0.0)
        // {
        //     color = Color.blue;
        // }
        // else if (result > 0.0)
        // {
        //     color = Color.green;
        // }
        // else if (result < 0.0)
        // {
        //     color = Color.red;
        // }

        var renderer = gameObject.GetComponent<MeshRenderer>();
        renderer.materials[0].color = color;
	}

    /// <summary>
    /// Return true if points (a, b, c) are in a counter clockwise order, else false.
    /// Computes the following determinant after reduction to a 2x2 matrix.
    ///   | ax  ay  1 |
    ///   | bx  by  1 | > 0
    ///   | cx  cy  1 |
    /// </summary>
    private bool Ccw(Vector3 a, Vector3 b, Vector3 c)
    {
        // return ((a.x - c.x) * (b.y - c.y) - (a.y - c.y) * (b.x - c.x)) > 0.0;
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0.0;
    }

    /// <summary>
    /// Return true if point pt are in the circumcercle of the three other ones (a, b, c).
    /// Computes the following determinant (assume a CCW abc triangle).
    /// after reduction to a 3x3 matrix.
    ///   | ax  ay  ax² + ay²  1 |
    ///   | bx  by  bx² + by²  1 | > 0
    ///   | cx  cy  cx² + cy²  1 |
    ///   | dx  dy  dx² + dy²  1 |
    /// </summary>
    private double Determinant(Vector3 pt, Vector3 a, Vector3 b, Vector3 c)
    {
        double Ax = a.x - pt.x;
        double Ay = a.y - pt.y;

        double Bx = b.x - pt.x;
        double By = b.y - pt.y;

        double Cx = c.x - pt.x;
        double Cy = c.y - pt.y;

        // Debug.Log(Ax);
        // Debug.Log(Ay);
        // Debug.Log(Bx);
        // Debug.Log(By);
        // Debug.Log(Cx);
        // Debug.Log(Cy);

        // Debug.Log("ddddddddddddddd");
        // Debug.Log(pt.x + ", " + pt.y);
        // Debug.Log(a.x + ", " + a.y);
        // Debug.Log(b.x + ", " + b.y);
        // Debug.Log(c.x + ", " + c.y);
        // Debug.Log("ddddddddddddddd");

        double AxAy = Ax * Ax + Ay * Ay;
        double BxBy = Bx * Bx + By * By;
        double CxCy = Cx * Cx + Cy * Cy;

        // // Assuming CCW triangle abc and a point pt
        // // Determinant > 0  => pt inside circle
        // // Determinant < 0  => pt outside circle
        // // Determinant == 0 => pt on circle
        return (AxAy * (Bx * Cy - By * Cx) -
                BxBy * (Ax * Cy - Ay * Cx) +
                CxCy * (Ax * By - Ay * Bx));
        // return (Ax * (By * CxCy - BxBy * Cy) -
        //         Ay * (Bx * CxCy - BxBy * Cx) +
        //         AxAy * (Bx * Cy - By * Cx));
    }

    public Vector3 ComputeCircumcenter(Vector3 _a, Vector3 _b, Vector3 _c)
    {
        Vector3 ca = _c - _a;
        Vector3 ba = _b - _a;

        // Precompute cross product
        Vector3 baca = Vector3.Cross(ba, ca);
        // Compute inverse of denominator
        float invDenominator = 0.5f / baca.sqrMagnitude;
        // Compute numerator
        Vector3 numerator =  Vector3.Cross(ca.sqrMagnitude * baca, ba) +
                             ba.sqrMagnitude * Vector3.Cross(ca, baca);
        // Compute circumcenter
        return _a + (numerator * invDenominator);
    }

    public float Radius(Vector3 _a, Vector3 _center)
    {
        return Vector3.Magnitude(_a - _center);
    }

    public Vector3 CircumcenterAndRadius(Vector3 a, Vector3 b, Vector3 c)
    {
        // Center
        double ByAy = b.y - a.y;
        double CyAy = c.y - a.y;
        double BxAx = b.x - a.x;
        double CxAx = c.x - a.x;
        double BAsquared = BxAx * BxAx + ByAy * ByAy;
        double CAsquared = CxAx * CxAx + CyAy * CyAy;

        double denominator = 0.5 / (BxAx * CyAy - ByAy * CxAx);
        double xRel = denominator * (ByAy * CAsquared - CyAy * BAsquared);
        double yRel = denominator * (BxAx * CAsquared - CxAx * BAsquared);

        // Radius
        double BxCx = b.x - c.x;
        double ByCy = b.y - c.y;
        double BCsquared = BxCx * BxCx + ByCy * ByCy;

        // Costly operation but only needed after triangulation
        double r = denominator * Math.Sqrt(BAsquared * CAsquared * BCsquared);

        return new Vector3((float)(a.x - xRel), (float)(a.y + yRel), (float)r);
    }
}
