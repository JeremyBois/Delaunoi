using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Delaunay.DataStructure
{
    /// <summary>
    /// Represent a triangle (ABC) assuming a CCW order where each vertex is a Vector3.
    /// Neighbors are also catched for quick access based on segment name (AB, BC, CA)
    /// or using segment index (0->AB, 1->BC, 2->CA).
    /// Circumcenter and SquaredRadius are cached lazily (first time access computed).
    /// </summary>
    public class Triangle
    {
        // Vertices coordinates
        private readonly Vector3 _a;
        private readonly Vector3 _b;
        private readonly Vector3 _c;

        // Used to compute unique ID
        private static int _currentID = 0;
        private int _id;

        // Adjacents triangles for each segments (AB, BC, CA)
        private Triangle[] _adjacents = new Triangle[3];

        // Circumcenter (lazy calculation)
        private Vector3 _circumcenter;
        private float _squaredCircumRadius;
        private bool _isCachedCircumcenter;

        // Centroid (lazy calculation)
        private Vector3 _centroid;
        private bool _isCachedCentroid;



    // CONSTRUCTOR
        /// <summary>
        /// Default constructor using 3 point to construct a Triangle.
        /// Points must be given in a CCW order for the triangle to be CCW.
        /// </summary>
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this._a = a;
            this._b = b;
            this._c = c;
            _id = _currentID++;

            // Mark cache value as invalid
            _isCachedCircumcenter = false;
            _isCachedCentroid = false;
        }

        /// <summary>
        /// Copy constructor using another triangle to construct a new one.
        /// Cache value are not copied and triangle id will be different than original
        /// </summary>
        public Triangle(Triangle tri)
            : this(tri._a, tri._b, tri._c)
        {
            tri._adjacents.CopyTo(this._adjacents, 0);
        }

        /// <summary>
        /// Create a triangle using a segment AB and a point C.
        /// Order matter to keep triangle ordered in CCW and segment must
        /// be in CCW order to get a CCW triangle.
        /// </summary>
        public Triangle(Vector3[] segment, Vector3 point)
            : this(segment[0], segment[1], point)
        {
        }


    // PROPERTIES
        public Vector3 A
        {
            get {return _a;}

        }

        public Vector3 B
        {
            get {return _b;}

        }

        public Vector3 C
        {
            get {return _c;}

        }

        public int ID
        {
            get {return _id;}
        }

        public Vector3 Centroid
        {
            get
            {
                if (!_isCachedCentroid)
                {
                    _centroid = (_a + _b + _c) / 3.0f;
                }
                return _centroid;
            }
        }

        public Vector3 Circumcenter
        {
            get
            {
                if (!_isCachedCircumcenter)
                {
                    ComputeCircumcenter();
                }
                return _circumcenter;
            }
        }

        public float SquaredCircumcenterRadius
        {
            get
            {
                if (!_isCachedCircumcenter)
                {
                    ComputeCircumcenter();
                }
                return _squaredCircumRadius;
            }
        }

        /// <summary>
        /// Get an array of adjacents triangles. Adjacents should remains readonly.
        /// </summary>
        public Triangle[] Adjacents
        {
            get {return _adjacents;}
        }

        /// <summary>
        /// Get vertices in CCW order.
        /// </summary>
        public Vector3[] Vertices
        {
            get {return new Vector3[] {_a, _b, _c};}
        }


    // METHODS

        // /// <summary>
        // /// Override to reduce time for its calculation and improve insert and remove
        // /// operations.
        // /// </summary>
        // public override int GetHashCode()
        // {
        //     // // C# 7
        //     // return (_a, _b, _c).GetHashCode();

        //     // http://eternallyconfuzzled.com/tuts/algorithms/jsw_tut_hashing.aspx#fnv
        //     // https://stackoverflow.com/questions/1835976/what-is-a-sensible-prime-for-hashcode-calculation/2816747#2816747
        //     unchecked // Overflow is fine, just wrap
        //     {
        //         // Hight primes reduces collisions for lower values
        //         int hash = 5323;
        //         hash = (hash * 4567) ^ _a.GetHashCode();
        //         hash = (hash * 4567) ^ _b.GetHashCode();
        //         hash = (hash * 4567) ^ _c.GetHashCode();
        //         return hash;
        //     }
        // }

        /// <summary>
        /// Test if a point is in fact a vertex of this.
        /// </summary>
        public bool IsVertex(Vector3 point, float epsilon=0.0000001f)
        {
            return (
                    (Vector3.SqrMagnitude(point - _a) < epsilon) ||
                    (Vector3.SqrMagnitude(point - _b) < epsilon) ||
                    (Vector3.SqrMagnitude(point - _c) < epsilon)
                   );
        }

        /// <summary>
        /// Return a pair of Vector3 representing the vertices of the edge
        /// at index edgeIndex (vertices are in a CCW order).
        /// </summary>
        public Vector3[] GetPointsFromSegment(int edgeIndex)
        {
            var result = new Vector3[2];
            switch (edgeIndex)
            {
                case 0:
                    // Segment AB
                    result[0] = this._a;
                    result[1] = this._b;
                    break;
                case 1:
                    // Segment BC
                    result[0] = this._b;
                    result[1] = this._c;
                    break;
                case 2:
                    // Segment CA
                    result[0] = this._c;
                    result[1] = this._a;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            return result;
        }

        /// <summary>
        /// Get adjacent triangle based on edge index.
        /// </summary>
        public Triangle GetAdjacent(int edgeIndex)
        {
            try
            {
                return _adjacents[edgeIndex];
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.LogError(string.Format("edgeIndex ({0}) not in [0, 3) range",
                                            edgeIndex));
                throw e;
            }
        }

        /// <summary>
        /// Set adjacent triangle for an edge index.
        /// </summary>
        public void SetAdjacent(int edgeIndex, Triangle tri)
        {
            try
            {
                _adjacents[edgeIndex] = tri;
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.LogError(string.Format("edgeIndex ({0}) not in [0, 3) range",
                                            edgeIndex));
                throw e;
            }
        }


        /// <summary>
        /// Compute circumcenter and store Squared circumcenter radius
        /// and circumcenter position.
        /// Computation is based difference using A as the origin to reduce
        /// numerical error.
        /// </summary>
        public void ComputeCircumcenter()
        {
            _circumcenter = Vector3.zero;
            // https://www.ics.uci.edu/~eppstein/junkyard/circumcenter.html
            // "|" for NORM and "x" for CROSS PRODUCT
            // Triangle in R^3:
            //         |c-a|^2 [(b-a)x(c-a)]x(b-a) + |b-a|^2 (c-a)x[(b-a)x(c-a)]
            // m = a + ---------------------------------------------------------.
            //                            2 | (b-a)x(c-a) |^2
            // Use a as origin for coordinates
            Vector3 ca = _c - _a;
            Vector3 ba = _b - _a;

            // Precompute cross product
            Vector3 baca = Vector3.Cross(ba, ca);
            // Compute inverse of denominator
            float invDenominator = 0.5f / baca.sqrMagnitude;
            // Compute numerator
            Vector3 numerator =  Vector3.Cross(ca.sqrMagnitude * baca, ba) +
                                 ba.sqrMagnitude * Vector3.Cross(ca, baca);
            // Compute circumcenter
            _circumcenter = _a + (numerator * invDenominator);
            // Then compute radius using one point and center (quicker than cramer rule)
            _squaredCircumRadius = Vector3.SqrMagnitude(A - _circumcenter);

            _isCachedCircumcenter = true;
        }

        /// <summary>
        /// Return true if point inside the triangle circumcercle (exclusive).
        /// </summary>
        public bool InCircumCircle(Vector3 pt)
        {
            if (!_isCachedCircumcenter)
            {
                ComputeCircumcenter();
            }

            // @TODO Implement a robust test
            // /https://www.cs.cmu.edu/%7Equake/robust.html
            return Vector3.SqrMagnitude(_circumcenter - pt) < SquaredCircumcenterRadius;
        }

        /// <summary>
        /// Test if a point in points is a vertex of the triangle.
        /// </summary>
        public bool IsOneOfVertices(HashSet<Vector3> points)
        {
            foreach (Vector3 vec in points)
            {
                if (this.IsVertex(vec))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Draw a triangle in Unity for debugging.
        /// </summary>
        public void Draw(GameObject[] shapes, Transform parent,
                              Color color, float scale, Material lineMat,
                              bool points=true, bool edges=false, bool area=false,
                              bool circumcenter=false, bool circumcercle=false)
        {
            // Draw points
            if (points)
            {
                foreach (Vector3 pos in this.Vertices)
                {
                    var newShape = GameObject.Instantiate(shapes[0]);
                    newShape.transform.SetParent(parent);
                    newShape.transform.position = pos;
                    newShape.transform.localScale = new Vector3(scale,
                                                                scale,
                                                                scale
                                                                );
                    // Color
                    var meshR = newShape.GetComponent<MeshRenderer>();
                    if (meshR != null)
                    {
                        meshR.materials[0].color = Color.black;
                    }
                }
            }


            // Draw edges
            if (edges)
            {
                LineRenderer lr = parent.gameObject.AddComponent<LineRenderer>();
                lr.material = lineMat;
                lr.positionCount = 3;
                lr.SetPositions(this.Vertices);
                lr.loop = true;
                lr.material.color = Color.black;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.startWidth = 0.25f;
                lr.endWidth = 0.25f;
            }


            // Fill triangles
            if (area)
            {
                parent.gameObject.AddComponent<MeshFilter>();
                parent.gameObject.AddComponent<MeshRenderer>();
                var filter = parent.gameObject.GetComponent<MeshFilter>();
                var renderer = parent.gameObject.GetComponent<MeshRenderer>();
                renderer.material = lineMat;
                renderer.materials[0].color = color;
                filter.mesh.vertices = this.Vertices;
                filter.mesh.triangles = new []{2, 1, 0};
            }

            if (circumcenter)
            {
                var newShape = GameObject.Instantiate(shapes[1]);
                newShape.transform.SetParent(parent);
                newShape.transform.position = Circumcenter;
                newShape.transform.localScale = new Vector3(scale,
                                                            scale,
                                                            scale
                                                            );

                // Color
                var meshR = newShape.GetComponent<MeshRenderer>();
                if (meshR != null)
                {
                    meshR.materials[0].color = Color.blue;
                }
            }

            if (circumcercle && !edges)
            {
                LineRenderer lr = parent.gameObject.AddComponent<LineRenderer>();
                lr.material = lineMat;
                lr.positionCount = 100;
                lr.loop = true;
                lr.material.color = Color.blue;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.startWidth = 0.5f;
                lr.endWidth = 0.5f;

                int segments = lr.positionCount;
                float radius = Mathf.Sqrt(this.SquaredCircumcenterRadius);
                float x;
                float y;
                float z = 0f;

                float angle = 20f;

                for (int i = 0; i < segments; i++)
                {
                    x = Circumcenter.x + Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
                    y = Circumcenter.y + Mathf.Cos (Mathf.Deg2Rad * angle) * radius;

                    lr.SetPosition (i, new Vector3(x, y, z) );

                    angle += (360f / segments);
                }
            }

        }
    }
}
































































































































































