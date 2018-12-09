using System;
using System.Collections.Generic;
using System.Linq;


namespace Delaunoi
{
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;


    /// <summary>
    /// 2D Delaunay triangulation based on a divide and conquer algorithm allowing incremental
    /// insertion in an already triangulated area. That is, new site can be added on the fly
    /// (Incremental) and/or each at the same time (Divide and Conquer). This approach
    /// constructs at the same time the primal (Delaunay) and the dual (Voronoi, Centroid, ...)
    /// The divide and conquer approach allows to build both, primal and dual, in O(nlog(n)) time
    /// and the insertion of a new site in O(n) time.
    /// Each directed edge contains a generic data field allowing to store
    /// whatever information you need.
    /// </summary>
    /// <remarks>
    /// It has been defined by LEONIDAS GUIBAS and JORGE STOLFI in
    /// Primitives for the Manipulation of General Subdivisions and the Computation of Voronoi Diagrams,
    /// ACM Transactions on Graphics, Vol. 4, No. 2, April 1985"
    /// </remarks>
    public class GuibasStolfi<T>
    {

// FIELDS
        protected Vec3[]          _points;
        protected QuadEdge<T>[]   _leftRightEdges;
        protected bool            _visitedTagState;

// CONSTRUCTORS

        private GuibasStolfi()
        {
            _visitedTagState = false;
        }

