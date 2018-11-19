using System;
using System.Collections.Generic;
using System.Linq;

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

// FIELDS

        private Vec3[]     _points;
        private QuadEdge<T>[] _leftRightEdges;
        private bool       visitedTagState;


// CONSTRUCTORS

        private GuibasStolfi()
        {
            visitedTagState = false;
        }

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


// PROPERTIES

        /// <summary>
        /// Return the leftmost edge if triangulation already done, else null.
        /// </summary>
        public QuadEdge<T> LeftMostEdge
        {
            get {return _leftRightEdges[0];}
        }

        /// <summary>
        /// Return the rightmost edge if triangulation already done, else null.
        /// </summary>
        public QuadEdge<T> RightMostEdge
        {
            get {return _leftRightEdges[1];}
        }


// METHODS (PUBLIC)

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
            _leftRightEdges = Triangulate(_points);

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
        /// Construct triangles based on Delaunay triangulation.
        /// </summary>
        public List<Vec3> ExportDelaunay()
        {
            // Container for triangles vertices
            var triangles = new List<Vec3>();
            // FIFO
            var queue = new Queue<QuadEdge<T>>();

            // Start at the far right
            QuadEdge<T> first = _leftRightEdges[1];
            queue.Enqueue(first);

            // Find the segment with no vertex on its right
            // starting from the far right vertex
            while (Geometry.RightOf(first.Oprev.Destination, first))
            {
                first = first.Oprev;
            }

            // Visit all edge of the convex hull in CW order and
            // add opposite edges to queue
            foreach (QuadEdge<T> hullEdge in first.RightEdges(CCW:false))
            {
                // Enqueue same edge but with opposite direction
                queue.Enqueue(hullEdge.Sym);
                hullEdge.Tag = !visitedTagState;
            }

            // Convex hull now closed. Start triangles construction
            while (queue.Count > 0)
            {
                QuadEdge<T> edge = queue.Dequeue();
                if (edge.Tag == visitedTagState)
                {
                    foreach (QuadEdge<T> current in edge.RightEdges(CCW:false))
                    {
                        triangles.Add(current.Origin);
                        if (current.Sym.Tag == visitedTagState)
                        {
                            queue.Enqueue(current.Sym);
                        }
                        current.Tag = !visitedTagState;
                    }
                }
            }

            // Inverse flag to be able to traverse again at next call
            visitedTagState = !visitedTagState;
            return triangles;
        }

        /// <summary>
        /// Locate the closest with respect to following constraints:
        ///   - pos is on the line of returned edge
        ///   - pos is inside the left face of returned edge
        /// </summary>
        public QuadEdge<T> Locate(Vec3 pos)
        {
            QuadEdge<T> edge = RightMostEdge;
            while (true)
            {
                if (pos == edge.Origin || pos == edge.Destination)
                {
                    return edge;
                }
                else if (Geometry.RightOf(pos, edge))
                {
                    edge = edge.Sym;
                }
                else if (Geometry.LeftOf(pos, edge.Onext))
                {
                    edge = edge.Onext;
                }
                else if (Geometry.LeftOf(pos, edge.Dprev))
                {
                    edge = edge.Dprev;
                }
                else
                {
                    // Previous triangle edge
                    QuadEdge<T> otherE = edge.Lprev;
                    if (Geometry.AlmostColinear(pos, otherE.Origin,
                                                otherE.Destination))
                    {
                        return otherE;
                    }


                    // Next triangle edge
                    otherE = edge.Lnext;
                    if (Geometry.AlmostColinear(pos, otherE.Origin,
                                                otherE.Destination))
                    {
                        return otherE;
                    }

                    return edge;
                }
            }
        }


