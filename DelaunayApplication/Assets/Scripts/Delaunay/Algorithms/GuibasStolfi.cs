using System;
using System.Collections.Generic;
using System.Linq;

// using UnityEngine;


namespace Delaunay.Algorithms
{
    using Delaunay.DataStructures;
    using Delaunay.Tools;


    /// <summary>
    /// Delaunay triangulation based on a divide and conquer algorithm.
    /// It has been defined by LEONIDAS GUIBAS and JORGE STOLFI.
    /// in Primitives for the Manipulation of General Subdivisions and the Computation of Voronoi Diagrams,
    /// ACM Transactions on Graphics, Vol. 4, No. 2, April 1985"
    /// </summary>
    public class GuibasStolfi<T>
    {
        private Vec3[]     _points;
        private QuadEdge<T>[] _convexHulls;
        private bool       visitedTagState;

        // // Can be used to store aditional information
        // private int edgeCount = 0;
        // private int edgeFirstID = 0;

        /// <summary>
        /// Load an array of position to be triangulate.
        /// </summary>
        public GuibasStolfi(Vec3[] points, bool alreadySorted=false)
            : this()
        {
            if (!alreadySorted)
            {
                // Linq faster than Sort even when only sorting needed
                this._points = points.OrderBy(vec => vec.x)
                                     .ThenBy(vec => vec.y)
                                     .ToArray();
            }
            else
            {
                this._points = points;
            }
        }

        private GuibasStolfi()
        {
            visitedTagState = false;
        }

        /// <summary>
        /// Return two QuadEdges:
        ///   - [0] --> CCW convex hull QuadEdge<T> out of the leftmost vertex
        ///   - [1] --> CW  convex hull QuadEdge<T> out of the rightmost vertex
        /// Will be an array of null if triangulation not yet done.
        /// </summary>
        public QuadEdge<T>[] ConvexHulls
        {
            get {return _convexHulls;}
        }

        /// <summary>
        /// Return true if Geometry.RightOf(edge.Destination, base1) is true.
        /// </summary>
        private bool IsValid(QuadEdge<T> edge, QuadEdge<T> base1)
        {
            // Geometry.Ccw called directly.
            return Geometry.Ccw(edge.Destination, base1.Destination, base1.Origin);
        }


        /// <summary>
        /// Return an array of Vec3 from the triangulation.
        /// </summary>
        public bool ComputeDelaunay()
        {
            // Reinit flag state
            visitedTagState = false;

            // Cannot triangulate only one point
            if (_points.Length < 2)
            {
                return false;
            }

            // Triangulate recursively
            _convexHulls = Triangulate(_points);

            return true;
        }

        /// <summary>
        /// Return all cell based on Delaunay triangulation. Vertices at infinity
        /// are define based on radius parameter. It should be large enough to avoid
        /// some circumcenters (finite voronoi vertices) to be further on.
        /// </summary>
        /// <param name="radius">Distance used to construct point that are at infinity.</param>
        /// <param name="useZCoord">If true cell center compute in R^3 else in R^2 (matter only if voronoi).</param>
        public List<Cell> ExportCells(CellConfig cellType, double radius, bool useZCoord=false)
        {
            switch (cellType)
            {
                case CellConfig.Centroid:
                    return Exportcells(radius, Geometry.Centroid);
                case CellConfig.Voronoi:
                    if (useZCoord)
                    {
                        return Exportcells(radius, Geometry.CircumCenter3D);
                    }
                    else
                    {
                        return Exportcells(radius, Geometry.CircumCenter2D);
                    }
                case CellConfig.InCenter:
                    return Exportcells(radius, Geometry.InCenter);
            }

            throw new NotImplementedException();

        }

