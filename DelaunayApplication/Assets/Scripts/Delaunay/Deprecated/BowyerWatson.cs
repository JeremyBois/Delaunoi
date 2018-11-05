using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;  // Debug only

// @TODO Construct2DBoundaries should correctly create bounds for any plane (x, y, z)
// @TODO Sort points on an axe before triangulation and account for it.


namespace Delaunay.Algorithms
{
    using Delaunay.Tools;
    using Delaunay.DataStructure;

    public class BowyerWatson
    {
        private Vector3[] _points;
        private HashSet<Triangle> _triangles;
        // private HashSet<Triangle> _activeTriangles;
        private HashSet<Triangle> _badTriangles;
        private List<Triangle>    _newTriangles;  // Order matter

        // Fake points used to create convex hull of all points at a starting triangulation.
        private Vector3[] _bounds = new Vector3[4];

        /// <summary>
        /// Prepare a set of points for triangulation. If bounds is not null
        /// they will be used to compute initial triangulation, else points
        /// will be used to find boundaries.
        /// </summary>
        /// <param name="points">A n-element array of Vector3 in any order.</param>
        /// <param name="bounds">A 4-element array of Vector3 in a CCW order.</param>
        public BowyerWatson(Vector3[] points, Vector3[] bounds=null)
        {
            // Remove duplicated values and store it
            this._points = points;

            // https://stackoverflow.com/questions/3106537/using-the-clear-method-vs-new-object#3106551
            // Init triangles set
            this._triangles = new HashSet<Triangle>();
            // Init bad triangles set
            this._badTriangles = new HashSet<Triangle>();
            // Init new triangle to add
            this._newTriangles = new List<Triangle>();

            Construct2DBoundaries(bounds);
        }

        public  Triangle[] DelaunayTriangulation
        {
            get {return _triangles.ToArray();}
        }

        /// <summary>
        /// Get the circumcenter of each triangle of triangulation.
        /// </summary>
        public  Vector3[] VoronoiVertices
        {
            get {return _triangles.Select(tri => tri.Circumcenter).ToArray();}
        }

        /// <summary>
        /// Return bounds used to compute triangulation
        /// </summary>
        public Vector3[] Bounds
        {
            get {return _bounds;}
        }

        /// <summary>
        /// Sort the array of points using x axis.
        /// </summary>
        private void Sort()
        {
            Array.Sort(_points, (a, b) => a.x.CompareTo(b.x));
        }



        /// <summary>
        /// Compute boundaries based on a set of points. Assume z coordinate
        /// to be fixed.
        /// </summary>
        private void Construct2DBoundaries(Vector3[] bounds)
        {
            if (bounds != null)
            {
                _bounds = bounds;
            }
            else
            {
                float maxX=-Mathf.Infinity,
                      maxY=-Mathf.Infinity,
                      minX=Mathf.Infinity,
                      minY=Mathf.Infinity;
                for (int i = 0; i < _points.Count(); i++)
                {
                    Vector3 pt = _points[i];
                    if (maxX < pt.x)
                        maxX = pt.x;

                    if (maxY < pt.y)
                        maxY = pt.y;

                    if (minX > pt.x)
                        minX = pt.x;

                    if (minY > pt.y)
                        minY = pt.y;
                }

                // Create fake points (larger bounds in case a point is also a corner)
                // 3 ----- 0
                // |       |
                // 2 ----- 1
                var delta = (maxX - minX) * 10.0f;
                _bounds[0] = new Vector3(maxX + delta, maxY + delta, _points[0].z);  // Right top corner
                _bounds[1] = new Vector3(maxX + delta, minY - delta, _points[0].z);  // Right bottom corner
                _bounds[2] = new Vector3(minX - delta, minY - delta, _points[0].z);  // Left bottom corner
                _bounds[3] = new Vector3(minX - delta, maxY + delta, _points[0].z);  // Left top corner
            }

            // Now create two CCW triangles as initial triangulation
            // B -- A                  C
            // | /    (tri1)        /  |  (tri2)
            // C                  A -- B
            var tri1 = new Triangle(_bounds[0], _bounds[3], _bounds[2]);
            var tri2 = new Triangle(_bounds[2], _bounds[1], _bounds[0]);

            // Add adjacency relationships
            tri1.SetAdjacent(2, tri2);
            tri2.SetAdjacent(2, tri1);

            // Add them to triangles
            _triangles.Add(tri1);
            _triangles.Add(tri2);
        }

        public bool OldTriangulate()
        {
            // Add each point one at the time
            foreach (Vector3 pt in _points)
            {
                // Remove all items from set
                // https://stackoverflow.com/questions/3106537/using-the-clear-method-vs-new-object#3106551
                _badTriangles.Clear();
                _newTriangles.Clear();

                // Find bad triangles (pt inside a triangle circumcenter)
                foreach (Triangle tri in _triangles)
                {
                    // @TODO Improvment can be done if array is sorted first
                    //       to avoid looping over the whole set.
                    if (tri.InCircumCircle(pt))
                    {
                        _badTriangles.Add(tri);
                    }
                }

                // Retriangulate
                Add(pt);

            }

            Clean();

            return true;
        }

