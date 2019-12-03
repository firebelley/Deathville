using Deathville.GameObject;
using Deathville.Singleton;
using Godot;

namespace Deathville.Component
{
    public class WeaponSocketComponent : Node2D
    {
        [Export]
        private bool _isPlayer;

        public Weapon Weapon
        {
            get
            {
                if (!IsInstanceValid(_weapon))
                {
                    return null;
                }
                return _weapon;
            }
        }
        private Weapon _weapon;

        public bool FacingLeft
        {
            get
            {
                return _weapon.Scale.x < 0f;
            }
        }

        public override void _Ready()
        {
            if (GetChildren().Count > 0)
            {
                _weapon = GetChild(0) as Weapon;
                _weapon.IsFriendly = _isPlayer;
                _weapon.Connect(nameof(Weapon.Fired), this, nameof(OnWeaponFired));
            }
        }

        public void EquipWeapon(Weapon weapon)
        {
            RemoveCurrentWeapon();
            _weapon = weapon;
            AddChild(_weapon);
            _weapon.IsFriendly = _isPlayer;
            _weapon.Connect(nameof(Weapon.Fired), this, nameof(OnWeaponFired));

            if (_isPlayer)
            {
                GameEventDispatcher.DispatchPlayerWeaponEquipped(_weapon);
            }
        }

        public void AimWeapon(Vector2 atPos)
        {
            if (Weapon == null) return;
            var facingLeft = atPos.x < _weapon.GlobalPosition.x;
            _weapon.Scale = facingLeft ? Scale.Abs() * new Vector2(-1, 1) : _weapon.Scale.Abs() * Vector2.One;
            _weapon.Rotation = (_weapon.GlobalPosition - atPos).Angle() - (!facingLeft ? Mathf.Pi : 0);
        }

        public void ResetWeaponAim()
        {
            Rotation = 0f;
        }

        private void RemoveCurrentWeapon()
        {
            if (GetChildren().Count > 0)
            {
                var child = GetChild(0);
                RemoveChild(child);
                child.QueueFree();
            }
        }

        private void OnWeaponFired()
        {
            if (_isPlayer)
            {
                GameEventDispatcher.DispatchWeaponFired();
            }
        }
    }
}