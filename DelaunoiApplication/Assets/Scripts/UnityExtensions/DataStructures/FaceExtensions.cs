using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;


namespace Delaunoi.DataStructures.Extensions
{
    public static class FaceExtensions
    {

        public static void DrawFace<T>(this Face<T> cell, Transform parent, Material mat,
                                       Color color)
        {
            GameObject newGo = new GameObject();
            newGo.name = "Face Face " + cell.ID.ToString();
            newGo.transform.SetParent(parent);
            newGo.AddComponent<MeshFilter>();
            newGo.AddComponent<MeshRenderer>();
            var filter = newGo.GetComponent<MeshFilter>();
            var renderer = newGo.GetComponent<MeshRenderer>();

            // Set Color
            renderer.material = mat;
            renderer.materials[0].color = color;

            var trianglesInt = new List<int>();
            var points = cell.Points.Select(vec => vec.AsVector3()).ToList();

            for (int idP = 1; idP < points.Count - 1; idP++)
            {
                trianglesInt.Add(0);
                trianglesInt.Add(idP);
                trianglesInt.Add(idP + 1);
            }
            trianglesInt.Add(0);
            trianglesInt.Add(points.Count - 1);
            trianglesInt.Add(1);

            filter.mesh.SetVertices(points);
            filter.mesh.triangles = trianglesInt.ToArray();
        }

        public static void DrawPoints<T>(this Face<T> cell, Transform parent,
                                         GameObject shape, Material mat,
                                         Color color, float scale=0.5f)
        {
            Vector3[] bounds = cell.Bounds.Select(vec => vec.AsVector3()).ToArray();

            for (int i = 0; i < bounds.Length; i++)
            {
                var point = GameObject.Instantiate(shape);
                point.name = string.Format("Face Point {0}", cell.ID.ToString());
                point.transform.SetParent(parent);
                point.transform.position = bounds[i];
                point.transform.localScale = new Vector3(scale, scale, scale);

                // Color
                var meshR = point.GetComponent<MeshRenderer>();
                if (meshR != null)
                {
                    meshR.materials[0].color = color;
                }
            }
        }

        public static void DrawLine<T>(this Face<T> cell, Transform parent, Material mat,
                                       Color color, float scale=1.0f, bool loop=true)
        {
            GameObject newGo = new GameObject();
            newGo.name = string.Format("Face Line {0}", cell.ID.ToString());
            newGo.transform.SetParent(parent);

            // Construct line
            Vector3[] bounds = cell.Bounds.Select(vec => vec.AsVector3()).ToArray();

            var lr = newGo.AddComponent<LineRenderer>();
            lr.material = mat;
            lr.positionCount = bounds.Length;
            lr.loop = loop;
            lr.material.color = color;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.startWidth = 0.2f * scale;
            lr.endWidth = 0.2f * scale;
            lr.SetPositions(bounds);
        }

        // public static void DrawCircumCercle<T>(this Face<T> cell, Transform parent,
        //                                        Material mat, Color color, float scale=1.0f)
        // {
        //     GameObject newGo = new GameObject();
        //     newGo.name = string.Format("Face Circle {0}", cell.ID.ToString());
        //     newGo.transform.SetParent(parent);

        //     // One Circle for each point
        //     for (int i = 0; i < cell.Bounds.Length; i++)
        //     {
        //         GameObject child = new GameObject();
        //         child.name = string.Format("{0}", i.ToString());
        //         child.transform.SetParent(newGo.transform);

        //         // Construct line
        //         var lr = child.AddComponent<LineRenderer>();
        //         lr.material = mat;
        //         lr.positionCount = 50;
        //         lr.loop = true;
        //         lr.material.color = color;
        //         lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //         lr.receiveShadows = false;
        //         lr.startWidth = 0.2f * scale;
        //         lr.endWidth = 0.2f * scale;

        //         int segments = lr.positionCount;
        //         var center = cell.Bounds[i].AsVector3();
        //         float radius = (float)cell.GetRadius(i);

        //         float x;
        //         float y;
        //         float z = 0f;

        //         float angle = 20f;

        //         for (int partInd = 0; partInd < segments; partInd++)
        //         {
        //             x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
        //             y = center.y + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

        //             lr.SetPosition (partInd, new Vector3(x, y, z) );

        //             angle += (360f / segments);
        //         }
        //     }
        // }
    }
}

