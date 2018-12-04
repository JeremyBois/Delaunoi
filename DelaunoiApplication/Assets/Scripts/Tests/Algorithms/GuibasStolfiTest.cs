using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

using Delaunoi.Algorithms;
using Delaunoi.Generators;
using Delaunoi.DataStructures;
using Delaunoi.Tools;
using Delaunoi.DataStructures.Extensions;
using Delaunoi.Tools.Extensions;


public enum Generator
{
    Halton,
    Poisson,
    Grid
}

public class GuibasStolfiTest : MonoBehaviour
{
    [Header("Generation Global Settings")]
    [SerializeField]
    private Generator usedGenerator;
    [Tooltip("Number of points to compute.")]
    [SerializeField]
    private int pointNumber = 10;
    [Tooltip("Boundaries used for drawing.")]
    [SerializeField]
    private int[] boundaries = {200, 200};
    [Tooltip("Seed used for point generation.")]
    [SerializeField]
    int seed = 154;

    [Space(5)]
    [Header("Halton Settings")]
    [Tooltip("Base used to compute Halton sequence.")]
    [SerializeField]
    private int[] bases = {2, 3};

    [Space(5)]
    [Header("Poisson Disk Settings")]
    [Tooltip("Base used to compute Halton sequence.")]
    [SerializeField]
    private float minimalDistance = 15.0f;
    [SerializeField]
    private int   maxAttemp = 30;
    private PoissonDisk2D poissonGen;
    [Space(30)]

    [Header("Drawing Settings")]
    [Tooltip("GameObject used to represent a point.")]
    [SerializeField]
    private GameObject[] shapes = new GameObject[2];
    [SerializeField]
    private float scale = 1.0f;
    [SerializeField]
    private float lineScale = 1.5f;
    [Tooltip("Material used for Line Renderer.")]
    [SerializeField]
    private Material mat;
    [SerializeField]
    private Gradient gradient;
    [Space(30)]


    [Header("Delaunay Settings")]
    public bool D_points = false;
    public bool D_lines = false;
    public bool D_faces = false;
    [Space(30)]

    [Header("Face Settings")]
    [Space(5)]
    [SerializeField]
    private FaceConfig celltype;
    [Space(5)]
    public bool V_points = false;
    public bool V_lines = false;
    public bool V_faces = false;
    public bool V_circles = false;
    [Space(30)]


    private List<Vec3> points;
    private GuibasStolfi<int> triangulator;

    void Awake ()
    {
        // Create random points
        int n = seed;
        points = new List<Vec3>();

        switch (usedGenerator)
        {
            case Generator.Halton:
                for (int i = 0; i < pointNumber; i++)
                {
                    // Random sparse distribution
                    Vec3 temp = HaltonSequence.Halton2D(n, bases[0], bases[1]);
                    points.Add(new Vec3(temp.x * boundaries[0] + transform.position.x,
                                         temp.y * boundaries[1] + transform.position.y,
                                         transform.position.z));
                    n++;
                }
                break;
            case Generator.Poisson:
                RandGen.Init(seed);
                poissonGen = new PoissonDisk2D(minimalDistance, boundaries[0], boundaries[1],
                                               maxAttemp);
                poissonGen.BuildSample(pointNumber);
                foreach (Vec3 point in poissonGen)
                {
                    points.Add(point);
                }
                break;
            default:
                throw new NotImplementedException();
        }


        // Place camera
        var cam = Camera.main;
        cam.transform.position = new Vector3(boundaries[0] / 2.0f, boundaries[1] / 2.0f, -0.8f * Mathf.Max(boundaries));
    }

