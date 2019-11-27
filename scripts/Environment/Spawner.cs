using System.Collections.Generic;
using System.Linq;
using Deathville.GameObject;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.Environment
{
    [Tool]
    public class Spawner : Node2D
    {
        private const int MAX_SPAWNED = 25;
        private static int _totalSpawned;

        [Export]
        private float _offset
        {
            get
            {
                return _realOffset;
            }
            set
            {
                _realOffset = value;
                Update();
            }
        }

        [Export]
        private int _maxSpawned = 1;
        [Export]
        private float _spawnDelay = 1f;
        [Export]
        private PackedScene _scene;

        private float _realOffset = 8f;

        private List<Node2D> _spawned = new List<Node2D>();

        private VisibilityNotifier2D _innerNotifier;
        private VisibilityNotifier2D _outerNotifier;

        public override void _Ready()
        {
            if (Engine.EditorHint) return;

            _innerNotifier = GetNode<VisibilityNotifier2D>("InnerNotifier");
            _outerNotifier = GetNode<VisibilityNotifier2D>("OuterNotifier");

            var timer = GetNode<Timer>("Timer");
            timer.WaitTime = _spawnDelay;
            timer.Start();
            timer.Connect("timeout", this, nameof(OnTimerTimeout));
        }

        public override void _Draw()
        {
            if (Engine.EditorHint)
            {
                DrawLine(-new Vector2(_offset, 0f), new Vector2(_offset, 0f), new Color(1f, 0f, 0f, 1f), 2f);
            }
        }

        private void RemoveInvalid()
        {
            var prevSize = _spawned.Count;
            _spawned = _spawned.Where(x => IsInstanceValid(x)).ToList();
            var diff = prevSize - _spawned.Count;
            _totalSpawned -= diff;
        }

        private void Spawn()
        {
            var scene = _scene.Instance() as Node2D;

            var spawnPos = GlobalPosition;
            var offset = Main.RNG.RandfRange(-_offset, _offset);
            spawnPos.x += offset;

            Zone.Current.EntitiesLayer.AddChild(scene);
            scene.GlobalPosition = spawnPos;
            _spawned.Add(scene);
            _totalSpawned++;
        }

        private bool ShouldSpawn()
        {
            var lessThan = _spawned.Count < _maxSpawned && _totalSpawned < MAX_SPAWNED;
            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            var inDistance = player != null && !_innerNotifier.IsOnScreen() && _outerNotifier.IsOnScreen();
            return lessThan && inDistance;
        }

        private void OnTimerTimeout()
        {
            if (_scene == null) return;
            RemoveInvalid();

            if (ShouldSpawn())
            {
                Spawn();
            }
        }
    }
}