using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Delaunoi.Generators;
using Delaunoi.DataStructures.Extensions;

public class HaltonTest : MonoBehaviour
{
    [Tooltip("Number of points to compute.")]
    [SerializeField]
    private int pointNumber = 10;

    [Tooltip("Boundaries used for drawing.")]
    [SerializeField]
    private int[] boundaries = {200, 100};


    [Tooltip("Seed used for point generation.")]
    [SerializeField]
    int sequenceInit = 0;

    [Tooltip("Base used to compute Halton sequence.")]
    [SerializeField]
    private int[] bases = {2, 3};

    [Tooltip("GameObject used to represent a point.")]
    [SerializeField]
    private GameObject shape;
    [Tooltip("Scale coefficient for the shape.")]
    [SerializeField]
    private int ShapeScale = 1;

    [Tooltip("Colors used for points.")]
    [SerializeField]
    private Color[] colors;
    private int colorsLength;

    private Vector3[] points;
    private GameObject[] pointsObject;

	// Use this for initialization
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
            points[i].x *= boundaries[0];
            points[i].x += transform.position.x;
            points[i].y *= boundaries[1];
            points[i].y += transform.position.y;
            points[i].z += transform.position.z;

            // Create a shape using points coordinates
            var newShape = Instantiate(shape);
            newShape.transform.SetParent(this.transform);
            newShape.transform.position = points[i];
            newShape.transform.localScale = new Vector3(ShapeScale, ShapeScale, ShapeScale);

            // Random color
            newShape.GetComponent<MeshRenderer>().materials[0].color = colors[Random.Range(0, colorsLength)];

            // Keep track of shape
            pointsObject[i] = newShape;
            n++;
        }

        // Place camera
        var cam = Camera.main;
        cam.transform.position = new Vector3(boundaries[0] / 2.0f, boundaries[1] / 2.0f, -0.8f * Mathf.Max(boundaries));
	}

    public Vector3[] Points
    {
        get {return points;}
    }
}