        /// <summary>
        /// Load an array of position to be triangulate.
        /// </summary>
        /// <param name="points">An array of points to triangulate.</param>
        /// <param name="alreadySorted">Points already sorted (base on x then y).</param>
        public GuibasStolfi(Vec3[] points, bool alreadySorted=false)
            : this()
        {
            if (!alreadySorted)
            {
                // Linq faster than Sort even when only sorting needed
                this._points = points.OrderBy(vec => vec.X)
                                     .ThenBy(vec => vec.Y)
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
            get
            {
                // Find the segment with no vertex on its left
                // starting from the far left vertex
                QuadEdge<T> boundEdge = _leftRightEdges[0];

                while (Geometry.LeftOf(boundEdge.Onext.Destination, boundEdge))
                {
                    boundEdge = boundEdge.Onext;
                }
                return boundEdge;
            }
        }

        /// <summary>
        /// Return the rightmost edge if triangulation already done, else null.
        /// </summary>
        public QuadEdge<T> RightMostEdge
        {
            get
            {
                QuadEdge<T> boundEdge = _leftRightEdges[1];
                // Find the segment with no vertex on its right
                // starting from the far right vertex
                while (Geometry.RightOf(boundEdge.Oprev.Destination, boundEdge))
                {
                    boundEdge = boundEdge.Oprev;
                }

                return boundEdge;
            }
        }

        /// <summary>
        /// Switch traversal flag.
        /// </summary>
        public bool VisitedTagState
        {
            get {return _visitedTagState;}
        }

        /// <summary>
        /// Switch traversal flag.
        /// </summary>
        public bool SwitchInternalFlag()
        {
            _visitedTagState = !_visitedTagState;
            return _visitedTagState;
        }



// METHODS (PUBLIC)

        /// <summary>
        /// Return an array of Vec3 from the triangulation.
        /// </summary>
        public virtual bool ComputeDelaunay()
        {
            // Reinit flag state
            _visitedTagState = false;

            // Cannot triangulate only one site
            if (_points.Length < 2)
            {
                return false;
            }

            // Triangulate recursively
            _leftRightEdges = Triangulate(_points);

            return true;
        }


        /// <summary>
        /// If <paramref name="pos"/> is outside the convex hull of the triangulation
        /// return edge with <paramref name="pos"/> on its left face.
        /// If inside the triangulation return null.
        /// </summary>
        /// <param name="pos">The position to locate</param>
        public QuadEdge<T> ClosestBoundingEdge(Vec3 pos)
        {
            var boundEdge = RightMostEdge;

            // No one can be larger than the larger one, right ??
            double lastDistFromPos = double.PositiveInfinity;
            bool found = false;

            // Visit all edge of the convex hull in CW order and
            // add opposite edges to queue
            foreach (QuadEdge<T> hullEdge in boundEdge.RightEdges(CCW:false))
            {
                // Find an edge from convex hull where
                if (Geometry.RightOf(pos, hullEdge))
                {
                    double distFromPos = Vec3.DistanceSquared(pos, hullEdge.Origin);

                    // Continues until we move away
                    if (distFromPos < lastDistFromPos)
                    {
                        lastDistFromPos = distFromPos;
                        found = true;
                    }
                    else
                    {
                        // Found the closest one
                        return hullEdge.Rnext.Sym;
                    }
                }
                else if (found)
                {
                    return hullEdge.Rnext.Sym;
                }
            }

            return null;
        }

        /// <summary>
        /// True if <paramref name="pos"/> inside the convex hull formed by the triangulation.
        /// </summary>
        /// <param name="pos">The position to test</param>
        public bool InsideConvexHull(Vec3 pos)
        {
            QuadEdge<T> boundEdge = RightMostEdge;
            foreach (QuadEdge<T> hullEdge in boundEdge.RightEdges(CCW:false))
            {
                if (Geometry.RightOf(pos, hullEdge))
                {
                    // Must be outside because hullEdge right face is outside
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Locate the closest with respect to following constraints:
        ///   - <paramref name="pos"/> is on the line of returned edge
        ///   - <paramref name="pos"/> is inside the left face of returned edge
        ///
        /// If site outside the convex hull Locate will loop forever looking for
        /// a corresponding edge that does not exists ... unless you first check
        /// you are in the convex hull or set <paramref name="checkBoundFirst"/> to true.
        /// </summary>
        /// <param name="pos">The position to locate</param>
        /// <param name="edge">Edge used to start locate process. Can be used to speed up search.</param>
        /// <param name="safe">If true, first check if pos in convex hull of triangulation</param>
        public QuadEdge<T> Locate(Vec3 pos, QuadEdge<T> edge=null, bool safe=false)
        {
            // Check boundary first
            if (safe)
            {
                QuadEdge<T> result = ClosestBoundingEdge(pos);
                if (result != null)
                {
                    // pos outside
                    return result;
                }
            }

            // Start somewhere if no hint
            if (edge == null)
            {
                edge = _leftRightEdges[1];
            }

            // Assume it must be inside ...
            while (true)
            {
                if (pos == edge.Origin || pos == edge.Destination)
                    return edge;
                else if (Geometry.RightOf(pos, edge))
                    edge = edge.Sym;
                else if (Geometry.LeftOf(pos, edge.Onext))
                    edge = edge.Onext;
                else if (Geometry.LeftOf(pos, edge.Dprev))
                    edge = edge.Dprev;
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

        /// <summary>
        /// Insert a new site inside an existing delaunay triangulation. New site
        /// must be inside the convex hull of previoulsy added sites.
        /// Set <paramref name="safe"/> to true to first test if new site is correct.
        /// </summary>
        /// <param name="newPos">The position to of new site</param>
        /// <param name="edge">Edge used to start locate process. Can be used to speed up search.</param>
        /// <param name="safe">If true, check if <paramref name="safe"/> inside the convex hull.</param>
        public bool InsertSite(Vec3 newPos, QuadEdge<T> edge=null, bool safe=false)
        {
            if (safe)
            {
                bool result = InsideConvexHull(newPos);
                if (!result)
                {
                    // Cannot add site not already inside the convex hull
                    return false;
                }
            }

            // Start somewhere if no hint
            if (edge == null)
            {
                edge = _leftRightEdges[1];
            }

            // Locate edge (must be inside the boundary)
            QuadEdge<T> foundE = Locate(newPos, edge, safe:false);

            // Site already triangulated
            if (Geometry.AlmostEquals(foundE.Origin, newPos) ||
                Geometry.AlmostEquals(foundE.Destination, newPos))
            {
                return false;
            }

            // On an edge ?
            if (Geometry.AlmostColinear(foundE.Origin, foundE.Destination, newPos))
            {
                var temp = foundE.Oprev;
                QuadEdge<T>.Delete(foundE);
                foundE = temp;
            }

            // Create new edge to connect new site to neighbors
            QuadEdge<T> baseE = QuadEdge<T>.MakeEdge(foundE.Origin, newPos);
            Vec3 first = baseE.Origin;
            QuadEdge<T>.Splice(baseE, foundE);
            // Up to 4 vertices if new site on an edge
            do
            {
                baseE = QuadEdge<T>.Connect(foundE, baseE.Sym);
                foundE = baseE.Oprev;

            } while (foundE.Destination != first);


            // Fill star shaped polygon and swap suspect edges
            // Adding a new point can break old condition about InCircle test
            foundE = baseE.Oprev;
            bool shouldExit = false;
            do
            {
                var tempE = foundE.Oprev;
                if (Geometry.RightOf(tempE.Destination, foundE) &&
                    Geometry.InCircumCercle2D(newPos, foundE.Origin, tempE.Destination, foundE.Destination))
                {
                    QuadEdge<T>.Swap(foundE);
                    // tempE != foundE.Oprev after swap
                    foundE = foundE.Oprev;
                }
                else if (foundE.Origin == first)
                {
                    // No more suspect edge ... exit
                    shouldExit = true;
                }
                else
                {
                    // Get next suspect edge from top to bottom
                    foundE = foundE.Onext.Lprev;
                }
            } while (!shouldExit);

            return true;
        }



        /// <summary>
        /// Return true if Geometry.RightOf(edge.Destination, baseEdge) is true.
        /// </summary>
        protected bool IsValid(QuadEdge<T> edge, QuadEdge<T> baseEdge)
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
                // Locate the first L site (lCand.Destination) to be encountered
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
