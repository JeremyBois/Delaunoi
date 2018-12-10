namespace Delaunoi
{
    /// <summary>
    /// Geometrical operation used to construct a face based on a Delaunay triangulation.
    /// </summary>
    public enum FaceConfig
    {
        Voronoi,
        Centroid,
        InCenter,
        RandomUniform,
        RandomNonUniform
    }
}
