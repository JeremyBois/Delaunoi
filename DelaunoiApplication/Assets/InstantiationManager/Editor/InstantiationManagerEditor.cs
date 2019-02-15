using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public class InstantiationManagerEditor : EditorWindow
{
    [SerializeField] InstantiationManagerParameters instantiationManagerParameters;

    class MeshInfo
    {
        public Mesh mesh;
        public Material material;
        public int number;
    }

    List<MeshInfo> meshes;
    MeshFilter [] meshFilters;

    [MenuItem("Tools/InstantiationManagerEditor")]
    static void OpenWindow()
    {
        InstantiationManagerEditor window =  GetWindow<InstantiationManagerEditor>();

        window.titleContent = new GUIContent("Instantiation");
        window.Show();

    }

    private void OnEnable()
    {
        meshFilters = FindObjectsOfType<MeshFilter>();

        //meshes = meshFilters.Select(x => x.sharedMesh).Distinct().Select(x => new MeshInfo() { mesh = x }).ToList();
        meshes = meshFilters.Select(x => new KeyValuePair<Mesh, Material>(x.sharedMesh, x.GetComponent<MeshRenderer>().sharedMaterial)).Distinct().Select(x => new MeshInfo() { mesh = x.Key, material = x.Value }).ToList();

        meshes.ForEach(x =>
        {
            x.number = meshFilters.Where(y => y.sharedMesh == x.mesh && y.GetComponent<MeshRenderer>().sharedMaterial == x.material).Count();
            //x.material = meshFilters.First(y => y.sharedMesh == x.mesh).GetComponent<MeshRenderer>().sharedMaterial;
        });

        meshes = meshes.OrderByDescending(x => x.number).ToList();


    }

    Vector2 scrollPos = Vector2.zero;
    void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        meshes.ForEach(x =>
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Set as Instance", GUILayout.Width(250)))
            {
                SetAsInstance(x);
            }

            if (GUILayout.Button("Select", GUILayout.Width(120)))
            {
                Selection.objects = FindObjectsOfType<MeshFilter>().Where(y => y.sharedMesh == x.mesh && y.GetComponent<MeshRenderer>().sharedMaterial == x.material).Select(y => y.gameObject).ToArray();
            }

            GUILayout.Label(x.mesh.name + " - " + x.material.name + " - " + x.number);
            GUILayout.EndHorizontal();
        });
        GUILayout.EndScrollView();
    }

    void SetAsInstance(MeshInfo _meshInfo)
    {
        InstantiationManagerParameters.InstanceParameters instanceParameters = new InstantiationManagerParameters.InstanceParameters()
        {
            instanceMesh = _meshInfo.mesh,
            instanceMaterial = _meshInfo.material
        };

        instanceParameters.transformBuffer = meshFilters.Where(x => x.sharedMesh == _meshInfo.mesh).Select(x => x.transform.localToWorldMatrix).ToList();

        instantiationManagerParameters.instancesParameters.Add(instanceParameters);
    }
}
