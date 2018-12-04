using System;
using System.Linq;
using System.Collections.Generic;


namespace Delaunoi.DataStructures
{
    /// <summary>
    /// Abstraction of a triangulation (primal graph) dual graph element.
    /// </summary>
    public class Face<T>
    {
        private static int nextID = 0;
        private int _id;

        private QuadEdge<T> _primal;
        private bool        _onBounds;
        private bool        _reconstructed;

        /// <summary>
        /// Construct a face abstraction based on a <paramref name="primal"/> edge.
        /// </summary>
        /// <param name="primal">Delaunay edge with Origin as face center.</param>
        /// <param name="onBounds">Mark face as a face on the faces bounds.</param>
        /// <param name="reconstructed">Must be true if face has a site at infinity.</param>
        public Face(QuadEdge<T> primal, bool onBounds, bool reconstructed)
        {
            this._primal  = primal;
            this._onBounds = onBounds;
            this._reconstructed = reconstructed;

            this._id = nextID++;
        }

        /// <summary>
        /// Get primal Edge with Origin as face center.
        /// </summary>
        public QuadEdge<T> PrimalEdge
        {
            get {return _primal;}
        }

        /// <summary>
        /// True if face primal edge origin is on the convex hull of Delaunay triangulation.
        /// </summary>
        public bool IsOnBounds
        {
            get {return _onBounds;}
        }

        /// <summary>
        /// True if face has been constructed because of a missing primal site (at infinity).
        /// </summary>
        public bool Reconstructed
        {
            get {return _reconstructed;}
        }

        /// <summary>
        /// Get face center
        /// </summary>
        public Vec3 Center
        {
            get {return _primal.Origin;}
        }

        public int ID
        {
            get {return _id;}
        }

        /// <summary>
        /// Get face boundary points.
        /// </summary>
        public IEnumerable<Vec3> Bounds
        {
            get
            {
                // Handle missing edge at infinity
                if (_onBounds)
                {
                    yield return _primal.Rot.Destination;
                }

                // Bounds
                foreach (Vec3 site in _primal.FaceLeftVertices())
                {
                    yield return site;
                }
            }
        }

        /// <summary>
        /// Get all points. Center is first one followed by face bounds.
        /// </summary>
        public IEnumerable<Vec3> Points
        {
            get
            {
                // Center
                yield return _primal.Origin;
                // Bounds
                foreach (Vec3 site in Bounds)
                {
                    yield return site;
                }
            }
        }
    }
}
