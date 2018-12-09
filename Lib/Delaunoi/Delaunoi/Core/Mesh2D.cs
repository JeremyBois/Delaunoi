using System;
using System.Linq;
using System.Collections.Generic;


namespace Delaunoi
{

    using Delaunoi.Interfaces;
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;


    public class Mesh2D<T>: IFluentExtended<Face<T>>, IFluent<Vec3>
    {
        // Context use to implement fluent pattern
        protected IEnumerable<Vec3>    _contextTriangles;
        protected IEnumerable<Face<T>> _contextFaces;

        // Internal representation of the mesh
        protected GuibasStolfi<T>      _mesh;


    // CONSTRUCTOR

        /// <summary>
        /// Load an array of position to be triangulate.
        /// </summary>
        /// <param name="points">An array of points to triangulate.</param>
        /// <param name="alreadySorted">Points already sorted (base on x then y).</param>
        public Mesh2D(Vec3[] points, bool alreadySorted=false)
        {
            _mesh = new GuibasStolfi<T>(points, alreadySorted);
        }



    // PROPERTIES

        /// <summary>
        /// Return the leftmost edge if triangulation already done, else null.
        /// </summary>
        public QuadEdge<T> LeftMostEdge
        {
            get {return _mesh.LeftMostEdge;}
        }

        /// <summary>
        /// Return the rightmost edge if triangulation already done, else null.
        /// </summary>
        public QuadEdge<T> RightMostEdge
        {
            get {return _mesh.RightMostEdge;}
        }




    // PUBLIC METHODS

        /// <summary>
        /// Export Triangles based on Delaunay triangulation.
        /// </summary>
        public IFluent<Vec3> Triangles()
        {
            _contextTriangles = ExportTriangles();
            return this;
        }

