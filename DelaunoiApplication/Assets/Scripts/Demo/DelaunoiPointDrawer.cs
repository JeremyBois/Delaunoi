using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.Linq;

using Delaunoi.DataStructures;


public class DelaunoiPointDrawer : MonoBehaviour
{
    private Mesh instanceMesh;
    private Material instanceMaterial;

    // Keep track of how many are created
    private int instancedCount;

    bool notSetup = true;

    // Reuse buffers
    private ComputeBuffer colorBuffer;
    private ComputeBuffer transformBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    // Fill data before use
    List<Vec3> positions;
    List<Vector2> colorRanges;
    List<Vector4> colors;


    private void Start()
    {
        // Init enumerable
        ClearDataStored();
    }


    public void AppendData(IEnumerable<Vec3> newPositions, Vector4 color)
    {
        var colorRange = new Vector2(positions.Count, 0);
        positions.AddRange(newPositions.Distinct());
        colorRange.y = positions.Count;
        colorRanges.Add(colorRange);
        colors.Add(color);
    }

    public void ClearDataStored()
    {
        positions = new List<Vec3>();
        colors = new List<Vector4>();
        colorRanges = new List<Vector2>();
    }

    private void OnDestroy()
    {
        Clean();
    }

    public void DrawInstancesOf(Mesh mesh, Material mat, List<Vec3> positions, List<Vector4> colors)
    {

    }


    public void DrawInstances(float scale, GameObject shape)
    {
        // How many to draw ??
        instancedCount = positions.Count;
        if (instancedCount <= 0)
        {
            return;
        }

        Clean();

        // Prepare data
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        transformBuffer = new ComputeBuffer(instancedCount, 16);
        colorBuffer = new ComputeBuffer(instancedCount, 16);

        Vector4[] transformDataToGPU = new Vector4[instancedCount];
        Vector4[] colorDataToGPU = new Vector4[instancedCount];

        // Assign correct color to each range of positions
        int ind = 0;
        for (int colorInd = 0; colorInd < colors.Count; colorInd++)
        {
            int start = (int)colorRanges[colorInd].x;
            int end = (int)colorRanges[colorInd].y;
            for (int i = start; i < end; i++)
            {
                transformDataToGPU[ind] = new Vector4((float)positions[ind].X, (float)positions[ind].Y, (float)positions[ind].Z, scale);
                colorDataToGPU[ind] = colors[colorInd];

                ++ind;
            }
        }

        transformBuffer.SetData(transformDataToGPU);
        colorBuffer.SetData(colorDataToGPU);

        // Prepare material with positions and color
        instanceMaterial = shape.GetComponent<MeshRenderer>().sharedMaterial;
        instanceMaterial.SetBuffer("transformBuffer", transformBuffer);
        instanceMaterial.SetBuffer("colorBuffer", colorBuffer);

        // Prepare mesh
        instanceMesh = shape.GetComponent<MeshFilter>().sharedMesh;
        args[0] = (uint)instanceMesh.GetIndexCount(0);
        args[1] = (uint)instancedCount;
        args[2] = (uint)instanceMesh.GetIndexStart(0);
        args[3] = (uint)instanceMesh.GetBaseVertex(0);

        argsBuffer.SetData(args);

        notSetup = false;

    }

    void Clean()
    {
        if (transformBuffer != null)
            transformBuffer.Release();
        transformBuffer = null;

        if (colorBuffer != null)
            colorBuffer.Release();
        colorBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }

    void Update()
    {
        if (!notSetup)
        {
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(5000.0f, 5000.0f, 5000.0f)), argsBuffer);
        }
    }
}
