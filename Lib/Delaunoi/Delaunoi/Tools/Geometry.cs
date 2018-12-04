using System;
using System.Collections.Generic;
using Delaunoi.DataStructures;


namespace Delaunoi.Tools
{
    public static class Geometry
    {

        /// <summary>
        /// Compute the circumcenter of the triangle abc.
        /// </summary>
        /// <remarks>
        /// Because the expression is purely a function of difference the relative
        /// error is not influenced by the absolute coordinate. In other word error
        /// will be lower when points are closer to each other which occurs every time
        /// for points resulting for triangulation.
        /// More at https://www.ics.uci.edu/~eppstein/junkyard/circumcenter.html
        ///
        ///             | by-ay  |b-a|^2 |
        ///             | cy-ay  |c-a|^2 |
        ///   mx = ax - ------------------
        ///               | bx-ax  by-ay |
        ///             2 | cx-ax  cy-ay |
        ///
        ///             | bx-ax  |b-a|^2 |
        ///             | cx-ax  |c-a|^2 |
        ///   my = ay + ------------------
        ///               | bx-ax  by-ay |
        ///             2 | cx-ax  cy-ay |
        ///
        /// </remarks>
        public static Vec3 CircumCenter2D(Vec3 a, Vec3 b, Vec3 c)
        {
            double ByAy = b.y - a.y;
            double CyAy = c.y - a.y;
            double BxAx = b.x - a.x;
            double CxAx = c.x - a.x;
            double BAsquared = BxAx * BxAx + ByAy * ByAy;
            double CAsquared = CxAx * CxAx + CyAy * CyAy;

            double denominator = 0.5 / (BxAx * CyAy - ByAy * CxAx);
            double xRel = denominator * (ByAy * CAsquared - CyAy * BAsquared);
            double yRel = denominator * (BxAx * CAsquared - CxAx * BAsquared);

            return new Vec3(a.x - xRel, a.y + yRel, 0.0);
        }

        /// <summary>
        /// Compute SphereCircumcenter (3D).
        /// </summary>
        /// <remarks>
        ///
        ///         |c-a|^2 [(b-a)x(c-a)]x(b-a) + |b-a|^2 (c-a)x[(b-a)x(c-a)]
        /// m = a + ---------------------------------------------------------.
        ///                            2 | (b-a)x(c-a) |^2
        /// </remarks>
        public static Vec3 CircumCenter3D(Vec3 a, Vec3 b, Vec3 c)
        {
            Vec3 ca = c - a;
            Vec3 ba = b - a;

            Vec3 baca = Vec3.Cross(ba, ca);
            double invDenominator = 0.5 / baca.SquaredMagnitude;

            Vec3 numerator =  Vec3.Cross(ca.SquaredMagnitude * baca, ba) +
                              ba.SquaredMagnitude * Vec3.Cross(ca, baca);

            return a + (numerator * invDenominator);
        }

        /// <summary>
        /// Compute triangle abc centroid.
        /// </summary>
        public static Vec3 Centroid(Vec3 a, Vec3 b, Vec3 c)
        {
            return (a + b + c) * (1.0 / 3.0);
        }

        /// <summary>
        /// Compute triangle abc Incenter.
        /// </summary>
        public static Vec3 InCenter(Vec3 a, Vec3 b, Vec3 c)
        {
            double bc = Vec3.Distance(b, c);
            double ca = Vec3.Distance(c, a);
            double ab = Vec3.Distance(a, b);
            double lengthSum = bc + ca + ab;

            return (bc * a + ca * b + ab * c) * (1.0 / lengthSum);
        }


        /// <summary>
        /// Compute circle circumcenter radius of triangle abc.
        /// </summary>
        /// <remarks>
        /// Because the expression is purely a function of difference the relative
        /// error is not influenced by the absolute coordinate. In other word error
        /// will be lower when points are closer to each other which occurs every time
        /// for points resulting for triangulation.
        /// More at https://www.ics.uci.edu/~eppstein/junkyard/circumcenter.html
        ///             |b-a| |c-a| |b-c|
        ///   r  =     ------------------
        ///              | bx-ax  by-ay |
        ///            2 | cx-ax  cy-ay |
        ///
        public static double CircumCircleRadius(Vec3 a, Vec3 b, Vec3 c)
        {
            double ByAy = b.y - a.y;
            double CyAy = c.y - a.y;
            double BxAx = b.x - a.x;
            double CxAx = c.x - a.x;
            double BAsquared = BxAx * BxAx + ByAy * ByAy;
            double CAsquared = CxAx * CxAx + CyAy * CyAy;

            double denominator = 0.5 / (BxAx * CyAy - ByAy * CxAx);

            double BxCx = b.x - c.x;
            double ByCy = b.y - c.y;
            double BCsquared = BxCx * BxCx + ByCy * ByCy;

            // Costly operation but only needed after triangulation
            return denominator * Math.Sqrt(BAsquared * CAsquared * BCsquared);
        }

