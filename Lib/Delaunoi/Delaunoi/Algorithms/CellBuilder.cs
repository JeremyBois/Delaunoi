using System;
using System.Collections.Generic;
using System.Linq;

// using UnityEngine;


namespace Delaunoi.Algorithms
{
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;


    /// <summary>
    /// Geometrical operation used to construct a cell based on Delaunay triangulation.
    /// </summary>
    public enum CellConfig
    {
        Voronoi,
        Centroid,
        InCenter,
        RandomUniform,
        RandomNonUniform,
        OrthoCenter
    }


    /// <summary>
    /// An abstraction over triangulation result to construct and extract cells.
    /// </summary>
    public class CellBuilder<T>
    {
        private GuibasStolfi<T>      _triangulation;
        private IEnumerable<Cell<T>> _context;

        public CellBuilder(GuibasStolfi<T> triangulation)
        {
            this._triangulation = triangulation;
        }

        /// <summary>
        /// Return all cell based on Delaunay triangulation. Vertices at infinity
        /// are define based on radius parameter. It should be large enough to avoid
        /// some circumcenters (finite voronoi vertices) to be further on.
        /// Using <see cref="CellConfig.RandomUniform"/> and <see cref="CellConfig.RandomNonUniform"/>
        /// can leads to intersection with cell area constructed for points at infinity because
        /// they are calculated based on <see cref="CellConfig.Voronoi"/> strategy.
        /// </summary>
        /// <remarks>
        /// Each cell is yield just after their construction. Then it's neighborhood
        /// is not guarantee to be constructed. To manipulate neighborhood first cast
        /// to a List or an array before performing operations.
        /// </remarks>
        /// <param name="triangulation">Triangulation used to extract cells.</param>
        /// <param name="cellType">Define the type of cell used for extraction.</param>
        /// <param name="radius">Distance used to construct site that are at infinity.</param>
        /// <param name="useZCoord">If true cell center compute in R^3 else in R^2 (matter only if voronoi).</param>
        public CellBuilder<T> Cells(CellConfig cellType, double radius, bool useZCoord=true)
        {
            switch (cellType)
            {
                case CellConfig.Centroid:
                    _context = _triangulation.Exportcells(radius, Geometry.Centroid);
                    break;
                case CellConfig.Voronoi:
                    if (useZCoord)
                    {
                        _context = _triangulation.Exportcells(radius, Geometry.CircumCenter3D);
                    }
                    else
                    {
                        _context = _triangulation.Exportcells(radius, Geometry.CircumCenter2D);
                    }
                    break;
                case CellConfig.InCenter:
                    _context = _triangulation.Exportcells(radius, Geometry.InCenter);
                    break;
                case CellConfig.RandomUniform:
                    _context = _triangulation.Exportcells(radius, RandGen.TriangleUniform);
                    break;
                case CellConfig.RandomNonUniform:
                    _context = _triangulation.Exportcells(radius, RandGen.TriangleNonUniform);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return this;
        }

        /// <summary>
        /// Keep only cells with at least one boundary site at infinity.
        /// </summary>
        public CellBuilder<T> AtInfinity()
        {
            _context = _context.Where(x => x.Reconstructed);
            return this;
        }

        /// <summary>
        /// Keep only cells on the convex hull boundary.
        /// </summary>
        public CellBuilder<T> Bounds()
        {
            _context = _context.Where(x => x.IsOnBounds);
            return this;
        }

        /// <summary>
        /// Keep only cells on the convex hull boundary with finite cell bounds.
        /// </summary>
        public CellBuilder<T> FiniteBounds()
        {
            _context = _context.Where(x => (x.IsOnBounds && !x.Reconstructed));
            return this;
        }

        /// <summary>
        /// Keep only cells with finite area.
        /// </summary>
        public CellBuilder<T> Finite()
        {
            _context = _context.Where(x => !x.Reconstructed);
            return this;
        }

        /// <summary>
        /// Keep only cells inside the convex hull excluding boundary cells.
        /// </summary>
        public CellBuilder<T> InsideHull()
        {
            _context = _context.Where(x => !x.IsOnBounds);
            return this;
        }

        /// <summary>
        /// Keep cells where their center is at a distance from <paramref name="origin"/>
        /// smaller than <paramref name="radius"/>.
        /// </summary>
        public CellBuilder<T> CenterCloseTo(Vec3 origin, double radius)
        {
            double radiusSq = Math.Pow(radius, 2.0);
            _context = _context.Where(x => Vec3.DistanceSquared(origin, x.Center) < radiusSq);
            return this;
        }

        /// <summary>
        /// Keep cells where each of its boundary sites is at a distance from
        /// <paramref name="origin"/> smaller than <paramref name="radius"/>.
        /// </summary>
        /// <param name="origin">Origin used as reference for distance calculation.</param>
        /// <param name="radius">Minimal distance from origin.</param>
        public CellBuilder<T> CloseTo(Vec3 origin, double radius)
        {
            double radiusSq = Math.Pow(radius, 2.0);
            _context = _context.Where(x => IsCloseTo(x, origin, radiusSq));
            return this;
        }

        /// <summary>
        /// Keep cells living inside a box defined by an <paramref name="origin"/>
        /// and its size (<paramref name="extends"/>).
        /// <param name="origin">Origin used for the box.</param>
        /// <param name="origin">Size of the box.</param>
        /// </summary>
        public CellBuilder<T> Inside(Vec3 origin, Vec3 extends)
        {
            _context = _context.Where(x => IsInBounds(x, origin, extends));
            return this;
        }

        /// <summary>
        /// Can be used to use fluent extensions from LINQ (<see cref="System.Linq"/>).
        /// </summary>
        public IEnumerable<Cell<T>> Select()
        {
            return _context;
        }

        /// <summary>
        /// Build a list of cell which accounts for previous operations.
        /// </summary>
        public List<Cell<T>> ToList()
        {
            return _context.ToList();
        }

        /// <summary>
        /// Build an array of cell which accounts for previous operations.
        /// </summary>
        public Cell<T>[] ToArray()
        {
            return _context.ToArray();
        }


    // PRIVATE
        private bool IsCloseTo(Cell<T> cell, Vec3 origin, double radiusSq)
        {
            foreach (Vec3 pos in cell.Bounds)
            {
                if (Vec3.DistanceSquared(origin, pos) > radiusSq)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsInBounds(Cell<T> cell, Vec3 origin, Vec3 extends)
        {
            Vec3 upBounds = origin + extends;
            foreach (Vec3 pos in cell.Bounds)
            {
                if (pos.x > upBounds.x || pos.y > upBounds.y || pos.z > upBounds.z)
                {
                    return false;
                }
                else if (pos.x < origin.x || pos.y < origin.y || pos.z < origin.z)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
