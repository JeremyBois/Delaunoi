using System;
using System.Collections;
using System.Collections.Generic;


namespace Delaunoi.DataStructures
{
    using Delaunoi.Tools;


    /// <summary>
    /// Implementation of the QuadEdge<T> structure (an edge algebra) define by LEONIDAS GUIBAS and JORGE STOLFI
    /// in Primitives for the Manipulation of General Subdivisions and the Computation of Voronoi Diagrams,
    /// ACM Transactions on Graphics, April 1985, Vol. 4, No. 2" (doi == 10.1145/282918.282923)
    /// Graphical version of class properties (corresponding to Fig.3).
    ///   - ■ Represent primal subdivisions (Delaunay)
    ///   - ○ Represent dual subdivisions (Voronoi)
    ///
    ///           XXXXXXX                         XXXXXXX
    ///          XX     XX                       XX     XX
    ///          X       X Next (CCW)            X       X Previous (CW)
    ///          X      XX                       XX      X
    ///           ►                                     ◄
    ///    .          |          .         .          |          .
    ///    .          |          .         .          |          .
    ///    .          |          .         .          |          .
    /// -------◄------■-----◄--------   -------►----■-----►--------
    ///    .  Lnext   |   Dnext  .         .  Dprev   |   Rprev  .
    ///    .          |          .         .          |          .
    ///    .          |          .         .          |          .
    ///    .          ▼ Sym      .         .          |          .
    ///    .          |          .         .          |          .
    ///    .          |          .         .          |          .
    ///    ○...............◄.....○         ○...►..................○
    ///    .          |    Rot   .         .   Rot-1  |          .
    ///    .          |          .         .          |          .
    ///    .          ▲          .         .          ▲          .
    ///    .          | this(e)  .         .          | this(e)  .
    ///    .          |          .         .          |          .
    ///    .          |          .         .          |          .
    /// -------◄------■-----◄--------   -------►----■-----►--------
    ///    .   Onext  |    Rnext .         .   Lprev  |    Oprev .
    ///    .          |          .         .          |          .
    ///    .          |          .         .          |          .
    ///
    /// </summary>
    public class QuadEdge<T>
    {
        // Avoid the necessity to store already visited edges during traversal
        public bool Tag;

        // Geometry data without meaning to the topology
        private Vec3 _origin;

        // Go to the dual world ...
        private QuadEdge<T> _rot;
        // Next QuadEdge in CCW order with same origin
        private QuadEdge<T> _oNext;

        // Can be used to store additional information outside the edge
        private static int nextID;
        private readonly int _id;
        private T _data;


        /// <summary>
        /// Construct an edge.
        /// </summary>
        public QuadEdge(QuadEdge<T> oNext, QuadEdge<T> rot, Vec3 origin)
        {
            this._oNext = oNext;
            this._rot = rot;
            this._origin = origin;

            this.Tag = false;

            _id = QuadEdge<T>.nextID++;
        }

        /// <summary>
        /// Construct a group of 4 edges {e[0], e[1], e[2], e[3]} where e[0] represent the canonical
        /// representative to which edge e belongs. This allows to represent and edge
        /// by dividing it into 4 edges.
        /// For more See 2.2 Duality (Definition 2.1.) and 5.BASIC TOPOLOGICAL OPERATORS in the article.
        /// </summary>
        public static QuadEdge<T> MakeEdge(Vec3 origin, Vec3 destination)
        {
            // Primal
            QuadEdge<T> qE0 = new QuadEdge<T>(null, null, origin);
            // Dual
            QuadEdge<T> qE1 = new QuadEdge<T>(null, null, null);
            // Primal
            QuadEdge<T> qE2 = new QuadEdge<T>(null, null, destination);
            // Dual
            QuadEdge<T> qE3 = new QuadEdge<T>(null, null, null);

            // Onext ring of the primal (delaunay)
            // See Fig.5 (c)
            qE0._oNext = qE0;
            qE2._oNext = qE2;

            // Onext ring of the Dual (Voronoi)
            // See Fig.5 (c)
            qE1._oNext = qE3;
            qE3._oNext = qE1;

            // Create connection between subdivision (allow to access Primal and Dual)
            // Implicitly set qE0.Destination = qE2.Origin (Destination == Rot.Rot.Origin)
            qE0._rot = qE1;
            qE1._rot = qE2;
            qE2._rot = qE3;
            qE3._rot = qE0;

            // Return the canonical representative.
            return qE0;
        }