        /// <summary>
        /// Construct voronoi cell based on Delaunay triangulation. Vertices at infinity
        /// are define based on radius parameter. It should be large enough to avoid
        /// some circumcenters (finite voronoi vertices) to be further on.
        /// </summary>
        /// <remarks>
        /// Using delegate to implement strategy pattern.
        /// </remarks>
        /// <param name="radius">Distance used to construct point that are at infinity.</param>
        private List<Cell> Exportcells(double radius, Func<Vec3, Vec3, Vec3, Vec3> centerCalculator)
        {
            // Container for vorFaces vertices
            var voronoiCells = new List<Cell>();

            // FIFO
            var queue = new Queue<QuadEdge<T>>();

            // Start at the far right
            QuadEdge<T> first = _convexHulls[1];

            // Find the segment with no vertex on its left
            // starting from the far right vertex
            while (Geometry.LeftOf(first.Onext.Destination, first))
            {
                first = first.Onext;
            }

            // Visit all edge of the convex hull in CCW order to compute dual vertices
            // at infinity. Also add symetrical edges to queue.
            QuadEdge<T> current = first;
            bool convexHullNotClosed = true;
            while (convexHullNotClosed)
            {
                QuadEdge<T> cellFirst = current;

                // Start construction of a new cell
                voronoiCells.Add(new Cell(current.Origin, true));
                Cell currentCell = voronoiCells.Last();
                // First infinite voronoi vertex
                if (current.Rot.Destination == null)
                {
                    current.Rot.Destination = ConstructVoronoiAtInfinity(current.Sym,
                                                                         radius,
                                                                         centerCalculator);
                }
                currentCell.Add(current.Rot.Destination);

                // Add other vertices by looping over current origin in CW order (Oprev)
                do
                {
                    if (current.Rot.Origin == null)
                    {
                        // Delaunay edge on the boundary
                        if (Geometry.LeftOf(current.Oprev.Destination, current))
                        {
                            current.Rot.Origin = ConstructVoronoiAtInfinity(current,
                                                                            radius,
                                                                            centerCalculator);
                        }
                        else
                        {
                            current.Rot.Origin = centerCalculator(current.Origin,
                                                                  current.Destination,
                                                                  current.Oprev.Destination);
                        }
                    }

                    if (current.Sym.Tag == visitedTagState)
                    {
                        queue.Enqueue(current.Sym);
                    }
                    current.Tag = !visitedTagState;
                    currentCell.Add(current.Rot.Origin);
                    current = current.Oprev;

                } while (cellFirst != current);

                // Go to previous segment with same left face (CCW order)
                current = current.Lprev;

                if (current == first)
                {
                    convexHullNotClosed = false;
                }
            }

            // Convex hull now closed --> Construct bounded voronoi cells
            while (queue.Count > 0)
            {
                QuadEdge<T> edge = queue.Dequeue();

                if (edge.Tag == visitedTagState)
                {
                    // Construct a new cell
                    current = edge;
                    voronoiCells.Add(new Cell(current.Origin, true));
                    Cell currentCell = voronoiCells.Last();
                    do
                    {
                        if (current.Rot.Origin == null)
                        {
                            current.Rot.Origin = centerCalculator(current.Origin,
                                                                  current.Destination,
                                                                  current.Oprev.Destination);
                        }
                        if (current.Sym.Tag  == visitedTagState)
                        {
                            queue.Enqueue(current.Sym);
                        }

                        current.Tag = !visitedTagState;
                        currentCell.Add(current.Rot.Origin);
                        current = current.Oprev;
                    }
                    while (current != edge);
                }
            }

            // Inverse flag to be able to traverse again at next call
            visitedTagState = !visitedTagState;
            return voronoiCells;
        }

