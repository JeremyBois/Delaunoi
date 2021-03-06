﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Delaunoi.Generators;
using Delaunoi.Tools;
using Delaunoi.DataStructures;
using Delaunoi.DataStructures.Extensions;

public class PoissonDiskTest : MonoBehaviour
{
    [Header("Generation Settings")]
    [Tooltip("Seed used for point generation.")]
    [SerializeField]
    int seed = 154;

    [Tooltip("Number of points to compute.")]
    [SerializeField]
    private int pointNumber = 10;
    [SerializeField]
    private float minimalDistance = 5.0f;

    [Tooltip("Boundaries used for drawing.")]
    [SerializeField]
    private int[] boundaries = {200, 200, 200};
    [SerializeField]
    private int maxAttemp = 30;


    [Header("Drawing Settings")]
    [Tooltip("GameObject used to represent a point.")]
    [SerializeField]
    private GameObject shape;
    [SerializeField]
    private float scale;

    [SerializeField]
    private Dimension dim;

    private enum Dimension
    {
        Two,
        Three
    }


    private PoissonDisk3D generator3D;
    private PoissonDisk2D generator2D;



	// Use this for initialization
	void Awake ()
    {
        RandGen.Init(seed);

        if (dim == Dimension.Two)
        {
            generator2D = new PoissonDisk2D(minimalDistance, boundaries[0], boundaries[1],
                                          maxAttemp);
        }
        else
        {
            generator3D = new PoissonDisk3D(minimalDistance, boundaries[0], boundaries[1], boundaries[2],
                                            maxAttemp);
        }

        // Place camera
        var cam = Camera.main;
        cam.transform.position = new Vector3(boundaries[0] / 2.0f, boundaries[1] / 2.0f, -0.8f * Mathf.Max(boundaries));
	}

    void Start()
    {
        // BUILDING  ---  ---  BUILDING  ---  ---  BUILDING
        System.DateTime previousTime = System.DateTime.Now;
        if (dim == Dimension.Two)
        {
            generator2D.BuildSample(pointNumber);
            System.TimeSpan delta = System.DateTime.Now - previousTime;
            Debug.Log(string.Format("BUILDING *** {0} secondes OU {1} milliseconds *** BUILDING",
                      delta.TotalSeconds, delta.TotalMilliseconds));
            Debug.Log("Total generated points: " + generator2D.Count);
        }
        else
        {
            generator3D.BuildSample(pointNumber);
            System.TimeSpan delta = System.DateTime.Now - previousTime;
            Debug.Log(string.Format("BUILDING *** {0} secondes OU {1} milliseconds *** BUILDING",
                      delta.TotalSeconds, delta.TotalMilliseconds));
            Debug.Log("Total generated points: " + generator3D.Count);
        }



        // DRAWING  ---  ---  DRAWING  ---  ---  DRAWING
        int ptId = 0;

        if (dim == Dimension.Two)
        {
            foreach (Vec3 point in generator2D)
            {
                var newGo = GameObject.Instantiate(shape);
                newGo.name = string.Format("Poisson Disk Sample {0}", ptId.ToString());
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
        else
        {
            foreach (Vec3 point in generator3D)
            {
                var newGo = GameObject.Instantiate(shape);
                newGo.name = string.Format("Poisson Disk Sample {0}", ptId.ToString());
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

	// Update is called once per frame
	void Update ()
    {

	}

}
