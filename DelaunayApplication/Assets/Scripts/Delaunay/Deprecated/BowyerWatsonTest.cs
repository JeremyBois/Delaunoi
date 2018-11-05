using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Delaunay.Algorithms;
using Delaunay.Generators;
using Delaunay.DataStructures;
using Delaunay.DataStructures.Extensions;

public class BowyerWatsonTest : MonoBehaviour
{
    [Tooltip("Number of points to compute.")]
    [SerializeField]
    private int pointNumber = 10;
    [Tooltip("Boundaries used for drawing.")]
    [SerializeField]
    private int[] boundaries = {200, 100};

    [Tooltip("Seed used for point generation.")]
    [SerializeField]
    int sequenceInit = 100;
    [Tooltip("Base used to compute Halton sequence.")]
    [SerializeField]
    private int[] bases = {2, 3};

    [Tooltip("GameObject used to represent a point.")]
    [SerializeField]
    private GameObject[] shapes = new GameObject[2];
    [Tooltip("Scale coefficient for the shape.")]
    [SerializeField]
    private float ShapeScale = 1.0f;
    [Tooltip("Material used for Line Renderer.")]
    [SerializeField]
    private Material lineMat;

    [Tooltip("Colors used for points.")]
    [SerializeField]
    private Color[] colors;
    private int colorsLength;

    private Vector3[] points;
    private GameObject[] pointsObject;

    public BowyerWatson triangulator;

    void Awake ()
    {
        colorsLength = colors.Length;
        int n = sequenceInit;
        points = new Vector3[pointNumber];
        pointsObject = new GameObject[pointNumber];

        for (int i = 0; i < points.Length; i++)
        {
            // Random sparse distribution
            points[i] = HaltonSequence.Halton2D(n, bases[0], bases[1]).AsVector3();
            // Scale to boundaries and translate based on generator position
            points[i].x *= boundaries[0] + transform.position.x;
            points[i].y *= boundaries[1] + transform.position.y;
            points[i].z += transform.position.z;
            n++;
        }

        // Place camera
        var cam = Camera.main;
        cam.transform.position = new Vector3(boundaries[0] / 2.0f, boundaries[1] / 2.0f, -0.7f * Mathf.Max(boundaries));
    }

	// Use this for initialization
	void Start ()
    {
        System.DateTime previousTime = System.DateTime.Now;

        // Generate
        triangulator = new BowyerWatson(points);
        triangulator.Triangulate();
        System.DateTime generated = System.DateTime.Now;
        System.TimeSpan delta = generated - previousTime;
        Debug.Log(string.Format("{0} secondes OU {1} milliseconds (Generation)",
                  delta.TotalSeconds, delta.TotalMilliseconds));

        // // Draw
        // triangulator.DrawDelaunay(shapes, this.transform, ShapeScale, colors, lineMat, points:true,
        //                        circumcenter:false, circumcercle:false, area:true, edges:true);
        // delta = System.DateTime.Now - generated;
        // Debug.Log(string.Format("{0} secondes OU {1} milliseconds (Drawing)",
        //           delta.TotalSeconds, delta.TotalMilliseconds));

        // foreach (PairVec vec in triangulator.ConstructVoronoi())
        // {
        //     Debug.DrawLine(vec.First, vec.Second, Color.blue, Mathf.Infinity);
        // }
	}
}