        /// <summary>
        /// Find correct position for a voronoi point that should be at infinite
        /// Assume primalEdge.Rot.Origin as the vertex to compute, that is
        /// there should be no vertex on the right of primalEdge.
        /// Point computed is the destination of a segment in a direction normal to
        /// the tangent vector of the primalEdge (destination - origin) with
        /// its symetrical (primalEdge.RotSym.Origin) as origin.
        /// Radius should be choose higher enough to avoid neighbor voronoi point
        /// to be further on. A good guest is the maximal distance between non infinite
        /// voronoi vertices or five times the maximal distance between delaunay vertices.
        /// </summary>
        /// <remarks>
        /// If primalEdge.RotSym.Origin is null, then its value is computed first
        /// using CircumCenter2D because this vertex is always inside a delaunay triangle.
        /// </remarks>
        private Vec3 ConstructVoronoiAtInfinity(QuadEdge<T> primalEdge, double radius,
                                                Func<Vec3, Vec3, Vec3, Vec3> centerCalculator)
        {
            // Find previous voronoi point
            if (primalEdge.RotSym.Origin == null)
            {
                primalEdge.RotSym.Origin = centerCalculator(primalEdge.Origin,
                                                            primalEdge.Destination,
                                                            primalEdge.Onext.Destination);
            }
            double xCenter = primalEdge.RotSym.Origin.x;
            double yCenter = primalEdge.RotSym.Origin.y;

            // Compute normalized tangent of primal edge scaled by radius
            double xTangent = primalEdge.Destination.x - primalEdge.Origin.x;
            double yTangent = primalEdge.Destination.y - primalEdge.Origin.y;
            double dist = Math.Sqrt(xTangent * xTangent + yTangent * yTangent);
            xTangent /= dist;
            yTangent /= dist;
            xTangent *= radius;
            yTangent *= radius;

            // Add vertex using edge dual destination as origin
            // in direction normal to the primal edge
            Vec3 normal = new Vec3(xCenter - yTangent, yCenter + xTangent);

            // If new voronoi vertex is on the left of the primal edge
            // we used the wrong normal vector --> get its opposite
            if (Geometry.LeftOf(normal, primalEdge))
            {
                normal = new Vec3(xCenter + yTangent, yCenter - xTangent);
            }
            return normal;
        }


        /// <summary>
        /// Construct triangles based on Delaunay triangulation.
        /// </summary>
        public List<Vec3> ExportDelaunayTriangles()
        {
            // Container for triangles vertices
            var triangles = new List<Vec3>();
            // FIFO
            var queue = new Queue<QuadEdge<T>>();

            // Start at the far right
            QuadEdge<T> first = _convexHulls[1];
            queue.Enqueue(first);

            // Find the segment with no vertex on its right
            // starting from the far right vertex
            while (Geometry.RightOf(first.Oprev.Destination, first))
            {
                first = first.Oprev;
            }

            // Visit all edge of the convex hull in CCW order and
            // add opposite edges to queue
            QuadEdge<T> current = first;
            bool convexHullNotClosed = true;
            while (convexHullNotClosed)
            {
                // Enqueue same edge but with opposite direction
                queue.Enqueue(current.Sym);
                current.Tag = !visitedTagState;
                current = current.Rprev;

                if (current == first)
                {
                    convexHullNotClosed = false;
                }
            }

            // Convex hull now closed. Start triangles construction
            while (queue.Count > 0)
            {
                QuadEdge<T> edge = queue.Dequeue();
                if (edge.Tag == visitedTagState)
                {
                    current = edge;
                    do
                    {
                        triangles.Add(current.Origin);
                        if (current.Sym.Tag == visitedTagState)
                        {
                            queue.Enqueue(current.Sym);
                        }

                        current.Tag = !visitedTagState;
                        current = current.Rprev;
                    }
                    while (current != edge);
                }
            }

            // Inverse flag to be able to traverse again at next call
            visitedTagState = !visitedTagState;
            return triangles;
        }


