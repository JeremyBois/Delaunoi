using System.Linq;
using System.Collections.Generic;
using UnityEngine;


using Delaunoi.DataStructures;
using Delaunoi.DataStructures.Extensions;


namespace Delaunoi.Tools.Extensions
{
    public static class TriangleDrawer
    {
        public static void DrawFace(List<Vec3> points, Transform parent,
                                    Material mat, Gradient gradient)
        {
            float nbTriangles = points.Count / 3.0f;
            int currentId = 0;

            for (int i = 0; i < points.Count; i = i + 3)
            {
                GameObject newGo = new GameObject();
                newGo.name = string.Format("Triangle Face {0}", currentId.ToString());
                newGo.transform.SetParent(parent);

                newGo.AddComponent<MeshFilter>();
                newGo.AddComponent<MeshRenderer>();
                var filter = newGo.GetComponent<MeshFilter>();
                var renderer = newGo.GetComponent<MeshRenderer>();
                renderer.material = mat;
                renderer.materials[0].color = gradient.Evaluate(currentId / nbTriangles);
                filter.mesh.SetVertices(new List<Vector3> {points[i].AsVector3(),
                                                           points[i + 1].AsVector3(),
                                                           points[i + 2].AsVector3()}
                                        );
                filter.mesh.triangles = new []{0, 1, 2};

                currentId++;
            }
        }

        public static void DrawPoints(List<Vec3> points, Transform parent,
                                      GameObject shape, Color color, float scale=2.0f)
        {
            // Iteration without duplicated
            int ptId = 0;
            foreach (Vec3 vec in points.Distinct())
            {
                var newGo = GameObject.Instantiate(shape);
                newGo.name = string.Format("Triangle Point {0}", ptId.ToString());
                newGo.transform.SetParent(parent);
                newGo.transform.position = vec.AsVector3();
                newGo.transform.localScale = new Vector3(scale, scale, scale);
                // Color
                var meshR = newGo.GetComponent<MeshRenderer>();
                if (meshR != null)
                {
                    meshR.materials[0].color = color;
                }

                ptId++;
            }
        }

        public static void DrawLine(List<Vec3> points, Transform parent,
                                    Material mat, Gradient gradient, float scale)
        {
            float nbTriangles = points.Count / 3.0f;
            int currentId = 0;

            for (int i = 0; i < points.Count; i = i + 3)
            {
                GameObject newGo = new GameObject();
                newGo.name = string.Format("Triangle Line {0}", currentId.ToString());
                newGo.transform.SetParent(parent);

                var lr = newGo.AddComponent<LineRenderer>();
                lr.material = mat;
                lr.positionCount = 3;
                lr.loop = true;
                lr.material.color = gradient.Evaluate(currentId / nbTriangles);
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.startWidth = 0.2f * scale;
                lr.endWidth = 0.2f * scale;

                // Set points
                lr.SetPosition(0, points[i].AsVector3() - Vector3.forward * 0.001f);
                lr.SetPosition(1, points[i + 1].AsVector3() - Vector3.forward * 0.001f);
                lr.SetPosition(2, points[i + 2].AsVector3() - Vector3.forward * 0.001f);

                currentId++;
            }
        }


        public static void DrawLine(List<Vec3> points, Transform parent,
                                    Material mat, Color color, float scale)
        {
            int currentId = 0;

            for (int i = 0; i < points.Count; i = i + 3)
            {
                GameObject newGo = new GameObject();
                newGo.name = string.Format("Triangle Line {0}", currentId.ToString());
                newGo.transform.SetParent(parent);

                var lr = newGo.AddComponent<LineRenderer>();
                lr.material = mat;
                lr.positionCount = 3;
                lr.loop = true;
                lr.material.color = color;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.startWidth = 0.2f * scale;
                lr.endWidth = 0.2f * scale;

                // Set points
                lr.SetPosition(0, points[i].AsVector3() - Vector3.forward * 0.001f);
                lr.SetPosition(1, points[i + 1].AsVector3() - Vector3.forward * 0.001f);
                lr.SetPosition(2, points[i + 2].AsVector3() - Vector3.forward * 0.001f);

                currentId++;
            }
        }
    }
}

