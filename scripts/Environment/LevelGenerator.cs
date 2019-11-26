using System.Collections.Generic;
using System.Linq;
using Deathville.Singleton;
using Godot;
using GodotApiTools.Util;

namespace Deathville.Environment
{
    public class LevelGenerator : Node2D
    {
        private const int CHUNK_TILE_COUNT = 16;
        private const int TILE_SIZE = 16;

        [Export]
        private int _minLevelPathLength = 3;
        [Export]
        private int _maxLevelPathLength = 5;
        [Export]
        private float _pathDirectionChangePercent = .25f;
        [Export]
        private int _minChunkWidth = 3;
        [Export]
        private int _maxChunkWidth = 6;
        [Export]
        private int _minChunkHeight = 3;
        [Export]
        private int _maxChunkHeight = 6;

        private RandomNumberGenerator _rng = new RandomNumberGenerator();
        private Vector2[] _directions = new Vector2[] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };

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
            public LevelPiece LevelPiece;
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
            var allChunks = AddChunksToArea(areas);
            SelectLevelPiecesForChunks(allChunks);

            foreach (var chunk in allChunks.Values)
            {
                foreach (var tile in chunk.LevelPiece.GetUsedCells())
                {
                    if (tile is Vector2 position)
                    {
                        var tilepos = position + (chunk.PathChunkArea.ChunkOffset + chunk.PositionInArea) * CHUNK_TILE_COUNT;
                        Zone.Current.TileMap.SetCellv(tilepos, 0);
                        Zone.Current.TileMap.UpdateBitmaskArea(tilepos);
                    }
                }
                chunk.LevelPiece.QueueFree();
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
            if (_directions[i] == excludeDirection)
            {
                i = Mathf.Wrap(i + 1, 0, 3);
            }
            return _directions[i];
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

        private Dictionary<Vector2, Chunk> AddChunksToArea(IEnumerable<PathChunkArea> pathChunkAreas)
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

        private void SelectLevelPiecesForChunks(Dictionary<Vector2, Chunk> allChunks)
        {
            // TODO: rework this to add all chunks to a global list
            // shuffle the global list and then add all the level pieces
            // account for neighbors when determining layout
            var shuffledChunks = allChunks.Values.OrderBy(x => _rng.Randf());

            foreach (var chunk in shuffledChunks)
            {

                var connectableNeighbors = GetConnectableNeighbors(allChunks, chunk);
                HashSet<string> validPiecePaths = new HashSet<string>();
                if (connectableNeighbors.Count == 0)
                {
                    // any are valid
                    validPiecePaths.UnionWith(MetadataLoader.LevelPieceToPath[LevelPiece.N]);
                    validPiecePaths.UnionWith(MetadataLoader.LevelPieceToPath[LevelPiece.E]);
                    validPiecePaths.UnionWith(MetadataLoader.LevelPieceToPath[LevelPiece.S]);
                    validPiecePaths.UnionWith(MetadataLoader.LevelPieceToPath[LevelPiece.W]);
                }
                else
                {
                    foreach (var keyValue in connectableNeighbors)
                    {
                        // if it is the north neighbor and it can connect to its south
                        if (keyValue.Key == Vector2.Up && (keyValue.Value.LevelPiece.ConnectsVia & LevelPiece.S) > 0)
                        {
                            // then mark all pieces that connect via north as valid
                            validPiecePaths.UnionWith(MetadataLoader.LevelPieceToPath[LevelPiece.N]);
                        }
                        else if (keyValue.Key == Vector2.Right && (keyValue.Value.LevelPiece.ConnectsVia & LevelPiece.W) > 0)
                        {
                            validPiecePaths.UnionWith(MetadataLoader.LevelPieceToPath[LevelPiece.E]);
                        }
                        else if (keyValue.Key == Vector2.Down && (keyValue.Value.LevelPiece.ConnectsVia & LevelPiece.N) > 0)
                        {
                            validPiecePaths.UnionWith(MetadataLoader.LevelPieceToPath[LevelPiece.S]);
                        }
                        else if (keyValue.Key == Vector2.Left && (keyValue.Value.LevelPiece.ConnectsVia & LevelPiece.E) > 0)
                        {
                            validPiecePaths.UnionWith(MetadataLoader.LevelPieceToPath[LevelPiece.W]);
                        }
                    }
                }

                var piece = validPiecePaths.ElementAt(_rng.RandiRange(0, validPiecePaths.Count - 1));
                var scene = GD.Load(piece) as PackedScene;
                chunk.LevelPiece = scene.Instance() as LevelPiece;
            }
        }

        private Dictionary<Vector2, Chunk> GetConnectableNeighbors(Dictionary<Vector2, Chunk> allChunks, Chunk currentChunk)
        {
            // don't want to consider a neighbor at all in this example
            // north neighbor exists but does not connect via south
            // in that instance, we don't want to factor in the north neighbor to our calculation
            // find other neighbors that can be connected
            // if no other connectable neighbors, then just pick any piece
            // if surrounded on all sides by non-connectable neihbors, then just fully fill in the piece
            var directionToNeighbor = new Dictionary<Vector2, Chunk>();
            foreach (var dir in _directions)
            {
                var neighborPos = currentChunk.GlobalPosition + dir;
                if (allChunks.ContainsKey(neighborPos) && allChunks[neighborPos].LevelPiece != null)
                {
                    var neighbor = allChunks[neighborPos];
                    var connectsSouth = dir == Vector2.Up && (neighbor.LevelPiece.ConnectsVia & LevelPiece.S) > 0;
                    var connectsNorth = dir == Vector2.Down && (neighbor.LevelPiece.ConnectsVia & LevelPiece.N) > 0;
                    var connectsEast = dir == Vector2.Left && (neighbor.LevelPiece.ConnectsVia & LevelPiece.E) > 0;
                    var connectsWest = dir == Vector2.Right && (neighbor.LevelPiece.ConnectsVia & LevelPiece.W) > 0;

                    if (connectsNorth || connectsSouth || connectsWest || connectsEast)
                    {
                        directionToNeighbor[dir] = allChunks[neighborPos];
                    }
                }
            }
            return directionToNeighbor;
        }
    }
}