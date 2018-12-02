using System;


namespace Delaunoi.Tools
{

    using Delaunoi.DataStructures;


    public static class RandGen
    {
        private const double TWOPI = Math.PI * 2.0;

        private static Random _rand = new Random(DateTime.Now.Second);


        public static void Init(int _seedValue)
        {
            _rand = new Random(_seedValue);
        }

        public static int NextInt()
        {
            return _rand.Next();
        }

        public static int NextInt(int min, int max)
        {
            return _rand.Next(min, max);
        }

        public static int NextInt(int max)
        {
            return _rand.Next(max);
        }

        public static double NextDouble()
        {
            return _rand.NextDouble();
        }

        public static double NextDouble(double min, double max)
        {
            return min + _rand.NextDouble() * (max - min);
        }

        public static double NextDouble(double max)
        {
            return _rand.NextDouble() * max;
        }

        /// <summary>
        /// Return a random number in a disk with magnitude is radius parameter.
        /// </summary>
        /// <remarks>
        /// For this to work we must use a pdf_integration = int_0^r(PDF(r)dr) = r² / R² == 1
        /// which leads to PDF(r) = 2 * r / R² and then CDF(r) = r² / R²
        /// Compute a random number in a disk then result in : r = R * sqrt(randUniform()).
        /// So a random unit number uniformly distributed in a disk is defined with
        ///     r = sqrt(RandUniform)
        ///     theta = RandomUniform(0, 2 * Pi)
        ///     x = r * cos(theta)
        ///     x = r * sin(theta)
        /// </remarks>
        public static Vec3 InCircle(double radius)
        {
            double r = radius * Math.Sqrt(NextDouble());
            double theta = NextDouble(TWOPI);

            return new Vec3(r * Math.Cos(theta), r * Math.Sin(theta));
        }

        /// <summary>
        /// Return a random number in a disk with minimal magnitude defined
        /// by minRadius and maximal by maxRadius.
        /// </summary>
        public static Vec3 InCircle(double minRadius, double maxRadius)
        {
            double r = minRadius + (maxRadius - minRadius) * Math.Sqrt(NextDouble());
            double theta = NextDouble(TWOPI);

            return new Vec3(r * Math.Cos(theta), r * Math.Sin(theta));
        }

        /// <summary>
        /// Return a random number in a unit circle perimeter.
        /// </summary>
        public static Vec3 UnitCircle()
        {
            double theta = NextDouble(TWOPI);
            return new Vec3(Math.Cos(theta), Math.Sin(theta));
        }

        /// <summary>
        /// Compute a random point uniformly distributed inside a triangle
        /// defined by vertices a, b, c.
        /// </summary>
        /// <remarks>
        /// More at http://mathworld.wolfram.com/TrianglePointPicking.html
        /// </remarks>
        public static Vec3 TriangleUniform(Vec3 a, Vec3 b, Vec3 c)
        {
            double rand_one = NextDouble();
            double rand_two = NextDouble();

            if (rand_two + rand_one >= 1)
            {
                // Remap point to triangle interior (mirror it)
                rand_two = 1 - rand_two;
                rand_one = 1 - rand_one;
            }

            return a + rand_one * (b - a) + rand_two * (c - a);
        }

        /// <summary>
        /// Compute a random point inside a triangle defined by vertices a, b, c.
        /// Unlike <see cref="TriangleUniform"/>, more points are close to triangle
        /// centroid.
        /// </summary>
        /// <remarks>
        /// Interactive trilinear point construction at https://www.geogebra.org/m/ybS3SaRS
        /// </remarks>
        public static Vec3 TriangleNonUniform(Vec3 a, Vec3 b, Vec3 c)
        {
            // https://en.wikipedia.org/wiki/Trilinear_coordinates
            double alpha = NextDouble() * Vec3.Distance(c, b);
            double beta  = NextDouble() * Vec3.Distance(a, c);
            double gamma = NextDouble() * Vec3.Distance(b, a);
            double denominatorInv = 1.0 / (alpha + beta + gamma);

            return (a * alpha + b * beta + c * gamma) * denominatorInv;
        }
    }
}

