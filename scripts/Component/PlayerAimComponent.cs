using Deathville.GameObject;
using Deathville.GameObject.Combat;
using Deathville.GameObject.Parts;
using Godot;

namespace Deathville.Component
{
    public class PlayerAimComponent : Node2D
    {
        [Export]
        private NodePath _weaponSocketComponentPath;
        [Export]
        private NodePath _playerHandsPath;

        private bool _isAttacking;

        private WeaponSocketComponent _weaponSocketComponent;
        private Hands _playerHands;

        public override void _Ready()
        {
            _weaponSocketComponent = GetNode<WeaponSocketComponent>(_weaponSocketComponentPath);
            _playerHands = GetNode<Hands>(_playerHandsPath);
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

            if (_weaponSocketComponent.Weapon != null)
            {
                _playerHands.AlignWithWeapon(_weaponSocketComponent.Weapon);
            }
            else
            {
                _playerHands.ResetTransforms();
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