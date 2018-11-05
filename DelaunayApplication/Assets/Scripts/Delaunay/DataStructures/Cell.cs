using System;
using System.Linq;
using System.Collections.Generic;


namespace Delaunay.DataStructures
{
    /// <summary>
    /// Abstraction of a Voronoi or Centroid cell.
    /// </summary>
    public class Cell
    {
        private static int nextID = 0;

        private int _id;
        private bool _isVoronoi;
        private double _radius;
        private List<Vec3> _points;

        public Cell(Vec3 center, bool isVoronoi=true)
        {
            this._isVoronoi = isVoronoi;
            this._points = new List<Vec3>();
            this._points.Add(center);

            _id = nextID++;
        }

        /// <summary>
        /// Get cell center
        /// </summary>
        public Vec3 Center
        {
            get {return _points[0];}
        }

        public int ID
        {
            get {return _id;}
        }

        /// <summary>
        /// Voronoi or Centroid
        /// </summary>
        public bool IsVoronoi
        {
            get {return _isVoronoi;}
        }

        /// <summary>
        /// Voronoi or Centroid
        /// </summary>
        public bool IsCentroid
        {
            get {return !_isVoronoi;}
        }

        /// <summary>
        /// Get cell boundary points.
        /// </summary>
        public Vec3[] Bounds
        {
            get {return _points.Skip(1).ToArray();}
        }

        /// <summary>
        /// Get all points. Center is first one followed by cell bounds.
        /// </summary>
        public Vec3[] Points
        {
            get {return _points.ToArray();}
        }

        /// <summary>
        /// Add a new point on the boundary.
        /// </summary>
        public void Add(Vec3 point)
        {
            _points.Add(point);
        }

        /// <summary>
        /// Compute Radius for bound point with index is ptInd.
        /// </summary>
        public double GetRadius(int ptInd)
        {
            double diffX = _points[ptInd + 1].x - _points[0].x;
            double diffY = _points[ptInd + 1].y - _points[0].y;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }
    }
}