        /// <summary>
        /// Triangulate the set of points using a divide and conquer approach.
        /// </summary>
        private QuadEdge<T>[] Triangulate(Vec3[] pts)
        {
            QuadEdge<T> a, b, c, nextCand;

            // Only two points -> One edge
            if (pts.Length == 2)
            {
                a = QuadEdge<T>.MakeEdge(pts[0], pts[1]);
                return new QuadEdge<T>[] {a, a.Sym};

            }
            // Only tree points
            else if (pts.Length == 3)
            {
                a = QuadEdge<T>.MakeEdge(pts[0], pts[1]);
                b = QuadEdge<T>.MakeEdge(pts[1], pts[2]);
                QuadEdge<T>.Splice(a.Sym, b);

                // Closing triangle
                if (Geometry.Ccw(pts[0], pts[1], pts[2]))
                {
                    c = QuadEdge<T>.Connect(b, a);
                    return new QuadEdge<T>[] {a, b.Sym};
                }
                else if (Geometry.Ccw(pts[0], pts[2], pts[1]))
                {
                    c = QuadEdge<T>.Connect(b, a);
                    return new QuadEdge<T>[] {c.Sym, c};
                }
                else
                {
                    // Points are collinear
                    return new QuadEdge<T>[] {a, b.Sym};
                }
            }

            // More than 3 points
            // Divide them into half calling recursively Triangulate
            int halfLength = (pts.Length + 1) / 2;
            QuadEdge<T>[] left = Triangulate(pts.Take(halfLength).ToArray());
            QuadEdge<T>[] right = Triangulate(pts.Skip(halfLength).ToArray());

            // From left to right
            QuadEdge<T> ldo = left[0];
            QuadEdge<T> ldi = left[1];
            QuadEdge<T> rdi = right[0];
            QuadEdge<T> rdo = right[1];

            // Compute lower common tangent to be able to merge both triangulations
            bool crossEdgeNotFound = true;
            while (crossEdgeNotFound)
            {
                if (Geometry.LeftOf(rdi.Origin, ldi))
                {
                    ldi = ldi.Lnext;
                }
                else if (Geometry.RightOf(ldi.Origin, rdi))
                {
                    rdi = rdi.Rprev;
                }
                else
                {
                    crossEdgeNotFound = false;
                }
            }

            // Start merging
            // 1) Creation of the base1 quad edge (See Fig.21)
            QuadEdge<T> base1 = QuadEdge<T>.Connect(rdi.Sym, ldi);
            if (ldi.Origin == ldo.Origin)
            {
                ldo = base1.Sym;
            }
            if (rdi.Origin == rdo.Origin)
            {
                rdo = base1;
            }

            // 2) Rising bubble (See Fig. 22)
            bool upperCommonTangentNotFound = true;
            while (upperCommonTangentNotFound)
            {
                // Locate the first L point (lCand.Destination) to be encountered
                // by the rising bubble, and delete L edges out of base1.Destination
                // that fail the circle test.
                QuadEdge<T> lCand = base1.Sym.Onext;
                if (IsValid(lCand, base1))
                {
                    while (Geometry.InCircumCercle2D(lCand.Onext.Destination,
                                          base1.Destination, base1.Origin, lCand.Destination))
                    {
                        nextCand = lCand.Onext;
                        QuadEdge<T>.Delete(lCand);
                        lCand = nextCand;
                    }
                }
                // Same for the right part (Symetrically)
                QuadEdge<T> rCand = base1.Oprev;
                if (IsValid(rCand, base1))
                {
                    while (Geometry.InCircumCercle2D(rCand.Oprev.Destination,
                                          base1.Destination, base1.Origin, rCand.Destination))
                    {
                        nextCand = rCand.Oprev;
                        QuadEdge<T>.Delete(rCand);
                        rCand = nextCand;
                    }
                }
                // Upper common tangent is base1
                if (!IsValid(lCand, base1) && !IsValid(rCand, base1))
                {
                    upperCommonTangentNotFound = false;
                }
                // Construct new cross edge between left and right
                // The next cross edge is to be connected to either lcand.Dest or rCand.Dest
                // If both are valid, then choose the appropriate one using the
                // Geometry.InCircumCercle2D test
                else if (!IsValid(lCand, base1) ||
                            (
                                IsValid(rCand, base1) &&
                                Geometry.InCircumCercle2D(rCand.Destination,
                                                          lCand.Destination,
                                                          lCand.Origin,
                                                          rCand.Origin)
                            )
                        )
                {
                    // Cross edge base1 added from rCand.Destination to basel.Destination
                    base1 = QuadEdge<T>.Connect(rCand, base1.Sym);
                }
                else
                {
                    // Cross edge base1 added from base1.Origin to lCand.Destination
                    base1 = QuadEdge<T>.Connect(base1.Sym, lCand.Sym);
                }
            }
            return new QuadEdge<T>[] {ldo, rdo};
        }
    }
}
