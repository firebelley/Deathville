using Godot;
using GodotApiTools.Extension;

namespace Deathville.GameObject
{
    public class Weapon : Node2D
    {
        private Sprite _sprite;
        private ResourcePreloader _resourcePreloader;
        private Position2D _muzzlePosition;

        public override void _Ready()
        {
            _sprite = GetNode<Sprite>("Sprite");
            _resourcePreloader = GetNode<ResourcePreloader>("ResourcePreloader");
            _muzzlePosition = GetNode<Position2D>("Sprite/MuzzlePosition");

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
            var bullet = _resourcePreloader.InstanceScene<Bullet>();
            Zone.Current.EffectsLayer.AddChild(bullet);
            bullet.ObeyTimeScale = false;
            bullet.Start(_muzzlePosition.GlobalPosition, GetGlobalMousePosition());
        }

        private void UpdatePosition()
        {
            var facingLeft = GetGlobalMousePosition().x < GlobalPosition.x;
            _sprite.Scale = facingLeft ? _sprite.Scale.Abs() * new Vector2(-1, 1) : _sprite.Scale.Abs() * Vector2.One;
            Rotation = (GlobalPosition - GetGlobalMousePosition()).Angle() - (!facingLeft ? Mathf.Pi : 0);
        }

        private void OnAttackStart()
        {
            Fire();
        }
    }
}