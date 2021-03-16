using System.Linq;
using System.Collections.Generic;
using UnityEngine;


using Delaunoi.DataStructures;
using Delaunoi.DataStructures.Extensions;


namespace Delaunoi.Tools.Extensions
{
    public static class TriangleDrawer
    {
        public static void DrawFaceOld(List<Vec3> points, Transform parent,
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
                filter.mesh.triangles = new []{0, 1, 2};//, 1, 0, 2};

                currentId++;
            }
        }

        public static void DrawFace(List<Vec3> points, Transform parent,
                                    Material mat, Gradient gradient)
        {
            // Unity max vertex per mesh is 65535 but we need to be able to divide it by 3
            const int maxVertices = 60000;

            float nbTriangles = points.Count / 3.0f;
            int currentId = 0;
            int meshID = 0;

            for (int iChunk = 0; iChunk < points.Count;)
            {
                // Get chunk index
                int iMin = iChunk;
                int iMax = iMin + Mathf.Min(maxVertices, points.Count - iChunk);

                // Start and End is the same
                int diff = iMax - iMin;
                if (diff < 1)
                {
                    break;
                }

                GameObject newGo = new GameObject();
                newGo.name = string.Format("Triangle Faces mesh {0}", meshID);
                newGo.transform.SetParent(parent);

                newGo.AddComponent<MeshFilter>();
                newGo.AddComponent<MeshRenderer>();
                var filter = newGo.GetComponent<MeshFilter>();
                var renderer = newGo.GetComponent<MeshRenderer>();
                renderer.material = mat;

                var vertices = new List<Vector3>();
                var colors = new List<Color>();
                var indices = new int[diff];

                // Draw minimal number of triangles
                Color color;
                for (int i = 0; i < diff; i = i + 3)
                {

                    // Vertices
                    color = gradient.Evaluate(currentId / nbTriangles);
                    vertices.Add(points[iMin + i].AsVector3());
                    vertices.Add(points[iMin + i + 1].AsVector3());
                    vertices.Add(points[iMin + i + 2].AsVector3());

                    // Colors
                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);

                    // indices
                    indices[i] = i;
                    indices[i + 1] = i + 1;
                    indices[i + 2] = i + 2;
                    currentId++;
                }

                filter.mesh.SetVertices(vertices);
                filter.mesh.SetColors(colors);
                filter.mesh.triangles = indices;

                iChunk = iMax;
            }


        }

        public static void DrawPoints(List<Vec3> points, Transform parent,
                                      GameObject shape, Color color, float scale=2.0f)
        {
            // Iteration without duplicated
            Material defaultMat = shape.GetComponent<MeshRenderer>().sharedMaterial;
            defaultMat.color = color;
            int ptId = 0;
            foreach (Vec3 vec in points.Distinct())
            {
                var newGo = GameObject.Instantiate(shape);
                newGo.name = string.Format("Triangle Point {0}", ptId.ToString());
                newGo.transform.SetParent(parent);
                newGo.transform.position = vec.AsVector3();
                newGo.transform.localScale = new Vector3(scale, scale, scale);

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

