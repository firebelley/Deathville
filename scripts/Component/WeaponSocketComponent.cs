using Godot;

namespace Deathville.Component
{
    public class WeaponSocketComponent : Node2D
    {

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

        public override void _Ready()
        {
            if (GetChildren().Count > 0)
            {
                _weapon = GetChild(0) as Weapon;
            }
        }

        public void AimWeapon(Vector2 atPos)
        {
            if (Weapon == null) return;
            var facingLeft = atPos.x < _weapon.GlobalPosition.x;
            _weapon.Scale = facingLeft ? Scale.Abs() * new Vector2(-1, 1) : _weapon.Scale.Abs() * Vector2.One;
            _weapon.Rotation = (_weapon.GlobalPosition - atPos).Angle() - (!facingLeft ? Mathf.Pi : 0);
        }
    }
}