        /// <summary>
        /// Construct all faces based on Delaunay triangulation. Vertices at infinity
        /// are define based on radius parameter. It should be large enough to avoid
        /// some circumcenters (finite voronoi vertices) to be further on.
        /// Using <see cref="FaceConfig.RandomUniform"/> and <see cref="FaceConfig.RandomNonUniform"/>
        /// can leads to intersection with face area constructed for points at infinity because
        /// they are calculated based on <see cref="FaceConfig.Voronoi"/> strategy.
        /// </summary>
        /// <remarks>
        /// Each face is yield just after their construction. Then it's neighborhood
        /// is not guarantee to be constructed. To manipulate neighborhood first cast
        /// to a List or an array before performing operations.
        /// </remarks>
        /// <param name="faceType">Define the type of face used for extraction.</param>
        /// <param name="radius">Distance used to construct site that are at infinity.</param>
        /// <param name="useZCoord">If true face center compute in R^3 else in R^2 (matter only if voronoi).</param>
        public IFluentExtended<Face<T>> Faces(FaceConfig faceType, double radius, bool useZCoord=true)
        {
            switch (faceType)
            {
                case FaceConfig.Centroid:
                    _contextFaces = ExportFaces(radius, Geometry.Centroid);
                    break;
                case FaceConfig.Voronoi:
                    if (useZCoord)
                    {
                        _contextFaces = ExportFaces(radius, Geometry.CircumCenter3D);
                    }
                    else
                    {
                        _contextFaces = ExportFaces(radius, Geometry.CircumCenter2D);
                    }
                    break;
                case FaceConfig.InCenter:
                    _contextFaces = ExportFaces(radius, Geometry.InCenter);
                    break;
                case FaceConfig.RandomUniform:
                    _contextFaces = ExportFaces(radius, RandGen.TriangleUniform);
                    break;
                case FaceConfig.RandomNonUniform:
                    _contextFaces = ExportFaces(radius, RandGen.TriangleNonUniform);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return this;
        }

        /// <summary>
        /// Return an array of Vec3 from the triangulation.
        /// </summary>
        public bool Construct()
        {
            _mesh.ComputeDelaunay();

            return true;
        }

        /// <summary>
        /// Locate the closest with respect to following constraints:
        ///   - <paramref name="pos"/> is on the line of returned edge
        ///   - <paramref name="pos"/> is inside the left face of returned edge
        ///
        /// If site outside the convex hull Locate will loop forever looking for
        /// a corresponding edge that does not exists ... unless you first check
        /// you are in the convex hull or set <paramref name="checkBoundFirst"/> to true.
        /// </summary>
        /// <param name="pos">The position to locate</param>
        /// <param name="edge">Edge used to start locate process. Can be used to speed up search.</param>
        /// <param name="safe">If true, first check if pos in convex hull of triangulation</param>
        public QuadEdge<T> Locate(Vec3 pos, QuadEdge<T> edge=null, bool safe=false)
        {
            return _mesh.Locate(pos, edge, safe);
        }

        /// <summary>
        /// Insert a new site inside an existing delaunay triangulation. New site
        /// must be inside the convex hull of previoulsy added sites.
        /// Set <paramref name="safe"/> to true to first test if new site is correct.
        /// </summary>
        /// <param name="newPos">The position to of new site</param>
        /// <param name="edge">Edge used to start locate process. Can be used to speed up search.</param>
        /// <param name="safe">If true, check if <paramref name="safe"/> inside the convex hull.</param>
        public bool Insert(Vec3 newPos, QuadEdge<T> edge=null, bool safe=false)
        {
            return _mesh.InsertSite(newPos, edge, safe);
        }

        /// <summary>
        /// True if <paramref name="pos"/> inside the convex hull formed by the triangulation.
        /// </summary>
        /// <param name="pos">The position to test</param>
        public bool InsideConvexHull(Vec3 pos)
        {
            return _mesh.InsideConvexHull(pos);
        }



    // FLUENT INTERFACE FOR TRIANGLES

        /// <summary>
        /// Can be used to use fluent extensions from LINQ (<see cref="System.Linq"/>).
        /// </summary>
        IEnumerable<Vec3> IFluent<Vec3>.Select()
        {
            return _contextTriangles;
        }

        /// <summary>
        /// Build a list of face which accounts for previous operations.
        /// </summary>
        List<Vec3> IFluent<Vec3>.ToList()
        {
            return _contextTriangles.ToList();
        }

        /// <summary>
        /// Build an array of face which accounts for previous operations.
        /// </summary>
        Vec3[] IFluent<Vec3>.ToArray()
        {
            return _contextTriangles.ToArray();
        }



    // FLUENT INTERFACE FOR FACES

        /// <summary>
        /// Can be used to use fluent extensions from LINQ (<see cref="System.Linq"/>).
        /// </summary>
        IEnumerable<Face<T>> IFluentExtended<Face<T>>.Select()
        {
            return _contextFaces;
        }

        /// <summary>
        /// Build a list of face which accounts for previous operations.
        /// </summary>
        List<Face<T>> IFluentExtended<Face<T>>.ToList()
        {
            return _contextFaces.ToList();
        }

        /// <summary>
        /// Build an array of face which accounts for previous operations.
        /// </summary>
        Face<T>[] IFluentExtended<Face<T>>.ToArray()
        {
            return _contextFaces.ToArray();
        }

        /// <summary>
        /// Keep only faces with at least one boundary site at infinity.
        /// </summary>
        public IFluentExtended<Face<T>> AtInfinity()
        {
            _contextFaces = _contextFaces.Where(x => x.Reconstructed);
            return this;
        }

        /// <summary>
        /// Keep only faces on the convex hull boundary.
        /// </summary>
        public IFluentExtended<Face<T>> Bounds()
        {
            _contextFaces = _contextFaces.Where(x => x.IsOnBounds);
            return this;
        }

        /// <summary>
        /// Keep only faces on the convex hull boundary with finite face bounds.
        /// </summary>
        public IFluentExtended<Face<T>> FiniteBounds()
        {
            _contextFaces = _contextFaces.Where(x => (x.IsOnBounds && !x.Reconstructed));
            return this;
        }

        /// <summary>
        /// Keep only faces with finite area.
        /// </summary>
        public IFluentExtended<Face<T>> Finite()
        {
            _contextFaces = _contextFaces.Where(x => !x.Reconstructed);
            return this;
        }

        /// <summary>
        /// Keep only faces inside the convex hull excluding boundary faces.
        /// </summary>
        public IFluentExtended<Face<T>> InsideHull()
        {
            _contextFaces = _contextFaces.Where(x => !x.IsOnBounds);
            return this;
        }

        /// <summary>
        /// Keep faces where their center is at a distance from <paramref name="origin"/>
        /// smaller than <paramref name="radius"/>.
        /// </summary>
        public IFluentExtended<Face<T>> CenterCloseTo(Vec3 origin, double radius)
        {
            double radiusSq = Math.Pow(radius, 2.0);
            _contextFaces = _contextFaces.Where(x => Vec3.DistanceSquared(origin, x.Center) < radiusSq);
            return this;
        }

        /// <summary>
        /// Keep faces where each of its boundary sites is at a distance from
        /// <paramref name="origin"/> smaller than <paramref name="radius"/>.
        /// </summary>
        /// <param name="origin">Origin used as reference for distance calculation.</param>
        /// <param name="radius">Minimal distance from origin.</param>
        public IFluentExtended<Face<T>> CloseTo(Vec3 origin, double radius)
        {
            double radiusSq = Math.Pow(radius, 2.0);
            _contextFaces = _contextFaces.Where(x => IsCloseTo(x, origin, radiusSq));
            return this;
        }

        /// <summary>
        /// Keep faces living inside a box defined by an <paramref name="origin"/>
        /// and its size (<paramref name="extends"/>).
        /// <param name="origin">Origin used for the box.</param>
        /// <param name="origin">Size of the box.</param>
        /// </summary>
        public IFluentExtended<Face<T>> Inside(Vec3 origin, Vec3 extends)
        {
            _contextFaces = _contextFaces.Where(x => IsInBounds(x, origin, extends));
            return this;
        }




    // PRIVATE METHOD

        private bool IsCloseTo(Face<T> face, Vec3 origin, double radiusSq)
        {
            foreach (Vec3 pos in face.Bounds)
            {
                if (Vec3.DistanceSquared(origin, pos) > radiusSq)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsInBounds(Face<T> face, Vec3 origin, Vec3 extends)
        {
            Vec3 upBounds = origin + extends;
            foreach (Vec3 pos in face.Bounds)
            {
                if (pos.X > upBounds.X || pos.Y > upBounds.Y || pos.Z > upBounds.Z)
                {
                    return false;
                }
                else if (pos.X < origin.X || pos.Y < origin.Y || pos.Z < origin.Z)
                {
                    return false;
                }
            }
            return true;
        }




    // PROTECTED METHOD

        /// <summary>
        /// Construct triangles based on Delaunay triangulation.
        /// </summary>
        protected IEnumerable<Vec3> ExportTriangles()
        {
            // FIFO
            var queue = new Queue<QuadEdge<T>>();

            // Start at the far right
            QuadEdge<T> first = _mesh.RightMostEdge;
            queue.Enqueue(first);

            // Visit all edge of the convex hull in CW order and
            // add opposite edges to queue
            foreach (QuadEdge<T> hullEdge in first.RightEdges(CCW:false))
            {
                // Enqueue same edge but with opposite direction
                queue.Enqueue(hullEdge.Sym);
                hullEdge.Tag = !_mesh.VisitedTagState;
            }

            // Convex hull now closed. Start triangles construction
            while (queue.Count > 0)
            {
                QuadEdge<T> edge = queue.Dequeue();
                if (edge.Tag == _mesh.VisitedTagState)
                {
                    foreach (QuadEdge<T> current in edge.RightEdges(CCW:false))
                    {
                        if (current.Sym.Tag == _mesh.VisitedTagState)
                        {
                            queue.Enqueue(current.Sym);
                        }
                        current.Tag = !_mesh.VisitedTagState;
                        yield return current.Origin;
                    }
                }
            }

            // Inverse flag to be able to traverse again at next call
            _mesh.SwitchInternalFlag();
        }

        /// <summary>
        /// Construct voronoi face based on Delaunay triangulation. Vertices at infinity
        /// are define based on radius parameter. It should be large enough to avoid
        /// some circumcenters (finite voronoi vertices) to be further on.
        /// </summary>
        /// <remarks>
        /// Each face is yield just after their construction. Then it's neighborhood
        /// is not guarantee to be constructed.
        /// </remarks>
        /// <param name="radius">Distance used to construct site that are at infinity.</param>
        protected IEnumerable<Face<T>> ExportFaces(double radius, Func<Vec3, Vec3, Vec3, Vec3> centerCalculator)
        {
            // FIFO
            var queue = new Queue<QuadEdge<T>>();

            // Start at the far left
            QuadEdge<T> first = _mesh.LeftMostEdge;

            // @TODO Bounds
            List<QuadEdge<T>> bounds = new List<QuadEdge<T>>();


            // Visit all edge of the convex hull to compute dual vertices
            // at infinity by looping in a CW order over edges with same left face.
            foreach (QuadEdge<T> hullEdge in first.LeftEdges(CCW:false))
            {
                // Construct a new face
                // First infinite voronoi vertex
                if (hullEdge.Rot.Destination == null)
                {
                    hullEdge.Rot.Destination = ConstructAtInfinity(hullEdge.Sym,
                                                                   radius,
                                                                   centerCalculator);
                }

                // Add other vertices by looping over hullEdge origin in CW order (Oprev)
                foreach (QuadEdge<T> current in hullEdge.EdgesFrom(CCW:false))
                {
                    if (current.Rot.Origin == null)
                    {
                        // Delaunay edge on the boundary
                        if (Geometry.LeftOf(current.Oprev.Destination, current))
                        {
                            current.Rot.Origin = ConstructAtInfinity(current,
                                                                     radius,
                                                                     centerCalculator);
                        }
                        else
                        {
                            current.Rot.Origin = centerCalculator(current.Origin,
                                                                  current.Destination,
                                                                  current.Oprev.Destination);

                            // Speed up computation of point coordinates
                            // All edges sharing the same origin should have same
                            // geometrical origin
                            foreach (QuadEdge<T> otherDual in current.Rot.EdgesFrom())
                            {
                                otherDual.Origin = current.Rot.Origin;
                            }
                        }
                    }

                    if (current.Sym.Tag == _mesh.VisitedTagState)
                    {
                        queue.Enqueue(current.Sym);
                        bounds.Add(current.Sym);
                    }
                    current.Tag = !_mesh.VisitedTagState;
                }

                // After face construction over
                yield return new Face<T>(hullEdge, true, true);
            }

            // Convex hull now closed --> Construct bounded voronoi faces
            while (queue.Count > 0)
            {
                QuadEdge<T> edge = queue.Dequeue();

                if (edge.Tag == _mesh.VisitedTagState)
                {
                    // Construct a new face
                    foreach (QuadEdge<T> current in edge.EdgesFrom(CCW:false))
                    {
                        if (current.Rot.Origin == null)
                        {
                            current.Rot.Origin = centerCalculator(current.Origin,
                                                                  current.Destination,
                                                                  current.Oprev.Destination);
                            // Speed up computation of point coordinates
                            // All edges sharing the same origin have same
                            // geometrical origin
                            foreach (QuadEdge<T> otherDual in current.Rot.EdgesFrom())
                            {
                                otherDual.Origin = current.Rot.Origin;
                            }
                        }
                        if (current.Sym.Tag  == _mesh.VisitedTagState)
                        {
                            queue.Enqueue(current.Sym);
                        }
                        current.Tag = !_mesh.VisitedTagState;
                    }

                    // After face construction over
                    if (bounds.Contains(edge))
                    {
                        yield return new Face<T>(edge, true, false);
                    }
                    else
                    {
                        yield return new Face<T>(edge, false, false);
                    }
                }
            }

            // Inverse flag to be able to traverse again at next call
            _mesh.SwitchInternalFlag();
        }


        /// <summary>
        /// Find correct position for a voronoi site that should be at infinite
        /// Assume primalEdge.Rot.Origin as the vertex to compute, that is
        /// there should be no vertex on the right of primalEdge.
        /// Site computed is the destination of a segment in a direction normal to
        /// the tangent vector of the primalEdge (destination - origin) with
        /// its symetrical (primalEdge.RotSym.Origin) as origin.
        /// Radius should be choose higher enough to avoid neighbor voronoi points
        /// to be further on. A good guest is the maximal distance between non infinite
        /// voronoi vertices or five times the maximal distance between delaunay vertices.
        /// </summary>
        /// <remarks>
        /// If primalEdge.RotSym.Origin is null, then its value is computed first
        /// using CircumCenter2D because this vertex is always inside a delaunay triangle.
        /// </remarks>
        protected Vec3 ConstructAtInfinity(QuadEdge<T> primalEdge, double radius,
                                           Func<Vec3, Vec3, Vec3, Vec3> centerCalculator)
        {
            var rotSym = primalEdge.RotSym;

            // Find previous voronoi site
            if (rotSym.Origin == null)
            {
                rotSym.Origin = centerCalculator(primalEdge.Origin,
                                                            primalEdge.Destination,
                                                            primalEdge.Onext.Destination);
            }
            double xCenter = rotSym.Origin.X;
            double yCenter = rotSym.Origin.Y;

            // Compute normalized tangent of primal edge scaled by radius
            double xTangent = primalEdge.Destination.X - primalEdge.Origin.X;
            double yTangent = primalEdge.Destination.Y - primalEdge.Origin.Y;
            double dist = Math.Sqrt(xTangent * xTangent + yTangent * yTangent);
            xTangent /= dist;
            yTangent /= dist;
            xTangent *= radius;
            yTangent *= radius;

            // Add vertex using edge dual destination as origin
            // in direction normal to the primal edge
            Vec3 normal = new Vec3(xCenter - yTangent, yCenter + xTangent, rotSym.Origin.Z);

            // If new voronoi vertex is on the left of the primal edge
            // we used the wrong normal vector --> get its opposite
            if (Geometry.LeftOf(normal, primalEdge))
            {
                normal = new Vec3(xCenter + yTangent, yCenter - xTangent, rotSym.Origin.Z);
            }
            return normal;
        }

    }
}
