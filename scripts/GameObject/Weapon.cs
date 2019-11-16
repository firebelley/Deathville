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
        }

        public void Fire()
        {
            if (_projectile == null) return;
            var bullet = _projectile.Instance() as Projectile;
            Zone.Current.EffectsLayer.AddChild(bullet);
            bullet.ObeyTimeScale = false;
            bullet.SetFriendly();
            bullet.Start(GlobalPosition, _muzzlePosition.GlobalPosition, GetGlobalMousePosition());
            GameEventDispatcher.DispatchWeaponFired();
        }
    }
}