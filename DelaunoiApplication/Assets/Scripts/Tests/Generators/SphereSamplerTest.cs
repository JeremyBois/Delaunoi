using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;


using Delaunoi;
using Delaunoi.Generators;
using Delaunoi.Tools;
using Delaunoi.DataStructures;
using Delaunoi.DataStructures.Extensions;
using Delaunoi.Tools.Extensions;


public class SphereSamplerTest : MonoBehaviour
{
    private enum Generator
    {
        Fibonnaci,
        Halton,
        Uniform
    }

    [Tooltip("Number of points to compute.")]
    [SerializeField]
    private int pointNumber = 1000;
    [SerializeField]
    private double radius = 50.0;
    [SerializeField]
    private Generator usedGenerator;


    [Space(5)]
    [Header("Halton Settings")]
    [Tooltip("Seed used for point generation.")]
    [SerializeField]
    int seed = 154;
    [Tooltip("Base used to compute Halton sequence.")]
    [SerializeField]
    private int[] bases = { 2, 3 };


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
    private Gradient gradient;

    [SerializeField]
    private bool triangulate = false;
    [SerializeField]
    private bool triangulationOnSphere = false;


    private SphericalMesh<int> sphereMeshUsed;


    void Start()
    {
        // BUILDING  ---  ---  BUILDING  ---  ---  BUILDING
        System.DateTime previousTime = System.DateTime.Now;

        var points = new List<Vec3>();
        int n = seed;
        RandGen.Init(seed);


        switch (usedGenerator)
        {
            case Generator.Halton:
                points = SphereSampler.Halton(pointNumber, seed, bases[0], bases[1]).ToList();
                break;
            case Generator.Uniform:
                for (int i = 0; i < pointNumber; i++)
                {
                    double phi = 2.0 * System.Math.PI * RandGen.NextDouble();
                    double theta = System.Math.Acos(2.0 * RandGen.NextDouble() - 1.0);

                    points.Add(Geometry.SphericalToEuclidean(phi, theta));
                    n++;
                }
                break;
            case Generator.Fibonnaci:
                points = SphereSampler.Fibonnaci(pointNumber).ToList();
                break;
        }


        System.TimeSpan delta = System.DateTime.Now - previousTime;
        Debug.Log(string.Format("BUILDING *** {0} secondes OU {1} milliseconds *** BUILDING",
                  delta.TotalSeconds, delta.TotalMilliseconds));
        Debug.Log("Total generated points: " + points.Count);


        if (triangulate)
        {
            // INIT  ---  ---  INIT  ---  ---  INIT
            previousTime = System.DateTime.Now;
            sphereMeshUsed = new SphericalMesh<int>(points.Select(x => Geometry.StereographicProjection(x)).ToArray(), false);
            delta = System.DateTime.Now - previousTime;
            Debug.Log("***");
            Debug.Log(string.Format("*** INIT *** {0} secondes OU {1} milliseconds *** INIT",
                      delta.TotalSeconds, delta.TotalMilliseconds));

            // TRIANGULATION  ---  ---  TRIANGULATION  ---  ---  TRIANGULATION
            previousTime = System.DateTime.Now;
            sphereMeshUsed.Construct();
            delta = System.DateTime.Now - previousTime;
            Debug.Log("***");
            Debug.Log(string.Format("*** TRIANGULATION *** {0} secondes OU {1} milliseconds *** TRIANGULATION",
                      delta.TotalSeconds, delta.TotalMilliseconds));


            // // START DEBUGTOOLS
            // // START DEBUGTOOLS
            // // START DEBUGTOOLS


            // var test = radius * sphereMeshUsed.RightMostEdge.Rot.Origin;
            // if (triangulationOnSphere)
            // {
            //     test = radius * Geometry.InvStereographicProjection(sphereMeshUsed.RightMostEdge.Rot.Origin);
            // }
            // var newGo = GameObject.Instantiate(shape);
            // newGo.transform.SetParent(transform);
            // newGo.transform.position = test.AsVector3();
            // newGo.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            // // Color
            // var meshR = newGo.GetComponent<MeshRenderer>();
            // if (meshR != null)
            // {
            //     meshR.materials[0].color = Color.blue;
            // }

            // var test2 = radius * sphereMeshUsed.RightMostEdge.Destination;
            // if (triangulationOnSphere)
            // {
            //     test2 = radius * Geometry.InvStereographicProjection(sphereMeshUsed.RightMostEdge.Destination);
            // }
            // var newGo2 = GameObject.Instantiate(shape);
            // newGo2.transform.SetParent(transform);
            // newGo2.transform.position = test2.AsVector3();
            // newGo2.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            // // Color
            // var meshR2 = newGo2.GetComponent<MeshRenderer>();
            // if (meshR2 != null)
            // {
            //     meshR2.materials[0].color = Color.red;
            // }

            // int i = 0;
            // foreach (QuadEdge<int> edge in baseEdge.RightEdges())
            // {
            //     var test3 = radius * edge.Destination;
            //     if (triangulationOnSphere)
            //     {
            //         test3 = radius * Geometry.InvStereographicProjection(edge.Destination);
            //     }
            //     var newGo3 = GameObject.Instantiate(shape);
            //     newGo3.transform.SetParent(transform);
            //     newGo3.transform.position = test3.AsVector3();
            //     newGo3.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
            //     // Color
            //     var meshR3 = newGo3.GetComponent<MeshRenderer>();
            //     if (meshR3 != null)
            //     {
            //         meshR3.materials[0].color = Color.yellow;
            //     }

            //     if (i >= 0)
            //     {
            //         break;
            //     }
            //     i++;
            // }

            // // END DEBUGTOOLS
            // // END DEBUGTOOLS
            // // END DEBUGTOOLS



            // DRAWING  ---  ---  DRAWING  ---  ---  DRAWING
            List<Vec3> triangles;
            if (triangulationOnSphere)
            {
                triangles = sphereMeshUsed.Triangles().ToList().Select(x => Geometry.InvStereographicProjection(x) * radius).ToList();
            }
            else
            {
                triangles = sphereMeshUsed.Triangles().ToList().Select(x => x * radius).ToList();
            }

            // TriangleDrawer.DrawFace(triangles, transform, mat, gradient);
            TriangleDrawer.DrawLine(triangles, transform, mat, Color.black, lineScale);
            TriangleDrawer.DrawPoints(triangles, transform, shape, Color.red, scale);

            List<Face<int>> faces = sphereMeshUsed.Faces(FaceConfig.Voronoi, radius).ToList();


            // // START DEBUGTOOLS
            // // START DEBUGTOOLS
            // // START DEBUGTOOLS


            int i = 0;
            foreach (QuadEdge<int> edge in sphereMeshUsed.RightMostEdge.Oprev.Sym.FaceLeftEdges())
            {
                var test3 = edge.Destination;
                // test3 = radius * test3;
                // if (triangulationOnSphere)
                // {
                //     test3 = radius * Geometry.InvStereographicProjection(edge.Destination);
                // }
                var newGo3 = GameObject.Instantiate(shape);
                newGo3.transform.SetParent(transform);
                newGo3.transform.position = test3.AsVector3();
                newGo3.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
                // Color
                var meshR3 = newGo3.GetComponent<MeshRenderer>();
                if (meshR3 != null)
                {
                    meshR3.materials[0].color = Color.yellow;
                }

                // if (i >= 1)
                // {
                //     break;
                // }
                // if (!Geometry.Ccw(edge.Destination, edge.Lprev.Destination, edge.Lprev.Origin))
                // {
                //     Debug.Log("FOUND");
                //     if (meshR3 != null)
                //     {
                //         meshR3.materials[0].color = Color.magenta;
                //     }
                // }
                i++;
            }

            // // END DEBUGTOOLS
            // // END DEBUGTOOLS
            // // END DEBUGTOOLS



            float nbCells = (float)faces.Count;
            int indcolor2 = 0;
            foreach (Face<int> face in faces)
            {
                var color = gradient.Evaluate(indcolor2 / nbCells);

                face.DrawFace(transform, mat, color, OnSphere:true);
                face.DrawLine(transform, mat, Color.white, lineScale, loop:true);
                face.DrawPoints(transform, shape, mat, Color.blue, 0.6f * scale);

                indcolor2++;

                // if (indcolor2 > 11)
                // {
                //     break;
                // }
            }
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
