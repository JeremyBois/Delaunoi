using System;


namespace Delaunay.Tools
{

    using Delaunay.DataStructures;


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
        /// Return a random number in a disk
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
        public static Vec3 InCircle(double scale=1.0, double shift=0.0)
        {
            double r = shift + Math.Sqrt(NextDouble()) * scale;
            double theta = NextDouble(TWOPI);

            return new Vec3(r * Math.Cos(theta), r * Math.Sin(theta));
        }
    }
}

