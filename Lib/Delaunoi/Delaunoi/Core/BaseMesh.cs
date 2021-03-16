using System;
using System.Linq;
using System.Collections.Generic;

namespace Delaunoi
{
    using Delaunoi.Interfaces;
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;


    public class BaseMesh<TEdge, TFace>: IFluent<Vec3>
    {

        // Context use to implement fluent pattern
        protected IEnumerable<Vec3>               _contextTriangles;
        protected IEnumerable<Face<TEdge, TFace>> _contextFaces;

        // Internal representation of the mesh
        protected GuibasStolfi<TEdge>      _mesh;


        /// <summary>
        /// Load an array of position to be triangulate.
        /// </summary>
        /// <param name="points">An array of points to triangulate.</param>
        /// <param name="alreadySorted">Points already sorted (base on x then y).</param>
        public BaseMesh(Vec3[] points, bool alreadySorted=false)
        {
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

        /// <summary>
        /// Get internal Mesh
        /// </summary>
        public GuibasStolfi<TEdge> Mesh
        {
            get {return _mesh;}
        }


    // PUBLIC METHODS

        /// <summary>
        /// Export Triangles based on Delaunay triangulation.
        /// </summary>
        public virtual IFluent<Vec3> Triangles()
        {
            _contextTriangles = ExportTriangles();
            return this;
        }

        /// <summary>
        /// Return an array of Vec3 from the triangulation.
        /// </summary>
        public virtual bool Construct()
        {
            _mesh.ComputeDelaunay();

            return true;
        }



    // FLUENT INTERFACE FOR TRIANGLES

        /// <summary>
        /// Can be used to use fluent extensions from LINQ (<see cref="System.Linq"/>).
        /// </summary>
        IEnumerable<Vec3> IFluent<Vec3>.Collection()
        {
            return _contextTriangles;
        }

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



    // PROTECTED METHOD

        /// <summary>
        /// Construct triangles based on Delaunay triangulation.
        /// </summary>
        protected virtual IEnumerable<Vec3> ExportTriangles()
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
        /// Clear buffered cells. Needed to be able to construct another diagram without reconstruction of
        /// triangulation.
        /// </summary>
        protected virtual void ClearDualData()
        {
            // FIFO
            var queue = new Queue<QuadEdge<TEdge>>();

            // Start at the far right
            QuadEdge<TEdge> first = _mesh.RightMostEdge;
            queue.Enqueue(first);

            // Visit all edge of the convex hull in CW order and
            // add opposite edges to queue
            foreach (QuadEdge<TEdge> hullEdge in first.RightEdges(CCW: false))
            {
                // Enqueue same edge but with opposite direction
                queue.Enqueue(hullEdge.Sym);
                hullEdge.Tag = !_mesh.VisitedTagState;
            }

            // Convex hull now closed. Start triangles construction
            while (queue.Count > 0)
            {
                QuadEdge<TEdge> edge = queue.Dequeue();
                if (edge.Tag == _mesh.VisitedTagState)
                {
                    foreach (QuadEdge<TEdge> current in edge.RightEdges(CCW: false))
                    {
                        if (current.Sym.Tag == _mesh.VisitedTagState)
                        {
                            queue.Enqueue(current.Sym);
                        }
                        current.Tag = !_mesh.VisitedTagState;

                        // Clear Dual data
                        current.Rot.Origin = null;
                    }
                }
            }

            // Inverse flag to be able to traverse again at next call
            _mesh.SwitchInternalFlag();
        }

    }
}
