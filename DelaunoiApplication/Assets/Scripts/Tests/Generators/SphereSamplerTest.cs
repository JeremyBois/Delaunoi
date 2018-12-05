using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;


using Delaunoi.Algorithms;
using Delaunoi.Generators;
using Delaunoi.Tools;
using Delaunoi.DataStructures;
using Delaunoi.DataStructures.Extensions;
using Delaunoi.Tools.Extensions;


public class SphereSamplerTest : MonoBehaviour
{
    [Tooltip("Number of points to compute.")]
    [SerializeField]
    private int pointNumber = 1000;
    [SerializeField]
    private double radius = 50.0;

    [Header("Drawing Settings")]
    [Tooltip("GameObject used to represent a point.")]
    [SerializeField]
    private GameObject shape;
    [SerializeField]
    private float scale = 0.5f;
    [SerializeField]
    private float lineScale = 1.5f;
    [Tooltip("Material used for Line Renderer.")]
    [SerializeField]
    private Material mat;

    [SerializeField]
    private bool triangulate = false;
    [SerializeField]
    private bool triangulationOnSphere = false;


    private GuibasStolfi<int> triangulator;


    void Start()
    {
        // BUILDING  ---  ---  BUILDING  ---  ---  BUILDING
        System.DateTime previousTime = System.DateTime.Now;

        List<Vec3> points = SphereSampler.FibonnaciSphere(pointNumber, 1.0).ToList();

        System.TimeSpan delta = System.DateTime.Now - previousTime;
        Debug.Log(string.Format("BUILDING *** {0} secondes OU {1} milliseconds *** BUILDING",
                  delta.TotalSeconds, delta.TotalMilliseconds));
        Debug.Log("Total generated points: " + points.Count);

        if (triangulate)
        {
            // INIT  ---  ---  INIT  ---  ---  INIT
            previousTime = System.DateTime.Now;
            triangulator = new GuibasStolfi<int>(points.Select(x => Geometry.StereographicProjection(x)).ToArray(), false);
            delta = System.DateTime.Now - previousTime;
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

            // Draw Delaunay
            var triangles = new List<Vec3>();
            if (triangulationOnSphere)
            {
                triangles = triangulator.ExportDelaunay().Select(x => Geometry.InvStereographicProjection(x) * radius).ToList();
            }
            else
            {
                triangles = triangulator.ExportDelaunay().Select(x => x * radius).ToList();
            }

            TriangleDrawer.DrawLine(triangles, transform, mat, Color.black, lineScale);
            TriangleDrawer.DrawPoints(triangles, transform, shape, Color.red, scale);
        }

        else
        {
            // DRAWING  ---  ---  DRAWING  ---  ---  DRAWING
            points = points.Select(x => x * radius).ToList();
            int ptId = 0;
            foreach (Vec3 point in points)
            {
                var newGo = GameObject.Instantiate(shape);
                newGo.name = string.Format("Fibonnaci Sphere {0}", ptId.ToString());
                newGo.transform.SetParent(transform);
                newGo.transform.position = point.AsVector3();
                newGo.transform.localScale = new Vector3(scale, scale, scale);
                // Color
                var meshR = newGo.GetComponent<MeshRenderer>();
                if (meshR != null)
                {
                    meshR.materials[0].color = Color.black;
                }

                ptId++;
            }
        }
    }
}
