using Deathville.Util;
using Godot;

namespace Deathville
{
    public class Zone : Node2D
    {
        public static Zone Current { get; private set; }

        public Node2D EffectsLayer
        {
            get
            {
                if (!IsInstanceValid(_effectsLayer))
                {
                    _effectsLayer = GetNode<Node2D>("Effects");
                }
                return _effectsLayer;
            }
        }
        private Node2D _effectsLayer;

        public Node2D EntitiesLayer
        {
            get
            {
                if (!IsInstanceValid(_entitiesLayer))
                {
                    _entitiesLayer = GetNode<Node2D>("Entities");
                }
                return _entitiesLayer;
            }
        }
        private Node2D _entitiesLayer;

        public TileMap TileMap
        {
            get
            {
                return GetNode<TileMap>("TileMap");
            }
        }

        public Pathfinder Pathfinder { get; private set; }

        public override void _Ready()
        {
            Current = this;
            Pathfinder = new Pathfinder(GetNode<TileMap>("TileMap"));
        }
    }
}