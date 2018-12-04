using System;
using System.Collections.Generic;


namespace Delaunoi.Generators
{
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;


    public static class SphereSampler
    {
       public static readonly double InvGoldenRatio = 2.0 / (Math.Sqrt(5.0) + 1);
	   public static readonly double TwoPi = Math.PI * 2.0;

       /// <summary>
       /// Return an iterator of points in 3D euclidean space representing an uniform
       /// distribution on points on a sphere.
       /// </summary>
       /// <remarks>
       /// Based on the paper `Spherical Fibonacci Mapping, Benjamin Keinert et al.
       /// Journal ACM Transactions on Graphics, Volume 34 Issue 6, November 2015`.
       /// </remarks>
       public static IEnumerable<Vec3> FibonnaciSphere(int number, double radius=1.0)
       {
            // Multiplication cheaper than division
            double oneDivN = 1.0 / (double)number;

            for (uint ind = 0; ind < number; ind++)
            {
                // Keep only decimal part of x
                double x = InvGoldenRatio * ind;
                double phi = TwoPi * (x - Math.Truncate(x));

                double theta = Math.Acos(1.0 - (2.0 * (double)ind + 1.0) * oneDivN);

                yield return radius * Geometry.SphericalToEuclidean(phi, theta);
            }
       }
    }
}