        /// <summary>
        /// This operation affects the two edge rings a.Origin and b.Origin and, independently,
        /// the two edge rings a.Left and b.Left. In each case,
        ///   - (a) if the two rings are distinct, Splice will combine them into one
        ///   - (b) if the two are exactly the same ring, Splice will break it in two separate pieces
        ///   - (c) if the two are the same ring taken with opposite orientations, Splice will Flip (and reverse the order)
        ///         of a segment of that ring
        /// Note: Splice is its own inverse and calling twice in a row revert it back to origin.
        /// Note: Splice does not affect Rot neither Origin and Destination that have no topological meaning.
        /// </summary>
        /// <param name="a">First QuadEdge<T></param>
        /// <param name="b">Second QuadEdge<T></param>
        public static void Splice(QuadEdge<T> a, QuadEdge<T> b)
        {
            QuadEdge<T> alpha = a._oNext._rot;
            QuadEdge<T> beta = b._oNext._rot;

            // Swap them all !
            Helper.Swap<QuadEdge<T>>(ref a._oNext, ref b._oNext);
            Helper.Swap<QuadEdge<T>>(ref alpha._oNext, ref beta._oNext);
        }

        /// <summary>
        /// Connect two edges together leading to a.Destination --> b.Origin
        /// </summary>
        public static QuadEdge<T> Connect(QuadEdge<T> a, QuadEdge<T> b)
        {
            // Connect a to b
            QuadEdge<T> edge = MakeEdge(a.Destination, b.Origin);

            // See 6. TOPOLOGICAL OPERATORS FOR DELAUNAY DIAGRAMS
            Splice(edge, a.Lnext);
            Splice(edge.Sym, b);

            return edge;
        }

        /// <summary>
        /// Disconnect edge from the structure. Opposite operation of Connect.
        /// </summary>
        public static void Delete(QuadEdge<T> edge)
        {
            Splice(edge, edge.Oprev);
            Splice(edge.Sym, edge.Sym.Oprev);
        }

        /// <summary>
        /// Swap an edge. Used to remove non delaunay triangulation.
        /// </summary>
        /// <remarks>
        ///
        ///    /|\              / \
        ///   / | \            /   \
        ///  /  |  \          /     \
        ///  \  |  /   --->   \-----/
        ///   \ | /            \   /
        ///    \|/              \ /
        ///
        ///
        /// </remarks>
        public static void Swap(QuadEdge<T> edge)
        {
            QuadEdge<T> a = edge.Oprev;
            QuadEdge<T> b = edge.Sym.Oprev;

            // Disconnect edge
            Splice(edge, a);
            Splice(edge.Sym, b);

            // Reconnect edge
            Splice(edge, a.Lnext);
            Splice(edge.Sym, b.Lnext);

            // Update pointer
            edge._origin = a.Destination;
            edge.Destination = b.Destination;
        }


    // Properties
        /// <summary>
        /// Represent the data store at edge. No topological meaning.
        /// </summary>
        public Vec3 Origin
        {
            get {return this._origin;}
            set {this._origin = value;}
        }

        /// <summary>
        /// Represent the data of the edge with same orientation
        /// but opposite direction of `this`. No topological meaning.
        /// </summary>
        public Vec3 Destination
        {
            get {return this.Sym._origin;}
            set {this.Sym._origin = value;}
        }

        /// <summary>
        /// Rotated version of `this` (dual of `this`) directed from `this` Right to `this` Left.
        /// </summary>
        public QuadEdge<T> Rot
        {
            get {return this._rot;}
        }

        /// <summary>
        /// Represent the symetrical of `this`.
        /// Same undirected edge with same orientation but opposite direction.
        /// </summary>
        public QuadEdge<T> Sym
        {
            get {return this._rot._rot;}
        }

        /// <summary>
        /// Represent the symetrical of the dual of `this`.
        /// Same undirected edge with same orientation but opposite direction.
        /// </summary>
        public QuadEdge<T> RotSym
        {
            get {return this._rot.Sym;}
        }

        /// <summary>
        /// Previous edge of `this` with same origin (rotation in CW order).
        /// More in the ASCII picture of class summary.
        /// </summary>
        public QuadEdge<T> Oprev
        {
            get {return this._rot._oNext._rot;}
        }

        /// <summary>
        /// Previous edge of `this` with same left face. (rotation in CW order).
        /// More in the ASCII picture of class summary.
        /// </summary>
        public QuadEdge<T> Lprev
        {
            get {return this._oNext.Sym;}
        }

        /// <summary>
        /// Previous edge of `this` with same right face. (rotation in CW order).
        /// More in the ASCII picture of class summary.
        /// </summary>
        public QuadEdge<T> Rprev
        {
            get {return this.Sym._oNext;}
        }

        /// <summary>
        /// Previous edge of `this` with same destination. (rotation in CW order).
        /// More in the ASCII picture of class summary.
        /// </summary>
        public QuadEdge<T> Dprev
        {
            get {return this.RotSym._oNext.RotSym;}
        }

        /// <summary>
        /// Next edge of `this` with same origin. (rotation in CCW order).
        /// More in the ASCII picture of class summary.
        /// </summary>
        public QuadEdge<T> Onext
        {
            get {return this._oNext;}
        }

