using Deathville.GameObject.Combat;
using Godot;

namespace Deathville.UI
{
    public class WeaponUI : VBoxContainer
    {
        public Weapon CurrentWeapon
        {
            get
            {
                if (IsInstanceValid(_currentWeapon))
                {
                    return _currentWeapon;
                }
                return null;
            }
        }

        private OverheatBar _overheatBar;
        private Weapon _currentWeapon;
        private TextureRect _textureRect;

        public override void _Ready()
        {
            _overheatBar = GetNode<OverheatBar>("OverheatBar");
            _textureRect = GetNode<TextureRect>("TextureRect");
        }

        public void ConnectWeapon(Weapon weapon)
        {
            _currentWeapon = null;
            _textureRect.Texture = null;
            _currentWeapon = weapon;
            _textureRect.Texture = _currentWeapon.Sprite.Texture;
            _overheatBar.ConnectWeapon(_currentWeapon);
        }
    }
}