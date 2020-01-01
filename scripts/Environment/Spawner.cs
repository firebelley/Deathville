using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Deathville.Environment
{
    public class Spawner : Node2D
    {
        private const float SPAWN_RADIUS = 400f;

        [Export]
        private int _maxSpawned = 8;
        [Export]
        private float _spawnDelay = .1f;
        [Export]
        private PackedScene _scene;

        private List<Node2D> _spawned = new List<Node2D>();

        public override void _Ready()
        {
            var timer = GetNode<Timer>("Timer");
            timer.WaitTime = _spawnDelay;
            timer.Start();
            timer.Connect("timeout", this, nameof(OnTimerTimeout));
        }

        private void RemoveInvalid()
        {
            _spawned = _spawned.Where(x => IsInstanceValid(x)).ToList();
        }

        private void Spawn()
        {
            var scene = _scene.Instance() as Node2D;
            Zone.Current.EntitiesLayer.AddChild(scene);
            scene.GlobalPosition = GlobalPosition;
            _spawned.Add(scene);
        }

        private void OnTimerTimeout()
        {
            if (_scene == null) return;

            RemoveInvalid();

            if (_spawned.Count < _maxSpawned)
            {
                Spawn();
            }
        }
    }
}