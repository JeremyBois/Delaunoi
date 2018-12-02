// using System;
// using System.Collections.Generic;
// using System.Linq;


// namespace Delaunoi.Algorithms
// {
//     using Delaunoi.DataStructures;
//     using Delaunoi.Tools;

//     public class CellConstructor
//     {

//         /// <summary>
//         /// Find correct position for a voronoi site that should be at infinite
//         /// Assume primalEdge.Rot.Origin as the vertex to compute, that is
//         /// there should be no vertex on the right of primalEdge.
//         /// Site computed is the destination of a segment in a direction normal to
//         /// the tangent vector of the primalEdge (destination - origin) with
//         /// its symetrical (primalEdge.RotSym.Origin) as origin.
//         /// Radius should be choose higher enough to avoid neighbor voronoi points
//         /// to be further on. A good guest is the maximal distance between non infinite
//         /// voronoi vertices or five times the maximal distance between delaunay vertices.
//         /// </summary>
//         /// <remarks>
//         /// If primalEdge.RotSym.Origin is null, then its value is computed first
//         /// using centerCalculator because this vertex is always inside a delaunay triangle.
//         /// </remarks>
//         private static Vec3 ConstructAtInfinity<T>(QuadEdge<T> primalEdge, double radius,
//                                                    Func<Vec3, Vec3, Vec3, Vec3> centerCalculator)
//         {
//             var rotSym = primalEdge.RotSym;

//             // Find previous voronoi site
//             if (rotSym.Origin == null)
//             {
//                 rotSym.Origin = centerCalculator(primalEdge.Origin,
//                                                  primalEdge.Destination,
//                                                  primalEdge.Onext.Destination);
//             }
//             double xCenter = rotSym.Origin.x;
//             double yCenter = rotSym.Origin.y;

//             // Compute normalized tangent of primal edge scaled by radius
//             double xTangent = primalEdge.Destination.x - primalEdge.Origin.x;
//             double yTangent = primalEdge.Destination.y - primalEdge.Origin.y;
//             double dist = Math.Sqrt(xTangent * xTangent + yTangent * yTangent);
//             xTangent /= dist;
//             yTangent /= dist;
//             xTangent *= radius;
//             yTangent *= radius;

//             // Add vertex using edge dual destination as origin
//             // in direction normal to the primal edge
//             Vec3 normal = new Vec3(xCenter - yTangent, yCenter + xTangent, rotSym.Origin.z);

//             // If new voronoi vertex is on the left of the primal edge
//             // we used the wrong normal vector --> get its opposite
//             if (Geometry.LeftOf(normal, primalEdge))
//             {
//                 normal = new Vec3(xCenter + yTangent, yCenter - xTangent, rotSym.Origin.z);
//             }
//             return normal;
//         }
//     }
// }