        /// <summary>
        /// Next edge of `this` with same left face. (rotation in CCW order).
        /// More in the ASCII picture of class summary.
        /// </summary>
        public QuadEdge<T> Lnext
        {
            get {return this.RotSym._oNext._rot;}
        }

        /// <summary>
        /// Next edge of `this` with same right face. (rotation in CCW order).
        /// More in the ASCII picture of class summary.
        /// </summary>
        public QuadEdge<T> Rnext
        {
            get {return this._rot._oNext.RotSym;}
        }

        /// <summary>
        /// Next edge of `this` with same destination. (rotation in CCW order).
        /// More in the ASCII picture of class summary.
        /// </summary>
        public QuadEdge<T> Dnext
        {
            get {return this.Sym._oNext.Sym;}
        }

        /// <summary>
        /// Get edge unique id.
        /// </summary>
        public int ID
        {
            get {return _id;}
        }

        /// <summary>
        /// Read or attach some external information to this edge.
        /// </summary>
        public T Data
        {
            get {return _data;}
            set {_data = value;}
        }

    // Helpers enumerables used to abstract structure complexity

        /// <summary>
        /// Iterate over edges with same RIGHT face (default to CW order)
        /// starting from this.
        /// </summary>
        /// <param name="CCW">Select in which order vertices should be returned.</param>
        public IEnumerable<QuadEdge<T>> RightEdges(bool CCW=false)
        {
            QuadEdge<T> current = this;
            do
            {
                yield return current;
                current = CCW ? current.Rnext : current.Rprev;

            } while (current != this);
        }

        /// <summary>
        /// Iterate over edges with same LEFT face (default to CW order)
        /// starting from this.
        /// </summary>
        /// <param name="CCW">Select in which order vertices should be returned.</param>
        public IEnumerable<QuadEdge<T>> LeftEdges(bool CCW=false)
        {
            QuadEdge<T> current = this;
            do
            {
                yield return current;
                current = CCW ? current.Lnext : current.Lprev;

            } while (current != this);
        }

        /// <summary>
        /// Iterate over edges with same RIGHT face (default to CW order)
        /// starting from this and return their origin.
        /// </summary>
        /// <param name="CCW">Select in which order vertices should be returned.</param>
        public IEnumerable<Vec3> RightVertices(bool CCW=false)
        {
            QuadEdge<T> current = this;
            do
            {
                yield return current.Origin;
                current = CCW ? current.Rnext : current.Rprev;

            } while (current != this);
        }

        /// <summary>
        /// Iterate over edges with same LEFT face (default to CW order)
        /// starting from this and return their origin.
        /// </summary>
        /// <param name="CCW">Select in which order vertices should be returned.</param>
        public IEnumerable<Vec3> LeftVertices(bool CCW=false)
        {
            QuadEdge<T> current = this;
            do
            {
                yield return current.Origin;
                current = CCW ? current.Lnext : current.Lprev;

            } while (current != this);
        }

        /// <summary>
        /// Iterate over every edges sharing the same origin (default to CW order)
        /// </summary>
        /// <param name="CCW">Select in which order edges should be returned.</param>
        public IEnumerable<QuadEdge<T>> EdgesFrom(bool CCW=false)
        {
            QuadEdge<T> current = this;
            do
            {
                yield return current;
                current = CCW ? current.Onext : current.Oprev;

            } while (current != this);
        }

        /// <summary>
        /// Iterate over every edge destination sharing the same origin (default to CW order)
        /// </summary>
        /// <param name="CCW">Select in which order vertices should be returned.</param>
        public IEnumerable<Vec3> DestinationsFrom(bool CCW=false)
        {
            QuadEdge<T> current = this;
            do
            {
                yield return current.Destination;
                current = CCW ? current.Onext : current.Oprev;

            } while (current != this);
        }

        /// <summary>
        /// Iterate over every edges forming a cell around this edge origin
        /// (default to CW order) assuming each edge cell share the same left face.
        /// </summary>
        /// <param name="CCW">Select in which order edges should be returned.</param>
        public IEnumerable<QuadEdge<T>> CellEdges(bool CCW=false)
        {
            QuadEdge<T> current = this.Rot;
            var first = current;
            do
            {
                yield return current;
                current = CCW ? current.Lnext : current.Lprev;

            } while (current != first);
        }

        /// <summary>
        /// Iterate over every vertices forming a cell around this edge origin
        /// (default to CW order) assuming each edge cell share the same left face.
        /// </summary>
        /// <param name="CCW">Select in which order vertices should be returned.</param>
        public IEnumerable<Vec3> CellVertices(bool CCW=false)
        {
            QuadEdge<T> current = this.Rot;
            var first = current;
            do
            {
                yield return current.Origin;
                current = CCW ? current.Lnext : current.Lprev;

            } while (current != first);
        }
    }
}

