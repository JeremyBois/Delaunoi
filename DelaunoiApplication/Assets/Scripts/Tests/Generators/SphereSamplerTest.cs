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
    private int[] bases = {2, 3};


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


    private GuibasStolfi<int> triangulator;


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
                for (int i = 0; i < pointNumber; i++)
                {
                    Vec3 randTemp = HaltonSequence.Halton2D(n, bases[0], bases[1]);
                    double phi   = 2.0 * System.Math.PI * randTemp.X;
                    double theta = System.Math.Acos(2.0 * randTemp.Y - 1.0);

                    points.Add(Geometry.SphericalToEuclidean(phi, theta));
                    n++;
                }
                break;
            case Generator.Uniform:
                for (int i = 0; i < pointNumber; i++)
                {
                    double phi   = 2.0 * System.Math.PI * RandGen.NextDouble();
                    double theta     = System.Math.Acos(2.0 * RandGen.NextDouble() - 1.0);

                    points.Add(Geometry.SphericalToEuclidean(phi, theta));
                    n++;
                }
                break;
            case Generator.Fibonnaci:
                points = SphereSampler.FibonnaciSphere(pointNumber, 1.0).ToList();
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
            triangulator = new GuibasStolfi<int>(points.Select(x => Geometry.StereographicProjection(x)).ToArray(), false);
            delta = System.DateTime.Now - previousTime;
            Debug.Log("***");
            Debug.Log(string.Format("*** INIT *** {0} secondes OU {1} milliseconds *** INIT",
                      delta.TotalSeconds, delta.TotalMilliseconds));

            // TRIANGULATION  ---  ---  TRIANGULATION  ---  ---  TRIANGULATION
            previousTime = System.DateTime.Now;
            triangulator.ComputeDelaunay(isCycling:false);
            delta = System.DateTime.Now - previousTime;
            Debug.Log("***");
            Debug.Log(string.Format("*** TRIANGULATION *** {0} secondes OU {1} milliseconds *** TRIANGULATION",
                      delta.TotalSeconds, delta.TotalMilliseconds));

            // // Find lcand, rcand, baseEdge
            // var lcand = triangulator.LeftMostEdge;
            // var rcand = triangulator.RightMostEdge;
            // while (lcand.Origin != rcand.Origin)
            // {
            //     lcand = lcand.Lprev;
            // }
            // lcand = lcand.Lnext;
            // var baseEdge = rcand.Oprev;
            // QuadEdge<int> nextCand;

            // var nextNeeded = lcand.Lnext;

            QuadEdge<int> nextCand;
            // QuadEdge<int> baseEdge = triangulator.RightMostEdge.Rnext.Rnext.Sym;

            bool crossEdgeNotFound = true;
            var ldi = triangulator.LeftMostEdge;
            var rdi = triangulator.RightMostEdge;
            while (crossEdgeNotFound)
            {
                if (Geometry.LeftOf(rdi.Origin, ldi))
                {
                    ldi = ldi.Lnext;
                }
                else if (Geometry.RightOf(ldi.Origin, rdi))
                {
                    rdi = rdi.Rprev;
                }
                else
                {
                    crossEdgeNotFound = false;
                }
            }

            // var baseEdge = rdi.Sym.Rnext;
            // var baseEdge = QuadEdge<int>.Connect(rdi.Sym, ldi);


            // Debug.Log(triangulator.IsValid(baseEdge.Sym.Oprev, baseEdge));
            // Debug.Log(triangulator.IsValid(baseEdge.Onext, baseEdge));


            // LCand
            Vec3 test = radius * Geometry.InvStereographicProjection(ldi.Destination);
            var newGo = GameObject.Instantiate(shape);
            newGo.transform.SetParent(transform);
            newGo.transform.position = test.AsVector3();
            newGo.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            // Color
            var meshR = newGo.GetComponent<MeshRenderer>();
            if (meshR != null)
            {
                meshR.materials[0].color = Color.blue;
            }

            // RCand
            test = radius * Geometry.InvStereographicProjection(rdi.Destination);
            newGo = GameObject.Instantiate(shape);
            newGo.transform.SetParent(transform);
            newGo.transform.position = test.AsVector3();
            newGo.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            // Color
            meshR = newGo.GetComponent<MeshRenderer>();
            if (meshR != null)
            {
                meshR.materials[0].color = Color.red;
            }

            // foreach (QuadEdge<int> edge in baseEdge.RightEdges())
            // {
            //     test = radius * Geometry.InvStereographicProjection(edge.Destination);
            //     newGo = GameObject.Instantiate(shape);
            //     newGo.transform.SetParent(transform);
            //     newGo.transform.position = test.AsVector3();
            //     newGo.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            //     // Color
            //     meshR = newGo.GetComponent<MeshRenderer>();
            //     if (meshR != null)
            //     {
            //         meshR.materials[0].color = Color.yellow;
            //     }
            //     break;
            // }

            // // 2) Rising bubble (See Fig. 22)
            // bool upperCommonTangentNotFound = true;
            // while (upperCommonTangentNotFound)
            // {
            //     // Locate the first L site (lCand.Destination) to be encountered
            //     // by the rising bubble, and delete L edges out of baseEdge.Destination
            //     // that fail the circle test.
            //     QuadEdge<int> lCand = baseEdge.Sym.Onext;
            //     if (triangulator.IsValid(lCand, baseEdge))
            //     {
            //         while (Geometry.InCircumCercle2D(lCand.Onext.Destination,
            //                               baseEdge.Destination, baseEdge.Origin, lCand.Destination))
            //         {
            //             nextCand = lCand.Onext;
            //             QuadEdge<int>.Delete(lCand);
            //             lCand = nextCand;
            //         }
            //     }
            //     // Same for the right part (Symetrically)
            //     QuadEdge<int> rCand = baseEdge.Oprev;
            //     if (triangulator.IsValid(rCand, baseEdge))
            //     {
            //         while (Geometry.InCircumCercle2D(rCand.Oprev.Destination,
            //                               baseEdge.Destination, baseEdge.Origin, rCand.Destination))
            //         {
            //             nextCand = rCand.Oprev;
            //             QuadEdge<int>.Delete(rCand);
            //             rCand = nextCand;
            //         }
            //     }
            //     // Upper common tangent is baseEdge
            //     if (!triangulator.IsValid(lCand, baseEdge) && !triangulator.IsValid(rCand, baseEdge))
            //     {
            //         upperCommonTangentNotFound = false;
            //     }
            //     // Construct new cross edge between left and right
            //     // The next cross edge is to be connected to either lcand.Dest or rCand.Dest
            //     // If both are valid, then choose the appropriate one using the
            //     // Geometry.InCircumCercle2D test
            //     else if (!triangulator.IsValid(lCand, baseEdge) ||
            //                 (
            //                     triangulator.IsValid(rCand, baseEdge) &&
            //                     Geometry.InCircumCercle2D(rCand.Destination,
            //                                               lCand.Destination,
            //                                               lCand.Origin,
            //                                               rCand.Origin)
            //                 )
            //             )
            //     {
            //         // Cross edge baseEdge added from rCand.Destination to basel.Destination
            //         baseEdge = QuadEdge<int>.Connect(rCand, baseEdge.Sym);
            //     }
            //     else
            //     {
            //         // Cross edge baseEdge added from baseEdge.Origin to lCand.Destination
            //         baseEdge = QuadEdge<int>.Connect(baseEdge.Sym, lCand.Sym);
            //     }
            // }

            // var otherE = QuadEdge<int>.Connect(lcand, baseEdge);
            // QuadEdge<int>.Connect(lcand.Dprev.Dprev.Sym, otherE.Sym);
            // QuadEdge<int>.Connect(triangulator.LeftMostEdge.Rprev.Rprev.Sym, triangulator.RightMostEdge);


            // // Helper
            // foreach (QuadEdge<int> edge in triangulator.LeftMostEdge.EdgesFrom())
            // {
            //     Vec3 test = radius * Geometry.InvStereographicProjection(edge.Destination);
            //     var newGo = GameObject.Instantiate(shape);
            //     newGo.transform.SetParent(transform);
            //     newGo.transform.position = test.AsVector3();
            //     newGo.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            //     // Color
            //     var meshR = newGo.GetComponent<MeshRenderer>();
            //     if (meshR != null)
            //     {
            //         meshR.materials[0].color = Color.blue;
            //     }
            //     break;
            // }

            // foreach (QuadEdge<int> edge in triangulator.RightMostEdge.EdgesFrom())
            // {
            //     Vec3 test = radius * Geometry.InvStereographicProjection(edge.Destination);
            //     var newGo = GameObject.Instantiate(shape);
            //     newGo.transform.SetParent(transform);
            //     newGo.transform.position = test.AsVector3();
            //     newGo.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            //     // Color
            //     var meshR = newGo.GetComponent<MeshRenderer>();
            //     if (meshR != null)
            //     {
            //         meshR.materials[0].color = Color.green;
            //     }
            //     break;
            // }


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

            TriangleDrawer.DrawFace(triangles, transform, mat, gradient);
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
