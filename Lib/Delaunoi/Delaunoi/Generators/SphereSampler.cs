using System;
using System.Collections.Generic;

using UnityEngine;



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
       /// distribution on points on a unit sphere.
       /// </summary>
       /// <remarks>
       /// Based on the paper `Spherical Fibonacci Mapping, Benjamin Keinert et al.
       /// Journal ACM Transactions on Graphics, Volume 34 Issue 6, November 2015`.
       /// </remarks>
       public static IEnumerable<Vec3> Fibonnaci(int number)
       {
            // Multiplication cheaper than division
            double oneDivN = 1.0 / (double)number;

            for (uint ind = 0; ind < number; ind++)
            {
                // Keep only decimal part of x
                double x = InvGoldenRatio * ind;
                double phi = TwoPi * (x - Math.Truncate(x));

                double theta = Math.Acos(1.0 - (2.0 * (double)ind + 1.0) * oneDivN);


                yield return Geometry.SphericalToEuclidean(phi, theta);
            }
       }

       /// <summary>
       /// Return an iterator of points in 3D euclidean space representing an uniform
       /// distribution on points on a unit sphere.
       /// </summary>
       /// <remarks>
       /// Based on the paper `Spherical Fibonacci Mapping, Benjamin Keinert et al.
       /// Journal ACM Transactions on Graphics, Volume 34 Issue 6, November 2015`.
       /// </remarks>
       public static IEnumerable<Vec3> Fibonnaci(int number, double jitter)
       {
            // Multiplication cheaper than division
            double oneDivN = 1.0 / (double)number;

            for (uint ind = 0; ind < number; ind++)
            {
                // Keep only decimal part of x
                double x = InvGoldenRatio * ind + jitter * RandGen.NextDouble();
                double phi = TwoPi * (x - Math.Truncate(x));

                // @Todo make it more random
                double theta = Math.Acos(1.0 - (2.0 * (double)ind + 1.0) * oneDivN);

                yield return Geometry.SphericalToEuclidean(phi, theta);
            }
       }

       /// <summary>
       /// Return an iterator of points in 3D euclidean space representing a
       /// pseudo uniform distribution of points on a unit sphere.
       /// </summary>
       /// <remarks>
       /// Mathematics convention used: Phi represent the elevation [0, PI]
       /// and Theta the azimuth [0, 2*PI].
       /// More at http://mathworld.wolfram.com/SphericalCoordinates.html
       /// </remarks>
       public static IEnumerable<Vec3> Halton(int number, int seed=110,
                                              int basePhi=2, int baseTheta=3)
       {
            for (int i = 0; i < number; i++)
            {
                Vec3 randTemp = HaltonSequence.Halton2D(seed, basePhi, baseTheta);
                double phi = 2.0 * System.Math.PI * randTemp.X;
                double theta = System.Math.Acos(2.0 * randTemp.Y - 1.0);

                yield return Geometry.SphericalToEuclidean(phi, theta);
                seed++;
            }
       }


       /// <summary>
       /// Return an iterator of points in 3D euclidean space regarding a disk
       /// sampling point distribution on a unit sphere.
       /// </summary>
       public static IEnumerable<Vec3> Poisson(int number, float radius, int maxAttempt=60)
       {
            var generator = new PoissonDisk2D(radius, (float)TwoPi, (float)Math.PI, maxAttempt);
            generator.BuildSample(number);

            foreach (Vec3 pt in generator)
            {
                yield return Geometry.SphericalToEuclidean(pt.X, pt.Y);
            }
       }
    }
}
