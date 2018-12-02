

namespace Delaunoi.Algorithms
{
    /// <summary>
    /// Geometrical operation used to construct a cell based on Delaunay triangulation.
    /// </summary>
    public enum CellConfig
    {
        Voronoi,
        Centroid,
        InCenter,
        RandomUniform,
        RandomNonUniform,
        OrthoCenter
    }

}
