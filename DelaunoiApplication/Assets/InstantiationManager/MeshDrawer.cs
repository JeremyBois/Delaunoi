using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshDrawer : MonoBehaviour
{
    [SerializeField] InstantiationManagerParameters instantiationManagerParameters;
    
    class DrawItem
    {
        public Mesh instanceMesh;
        public Material instanceMaterial;

        public int count;

        public ComputeBuffer transformBuffer;
        public ComputeBuffer argsBuffer;
        public uint [] args = new uint [5] { 0, 0, 0, 0, 0 };

        public DrawItem(InstantiationManagerParameters.InstanceParameters _item)
        {

            instanceMesh = _item.instanceMesh;
            instanceMaterial = _item.instanceMaterial;
            count = _item.transformBuffer.Count;

            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            transformBuffer = new ComputeBuffer(count, 64);
            transformBuffer.SetData(_item.transformBuffer.ToArray());

            instanceMaterial.SetBuffer("transformBuffer", transformBuffer);
            args [0] = (uint) instanceMesh.GetIndexCount(0);
            args [1] = (uint) count;
            args [2] = (uint) instanceMesh.GetIndexStart(0);
            args [3] = (uint) instanceMesh.GetBaseVertex(0);

            argsBuffer.SetData(args);
        }

        public void Clear()
        {
            if (transformBuffer != null)
                transformBuffer.Release();
            transformBuffer = null;

            if (argsBuffer != null)
                argsBuffer.Release();
            argsBuffer = null;
        }
    }

    List<DrawItem> drawItems = new List<DrawItem>();

    void Start()
    {
        drawItems = instantiationManagerParameters.instancesParameters.Select(x => new DrawItem(x)).ToList();
    }

    void Update()
    {
        drawItems.ForEach(x =>
        {
            Graphics.DrawMeshInstancedIndirect(x.instanceMesh, 0, x.instanceMaterial, new Bounds(Vector3.zero, new Vector3(5000.0f, 5000.0f, 5000.0f)), x.argsBuffer);
        });
    }

    private void OnDisable()
    {
        drawItems.ForEach(x => x.Clear() );
    }
}