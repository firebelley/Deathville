using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Deathville.Environment
{
    public class LevelGenerator : Node
    {
        private const int CHUNK_TILE_COUNT = 32;
        private const int TILE_SIZE = 16;
        private const float NOISE_XSCALE = 25f;
        private const float NOISE_YSCALE = 60f;

        public Vector2 PlayerSpawnPosition { get; private set; }

        [Export]
        private int _minLevelPathLength = 3;
        [Export]
        private int _maxLevelPathLength = 5;
        [Export]
        private float _pathDirectionChangePercent = .25f;
        [Export]
        private int _minChunkWidth = 2;
        [Export]
        private int _maxChunkWidth = 4;
        [Export]
        private int _minChunkHeight = 2;
        [Export]
        private int _maxChunkHeight = 4;
        [Export]
        private NodePath _playerPath;

        private readonly Vector2[] _fourDirections = new Vector2[] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };
        private RandomNumberGenerator _rng = new RandomNumberGenerator();
        private OpenSimplexNoise _noise = new OpenSimplexNoise();

        private struct LevelPathCell
        {
            public Vector2 Position;
            public Vector2 Direction;

            public LevelPathCell(Vector2 position, Vector2 direction)
            {
                Position = position;
                Direction = direction;
            }
        }

        private class Chunk
        {
            public PathChunkArea PathChunkArea;
            public Vector2 PositionInArea;
            public Vector2 GlobalPosition;
        }

        private class PathChunkArea
        {
            public LevelPathCell LevelPathCell;
            public int HorizontalChunkCount;
            public int VerticalChunkCount;
            public Vector2 ChunkOffset;
            public Dictionary<Vector2, Chunk> PositionToChunk = new Dictionary<Vector2, Chunk>();
        }

        public override void _Ready()
        {
            _rng.Randomize();
            var noiseTex = GetNode<Sprite>("Sprite").Texture as NoiseTexture;
            _noise = noiseTex.Noise;
            _noise.Seed = _rng.RandiRange(0, int.MaxValue);
        }

        public void Generate()
        {
            var path = GetLevelPath();

            var areas = new List<PathChunkArea>();
            foreach (var cell in path)
            {
                areas.Add(GenerateAreaForLevelPathCell(cell));
            }

            OffsetAreas(areas);
            var allChunks = AddChunksToAreas(areas);
            var boundingArea = GetBoundingArea(areas);
            FillBoundingArea(allChunks, boundingArea);
            SecondPass();
        }

        private List<LevelPathCell> GetLevelPath()
        {
            var path = new List<LevelPathCell>();
            var numPathCells = _rng.RandiRange(_minLevelPathLength, _maxLevelPathLength);
            for (int i = 0; i < numPathCells; i++)
            {
                if (i == 0)
                {
                    path.Add(GetFirstPathPoint());
                }
                else
                {
                    path.Add(GetNextPathPoint(path[i - 1]));
                }
            }
            return path;
        }

        private LevelPathCell GetFirstPathPoint()
        {
            return new LevelPathCell(Vector2.Zero, ChooseDirection(Vector2.Zero));
        }

        private LevelPathCell GetNextPathPoint(LevelPathCell prevCell)
        {
            var directionChange = _rng.Randf();
            var direction = prevCell.Direction;
            if (directionChange <= _pathDirectionChangePercent)
            {
                direction = ChooseDirection(direction);
            }
            return new LevelPathCell(prevCell.Position + direction, direction);
        }

        private Vector2 ChooseDirection(Vector2 excludeDirection)
        {
            var i = _rng.RandiRange(0, 3);
            if (_fourDirections[i] == excludeDirection)
            {
                i = Mathf.Wrap(i + 1, 0, 3);
            }
            return _fourDirections[i];
        }

        private PathChunkArea GenerateAreaForLevelPathCell(LevelPathCell levelPathCell)
        {
            var area = new PathChunkArea();
            area.HorizontalChunkCount = _rng.RandiRange(_minChunkWidth, _maxChunkWidth);
            area.VerticalChunkCount = _rng.RandiRange(_minChunkHeight, _maxChunkHeight);
            area.LevelPathCell = levelPathCell;
            return area;
        }

        private void OffsetAreas(List<PathChunkArea> areas)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                if (i == 0) continue;
                var area = areas[i];
                var rootArea = areas[i - 1];

                if (rootArea.LevelPathCell.Direction == Vector2.Up)
                {
                    AlignAreasX(rootArea, area);
                    area.ChunkOffset.y = rootArea.ChunkOffset.y - area.VerticalChunkCount;
                }
                else if (rootArea.LevelPathCell.Direction == Vector2.Down)
                {
                    AlignAreasX(rootArea, area);
                    area.ChunkOffset.y = rootArea.ChunkOffset.y + rootArea.VerticalChunkCount;
                }
                else if (rootArea.LevelPathCell.Direction == Vector2.Left)
                {
                    AlignAreasY(rootArea, area);
                    area.ChunkOffset.x = rootArea.ChunkOffset.x - area.HorizontalChunkCount;
                }
                else if (rootArea.LevelPathCell.Direction == Vector2.Right)
                {
                    AlignAreasY(rootArea, area);
                    area.ChunkOffset.x = rootArea.ChunkOffset.x + rootArea.HorizontalChunkCount;
                }
            }
        }

        private void AlignAreasX(PathChunkArea rootChunkArea, PathChunkArea toAlignChunkArea)
        {
            toAlignChunkArea.ChunkOffset.x = rootChunkArea.ChunkOffset.x;
            toAlignChunkArea.ChunkOffset.x += _rng.RandiRange(-(toAlignChunkArea.HorizontalChunkCount - 1), rootChunkArea.HorizontalChunkCount - 1);
        }

        private void AlignAreasY(PathChunkArea rootChunkArea, PathChunkArea toAlignChunkArea)
        {
            toAlignChunkArea.ChunkOffset.y = rootChunkArea.ChunkOffset.y;
            toAlignChunkArea.ChunkOffset.y += _rng.RandiRange(-(toAlignChunkArea.VerticalChunkCount - 1), rootChunkArea.VerticalChunkCount - 1);
        }

        private Dictionary<Vector2, Chunk> AddChunksToAreas(IEnumerable<PathChunkArea> pathChunkAreas)
        {
            var allChunks = new Dictionary<Vector2, Chunk>();
            foreach (var area in pathChunkAreas)
            {
                CreateAreaChunks(area);
                foreach (var chunk in area.PositionToChunk.Values)
                {
                    // don't add duplicates in the case of overlapping areas
                    if (!allChunks.ContainsKey(chunk.GlobalPosition))
                    {
                        allChunks[chunk.GlobalPosition] = chunk;
                    }
                }
            }
            return allChunks;
        }

        private void CreateAreaChunks(PathChunkArea pathChunkArea)
        {
            for (int i = 0; i < pathChunkArea.HorizontalChunkCount; i++)
            {
                for (int j = 0; j < pathChunkArea.VerticalChunkCount; j++)
                {
                    var pos = new Vector2(i, j);
                    var chunk = new Chunk();
                    chunk.PathChunkArea = pathChunkArea;
                    chunk.PositionInArea = pos;
                    chunk.GlobalPosition = pos + pathChunkArea.ChunkOffset;
                    pathChunkArea.PositionToChunk[pos] = chunk;
                }
            }
        }

        private Rect2 GetBoundingArea(IEnumerable<PathChunkArea> areas)
        {
            var firstArea = areas.ElementAt(0);
            var boundingRect = new Rect2(firstArea.ChunkOffset, new Vector2(firstArea.HorizontalChunkCount, firstArea.VerticalChunkCount));
            foreach (var area in areas)
            {
                boundingRect = boundingRect.Merge(new Rect2(area.ChunkOffset, new Vector2(area.HorizontalChunkCount, area.VerticalChunkCount)));
            }
            boundingRect = boundingRect.Grow(1f);
            return boundingRect;
        }

        private void FillBoundingArea(Dictionary<Vector2, Chunk> allChunks, Rect2 boundingArea)
        {
            for (int x = (int) boundingArea.Position.x; x < boundingArea.Position.x + boundingArea.Size.x; x++)
            {
                for (int y = (int) boundingArea.Position.y; y < boundingArea.Position.y + boundingArea.Size.y; y++)
                {
                    var chunkPos = new Vector2(x, y);
                    FillChunk(allChunks, chunkPos);
                }
            }
        }

        private void FillChunk(Dictionary<Vector2, Chunk> allChunks, Vector2 chunkPosition)
        {
            var offset = chunkPosition * CHUNK_TILE_COUNT;
            if (allChunks.ContainsKey(chunkPosition))
            {
                for (int x = 0; x < CHUNK_TILE_COUNT; x++)
                {
                    for (int y = 0; y < CHUNK_TILE_COUNT; y++)
                    {
                        var tilePos = new Vector2(x, y) + offset;

                        if (GetAverageValue(tilePos, NOISE_XSCALE, NOISE_YSCALE) > 0.06f)
                        {
                            Zone.Current.TileMap.SetCellv(tilePos, 0);
                            Zone.Current.TileMap.UpdateBitmaskArea(tilePos);
                        }
                        else if (PlayerSpawnPosition == Vector2.Zero)
                        {
                            PlayerSpawnPosition = tilePos * TILE_SIZE;
                            if (PlayerSpawnPosition != Vector2.Zero)
                            {
                                PlayerSpawnPosition += Vector2.Down * 16f;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < CHUNK_TILE_COUNT; x++)
                {
                    for (int y = 0; y < CHUNK_TILE_COUNT; y++)
                    {
                        var tilePos = new Vector2(x, y) + offset;
                        Zone.Current.TileMap.SetCellv(tilePos, 0);
                        Zone.Current.TileMap.UpdateBitmaskArea(tilePos);
                    }
                }
            }
        }

        private float GetAverageValue(Vector2 tilePos, float xScale, float yScale)
        {
            var sum = 0f;
            var scale = new Vector2(xScale, yScale);
            tilePos *= scale;
            foreach (var dir in _fourDirections)
            {
                sum += _noise.GetNoise2dv(tilePos + dir * scale) * .1f;
            }
            sum += _noise.GetNoise2dv(tilePos);
            return sum / (_fourDirections.Length + 1);
        }

        private void SecondPass()
        {
            var usedCells = Zone.Current.TileMap.GetUsedCells();
            Zone.Current.TileMap.Clear();
            foreach (var tile in usedCells)
            {
                if (tile is Vector2 tilePos)
                {
                    tilePos *= 2f;
                    Zone.Current.TileMap.SetCellv(tilePos, 0);
                    Zone.Current.TileMap.SetCellv(tilePos + Vector2.Right, 0);
                    Zone.Current.TileMap.SetCellv(tilePos + Vector2.Down, 0);
                    Zone.Current.TileMap.SetCellv(tilePos + Vector2.Down + Vector2.Right, 0);
                    Zone.Current.TileMap.UpdateBitmaskArea(tilePos);
                }
            }
        }
    }
}