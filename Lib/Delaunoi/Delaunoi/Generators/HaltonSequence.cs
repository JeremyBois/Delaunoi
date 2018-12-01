using System;
using Delaunoi.Tools;
using Delaunoi.DataStructures;

namespace Delaunoi.Generators
{
    /// <summary>
    /// Generation of pseudo random points based on a low-discrepancy sequence
    /// describe by Halton in "On the efficiency of certain quasi-random sequences
    /// of points in evaluating multi-dimensional integrals, Numerische Mathematik,
    /// December 1960, Volume 2, Issue 1, pp 84–90" (doi == 10.1007/BF01386213)
    /// </summary>
    public static class HaltonSequence
    {
        static public Vec3 Halton2D(int n, int base1, int base2)
        {
            return new Vec3 (VanDerCorput(n, base1), VanDerCorput(n, base2));
        }

        static public Vec3 Halton3D(int n, int base1, int base2, int base3)
        {
            return new Vec3 (VanDerCorput(n, base1),
                             VanDerCorput(n, base2),
                             VanDerCorput(n, base3));
        }

        public static float VanDerCorput(int n, int b)
        {
            // Using double leads to errors (too much precision ??)
            float toReturn = 0;

            int[] bitArray = GetBitsArray(n, b);

            for (int k = 0; k < bitArray.Length; k++)
            {
                toReturn += bitArray[k] * (float)Math.Pow(b, - (k + 1));
            }

            return toReturn;
        }

        static int[] GetBitsArray(int i, int b)
        {
            string s = Helper.ChangeBase(i,b);

            int[] bitArray = new int[s.Length];

            for (int j = 0; j < s.Length; j++)
            {
                bitArray[j] =  s[s.Length - j - 1] - '0';
            }

            return bitArray;
        }
    }
}
