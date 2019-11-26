using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Deathville.Environment
{
    public class LevelGenerator : Node2D
    {
        private const int CHUNK_TILE_COUNT = 16;
        private const int TILE_SIZE = 16;

        [Export]
        private int _minLevelPathLength = 10;
        [Export]
        private int _maxLevelPathLength = 15;
        [Export]
        private float _pathDirectionChangePercent = .25f;
        [Export]
        private int _minChunkWidth = 5;
        [Export]
        private int _maxChunkWidth = 10;
        [Export]
        private int _minChunkHeight = 5;
        [Export]
        private int _maxChunkHeight = 10;

        private RandomNumberGenerator _rng = new RandomNumberGenerator();

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

        private struct Chunk
        {
            public Vector2 PositionInArea;
            public Vector2 GlobalPosition;
            public PackedScene LevelPieceScene;

            public Chunk(Vector2 position, Vector2 globalPos, PackedScene levelPieceScene)
            {
                PositionInArea = position;
                GlobalPosition = globalPos;
                LevelPieceScene = levelPieceScene;
            }
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
            CallDeferred(nameof(Generate));
        }

        private void Generate()
        {
            var path = GetLevelPath();

            var areas = new List<PathChunkArea>();
            foreach (var cell in path)
            {
                areas.Add(GenerateAreaForLevelPathCell(cell));
            }

            OffsetAreas(areas);
            AddLevelPiecesToAreas(areas);

            foreach (var area in areas)
            {
                foreach (var chunk in area.PositionToChunk.Values)
                {
                    var levelPiece = chunk.LevelPieceScene.Instance() as LevelPiece;
                    foreach (var tile in levelPiece.GetUsedCells())
                    {
                        if (tile is Vector2 position)
                        {
                            var tilepos = position + (area.ChunkOffset + chunk.PositionInArea) * CHUNK_TILE_COUNT;
                            Zone.Current.TileMap.SetCellv(tilepos, 0);
                            Zone.Current.TileMap.UpdateBitmaskArea(tilepos);
                        }
                    }
                    levelPiece.QueueFree();
                }
            }

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
            Vector2[] directions = new Vector2[] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };
            if (directions[i] == excludeDirection)
            {
                i = Mathf.Wrap(i + 1, 0, 3);
            }
            return directions[i];
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

        private void AddLevelPiecesToAreas(IEnumerable<PathChunkArea> pathChunkAreas)
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
            SelectLevelPieces(allChunks.Values);
        }

        private void CreateAreaChunks(PathChunkArea pathChunkArea)
        {
            for (int i = 0; i < pathChunkArea.HorizontalChunkCount; i++)
            {
                for (int j = 0; j < pathChunkArea.VerticalChunkCount; j++)
                {
                    var pos = new Vector2(i, j);
                    pathChunkArea.PositionToChunk[pos] = new Chunk(pos, pos + pathChunkArea.ChunkOffset, null);
                }
            }
        }

        private void SelectLevelPieces(IEnumerable<Chunk> allChunks)
        {
            // TODO: rework this to add all chunks to a global list
            // shuffle the global list and then add all the level pieces
            // account for neighbors when determining layout
            var shuffledChunks = allChunks.OrderBy(x => _rng.Randf());
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
    }
}