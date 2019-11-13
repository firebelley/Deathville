using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Deathville.Util
{
    public class Pathfinder
    {
        private const float MAX_CELL_DISTANCE = 6f;

        private List<AstarCornerCell> _astarCornerCells = new List<AstarCornerCell>();
        private List<AstarCell> _astarCells = new List<AstarCell>();
        private AStar2D _astar = new AStar2D();
        private int _astarId;
        private TileMap _tileMap;

        private struct AstarCornerCell
        {
            public Vector2 Position;
            public bool IsLeftCorner;

            public AstarCornerCell(Vector2 position, bool isLeftCorner)
            {
                Position = position;
                IsLeftCorner = isLeftCorner;
            }
        }

        private struct AstarCell
        {
            public Vector2 Position;
            public int Id;

            public AstarCell(Vector2 position, int id)
            {
                Position = position;
                Id = id;
            }
        }

        public Pathfinder(TileMap tileMap)
        {
            _tileMap = tileMap;
            GenerateAstar();
        }

        private void GenerateAstar()
        {
            foreach (var tilePos in _tileMap.GetUsedCells())
            {
                // TODO: only use tiles with a collision associated
                TryAddTileCell((Vector2) tilePos);
            }

            ConnectCornerCells();

            foreach (var cell in _astarCornerCells)
            {
                GD.Print(cell.Position);
            }
        }

        private void ConnectCornerCells()
        {
            // find all cells with a higher (lower on the screen) y value AND an x value to the right (if right corner) or to the left (if left corner) AND within the max distance
            foreach (var cornerCell in _astarCornerCells)
            {
                IEnumerable<AstarCell> cellsToConnect;
                if (cornerCell.IsLeftCorner)
                {
                    cellsToConnect = _astarCells
                        .Where(cell =>
                            cell.Position.y > cornerCell.Position.y &&
                            cell.Position.x < cornerCell.Position.x &&
                            cell.Position.DistanceSquaredTo(cornerCell.Position) <= MAX_CELL_DISTANCE * MAX_CELL_DISTANCE
                        );
                }
                else
                {
                    cellsToConnect = _astarCells
                        .Where(cell =>
                            cell.Position.y > cornerCell.Position.y &&
                            cell.Position.x > cornerCell.Position.x &&
                            cell.Position.DistanceSquaredTo(cornerCell.Position) <= MAX_CELL_DISTANCE * MAX_CELL_DISTANCE
                        );
                }

                // for each cell that matches the above conditions
                // create a new astar cell at the corner position with a weight equal to the distance of the cell to connect to
                // then connect the two cells together
                // TODO: may need to connect the new corner cells to the OG cell, test to find out
                foreach (var toConnectCell in cellsToConnect)
                {
                    var dist = toConnectCell.Position.DistanceTo(cornerCell.Position);
                    _astar.AddPoint(_astarId, cornerCell.Position, dist);
                    _astar.ConnectPoints(toConnectCell.Id, _astarId, true);
                    _astarId++;
                }
            }
        }

        private void TryAddTileCell(Vector2 tilePos)
        {
            var astarPos = tilePos + Vector2.Up;
            if (_tileMap.GetCellv(astarPos) == TileMap.InvalidCell)
            {
                AddTileCell(tilePos);
            }
        }

        private void AddTileCell(Vector2 tilePos)
        {
            var astarPos = tilePos + Vector2.Up;
            _astar.AddPoint(_astarId, astarPos, 1f);
            _astarCells.Add(new AstarCell(astarPos, _astarId));

            // connect the point to the left
            if (_astarId > 0 && _astar.GetPointPosition(_astarId - 1) == astarPos + Vector2.Left)
            {
                _astar.ConnectPoints(_astarId - 1, _astarId, true);
            }

            var isRightCorner = _tileMap.GetCellv(tilePos + Vector2.Right) == TileMap.InvalidCell;
            var isLeftCorner = _tileMap.GetCellv(tilePos + Vector2.Left) == TileMap.InvalidCell;
            if (isRightCorner || isLeftCorner)
            {
                var cornerCell = new AstarCornerCell(astarPos, isLeftCorner);
                _astarCornerCells.Add(cornerCell);
            }

            _astarId++;
        }
    }
}