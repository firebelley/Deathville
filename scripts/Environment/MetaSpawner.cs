using System.Collections.Generic;
using System.Linq;
using Deathville.GameObject;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.Environment
{
    public class MetaSpawner : Node
    {
        [Export]
        private PackedScene _scene;
        [Export]
        private float _spawnDelay = 1f;
        [Export]
        private int _maxSpawned = 3;

        private List<Spawner> _spawned = new List<Spawner>();

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

        private void PlaceSpawner()
        {
            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            if (_scene == null || player == null) return;

            var dir = Vector2.Right.Rotated(Main.RNG.RandfRange(0f, 2f * Mathf.Pi));
            dir *= Main.RNG.RandfRange(100f, 200f);

            var point = Zone.Current.Pathfinder.GetClosestGlobalPoint(player.GlobalPosition + dir);

            var scene = _scene.Instance() as Spawner;
            Zone.Current.BackgroundLayer.AddChild(scene);
            scene.GlobalPosition = point;

            _spawned.Add(scene);
        }

        private void OnTimerTimeout()
        {
            RemoveInvalid();

            if (_spawned.Count < _maxSpawned)
            {
                PlaceSpawner();
            }
        }
    }
}