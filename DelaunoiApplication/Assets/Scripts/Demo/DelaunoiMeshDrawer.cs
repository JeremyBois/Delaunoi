using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Delaunoi.DataStructures;


public class DelaunoiMeshDrawer : MonoBehaviour
{
    public Mesh instanceMesh;
    public Material instanceMaterial;

    // Keep track of how many are created
    public int instancedCount;

    bool notSetup = true;

    // Used to
    private ComputeBuffer transformBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    public void DrawInstances(List<Vec3> positions, float scale, GameObject shape)
    {
        if (transformBuffer != null)
            transformBuffer.Release();

        // How many to draw ??
        instancedCount = positions.Count;

        // Prepare data
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        transformBuffer = new ComputeBuffer(instancedCount, 16);

        Debug.Log(instancedCount);

        Vector4[] dataToGPU = new Vector4[instancedCount];
        for (int i = 0; i < instancedCount; i++)
        {
            dataToGPU[i] = new Vector4((float)positions[i].X, (float)positions[i].Y, (float)positions[i].Z, scale);
        }
        transformBuffer.SetData(dataToGPU);

        // Prepare material
        instanceMaterial = shape.GetComponent<MeshRenderer>().sharedMaterial;
        instanceMaterial.SetBuffer("transformBuffer", transformBuffer);

        // Prepare mesh
        instanceMesh = shape.GetComponent<MeshFilter>().sharedMesh;
        args[0] = (uint)instanceMesh.GetIndexCount(0);
        args[1] = (uint)instancedCount;
        args[2] = (uint)instanceMesh.GetIndexStart(0);
        args[3] = (uint)instanceMesh.GetBaseVertex(0);

        argsBuffer.SetData(args);

        notSetup = false;
    }

    void OnDisable()
    {
        if (transformBuffer != null)
            transformBuffer.Release();
        transformBuffer = null;

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
