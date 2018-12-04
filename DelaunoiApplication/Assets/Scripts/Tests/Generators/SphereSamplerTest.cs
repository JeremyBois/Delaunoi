using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;


using Delaunoi.Generators;
using Delaunoi.Tools;
using Delaunoi.DataStructures;
using Delaunoi.DataStructures.Extensions;

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


    void Start()
    {
        // BUILDING  ---  ---  BUILDING  ---  ---  BUILDING
        System.DateTime previousTime = System.DateTime.Now;

        List<Vec3> points = SphereSampler.FibonnaciSphere(pointNumber, radius).ToList();

        System.TimeSpan delta = System.DateTime.Now - previousTime;
        Debug.Log(string.Format("BUILDING *** {0} secondes OU {1} milliseconds *** BUILDING",
                  delta.TotalSeconds, delta.TotalMilliseconds));
        Debug.Log("Total generated points: " + points.Count);


        // DRAWING  ---  ---  DRAWING  ---  ---  DRAWING
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
