using System.Collections.Generic;
using Godot;

namespace Deathville.Environment
{
    public class LevelGenerator : Node2D
    {
        [Export]
        private int _minLevelPathLength = 10;
        [Export]
        private int _maxLevelPathLength = 15;
        [Export]
        private float _pathDirectionChangePercent = .25f;

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

        public override void _Ready()
        {
            _rng.Randomize();
            var path = GetLevelPath();
            foreach (var cell in path)
            {
                GD.Print(cell.Position);
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
    }
}