    // Use this for initialization
    void Start ()
    {
        // INIT  ---  ---  INIT  ---  ---  INIT
        System.DateTime previousTime = System.DateTime.Now;
        triangulator = new GuibasStolfi<int>(points.ToArray(), false);
        System.TimeSpan delta = System.DateTime.Now - previousTime;
        Debug.Log("***");
        Debug.Log(string.Format("*** INIT *** {0} secondes OU {1} milliseconds *** INIT",
                  delta.TotalSeconds, delta.TotalMilliseconds));


        // TRIANGULATION  ---  ---  TRIANGULATION  ---  ---  TRIANGULATION
        previousTime = System.DateTime.Now;
        triangulator.ComputeDelaunay();
        delta = System.DateTime.Now - previousTime;
        Debug.Log("***");
        Debug.Log(string.Format("*** TRIANGULATION *** {0} secondes OU {1} milliseconds *** TRIANGULATION",
                  delta.TotalSeconds, delta.TotalMilliseconds));

        Vec3 pos;

        // LOCATE  ---  ---  LOCATE  ---  ---  LOCATE
        // points >= 10 | seed 154
        pos = new Vec3(0.72 * boundaries[0], 0.546 * boundaries[1], 0.0);
        // var newGo = GameObject.Instantiate(shapes[0]);
        // newGo.name = "PointLocated";
        // newGo.transform.SetParent(transform);
        // newGo.transform.position = pos.AsVector3();
        // newGo.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
        // // Color
        // var meshR = newGo.GetComponent<MeshRenderer>();
        // if (meshR != null)
        // {
        //     meshR.materials[0].color = Color.green;
        // }
        // Start locate
        previousTime = System.DateTime.Now;
        var edge = triangulator.Locate(pos, safe: true);

        delta = System.DateTime.Now - previousTime;
        Debug.Log("***");
        Debug.Log(string.Format("*** LOCATE *** {0} secondes OU {1} milliseconds *** LOCATE",
                  delta.TotalSeconds, delta.TotalMilliseconds));
        Debug.Log("Point is inside: " + triangulator.InsideConvexHull(pos));
        Debug.Log("Edge origin is: " + edge.Origin);


        // INSERT  ---  ---  INSERT  ---  ---  INSERT
        // points >= 10 | seed 154
        previousTime = System.DateTime.Now;
        pos = new Vec3(1.1 * boundaries[0], 0.8 * boundaries[1], 0.0);
        bool result = triangulator.InsertSite(pos, safe:true);
        Debug.Log("Site outside --> Not added: " + !result);
        pos = new Vec3(0.72265633333 * boundaries[0], 0.54732506667 * boundaries[1], 0.0);
        result = triangulator.InsertSite(pos);
        Debug.Log("Site already existing --> Not added: " + !result);
        pos = new Vec3(0.76666666666667 * boundaries[0], pos.y, pos.z);
        result = triangulator.InsertSite(pos);
        Debug.Log("Inside convex Hull --> Added: " + result);
        delta = System.DateTime.Now - previousTime;
        Debug.Log("***");
        Debug.Log(string.Format("*** INSERT *** {0} secondes OU {1} milliseconds *** INSERT",
                  delta.TotalSeconds, delta.TotalMilliseconds));




        // DRAWING  ---  ---  DRAWING  ---  ---  DRAWING
        previousTime = System.DateTime.Now;

        // Draw Delaunay
        var triangles = triangulator.ExportDelaunay();
        if (D_faces)
        {
            TriangleDrawer.DrawFace(triangles, transform, mat, gradient);
        }
        if (D_lines)
        {
            TriangleDrawer.DrawLine(triangles, transform, mat, Color.black, lineScale);
        }
        if (D_points)
        {
            TriangleDrawer.DrawPoints(triangles, transform, shapes[0], Color.red, 1.1f * scale);
        }

        // Draw faces
        FaceBuilder<int> facesBuilder = new FaceBuilder<int>(triangulator);

        List<Face<int>> faces = facesBuilder.Faces(celltype, Mathf.Max(boundaries) * 5.0, true)
                                            // .InsideHull()
                                            // .FiniteBounds()
                                            // .Finite()
                                            // .Bounds()
                                            // .AtInfinity()
                                            // .CenterCloseTo(new Vec3(boundaries[0] / 2.0, boundaries[1] / 2.0, 0.0), 50.0)
                                            // .CloseTo(new Vec3(boundaries[0] / 2.0, boundaries[1] / 2.0, 0.0), 50.0)
                                            // .Inside(Vec3.Zero, new Vec3(boundaries[0] * 0.25, boundaries[1] * 1.0, 1.0))
                                            // .Inside(Vec3.Zero, new Vec3(boundaries[0] * 0.25, boundaries[1] * 0.5, 1.0))
                                            .ToList();

        float nbCells = (float)faces.Count;
        int indcolor2 = 0;
        foreach (Face<int> face in faces)
        {
            var color = gradient.Evaluate(indcolor2 / nbCells);

            if (V_faces)
            {
                face.DrawFace(transform, mat, color);
            }
            if (V_lines)
            {
                face.DrawLine(transform, mat, Color.white, lineScale, loop:true);
            }
            if (V_points)
            {
                face.DrawPoints(transform, shapes[1], mat, Color.blue, 0.8f * scale);
            }
            // if (V_circles)
            // {
            //     face.DrawCircumCercle(transform, mat, color);
            // }

            indcolor2++;
        }

        delta = System.DateTime.Now - previousTime;
        Debug.Log("***");
        Debug.Log(string.Format("*** DRAWING *** {0} secondes OU {1} milliseconds *** DRAWING",
                  delta.TotalSeconds, delta.TotalMilliseconds));
        Debug.Log("Points count : " + points.Count);
        Debug.Log("Triangle count : " + triangles.Count / 3);
        Debug.Log("Face count : " + nbCells);
    }




    /// <summary>
    /// Helper to convert Vector3 to Vec3
    /// </summary>
    public Vec3[] Vector3ToVec3(Vector3[] points, bool alreadySorted=true)
    {
        if (!alreadySorted)
        {
            return points.Select(val => new Vec3(x:val.x, y:val.y, z:val.z))
                                          .OrderBy(vec => vec.x)
                                          .ThenBy(vec => vec.y)
                                          .ToArray();
        }
        else
        {
            return points.Select(val => new Vec3(x:val.x, y:val.y, z:val.z)).ToArray();
        }
    }
}
