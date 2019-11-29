using System.Collections.Generic;
using System.Linq;
using Deathville.GameObject;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.Environment
{
    public class Spawner : Node2D
    {
        private const float SPAWN_RADIUS = 400f;

        [Export]
        private int _maxSpawned = 25;
        [Export]
        private float _spawnDelay = .1f;
        [Export]
        private PackedScene _scene;

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

        private void RemoveInvalid()
        {
            var prevSize = _spawned.Count;
            _spawned = _spawned.Where(x => IsInstanceValid(x)).ToList();
            var diff = prevSize - _spawned.Count;
        }

        private void Spawn()
        {
            var spawnPos = GetSpawnPosition();

            if (spawnPos.DistanceSquaredTo(GlobalPosition) < 320f * 320f)
            {
                return;
            }

            var scene = _scene.Instance() as Node2D;

            Zone.Current.EntitiesLayer.AddChild(scene);
            scene.GlobalPosition = spawnPos;
            _spawned.Add(scene);
        }

        private Vector2 GetSpawnPosition()
        {
            var dir = Vector2.Right.Rotated(Main.RNG.RandfRange(0f, 2f * Mathf.Pi));
            dir *= SPAWN_RADIUS;
            return Zone.Current.Pathfinder.GetClosestGlobalPoint(GlobalPosition + dir);
        }

        private void OnTimerTimeout()
        {
            if (_scene == null) return;

            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            if (player != null)
            {
                GlobalPosition = player.GlobalPosition;
            }

            RemoveInvalid();

            if (_spawned.Count < _maxSpawned)
            {
                Spawn();
            }
        }
    }
}