        /// <summary>
        /// Compute sphere circumcenter radius of triangle abc.
        /// </summary>
        /// <remarks>
        ///     |                                                           |
        ///     | |c-a|^2 [(b-a)x(c-a)]x(b-a) + |b-a|^2 (c-a)x[(b-a)x(c-a)] |
        ///     |                                                           |
        /// r = -------------------------------------------------------------,
        ///                          2 | (b-a)x(c-a) |^2
        ///
        /// </remarks>
        public static double CircumSphereRadius(Vec3 a, Vec3 b, Vec3 c)
        {
            Vec3 ca = c - a;
            Vec3 ba = b - a;

            Vec3 baca = Vec3.Cross(ba, ca);
            double invDenominator = 0.5f / baca.SquaredMagnitude;

            Vec3 numerator =  Vec3.Cross(ca.SquaredMagnitude * baca, ba) +
                              ba.SquaredMagnitude * Vec3.Cross(ca, baca);

            return Math.Sqrt(numerator.SquaredMagnitude) * invDenominator;
        }

        /// <summary>
        /// Compute the circumcenter of the triangle abc and its circumcercle radius.
        /// Result store in a key pair/value where key is radius and value circumcenter position.
        /// </summary>
        /// <remarks>
        /// Because the expression is purely a function of difference the relative
        /// error is not influenced by the absolute coordinate. In other word error
        /// will be lower when points are closer to each other which occurs every time
        /// for points resulting for triangulation.
        /// More at https://www.ics.uci.edu/~eppstein/junkyard/circumcenter.html
        ///
        ///             | by-ay  |b-a|^2 |
        ///             | cy-ay  |c-a|^2 |
        ///   mx = ax - ------------------
        ///               | bx-ax  by-ay |
        ///             2 | cx-ax  cy-ay |
        ///
        ///             | bx-ax  |b-a|^2 |
        ///             | cx-ax  |c-a|^2 |
        ///   my = ay + ------------------
        ///               | bx-ax  by-ay |
        ///             2 | cx-ax  cy-ay |
        ///
        ///             |b-a| |c-a| |b-c|
        ///   r  =     ------------------
        ///              | bx-ax  by-ay |
        ///            2 | cx-ax  cy-ay |
        ///
        /// </remarks>
        public static KeyValuePair<Vec3, double> CircumCenterAndRadius2D(Vec3 a, Vec3 b, Vec3 c)
        {
            // Center
            double ByAy = b.y - a.y;
            double CyAy = c.y - a.y;
            double BxAx = b.x - a.x;
            double CxAx = c.x - a.x;
            double BAsquared = BxAx * BxAx + ByAy * ByAy;
            double CAsquared = CxAx * CxAx + CyAy * CyAy;

            double denominator = 0.5 / (BxAx * CyAy - ByAy * CxAx);
            double xRel = denominator * (ByAy * CAsquared - CyAy * BAsquared);
            double yRel = denominator * (BxAx * CAsquared - CxAx * BAsquared);

            // Radius
            double BxCx = b.x - c.x;
            double ByCy = b.y - c.y;
            double BCsquared = BxCx * BxCx + ByCy * ByCy;

            // Costly operation but only needed after triangulation
            double r = denominator * Math.Sqrt(BAsquared * CAsquared * BCsquared);

            return new KeyValuePair<Vec3, double>(new Vec3(a.x - xRel, a.y + yRel, 0.0), r);
        }

