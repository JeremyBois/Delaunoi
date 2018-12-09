using System;
using System.Collections.Generic;

namespace Delaunoi.Interfaces
{
    using Delaunoi.DataStructures;


    public interface IFluent<T>
    {
        IEnumerable<T> Collection();

        IFluent<T> ForEach(Func<T, T> selector);

        List<T> ToList();

        T[] ToArray();
    }

    public interface IFluentExtended<T>
    {
        IEnumerable<T> Collection();

        IFluentExtended<T> ForEach(Func<T, T> selector);

        List<T> ToList();

        T[] ToArray();

        IFluentExtended<T> AtInfinity();

        IFluentExtended<T> Bounds();

        IFluentExtended<T> FiniteBounds();

        IFluentExtended<T> Finite();

        IFluentExtended<T> InsideHull();

        IFluentExtended<T> CenterCloseTo(Vec3 origin, double radius);

        IFluentExtended<T> CloseTo(Vec3 origin, double radius);

        IFluentExtended<T> Inside(Vec3 origin, Vec3 extends);
    }
}
