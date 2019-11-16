using Deathville.GameObject;
using Godot;

namespace Deathville.Component
{
    public class Weapon : Sprite
    {
        [Export]
        private PackedScene _projectile;

        private Position2D _muzzlePosition;
        private Position2D _chamberPosition;

        public override void _Ready()
        {
            _muzzlePosition = GetNode<Position2D>("MuzzlePosition");
            _chamberPosition = GetNode<Position2D>("ChamberPosition");
        }

        public void Fire()
        {
            if (_projectile == null) return;
            var bullet = _projectile.Instance() as Projectile;
            Zone.Current.EffectsLayer.AddChild(bullet);
            bullet.ObeyTimeScale = false;
            bullet.SetFriendly();
            bullet.Start(_chamberPosition.GlobalPosition, _muzzlePosition.GlobalPosition, GetGlobalMousePosition());
            GameEventDispatcher.DispatchWeaponFired();
        }
    }
}