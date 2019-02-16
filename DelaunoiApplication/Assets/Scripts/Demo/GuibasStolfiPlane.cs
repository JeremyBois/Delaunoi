using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.Linq;


using Delaunoi;
using Delaunoi.Generators;
using Delaunoi.DataStructures;
using Delaunoi.Tools;
using Delaunoi.DataStructures.Extensions;
using Delaunoi.Tools.Extensions;


public class GuibasStolfiPlane : MonoBehaviour
{

    private List<Vec3> points;
    private Mesh2D<int, int> meshUsed;
    private PoissonDisk2D poissonGen;

    // Settings
    public int pointNumber = 10;
    public int seed = 154;
    public int[] bases = { 2, 3 };
    public GeneratorType usedGenerator;
    private int[] boundaries = { 300, 200 };
    public float minimalDistance = 15.0f;
    private int maxAttemp = 40;

    // Drawing
    [SerializeField]
    private GameObject shape;
    [SerializeField]
    private Material mat;
    [SerializeField]
    private Gradient gradient;
    public float scale = 1.0f;
    public float lineScale = 1.5f;
	// Delaunay
    public bool D_points = false;
    public bool D_lines = false;
    public bool D_faces = false;
	// Voronoï
    public FaceConfig celltype;
    public bool V_points = false;
    public bool V_lines = false;
    public bool V_faces = false;


    // Reuse memory
    List<Face<int, int>> faces;
    List<Vec3> triangles;
    DelaunoiPointDrawer ptsDrawerComponent;
    IEnumerable ptsEnumerable;

    void Awake()
    {
        // // Place camera
        // var cam = Camera.main;
        // cam.transform.position = new Vector3(boundaries[0] / 2.0f, boundaries[1] / 2.0f, -0.8f * Mathf.Max(boundaries));
    }

	public void UpdateTriangulation(bool reconstructTriangulation, bool clearCells)
	{
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}

        if (clearCells)
        {
            meshUsed.ClearFacesData();
        }

        // Flag to avoid to retriangulate
        if (reconstructTriangulation)
        {
            BuildSample();
            ConstructMesh();

            // Init GPU instancing for points
            if (ptsDrawerComponent)
            {
                Destroy(ptsDrawerComponent.gameObject);
            }
            var ptsDrawer = new GameObject("Points Drawer");
            ptsDrawerComponent = ptsDrawer.AddComponent<DelaunoiPointDrawer>();
        }
        // We don't want pts to be drawn
        else if (!V_points && !D_points && ptsDrawerComponent != null)
        {
            Destroy(ptsDrawerComponent.gameObject);
        }

        DrawMesh();
    }

	void ConstructMesh()
	{
        meshUsed = new Mesh2D<int, int>(points.ToArray(), false);
        meshUsed.Construct();
	}

	void DrawMesh()
	{
        // Clear data stored
        ptsDrawerComponent.ClearDataStored();


        // Draw Delaunay
        triangles = meshUsed.Triangles().ToList();
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
            // CPU instances
            // TriangleDrawer.DrawPoints(triangles, transform, shape, Color.red, 1.1f * scale);

            // GPU instancing below
            ptsDrawerComponent.AppendData(triangles, Color.red);
        }

        // Get faces
        faces = meshUsed.Faces(celltype, Mathf.Max(boundaries) * 5.0, true)
                                             .ToList();

        float nbCells = (float)faces.Count;
        int indcolor2 = 0;
        foreach (Face<int, int> face in faces)
        {
            var color = gradient.Evaluate(indcolor2 / nbCells);

            if (V_faces)
            {
                face.DrawFace(transform, mat, color);
            }
            if (V_lines)
            {
                face.DrawLine(transform, mat, Color.white, lineScale, loop: true);
            }
            // CPU
            // if (V_points)
            // {
            //     face.DrawPoints(transform, shapes[1], mat, Color.blue, 0.8f * scale);
            // }
            indcolor2++;
        }

        // Append also Cells points
        if (V_points)
        {
            // GPU
            ptsDrawerComponent.AppendData(faces.SelectMany(face => face.Bounds).Distinct(), Color.blue);
        }

        // Draw all points
        if (V_points || D_points)
            ptsDrawerComponent.DrawInstances(1.1f * scale, shape);

    }

    void BuildSample()
    {
        // Create random points
        int n = seed;
        points = new List<Vec3>();

        switch (usedGenerator)
        {
            case GeneratorType.Halton:
                for (int i = 0; i < pointNumber; i++)
                {
                    // Random sparse distribution
                    Vec3 temp = HaltonSequence.Halton2D(n, bases[0], bases[1]);
                    points.Add(new Vec3(temp.X * boundaries[0] + transform.position.x,
                                         temp.Y * boundaries[1] + transform.position.y,
                                         transform.position.z));
                    n++;
                }
                break;
            case GeneratorType.Poisson:
                RandGen.Init(seed);
                poissonGen = new PoissonDisk2D(minimalDistance, boundaries[0], boundaries[1],
                                               maxAttemp);
                poissonGen.BuildSample(pointNumber);
                foreach (Vec3 point in poissonGen)
                {
                    points.Add(point);
                }
                break;
        }
    }
}
