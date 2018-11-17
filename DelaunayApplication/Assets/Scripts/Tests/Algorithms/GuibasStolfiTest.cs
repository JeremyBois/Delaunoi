using UnityEngine;

using System.Linq;
using System.Collections.Generic;

using Delaunay.Algorithms;
using Delaunay.Generators;
using Delaunay.DataStructures;
using Delaunay.DataStructures.Extensions;
using Delaunay.Tools.Extensions;


public class GuibasStolfiTest : MonoBehaviour
{
    [Header("Generation Settings")]
    [Tooltip("Number of points to compute.")]
    [SerializeField]
    private int pointNumber = 10;
    [Tooltip("Boundaries used for drawing.")]
    [SerializeField]
    private int[] boundaries = {200, 200};
    [Tooltip("Seed used for point generation.")]
    [SerializeField]
    int sequenceInit = 154;
    [Tooltip("Base used to compute Halton sequence.")]
    [SerializeField]
    private int[] bases = {2, 3};
    [Space(30)]

    [Header("Drawing Settings")]
    [Tooltip("GameObject used to represent a point.")]
    [SerializeField]
    private GameObject[] shapes = new GameObject[2];
    [SerializeField]
    private float scale = 1.0f;
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

    [Header("Cell Settings")]
    [Space(5)]
    [SerializeField]
    private CellConfig celltype;
    [Space(5)]
    public bool V_points = false;
    public bool V_lines = false;
    public bool V_faces = false;
    public bool V_circles = false;
    [Space(30)]


    private Vec3[] points;
    private GuibasStolfi<int> triangulator;

    void Awake ()
    {
        // Create random points
        int n = sequenceInit;
        points = new Vec3[pointNumber];

        for (int i = 0; i < points.Length; i++)
        {
            // Random sparse distribution
            Vec3 temp = HaltonSequence.Halton2D(n, bases[0], bases[1]);
            points[i] = new Vec3(temp.x * boundaries[0] + transform.position.x,
                                 temp.y * boundaries[1] + transform.position.y,
                                 transform.position.z);
            n++;
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
        triangulator = new GuibasStolfi<int>(points, false);
        System.TimeSpan delta = System.DateTime.Now - previousTime;
        Debug.Log(string.Format("INIT *** {0} secondes OU {1} milliseconds *** INIT",
                  delta.TotalSeconds, delta.TotalMilliseconds));


        // TRIANGULATION  ---  ---  TRIANGULATION  ---  ---  TRIANGULATION
        previousTime = System.DateTime.Now;
        triangulator.ComputeDelaunay();
        delta = System.DateTime.Now - previousTime;
        Debug.Log(string.Format("TRIANGULATION *** {0} secondes OU {1} milliseconds *** TRIANGULATION",
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
            TriangleDrawer.DrawLine(triangles, transform, mat, Color.black, 1.5f);
        }
        if (D_points)
        {
            TriangleDrawer.DrawPoints(triangles, transform, shapes[0], Color.black, 1.1f * scale);
        }

        // Draw cells
        List<Cell> cells = triangulator.ExportCells(celltype, Mathf.Max(boundaries) * 5.0);

        float nbCells = (float)cells.Count;
        int indcolor2 = 0;
        foreach (Cell cell in cells)
        {
            var color = gradient.Evaluate(indcolor2 / nbCells);

            if (V_faces)
            {
                cell.DrawFace(transform, mat, color);
            }
            if (V_lines)
            {
                cell.DrawLine(transform, mat, Color.blue, 1.5f, loop:true);
            }
            if (V_points)
            {
                cell.DrawPoints(transform, shapes[1], mat, Color.blue, 0.8f * scale);
            }
            if (V_circles)
            {
                cell.DrawCircumCercle(transform, mat, color);
            }

            indcolor2++;
        }

        delta = System.DateTime.Now - previousTime;
        Debug.Log(string.Format("DRAWING *** {0} secondes OU {1} milliseconds *** DRAWING",
                  delta.TotalSeconds, delta.TotalMilliseconds));
        Debug.Log("Triangle count : " + triangles.Count / 3);
        Debug.Log("Cell count : " + nbCells);
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
