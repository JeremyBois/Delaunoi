using System;
using System.Linq;
using System.Collections.Generic;


// @TODO Make sure CyclingMerge compute a valide triangulation.
//       Solve issue with Obtuse angles facing each other.



namespace Delaunoi
{
    using Delaunoi.Interfaces;
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;

    /// <summary>
    /// Create a cycling mesh representing a sphere. Wrap primal (Delaunay) and
    /// dual (Voronoi, Centroid, ...) meshes.
    /// </summary>
    public class SphericalMesh<TEdge, TFace>: IFluent<Face<TEdge, TFace>>, IFluent<Vec3>
    {
        // Context use to implement fluent pattern
        protected IEnumerable<Vec3>               _contextTriangles;
        protected IEnumerable<Face<TEdge, TFace>> _contextFaces;

        // Internal representation of the mesh
        protected GuibasStolfi<TEdge>      _mesh;


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

            _mesh = new GuibasStolfi<TEdge>(points, alreadySorted);

        }



    // PROPERTIES

        /// <summary>
        /// Return the leftmost edge if triangulation already done, else null.
        /// </summary>
        public QuadEdge<TEdge> LeftMostEdge
        {
            get {return _mesh.LeftMostEdge;}
        }

        /// <summary>
        /// Return the rightmost edge if triangulation already done, else null.
        /// </summary>
        public QuadEdge<TEdge> RightMostEdge
        {
            get {return _mesh.RightMostEdge;}
        }



    // PUBLIC METHODS

        /// <summary>
        /// Export Triangles based on Delaunay triangulation. Note that vertices
        /// inside the triangulation are not projected back into the sphere.
        /// See <see cref="Delaunoi.Geometry.InvStereographicProjection"/>.
        /// </summary>
        public IFluent<Vec3> Triangles()
        {
            _contextTriangles = ExportTriangles();
            return this;
        }

        /// <summary>
        /// Construct all faces based on Delaunay triangulation sites. Each face site
        /// are computed with delaunay site projected into the sphere, then scale
        /// by <paramref name="scaleFactor"/>. Finally resulting sites are moved to
        /// the sphere surface using their magnitude.
        /// </summary>
        /// <remarks>
        /// Each face is yield just after their construction. Then it's neighborhood
        /// is not guarantee to be constructed. To manipulate neighborhood first cast
        /// to a List or an Array before performing any operations.
        /// </remarks>
        /// <param name="faceType">Define the type of face used for extraction.</param>
        /// <param name="scaleFactor">Radius used to scale each face sites.</param>
        public IFluent<Face<TEdge, TFace>> Faces(FaceConfig faceType, double scaleFactor)
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
        /// Can be used to use to expose collection to fluent extensions from LINQ
        /// (<see cref="System.Linq"/>).
        /// </summary>
        IEnumerable<Vec3> IFluent<Vec3>.Collection()
        {
            return _contextTriangles;
        }

