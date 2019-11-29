using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Deathville.Environment
{
    public class LevelPiece : TileMap
    {
        public const int N = 1;
        public const int E = 1 << 1;
        public const int S = 1 << 2;
        public const int W = 1 << 3;
        public const int NE = 1 << 4;
        public const int SE = 1 << 5;
        public const int SW = 1 << 6;
        public const int NW = 1 << 7;

        [Export(PropertyHint.Flags, "N,E,S,W")]
        public int ConnectsVia { get; private set; }

        public Vector2 PlayerSpawnPosition
        {
            get
            {
                return GetNode<Position2D>("PlayerSpawn").Position;
            }
        }
    }
}