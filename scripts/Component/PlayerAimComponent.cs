using Deathville.GameObject;
using Godot;

namespace Deathville.Component
{
    public class PlayerAimComponent : Node2D
    {
        [Export]
        private NodePath _weaponSocketComponentPath;

        private bool _isAttacking;

        private WeaponSocketComponent _weaponSocketComponent;

        public override void _Ready()
        {
            _weaponSocketComponent = GetNode<WeaponSocketComponent>(_weaponSocketComponentPath);
            if (Owner is Player p)
            {
                p.Connect(nameof(Player.AttackStart), this, nameof(OnAttackStart));
                p.Connect(nameof(Player.AttackEnd), this, nameof(OnAttackEnd));
            }
        }

        public override void _Process(float delta)
        {
            _weaponSocketComponent.AimWeapon(GetGlobalMousePosition());
            if (_isAttacking)
            {
                _weaponSocketComponent.Weapon?.AttemptFire(GetGlobalMousePosition());
            }
        }

        private void OnAttackStart()
        {
            _isAttacking = true;
            _weaponSocketComponent.Weapon?.AttemptFire(GetGlobalMousePosition());
        }

        private void OnAttackEnd()
        {
            _isAttacking = false;
        }
    }
}