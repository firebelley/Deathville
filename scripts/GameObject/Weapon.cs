using Deathville.GameObject;
using Godot;

namespace Deathville.Component
{
    public class Weapon : Sprite
    {
        [Signal]
        public delegate void Fired();

        [Export]
        private PackedScene _projectile;

        [Export]
        private float _projectilesPerSecond = 10f;

        public bool IsFriendly;

        private Position2D _muzzlePosition;
        private Position2D _chamberPosition;

        private Timer _fireTimer;

        public override void _Ready()
        {
            _fireTimer = GetNode<Timer>("FireTimer");
            _muzzlePosition = GetNode<Position2D>("MuzzlePosition");
            _chamberPosition = GetNode<Position2D>("ChamberPosition");
        }

        public void AttemptFire(Vector2 atTarget)
        {
            if (_fireTimer.IsStopped())
            {
                Fire(atTarget);
                _fireTimer.WaitTime = 1f / _projectilesPerSecond;
                _fireTimer.Start();
            }
        }

        public void Fire(Vector2 atTarget)
        {
            if (_projectile == null) return;
            var projectile = _projectile.Instance() as Projectile;
            Zone.Current.EffectsLayer.AddChild(projectile);
            if (IsFriendly)
            {
                projectile.SetFriendly();
            }
            else
            {
                projectile.SetEnemy();
            }

            projectile.Start(_chamberPosition.GlobalPosition, _muzzlePosition.GlobalPosition, atTarget);
            EmitSignal(nameof(Fired));
        }
    }
}