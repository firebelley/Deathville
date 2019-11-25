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
            public PackedScene LevelPieceScene;

            public Chunk(Vector2 position, PackedScene levelPieceScene)
            {
                PositionInArea = position;
                LevelPieceScene = levelPieceScene;
            }
        }

        private class PathChunkArea
        {
            public LevelPathCell LevelPathCell;
            public int HorizontalChunkCount;
            public int VerticalChunkCount;
            public Dictionary<Vector2, Chunk> PositionToChunk = new Dictionary<Vector2, Chunk>();
        }

        public override void _Ready()
        {
            _rng.Randomize();
            CallDeferred(nameof(Generate));
            // Generate();
        }

        private void Generate()
        {
            var path = GetLevelPath();

            var areas = new List<PathChunkArea>();
            foreach (var cell in path)
            {
                areas.Add(GenerateAreaForLevelPathCell(cell));
            }

            foreach (var area in areas)
            {
                FillArea(area);
            }

            // foreach (var area in areas)
            // {

            foreach (var chunk in areas[0].PositionToChunk.Values)
            {
                var levelPiece = chunk.LevelPieceScene.Instance() as LevelPiece;
                foreach (var tile in levelPiece.GetUsedCells())
                {
                    if (tile is Vector2 position)
                    {
                        var tilepos = position + chunk.PositionInArea * CHUNK_TILE_COUNT;
                        Zone.Current.TileMap.SetCellv(tilepos, 0);
                        Zone.Current.TileMap.UpdateBitmaskArea(tilepos);
                    }
                }
                levelPiece.QueueFree();

                // GetParent().AddChild(chunk.LevelPiece);
                // chunk.LevelPiece.GlobalPosition = chunk.PositionInArea * CHUNK_TILE_COUNT * TILE_SIZE;
            }
            // }
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

        private void FillArea(PathChunkArea pathChunkArea)
        {
            List<Vector2> chunkPositions = new List<Vector2>();
            for (int i = 0; i < pathChunkArea.HorizontalChunkCount; i++)
            {
                for (int j = 0; j < pathChunkArea.VerticalChunkCount; j++)
                {
                    chunkPositions.Add(new Vector2(i, j));
                }
            }

            chunkPositions = chunkPositions.OrderBy(x => _rng.Randf()).ToList();
            foreach (var position in chunkPositions)
            {
                var chunk = new Chunk(position, (GD.Load("res://scenes/Environment/LevelPieces/001.tscn") as PackedScene));
                pathChunkArea.PositionToChunk[position] = chunk;
            }
        }
    }
}