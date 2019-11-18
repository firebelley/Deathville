using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Deathville.Environment
{
    [Tool]
    public class Spawner : Node2D
    {
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

        public override void _Ready()
        {
            if (Engine.EditorHint) return;

            var timer = GetNode<Timer>("Timer");
            timer.WaitTime = _spawnDelay;
            timer.Start();
            timer.Connect("timeout", this, nameof(OnTimerTimeout));

            CallDeferred(nameof(SpawnMax));
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
            _spawned = _spawned.Where(x => IsInstanceValid(x)).ToList();
        }

        private void SpawnMax()
        {
            while (_spawned.Count < _maxSpawned)
            {
                Spawn();
            }
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