using System;


namespace Delaunoi.DataStructures
{
    /// <summary>
    /// A vector class used as data container inside a QuadEdge.
    /// Vec3 support sorting (first using x value, then y one).
    /// </summary>
    public class Vec3: IComparable<Vec3>
    {
        private readonly double _x, _y, _z;

        public Vec3(double x, double y)
        {
            this._x = x;
            this._y = y;
            this._z = 0.0;
        }

        public Vec3(double x, double y, double z)
        {
            this._x = x;
            this._y = y;
            this._z = z;
        }

        public Vec3(float x, float y, float z)
        {
            this._x = x;
            this._y = y;
            this._z = z;
        }

        public double x
        {
            get {return _x;}
        }

        public double y
        {
            get {return _y;}
        }

        public double z
        {
            get {return _z;}
        }

        /// <summary>
        /// Implement the IComparable interface.
        /// </summary>
        public int CompareTo(Vec3 other)
        {
            // Null values should throw an exception so
            // no need to test it, let it crash !
            // Sort using x, then y
            return (other.x == this.x) ? this.y.CompareTo(other.y)
                                       : this.x.CompareTo(other.x);
        }

        public override string ToString()
        {
            return string.Format("Vec({0}, {1}, {2})", _x, _y, _z);
        }

        public Vec3 WithMagnitude(double mag)
        {
            double ratio = mag / this.Magnitude;
            return new Vec3(_x * ratio, _y * ratio, _z * ratio);
        }

        public double SquaredMagnitude
        {
            get {return _x * _x + _y * _y + _z * _z;}
        }

        public double Magnitude
        {
            get {return Math.Sqrt(_x * _x + _y * _y + _z * _z);}
        }

        /// <summary>
        /// Return Distance squared between first and second.
        /// </summary>
        public static double DistanceSquared(Vec3 first, Vec3 second)
        {
            double x = first._x - second._x;
            double y = first._y - second._y;
            double z = first._z - second._z;
            return x * x + y * y + z * z;
        }

        /// <summary>
        /// Return Euclidean distance between first and second.
        /// </summary>
        public static double Distance(Vec3 first, Vec3 second)
        {
            return Math.Sqrt(DistanceSquared(first, second));
        }

        public static Vec3 operator +(Vec3 first, Vec3 second)
        {
            return new Vec3
                (
                    first.x + second.x,
                    first.y + second.y,
                    first.z + second.z
                );
        }

        public static Vec3 operator -(Vec3 first, Vec3 second)
        {
            return new Vec3
                (
                    first.x - second.x,
                    first.y - second.y,
                    first.z - second.z
                );
        }

        public static Vec3 operator *(Vec3 first, double scale)
        {
            return new Vec3
                (
                    first.x * scale,
                    first.y * scale,
                    first.z * scale
                );
        }

        public static Vec3 operator *(double scale, Vec3 first)
        {
            return new Vec3
                (
                    first.x * scale,
                    first.y * scale,
                    first.z * scale
                );
        }

        public static Vec3 Cross(Vec3 first, Vec3 second)
        {
            return new Vec3
                (
                    first.y * second.z - first.z * second.y,
                    first.z * second.x - first.x * second.z,
                    first.x * second.y - first.y * second.x
                );
        }

        public static Vec3 Substract(Vec3 first, Vec3 second)
        {
            return new Vec3
                (
                    first.x - second.x,
                    first.y - second.y,
                    first.z - second.z
                );
        }
    }
}

