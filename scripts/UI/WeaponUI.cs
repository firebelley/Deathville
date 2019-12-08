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

        [Export]
        private bool _isLeft;

        private OverheatBar _overheatBar;
        private Weapon _currentWeapon;
        private TextureRect _textureRect;
        private Label _hotLabel;
        private AnimationPlayer _animationPlayer;

        public override void _Ready()
        {
            _overheatBar = GetNode<OverheatBar>("OverheatBar");
            _textureRect = GetNode<TextureRect>("TextureRect");
            _hotLabel = GetNode<Label>("TextureRect/HotLabel");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            UpdatePivotOffset();
        }

        public void ConnectWeapon(Weapon weapon)
        {
            _currentWeapon = null;
            _textureRect.Texture = null;
            _currentWeapon = weapon;
            _textureRect.Texture = _currentWeapon.Sprite.Texture;
            _overheatBar.ConnectWeapon(_currentWeapon);
        }

        public void PlaySwapAnimation()
        {
            // if (_animationPlayer.IsPlaying())
            // {
            //     _animationPlayer.Seek(10f, true);
            // }
            _animationPlayer.Play("shrink", -1f, 1f, false);
        }

        public void PlayStowAnimation()
        {
            _animationPlayer.Play("shrink", -1f, 1f, true);
        }

        private void UpdatePivotOffset()
        {
            RectPivotOffset = new Vector2(_isLeft ? RectSize.x : 0f, RectSize.y);
        }
    }
}