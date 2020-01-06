using Godot;

namespace Deathville.Environment
{
    public class Spawner : Node2D
    {
        private const float SPAWN_RADIUS = 400f;

        [Export]
        private int _maxSpawned = 5;
        [Export]
        private float _spawnDelay = .3f;
        [Export]
        private PackedScene _scene;

        private AnimationPlayer _animationPlayer;
        private Timer _timer;
        private int _spawned = 0;

        public override void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _animationPlayer.Connect("animation_changed", this, nameof(OnAnimationChanged));

            _timer = GetNode<Timer>("Timer");
            _timer.WaitTime = _spawnDelay;
            _timer.Connect("timeout", this, nameof(OnTimerTimeout));
        }

        private void Spawn()
        {
            var scene = _scene.Instance() as Node2D;
            Zone.Current.EntitiesLayer.AddChild(scene);
            scene.GlobalPosition = GlobalPosition;
            _spawned++;
        }

        private void OnTimerTimeout()
        {
            if (_scene == null) return;

            if (_spawned < _maxSpawned)
            {
                Spawn();
            }
        }

        private void OnAnimationChanged(string oldName, string newName)
        {
            if (oldName == "default")
            {
                _timer.Start();
            }
        }
    }
}