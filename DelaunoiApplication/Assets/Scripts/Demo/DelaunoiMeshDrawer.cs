using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using Delaunoi.DataStructures;



public class DelaunoiMeshDrawer : MonoBehaviour
{
    private Mesh instanceMesh;
    private Material instanceMaterial;

    // Keep track of how many are created
    private int instancedCount;

    // Reuse buffers
    private ComputeBuffer colorBuffer;
    private ComputeBuffer transformBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    bool isSetup = false;


    public void SetupMesh(Mesh meshUsed, Material matUsed)
    {
        instanceMesh = meshUsed;
        instanceMaterial = matUsed;
    }

    public void DrawInstances(List<Vec3> positions, List<Vector4> colors)
    {
        if (!instanceMesh || !instanceMaterial)
        {
            return;
        }

        // How many to draw ??
        instancedCount = positions.Count;
        if (instancedCount <= 0 || (instancedCount != colors.Count))
        {
            return;
        }

        // Update buffers
        Clean();

        // Prepare data
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        transformBuffer = new ComputeBuffer(instancedCount, 16);
        colorBuffer = new ComputeBuffer(instancedCount, 16);

        Vector4[] transformDataToGPU = new Vector4[instancedCount];
        Vector4[] colorDataToGPU = new Vector4[instancedCount];

        // Assign correct color to each range of positions
        for (int i = 0; i < instancedCount; i++)
        {
            transformDataToGPU[i] = new Vector4((float)positions[i].X, (float)positions[i].Y, (float)positions[i].Z, 1.0f);
            colorDataToGPU[i] = colors[i];
        }

        transformBuffer.SetData(transformDataToGPU);
        colorBuffer.SetData(colorDataToGPU);

        // Prepare material with positions and color
        instanceMaterial.SetBuffer("transformBuffer", transformBuffer);
        instanceMaterial.SetBuffer("colorBuffer", colorBuffer);

        // Prepare mesh
        args[0] = (uint)instanceMesh.GetIndexCount(0);
        args[1] = (uint)instancedCount;
        args[2] = (uint)instanceMesh.GetIndexStart(0);
        args[3] = (uint)instanceMesh.GetBaseVertex(0);

        argsBuffer.SetData(args);

        isSetup = true;
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
        if (isSetup)
        {
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(5000.0f, 5000.0f, 5000.0f)), argsBuffer);
        }
    }
}