        /// <summary>
        /// Return true if point pt are in the circumcercle of the three other ones (a, b, c).
        /// </summary>
        /// <remarks>
        /// Computes the following determinant (assume a CCW abc triangle).
        /// after reduction to a 3x3 matrix.
        ///   | ax  ay  ax² + ay²  1 |
        ///   | bx  by  bx² + by²  1 | > 0
        ///   | cx  cy  cx² + cy²  1 |
        ///   | dx  dy  dx² + dy²  1 |
        /// </remarks>
        public static bool InCircumCercle2D(Vec3 pt, Vec3 a, Vec3 b, Vec3 c)
        {
            double Ax = a.x - pt.x;
            double Ay = a.y - pt.y;

            double Bx = b.x - pt.x;
            double By = b.y - pt.y;

            double Cx = c.x - pt.x;
            double Cy = c.y - pt.y;

            double AxAy = Ax * Ax + Ay * Ay;
            double BxBy = Bx * Bx + By * By;
            double CxCy = Cx * Cx + Cy * Cy;

            // Assuming CCW triangle abc and a point pt
            // Determinant > 0  => pt inside circle
            // Determinant < 0  => pt outside circle
            // Determinant == 0 => pt on circle
            return (AxAy * (Bx * Cy - By * Cx) -
                    BxBy * (Ax * Cy - Ay * Cx) +
                    CxCy * (Ax * By - Ay * Bx)) > 0.0;
        }

        /// <summary>
        /// Return true if point pt projection are in the circumcercle of the
        /// three other ones (a, b, c).
        /// </summary>
        /// <remarks>
        /// More at :
        ///   - https://gamedev.stackexchange.com/questions/60630/how-do-i-find-the-circumcenter-of-a-triangle-in-3d
        ///   - https://math.stackexchange.com/questions/544946/determine-if-projection-of-3d-point-onto-plane-is-within-a-triangle
        ///   - https://math.stackexchange.com/questions/4322/check-whether-a-point-is-within-a-3d-triangle
        /// </remarks>
        public static bool InCircumCercle3D(Vec3 pt, Vec3 a, Vec3 b, Vec3 c)
        {
            Vec3 ba = b - a;
            Vec3 ca = c - a;
            Vec3 pa = pt - a;

            Vec3 normal = Vec3.Cross(ba, ca);
            double normalMagInv = 1.0 / normal.Magnitude;

            // Note : if one coordinate is 0 and total lower than 1 p lies on
            // a triangle segment.
            double gamma = Vec3.Dot(Vec3.Cross(ba, pa), normal) * normalMagInv;
            double beta = Vec3.Dot(Vec3.Cross(pa, ca), normal) * normalMagInv;

            // Inside if barycentric coordinates sum is lower or equal than 1
            // Because last coordinate is computed based on gamma and beta
            // it's not necessary to compute it, just test sum of first two
            // does not exceed 1 already.
            return (gamma + beta) >= 1.0;

        }

        /// <summary>
        /// Return true if point pt are in the circumsphere of the fourth other ones
        /// (a, b, c, d).
        /// </summary>
        public static bool InCircumSphere3D(Vec3 pt, Vec3 a, Vec3 b, Vec3 c, Vec3 d)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return true if the points (a, b, c) are in a counter clockwise order, else false.
        /// </summary>
        /// <remarks>
        /// Computes the following determinant after reduction to a 2x2 matrix.
        ///   | ax  ay  1 |
        ///   | bx  by  1 | > 0
        ///   | cx  cy  1 |
        /// </remarks>
        public static bool Ccw(Vec3 a, Vec3 b, Vec3 c)
        {
            return ((a.x - c.x) * (b.y - c.y) - (a.y - c.y) * (b.x - c.x)) > 0.0;
        }

        /// <summary>
        /// Return true if pt is on the right of edge segment
        /// (edge.Destination -> edge.Origin)
        /// </summary>
        public static bool RightOf<T>(Vec3 pt, QuadEdge<T> edge)
        {
            return Ccw(pt, edge.Destination, edge.Origin);
        }

        /// <summary>
        /// Return true if pt is on the left of edge segment
        /// (edge.Origin -> edge.Destination)
        /// </summary>
        public static bool LeftOf<T>(Vec3 pt, QuadEdge<T> edge)
        {
            return Ccw(pt, edge.Origin, edge.Destination);
        }

        /// <summary>
        /// Test if 3 points are on the same line.
        /// </summary>
        public static bool AlmostColinear(Vec3 a, Vec3 b, Vec3 c, double epsilon=0.0000001)
        {
            Vec3 ac = a - c;
            Vec3 bc = b - c;

            return Math.Abs(ac.x * bc.y - ac.y * bc.x) < epsilon;
        }

        /// <summary>
        /// Test if two points are almost equals.
        /// </summary>
        public static bool AlmostEquals(Vec3 a, Vec3 b, double epsilon=0.0000001)
        {
            return Vec3.DistanceSquared(a, b) < epsilon;
        }
    }

}
