using Deathville.GameObject;
using Godot;

namespace Deathville.Component
{
    public class PlayerAimComponent : Node2D
    {
        [Export]
        private NodePath _weaponSocketComponentPath;

        private WeaponSocketComponent _weaponSocketComponent;

        public override void _Ready()
        {
            _weaponSocketComponent = GetNode<WeaponSocketComponent>(_weaponSocketComponentPath);
            if (Owner is Player p)
            {
                p.Connect(nameof(Player.AttackStart), this, nameof(OnAttackStart));
            }
        }

        public override void _Process(float delta)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var weapon = _weaponSocketComponent.Weapon;
            if (weapon == null) return;
            var facingLeft = GetGlobalMousePosition().x < weapon.GlobalPosition.x;
            weapon.Scale = facingLeft ? Scale.Abs() * new Vector2(-1, 1) : weapon.Scale.Abs() * Vector2.One;
            weapon.Rotation = (weapon.GlobalPosition - GetGlobalMousePosition()).Angle() - (!facingLeft ? Mathf.Pi : 0);
        }

        private void OnAttackStart()
        {
            _weaponSocketComponent.Weapon?.Fire();
        }
    }
}