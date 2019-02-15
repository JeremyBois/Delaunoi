using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "InstantiationManagerParameters")]
public class InstantiationManagerParameters : ScriptableObject
{
    [System.Serializable]
    public class InstanceParameters
    {
        public Mesh instanceMesh;
        public Material instanceMaterial;
        public List<Matrix4x4> transformBuffer;
    }

    public List<InstanceParameters> instancesParameters = new List<InstanceParameters>();
}