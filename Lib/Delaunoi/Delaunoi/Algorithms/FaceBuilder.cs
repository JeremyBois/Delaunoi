using System;
using System.Collections.Generic;
using System.Linq;

// using UnityEngine;


namespace Delaunoi.Algorithms
{
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;


    /// <summary>
    /// Geometrical operation used to construct a face based on Delaunay triangulation.
    /// </summary>
    public enum FaceConfig
    {
        Voronoi,
        Centroid,
        InCenter,
        RandomUniform,
        RandomNonUniform,
        OrthoCenter
    }


    /// <summary>
    /// An abstraction over triangulation result to construct and extract faces.
    /// </summary>
    public class FaceBuilder<T>
    {
        private GuibasStolfi<T>      _triangulation;
        private IEnumerable<Face<T>> _context;

        public FaceBuilder(GuibasStolfi<T> triangulation)
        {
            this._triangulation = triangulation;
        }

        /// <summary>
        /// Return all face based on Delaunay triangulation. Vertices at infinity
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
        /// <param name="triangulation">Triangulation used to extract faces.</param>
        /// <param name="faceType">Define the type of face used for extraction.</param>
        /// <param name="radius">Distance used to construct site that are at infinity.</param>
        /// <param name="useZCoord">If true face center compute in R^3 else in R^2 (matter only if voronoi).</param>
        public FaceBuilder<T> Faces(FaceConfig faceType, double radius, bool useZCoord=true)
        {
            switch (faceType)
            {
                case FaceConfig.Centroid:
                    _context = _triangulation.ExportFaces(radius, Geometry.Centroid);
                    break;
                case FaceConfig.Voronoi:
                    if (useZCoord)
                    {
                        _context = _triangulation.ExportFaces(radius, Geometry.CircumCenter3D);
                    }
                    else
                    {
                        _context = _triangulation.ExportFaces(radius, Geometry.CircumCenter2D);
                    }
                    break;
                case FaceConfig.InCenter:
                    _context = _triangulation.ExportFaces(radius, Geometry.InCenter);
                    break;
                case FaceConfig.RandomUniform:
                    _context = _triangulation.ExportFaces(radius, RandGen.TriangleUniform);
                    break;
                case FaceConfig.RandomNonUniform:
                    _context = _triangulation.ExportFaces(radius, RandGen.TriangleNonUniform);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return this;
        }

        /// <summary>
        /// Keep only faces with at least one boundary site at infinity.
        /// </summary>
        public FaceBuilder<T> AtInfinity()
        {
            _context = _context.Where(x => x.Reconstructed);
            return this;
        }

        /// <summary>
        /// Keep only faces on the convex hull boundary.
        /// </summary>
        public FaceBuilder<T> Bounds()
        {
            _context = _context.Where(x => x.IsOnBounds);
            return this;
        }

        /// <summary>
        /// Keep only faces on the convex hull boundary with finite face bounds.
        /// </summary>
        public FaceBuilder<T> FiniteBounds()
        {
            _context = _context.Where(x => (x.IsOnBounds && !x.Reconstructed));
            return this;
        }

        /// <summary>
        /// Keep only faces with finite area.
        /// </summary>
        public FaceBuilder<T> Finite()
        {
            _context = _context.Where(x => !x.Reconstructed);
            return this;
        }

        /// <summary>
        /// Keep only faces inside the convex hull excluding boundary faces.
        /// </summary>
        public FaceBuilder<T> InsideHull()
        {
            _context = _context.Where(x => !x.IsOnBounds);
            return this;
        }

        /// <summary>
        /// Keep faces where their center is at a distance from <paramref name="origin"/>
        /// smaller than <paramref name="radius"/>.
        /// </summary>
        public FaceBuilder<T> CenterCloseTo(Vec3 origin, double radius)
        {
            double radiusSq = Math.Pow(radius, 2.0);
            _context = _context.Where(x => Vec3.DistanceSquared(origin, x.Center) < radiusSq);
            return this;
        }

        /// <summary>
        /// Keep faces where each of its boundary sites is at a distance from
        /// <paramref name="origin"/> smaller than <paramref name="radius"/>.
        /// </summary>
        /// <param name="origin">Origin used as reference for distance calculation.</param>
        /// <param name="radius">Minimal distance from origin.</param>
        public FaceBuilder<T> CloseTo(Vec3 origin, double radius)
        {
            double radiusSq = Math.Pow(radius, 2.0);
            _context = _context.Where(x => IsCloseTo(x, origin, radiusSq));
            return this;
        }

        /// <summary>
        /// Keep faces living inside a box defined by an <paramref name="origin"/>
        /// and its size (<paramref name="extends"/>).
        /// <param name="origin">Origin used for the box.</param>
        /// <param name="origin">Size of the box.</param>
        /// </summary>
        public FaceBuilder<T> Inside(Vec3 origin, Vec3 extends)
        {
            _context = _context.Where(x => IsInBounds(x, origin, extends));
            return this;
        }

        /// <summary>
        /// Can be used to use fluent extensions from LINQ (<see cref="System.Linq"/>).
        /// </summary>
        public IEnumerable<Face<T>> Select()
        {
            return _context;
        }

        /// <summary>
        /// Build a list of face which accounts for previous operations.
        /// </summary>
        public List<Face<T>> ToList()
        {
            return _context.ToList();
        }

        /// <summary>
        /// Build an array of face which accounts for previous operations.
        /// </summary>
        public Face<T>[] ToArray()
        {
            return _context.ToArray();
        }


    // PRIVATE
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
