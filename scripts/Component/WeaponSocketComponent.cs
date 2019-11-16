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
    }
}