        /// <summary>
        /// Use a recursive call to construct bad triangles from neighbors.
        /// Avoid looping over all triangles.
        /// </summary>
        public bool Triangulate()
        {
            // Add each point one at the time
            foreach (Vector3 pt in _points)
            {
                // Remove all items from set
                _badTriangles.Clear();
                _newTriangles.Clear();

                // Find bad triangles (pt inside a triangle circumcenter)
                foreach (Triangle tri in _triangles)
                {
                    if (tri.InCircumCircle(pt))
                    {
                        _badTriangles.Add(tri);
                        break;
                    }
                }

                // Recursive search from first bad triangle founded
                AddNeighborsIfBad(_badTriangles.First(), pt);

                // Retriangulate
                Add(pt);

            }

            Clean();

            return true;
        }

        /// <summary>
        /// Find recursively in adjacents every bad triangles.
        /// </summary>
        private void AddNeighborsIfBad(Triangle triOrigin, Vector3 pt)
        {
            if (triOrigin != null)
            {
                foreach (Triangle tri in triOrigin.Adjacents)
                {
                    if (tri != null && !_badTriangles.Contains(tri) && tri.InCircumCircle(pt))
                    {
                        _badTriangles.Add(tri);
                        AddNeighborsIfBad(tri, pt);
                    }
                }
            }


            return;
        }

        /// <summary>
        /// Create star shape polygon based on bad triangles
        /// and used it to update triangulation.
        /// </summary>
        private void Add(Vector3 pt)
        {
            Triangle firstT = null;
            bool starShapePolygonCompleted = false;

            // Get random bad but best edge to start star polygon construction
            Triangle badT = _badTriangles.First();
            int edgeIndex = 0;

            // First point for starting
            Vector3 originPos = badT.GetPointsFromSegment(edgeIndex)[0];

            // Start shape creation in CCW order
            // https://en.wikipedia.org/wiki/Star-shaped_polygon
            while (!starShapePolygonCompleted)
            {
                Triangle adj_badT = badT.GetAdjacent(edgeIndex);
                if (!_badTriangles.Contains(adj_badT))
                {
                    // Construct a new triangle with correct edge
                    Vector3[] segment = badT.GetPointsFromSegment(edgeIndex);
                    Triangle newT = new Triangle(segment, pt);
                    newT.SetAdjacent(0, adj_badT);
                    if (adj_badT != null)
                    {
                        // Repair adj_badT pointer from badT to newT
                        // adjacent is null by default so bounds are automagically repaired
                        int adj_badT_Index = Array.IndexOf(adj_badT.Adjacents, badT);
                        adj_badT.SetAdjacent(adj_badT_Index, newT);
                    }

                    // Already another new triangle that need to be connected ?
                    if (_newTriangles.Count > 0)
                    {
                        // Link them together
                        Triangle previous_newT = _newTriangles.Last();
                        previous_newT.SetAdjacent(1, newT);
                        newT.SetAdjacent(2, previous_newT);
                    }
                    else
                    {
                        // Keep reference to first one to be able to close links
                        firstT = newT;
                    }

                    // Update new triangles and increment edge index (CCW)
                    _newTriangles.Add(newT);
                    edgeIndex = (edgeIndex + 1) % 3;

                    // Star shaped polygon completed
                    // First point of first segment == last point of current segment
                    if (originPos == segment[1])  // Should also be equal to newT.B
                    {
                        // Add connection between current and first triangles
                        newT.SetAdjacent(1, firstT);
                        firstT.SetAdjacent(2, newT);

                        starShapePolygonCompleted = true;
                    }
                }
                else
                {
                    // Go to next bad triangle (opposite of current segment)
                    // then add 1 to go to next one.
                    // adj_badT cannot be null here because of first if clause
                    edgeIndex = (Array.IndexOf(adj_badT.Adjacents, badT) + 1) % 3;
                    badT = adj_badT;
                }
            }

            // Loop over ... update the triangulation.
            _triangles.ExceptWith(_badTriangles);
            _triangles.UnionWith(_newTriangles);
        }

        /// <summary>
        /// Remove triangles formed using at least a bound as one of its vertices.
        /// </summary>
        private void Clean()
        {
            var bounds = new HashSet<Vector3>(_bounds);
            _triangles.RemoveWhere(tri => tri.IsOneOfVertices(bounds));
        }


        /// <summary>
        /// Draw a triangle in Unity for debugging.
        /// </summary>
        public void DrawDelaunay(GameObject[] shapes, Transform parent,
                              float scale, Color[] colors, Material lineMat,
                              bool points=false, bool edges=false, bool area=false,
                              bool circumcenter=false, bool circumcercle=false)
        {
            var colorIndex = 0;
            var maxColors = colors.Length;
            foreach (Triangle tri in _triangles)
            {
                GameObject triangleChild = new GameObject(tri.ID.ToString());
                triangleChild.transform.SetParent(parent);
                Color color = colors[colorIndex];
                tri.Draw(shapes, triangleChild.transform, color, scale, lineMat,
                              points:points, edges:edges, area:area,
                              circumcenter:circumcenter, circumcercle:circumcercle);


                colorIndex = (colorIndex + 1) % maxColors;
            }
        }

        public void DrawVoronoi(GameObject[] shapes, Transform parent,
                              float scale, Color[] colors, Material lineMat,
                              bool points=false, bool edges=false, bool area=false,
                              bool circumcenter=false, bool circumcercle=false)
        {

        }
    }
}