// METHODS (PRIVATE)

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
            QuadEdge<T> first = _leftRightEdges[1];

            // Find the segment with no vertex on its left
            // starting from the far right vertex
            while (Geometry.LeftOf(first.Onext.Destination, first))
            {
                first = first.Onext;
            }

            // Visit all edge of the convex hull to compute dual vertices
            // at infinity by looping in a CW order over edges with same left face.
            foreach (QuadEdge<T> hullEdge in first.LeftEdges(CCW:false))
            {
                // Start construction of a new cell
                voronoiCells.Add(new Cell(hullEdge.Origin, true));
                Cell currentCell = voronoiCells.Last();
                // First infinite voronoi vertex
                if (hullEdge.Rot.Destination == null)
                {
                    hullEdge.Rot.Destination = ConstructAtInfinity(hullEdge.Sym,
                                                                   radius,
                                                                   centerCalculator);
                }
                currentCell.Add(hullEdge.Rot.Destination);

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
                        }
                    }

                    if (current.Sym.Tag == visitedTagState)
                    {
                        queue.Enqueue(current.Sym);
                    }
                    current.Tag = !visitedTagState;
                    currentCell.Add(current.Rot.Origin);
                }
            }

            // Convex hull now closed --> Construct bounded voronoi cells
            while (queue.Count > 0)
            {
                QuadEdge<T> edge = queue.Dequeue();

                if (edge.Tag == visitedTagState)
                {
                    // Construct a new cell
                    voronoiCells.Add(new Cell(edge.Origin, true));
                    Cell currentCell = voronoiCells.Last();
                    foreach (QuadEdge<T> current in edge.EdgesFrom(CCW:false))
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
                    }
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
        /// Radius should be choose higher enough to avoid neighbor voronoi points
        /// to be further on. A good guest is the maximal distance between non infinite
        /// voronoi vertices or five times the maximal distance between delaunay vertices.
        /// </summary>
        /// <remarks>
        /// If primalEdge.RotSym.Origin is null, then its value is computed first
        /// using CircumCenter2D because this vertex is always inside a delaunay triangle.
        /// </remarks>
        private Vec3 ConstructAtInfinity(QuadEdge<T> primalEdge, double radius,
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
        /// Return true if Geometry.RightOf(edge.Destination, baseEdge) is true.
        /// </summary>
        private bool IsValid(QuadEdge<T> edge, QuadEdge<T> baseEdge)
        {
            // Geometry.Ccw called directly.
            return Geometry.Ccw(edge.Destination, baseEdge.Destination, baseEdge.Origin);
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

            // SPLITTING
            // Divide them halfsize recursively
            int halfLength = (pts.Length + 1) / 2;
            QuadEdge<T>[] left = Triangulate(pts.Take(halfLength).ToArray());
            QuadEdge<T>[] right = Triangulate(pts.Skip(halfLength).ToArray());


            // MERGING
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
            // 1) Creation of the baseEdge quad edge (See Fig.21)
            QuadEdge<T> baseEdge = QuadEdge<T>.Connect(rdi.Sym, ldi);
            if (ldi.Origin == ldo.Origin)
            {
                ldo = baseEdge.Sym;
            }
            if (rdi.Origin == rdo.Origin)
            {
                rdo = baseEdge;
            }

            // 2) Rising bubble (See Fig. 22)
            bool upperCommonTangentNotFound = true;
            while (upperCommonTangentNotFound)
            {
                // Locate the first L point (lCand.Destination) to be encountered
                // by the rising bubble, and delete L edges out of baseEdge.Destination
                // that fail the circle test.
                QuadEdge<T> lCand = baseEdge.Sym.Onext;
                if (IsValid(lCand, baseEdge))
                {
                    while (Geometry.InCircumCercle2D(lCand.Onext.Destination,
                                          baseEdge.Destination, baseEdge.Origin, lCand.Destination))
                    {
                        nextCand = lCand.Onext;
                        QuadEdge<T>.Delete(lCand);
                        lCand = nextCand;
                    }
                }
                // Same for the right part (Symetrically)
                QuadEdge<T> rCand = baseEdge.Oprev;
                if (IsValid(rCand, baseEdge))
                {
                    while (Geometry.InCircumCercle2D(rCand.Oprev.Destination,
                                          baseEdge.Destination, baseEdge.Origin, rCand.Destination))
                    {
                        nextCand = rCand.Oprev;
                        QuadEdge<T>.Delete(rCand);
                        rCand = nextCand;
                    }
                }
                // Upper common tangent is baseEdge
                if (!IsValid(lCand, baseEdge) && !IsValid(rCand, baseEdge))
                {
                    upperCommonTangentNotFound = false;
                }
                // Construct new cross edge between left and right
                // The next cross edge is to be connected to either lcand.Dest or rCand.Dest
                // If both are valid, then choose the appropriate one using the
                // Geometry.InCircumCercle2D test
                else if (!IsValid(lCand, baseEdge) ||
                            (
                                IsValid(rCand, baseEdge) &&
                                Geometry.InCircumCercle2D(rCand.Destination,
                                                          lCand.Destination,
                                                          lCand.Origin,
                                                          rCand.Origin)
                            )
                        )
                {
                    // Cross edge baseEdge added from rCand.Destination to basel.Destination
                    baseEdge = QuadEdge<T>.Connect(rCand, baseEdge.Sym);
                }
                else
                {
                    // Cross edge baseEdge added from baseEdge.Origin to lCand.Destination
                    baseEdge = QuadEdge<T>.Connect(baseEdge.Sym, lCand.Sym);
                }
            }
            return new QuadEdge<T>[] {ldo, rdo};
        }
    }
}