        /// <summary>
        /// Can be use to apply an operation on each element of the collection
        /// (<see cref="System.Linq.Select"/>).
        /// </summary>
        IFluent<Vec3> IFluent<Vec3>.ForEach(Func<Vec3, Vec3> selector)
        {
            _contextTriangles = _contextTriangles.Select(vec => selector(vec));
            return this;
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
        /// Can be used to use to expose collection to fluent extensions from LINQ
        /// (<see cref="System.Linq"/>).
        /// </summary>
        IEnumerable<Face<TEdge, TFace>> IFluent<Face<TEdge, TFace>>.Collection()
        {
            return _contextFaces;
        }

        /// <summary>
        /// Can be use to apply an operation on each element of the collection
        /// (<see cref="System.Linq.Select"/>).
        /// </summary>
        IFluent<Face<TEdge, TFace>> IFluent<Face<TEdge, TFace>>.ForEach(Func<Face<TEdge, TFace>, Face<TEdge, TFace>> selector)
        {
            _contextFaces = _contextFaces.Select(face => selector(face));
            return this;
        }

        /// <summary>
        /// Build a list of face which accounts for previous operations.
        /// </summary>
        List<Face<TEdge, TFace>> IFluent<Face<TEdge, TFace>>.ToList()
        {
            return _contextFaces.ToList();
        }

        /// <summary>
        /// Build an array of face which accounts for previous operations.
        /// </summary>
        Face<TEdge, TFace>[] IFluent<Face<TEdge, TFace>>.ToArray()
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
            // @TODO Make sure CyclingMerge compute a valide triangulation
            QuadEdge<TEdge> baseEdge = _mesh.RightMostEdge.Rnext;
            QuadEdge<TEdge> lCand = baseEdge.Sym.Onext;
            QuadEdge<TEdge> rCand = baseEdge.Oprev;

            // Get edges CCW order from left extremum until reach right extremum
            // to complete the sphere triangulation
            // All edges must be stored first because pointer are updated
            // during construction and will eventually leads to infinite loop ...
            var toCheckEdge = rCand.LeftEdges(true).ToList();
            foreach (QuadEdge<TEdge> rightEdge in toCheckEdge)
            {
                if (rightEdge.Destination != lCand.Destination)
                {
                    // Connect rightEdge.Destination to baseEdge.Destination
                    // Construct a fan
                    baseEdge = QuadEdge<TEdge>.Connect(rightEdge, baseEdge.Sym);
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
            var queue = new Queue<QuadEdge<TEdge>>();

            // Start at the far right
            QuadEdge<TEdge> first = _mesh.RightMostEdge;
            queue.Enqueue(first);

            // Visit all edge of the convex hull in CW order and
            // add opposite edges to queue
            foreach (QuadEdge<TEdge> hullEdge in first.RightEdges(CCW:false))
            {
                // Enqueue same edge but with opposite direction
                queue.Enqueue(hullEdge.Sym);
                hullEdge.Tag = !_mesh.VisitedTagState;

                // Because mesh does not have any boundary this is also a triangle
                yield return hullEdge.Origin;
            }

            // Convex hull now closed. Start triangles construction
            while (queue.Count > 0)
            {
                QuadEdge<TEdge> edge = queue.Dequeue();
                if (edge.Tag == _mesh.VisitedTagState)
                {
                    foreach (QuadEdge<TEdge> current in edge.RightEdges(CCW:false))
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
        protected IEnumerable<Face<TEdge, TFace>> ExportFaces(Func<Vec3, Vec3, Vec3, Vec3> centerCalculator,
                                                   double scaleFactor)
        {
            // FIFO
            var queue = new Queue<QuadEdge<TEdge>>();

            // Start at the far left
            QuadEdge<TEdge> first = LeftMostEdge;

            // @TODO Make sure CyclingMerge compute a valide triangulation
            // Construct first face using Centroid because
            // triangulation is not necessary delaunay
            foreach (QuadEdge<TEdge> current in first.EdgesFrom(CCW:false))
            {
                if (current.Rot.Origin == null)
                {
                    current.Rot.Origin = Geometry.Centroid(Geometry.InvStereographicProjection(current.Origin),
                                                           Geometry.InvStereographicProjection(current.Destination),
                                                           Geometry.InvStereographicProjection(current.Oprev.Destination));
                    double invDistanceScaled = scaleFactor / current.Rot.Origin.Magnitude;
                    current.Rot.Origin *= invDistanceScaled;

                    // Speed up computation of point coordinates
                    // All edges sharing the same origin have same
                    // geometrical origin
                    foreach (QuadEdge<TEdge> otherDual in current.Rot.EdgesFrom())
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
            yield return new Face<TEdge, TFace>(first, false, false);

            // Convex hull now closed --> Construct bounded voronoi faces
            while (queue.Count > 0)
            {
                QuadEdge<TEdge> edge = queue.Dequeue();

                if (edge.Tag == _mesh.VisitedTagState)
                {
                    // Construct a new face
                    foreach (QuadEdge<TEdge> current in edge.EdgesFrom(CCW:false))
                    {
                        if (current.Rot.Origin == null)
                        {
                            current.Rot.Origin = centerCalculator(Geometry.InvStereographicProjection(current.Origin),
                                                                  Geometry.InvStereographicProjection(current.Destination),
                                                                  Geometry.InvStereographicProjection(current.Oprev.Destination));
                            double invDistanceScaled = scaleFactor / current.Rot.Origin.Magnitude;
                            current.Rot.Origin *= invDistanceScaled;

                            // Speed up computation of point coordinates
                            // All edges sharing the same origin have same
                            // geometrical origin
                            foreach (QuadEdge<TEdge> otherDual in current.Rot.EdgesFrom())
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

                    yield return new Face<TEdge, TFace>(edge, false, false);
                }
            }

            // Inverse flag to be able to traverse again at next call
            _mesh.SwitchInternalFlag();
        }
    }
}
