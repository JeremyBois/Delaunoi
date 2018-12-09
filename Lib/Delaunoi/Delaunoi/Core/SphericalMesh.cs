using System;
using System.Linq;
using System.Collections.Generic;


namespace Delaunoi
{
    using Delaunoi.Interfaces;
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;


    public class SphericalMesh<T>: IFluent<Face<T>>, IFluent<Vec3>
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
        public SphericalMesh(Vec3[] points, bool alreadySorted=false)
        {
            if (points.Length < 8)
            {
                throw new NotSupportedException("At least 8 points is needed to triangulate a sphere.");
            }

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
        /// Construct all faces based on Delaunay triangulation.
        /// </summary>
        /// <remarks>
        /// Each face is yield just after their construction. Then it's neighborhood
        /// is not guarantee to be constructed. To manipulate neighborhood first cast
        /// to a List or an Array before performing any operations.
        /// </remarks>
        /// <param name="faceType">Define the type of face used for extraction.</param>
        public IFluent<Face<T>> Faces(FaceConfig faceType, double scaleFactor)
        {
            switch (faceType)
            {
                case FaceConfig.Centroid:
                    _contextFaces = ExportFaces(Geometry.Centroid, scaleFactor);
                    break;
                case FaceConfig.Voronoi:
                    _contextFaces = ExportFaces(Geometry.CircumCenter3D, scaleFactor);
                    break;
                case FaceConfig.InCenter:
                    _contextFaces = ExportFaces(Geometry.InCenter, scaleFactor);
                    break;
                case FaceConfig.RandomUniform:
                    _contextFaces = ExportFaces(RandGen.TriangleUniform, scaleFactor);
                    break;
                case FaceConfig.RandomNonUniform:
                    _contextFaces = ExportFaces(RandGen.TriangleNonUniform, scaleFactor);
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

            // Connect left and right most edges together
            CyclingMerge();

            return true;
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
        IEnumerable<Face<T>> IFluent<Face<T>>.Select()
        {
            return _contextFaces;
        }

        /// <summary>
        /// Build a list of face which accounts for previous operations.
        /// </summary>
        List<Face<T>> IFluent<Face<T>>.ToList()
        {
            return _contextFaces.ToList();
        }

        /// <summary>
        /// Build an array of face which accounts for previous operations.
        /// </summary>
        Face<T>[] IFluent<Face<T>>.ToArray()
        {
            return _contextFaces.ToArray();
        }



    // PRIVATE METHODS




    // PROTECTED METHODS

        /// <summary>
        /// Merge left and right most edges together to construct a cycling triangulation.
        /// Needed for sphere for example after normal triangulation step.
        /// </summary>
        protected void CyclingMerge()
        {
            QuadEdge<T> baseEdge = _mesh.RightMostEdge.Rnext;
            QuadEdge<T> lCand = baseEdge.Sym.Onext;
            QuadEdge<T> rCand = baseEdge.Oprev;

            // Get edges CCW order from left extremum until reach right extremum
            // to complete the sphere triangulation
            // All edges must be stored first because pointer are updated
            // during construction and will eventually leads to infinite loop ...
            var toCheckEdge = rCand.LeftEdges(true).ToList();
            foreach (QuadEdge<T> rightEdge in toCheckEdge)
            {
                if (rightEdge.Destination != lCand.Destination)
                {
                    // Connect rightEdge.Destination to baseEdge.Destination
                    // Construct a fan
                    baseEdge = QuadEdge<T>.Connect(rightEdge, baseEdge.Sym);
                }
                else
                {
                    break;
                }
            }
        }


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

            // @TODO Possible to handle it properly ???
            // Will be true only when extermum are connected together
            // Should be the case for a sphere
            // Avoid false true when only one triangle
            foreach (QuadEdge<T> current in first.RightEdges(CCW:false))
            {
                current.Tag = !_mesh.VisitedTagState;
                yield return current.Origin;
            }

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
        protected IEnumerable<Face<T>> ExportFaces(Func<Vec3, Vec3, Vec3, Vec3> centerCalculator,
                                                   double scaleFactor)
        {
            // FIFO
            var queue = new Queue<QuadEdge<T>>();

            // Start at the far left
            QuadEdge<T> first = LeftMostEdge;

            // Construct a new face
            foreach (QuadEdge<T> current in first.EdgesFrom(CCW:false))
            {
                if (current.Rot.Origin == null)
                {
                    current.Rot.Origin = Geometry.Centroid(scaleFactor * Geometry.InvStereographicProjection(current.Origin),
                                                           scaleFactor * Geometry.InvStereographicProjection(current.Destination),
                                                           scaleFactor * Geometry.InvStereographicProjection(current.Oprev.Destination));
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
            yield return new Face<T>(first, false, false);

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
                            current.Rot.Origin = centerCalculator(scaleFactor * Geometry.InvStereographicProjection(current.Origin),
                                                                  scaleFactor * Geometry.InvStereographicProjection(current.Destination),
                                                                  scaleFactor * Geometry.InvStereographicProjection(current.Oprev.Destination));

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

                    yield return new Face<T>(edge, false, false);
                }
            }

            // Inverse flag to be able to traverse again at next call
            _mesh.SwitchInternalFlag();
        }
    }
}
