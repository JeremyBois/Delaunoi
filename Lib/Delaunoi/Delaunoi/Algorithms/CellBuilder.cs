using System;
using System.Collections.Generic;
using System.Linq;

// using UnityEngine;


namespace Delaunoi.Algorithms
{
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;

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

        public CellBuilder<T> AtInfinity()
        {
            _context = _context.Where(x => x.Reconstructed);
            return this;
        }

        public CellBuilder<T> Bounds()
        {
            _context = _context.Where(x => x.IsOnBounds);
            return this;
        }

        public CellBuilder<T> FiniteBounds()
        {
            _context = _context.Where(x => (x.IsOnBounds && !x.Reconstructed));
            return this;
        }

        public CellBuilder<T> InsideHull()
        {
            _context = _context.Where(x => x.IsOnBounds);
            return this;
        }

        public CellBuilder<T> CenterCloseTo(Vec3 origin, double radius)
        {
            double radiusSq = Math.Pow(radius, 2.0);
            _context = _context.Where(x => Vec3.DistanceSquared(origin, x.Center) < radiusSq);
            return this;
        }

        public CellBuilder<T> CloseTo(Vec3 origin, double radius)
        {
            double radiusSq = Math.Pow(radius, 2.0);
            _context = _context.Where(x => IsCloseTo(x, origin, radiusSq));
            return this;
        }

        public CellBuilder<T> Inside(Vec3 origin, Vec3 extends)
        {
            _context = _context.Where(x => IsInBounds(x, origin, extends));
            return this;
        }

        public IEnumerable<Cell<T>> Select()
        {
            return _context;
        }

        public List<Cell<T>> ToList()
        {
            return _context.ToList();
        }

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
