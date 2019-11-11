using Godot;
using GodotApiTools.Extension;

namespace Deathville.GameObject
{
    public class Enemy : KinematicBody2D
    {
        private Vector2 _playerPos;
        private ResourcePreloader _resourcePreloader;

        public override void _Ready()
        {
            _resourcePreloader = GetNode<ResourcePreloader>("ResourcePreloader");
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerPositionUpdated), this, nameof(OnPlayerPositionUpdated));
            GetNode<Timer>("Timer").Connect("timeout", this, nameof(OnTimerTimeout));
        }

        private void OnPlayerPositionUpdated(Vector2 pos)
        {
            _playerPos = pos;
        }

        private void OnTimerTimeout()
        {
            var bullet = _resourcePreloader.InstanceScene<Bullet>();
            AddChild(bullet);
            bullet.GlobalPosition = GlobalPosition;
            bullet.LinearVelocity = 300f * (_playerPos - GlobalPosition).Normalized();
            bullet.Rotation = (_playerPos - GlobalPosition).Normalized().Angle();
        }
    }
}