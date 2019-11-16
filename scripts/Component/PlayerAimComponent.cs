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
            _weaponSocketComponent.AimWeapon(GetGlobalMousePosition());
        }

        private void OnAttackStart()
        {
            _weaponSocketComponent.Weapon?.Fire(GetGlobalMousePosition());
        }
    }
}