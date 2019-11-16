using Deathville.GameObject;
using Godot;

namespace Deathville.Component
{
    public class Weapon : Sprite
    {
        private Position2D _muzzlePosition;

        [Export]
        private PackedScene _projectile;

        public override void _Ready()
        {
            _muzzlePosition = GetNode<Position2D>("MuzzlePosition");

            if (Owner is Player p)
            {
                p.Connect(nameof(Player.AttackStart), this, nameof(OnAttackStart));
            }
        }

        public override void _Process(float delta)
        {
            UpdatePosition();
        }

        private void Fire()
        {
            if (_projectile == null) return;
            var bullet = _projectile.Instance() as Projectile;
            Zone.Current.EffectsLayer.AddChild(bullet);
            bullet.ObeyTimeScale = false;
            bullet.SetFriendly();
            bullet.Start(GlobalPosition, _muzzlePosition.GlobalPosition, GetGlobalMousePosition());
            GameEventDispatcher.DispatchWeaponFired();
        }

        private void UpdatePosition()
        {
            var facingLeft = GetGlobalMousePosition().x < GlobalPosition.x;
            Scale = facingLeft ? Scale.Abs() * new Vector2(-1, 1) : Scale.Abs() * Vector2.One;
            Rotation = (GlobalPosition - GetGlobalMousePosition()).Angle() - (!facingLeft ? Mathf.Pi : 0);
        }

        private void OnAttackStart()
        {
            Fire();
        }
    }
}