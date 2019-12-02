using Deathville.GameObject;
using Deathville.Util;
using Godot;

namespace Deathville.Environment
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
                if (!IsInstanceValid(_tileMap))
                {
                    _tileMap = GetNode<TileMap>("TileMap");
                }
                return _tileMap;
            }
            set
            {
                _tileMap = value;
            }
        }
        private TileMap _tileMap;

        public Pathfinder Pathfinder { get; private set; }

        public override void _Ready()
        {
            Current = this;
            CallDeferred(nameof(Init));
        }

        private void Init()
        {
            var generator = GetNode<LevelGenerator>("LevelGenerator");
            generator.Generate();
            if (EntitiesLayer.GetChildCount() > 0)
            {
                EntitiesLayer.GetNode<Player>("Player").GlobalPosition = generator.PlayerSpawnPosition;
            }
            Pathfinder = new Pathfinder(TileMap);
        }
    }
}