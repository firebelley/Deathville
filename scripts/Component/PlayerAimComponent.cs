using Deathville.GameObject;
using Deathville.GameObject.Parts;
using Deathville.Singleton;
using Godot;

namespace Deathville.Component
{
    public class PlayerAimComponent : Node2D
    {
        [Export]
        private NodePath _weaponSocket1Path;
        [Export]
        private NodePath _weaponSocket2Path;
        [Export]
        private NodePath _playerHandsPath;

        private bool _isAttacking;
        private int _currentSocket;

        private Hands _playerHands;
        private WeaponSocketComponent[] _weaponSocketComponents = new WeaponSocketComponent[2];

        public override void _Ready()
        {
            _weaponSocketComponents[0] = GetNode<WeaponSocketComponent>(_weaponSocket1Path);
            _weaponSocketComponents[1] = GetNode<WeaponSocketComponent>(_weaponSocket2Path);
            _playerHands = GetNode<Hands>(_playerHandsPath);
            if (Owner is Player p)
            {
                p.Connect(nameof(Player.AttackStart), this, nameof(OnAttackStart));
                p.Connect(nameof(Player.AttackEnd), this, nameof(OnAttackEnd));
            }
        }

        public override void _Process(float delta)
        {
            var weaponSocket = _weaponSocketComponents[_currentSocket];
            weaponSocket.AimWeapon(GetGlobalMousePosition());
            if (_isAttacking)
            {
                weaponSocket.Weapon?.AttemptFire(GetGlobalMousePosition());
            }

            if (weaponSocket.Weapon != null)
            {
                _playerHands.AlignWithWeapon(weaponSocket.Weapon);
            }
            else
            {
                _playerHands.ResetTransforms();
            }
        }

        private void UpdateVisible()
        {
            foreach (var socket in _weaponSocketComponents)
            {
                socket.Visible = false;
            }
            _weaponSocketComponents[_currentSocket].Visible = true;
        }

        private void OnAttackStart(int socket)
        {
            _isAttacking = true;
            var prevSocket = _currentSocket;
            _currentSocket = socket;
            _weaponSocketComponents[_currentSocket].Weapon?.AttemptFire(GetGlobalMousePosition());
            UpdateVisible();

            if (prevSocket != _currentSocket)
            {
                GameEventDispatcher.DispatchPlayerWeaponSwapped(_weaponSocketComponents[_currentSocket].Weapon);
            }
        }

        private void OnAttackEnd(int socket)
        {
            _isAttacking = false;
        }
    }
}