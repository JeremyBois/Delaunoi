using System;
using System.Collections.Generic;
using System.Collections;

namespace Delaunoi.Generators
{
    using Delaunoi.DataStructures;
    using Delaunoi.Tools;


    /// <summary>
    /// Generate a blue noise using the technique described by Robert Bridson
    /// in "Fast Poisson disk sampling in arbitrary dimensions, ACM SIGGRAPH 2007
    /// sketches, August 2007, Article No. 22" (doi == 10.1145/1278780.1278807).
    /// </summary>
    public class PoissonDisk3D: IEnumerable
    {
        private struct Coord
        {
            public Coord(int row, int col, int depth)
            {
                this.Row = row;
                this.Col = col;
                this.Depth = depth;
            }

            public int Row { get; private set; }
            public int Col { get; private set; }
            public int Depth { get; private set; }
        };


        // Maximal number of random point to try for each sample
        private readonly int _maxAttemp;
        private const int _dim = 3;

        // Each element is initialized to null
        private Vec3[,,]        _grid;
        private List<Vec3>     _activeList;
        private readonly float _cellSize, _radius, _radiusSq, _width, _height, _depth;
        private readonly int   _rows, _cols, _depths;

        private int _count;

        /// <summary>
        /// Initialize a PoissonDisk3D generator.
        /// </summary>
        /// <param name="radius">Minimal distance between samples.</param>
        /// <param name="width">Maximal x coordinate for a sample.</param>
        /// <param name="height">Maximal y coordinate for a sample.</param>
        /// <param name="maxAttemp">Maximal number of random point generation by sample.</param>
        public PoissonDisk3D(float radius, float width, float height, float depth, int maxAttemp=30)
        {
            _maxAttemp = maxAttemp;

            _cellSize = radius / (float)Math.Sqrt(_dim);

            _rows = (int)Math.Floor((height / _cellSize));
            _cols = (int)Math.Floor((width / _cellSize));
            _depths = (int)Math.Floor((depth / _cellSize));

            _radiusSq = radius * radius;
            _radius = radius;
            _width = width;
            _height = height;
            _depth = depth;
        }

        public int Count
        {
            get {return _count;}
        }

        /// <summary>
        /// Construct a set of point. Note that maximal number of point if limited
        /// due to radius parameter in a finite rectangular shape (with, height).
        /// That is, a finite maximal number of sample exists and sample size will
        /// then eventually lower that the required sampleSize as a parameter.
        /// </summary>
        /// <param name="sampleSize">Maximal number of point to sample.</param>
        public void BuildSample(int sampleSize)
        {
            // Cols represent the number of cell for the width (x coordinate)
            // Initialize will null by default
            _grid = new Vec3[_rows, _cols, _depths];
            _activeList = new List<Vec3>();
            _count = 0;

            // Add first point in the center
            Vec3 firstSample = new Vec3(_width / 2.0f, _height / 2.0f, _depth / 2.0f);
            _activeList.Add(firstSample);
            Store(firstSample);

            // Try adding sampleSize - 1 other samples
            while (_count < sampleSize && _activeList.Count > 0)
            {
                    // Select a site from the list
                    int sampleIndex = RandGen.NextInt(0, _activeList.Count);
                    Vec3 sample = _activeList[sampleIndex];

                    // Try adding a new sample
                    if (!TryAddCandidate(sample))
                    {
                        // If not possible remove sample from active list
                        _activeList.RemoveAt(sampleIndex);
                    }
            }
        }


        private void Store(Vec3 sample)
        {
            int col = (int)Math.Floor(sample.X / _cellSize);
            int row = (int)Math.Floor(sample.Y / _cellSize);
            int depth = (int)Math.Floor(sample.Z / _cellSize);
            _grid[row, col, depth] = sample;

            _count++;
        }

        private void Store(Vec3 sample, Coord coord)
        {
            _grid[coord.Row, coord.Col, coord.Depth] = sample;
            _count++;
        }

        private Coord GridCoord(Vec3 sample)
        {
            int col = (int)Math.Floor(sample.X / _cellSize);
            int row = (int)Math.Floor(sample.Y / _cellSize);
            int depth = (int)Math.Floor(sample.Z / _cellSize);

            return new Coord(row, col, depth);
        }

        private bool InBounds(Coord coord)
        {
            if (coord.Row >= 0 && coord.Row < _rows &&
                coord.Col >= 0 && coord.Col < _cols &&
                coord.Depth >= 0 && coord.Depth < _depths)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to get a sample position using grid coordinate.
        /// Can be null in two case:
        ///  1) No value yet assigned to corresponding grid cell
        ///  2) sample outside the grid
        /// </summary>
        private Vec3 Get(Coord coord)
        {
            if (coord.Row >= 0 && coord.Row < _rows &&
                coord.Col >= 0 && coord.Col < _cols &&
                coord.Depth >= 0 && coord.Depth < _depths)
            {
                return _grid[coord.Row, coord.Col, coord.Depth];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loop over adjacent cells of a sample coordinates.
        /// Can return nothing if no neighbors yet.
        /// </summary>
        private IEnumerable Adjacents(Coord coord)
        {
            // Loop over adjacents
            for (int k = -1; k < 1; k++)
            {
                for (int i = -1; i < 2 ; i++)
                {
                    for (int j = -1; j < 2 ; j++)
                    {
                        // Account for bounds limits
                        int row = coord.Row + i;
                        int col = coord.Col + j;
                        int depth = coord.Depth + k;

                        if (row >= 0 && row < _rows &&
                            col >= 0 && col < _cols &&
                            depth >= 0 && depth < _depths)
                        {
                            Vec3 adj =  _grid[row, col, depth];
                            if (null != adj)
                            {
                                yield return adj;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Try to add a new sample which respect boundaries (width, height) and
        /// that is at least at a radius distance from any other existing samples.
        /// </summary>
        private bool TryAddCandidate(Vec3 sample)
        {
            for (int attemp = 0; attemp < _maxAttemp; attemp++)
            {
                // New candidate chosen uniformly from the spherical annulus
                // between _radius and 2 x _radius.
                Vec3 cand = sample + RandGen.InSphere(_radius, _radius * 2.0f);

                Coord canCoord = GridCoord(cand);
                // Inside bounds and not in an already used cell
                if (InBounds(canCoord) && _grid[canCoord.Row, canCoord.Col, canCoord.Depth] == null)
                {
                    bool notTooClose = true;
                    foreach (Vec3 adj in Adjacents(canCoord))
                    {
                        if (Vec3.DistanceSquared(cand, adj) < _radiusSq)
                        {
                            notTooClose = false;
                            break;
                        }
                    }

                    // Found good candidate
                    if (notTooClose)
                    {
                        _activeList.Add(cand);
                        Store(cand, canCoord);
                        return true;
                    }
                }
            }
            return false;
        }


        public IEnumerator GetEnumerator()
        {
            for(int row = 0; row < _grid.GetLength(0); row++)
            {
                for(int col = 0; col < _grid.GetLength(1); col++)
                {
                    for(int depth = 0; depth < _grid.GetLength(2); depth++)
                    {
                        if (null != _grid[row, col, depth])
                        {
                            yield return _grid[row, col, depth];
                        }
                    }
                }
            }
        }
    }
}
