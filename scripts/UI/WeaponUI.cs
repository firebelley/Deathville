using Deathville.GameObject.Combat;
using Godot;

namespace Deathville.UI
{
    public class WeaponUI : VBoxContainer
    {

        private const string ANIM_GROW = "grow";
        private const string ANIM_SHRINK = "shrink";

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

        public override void _Process(float delta)
        {
            _animationPlayer.PlaybackSpeed = 1 / Engine.TimeScale;
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
            _animationPlayer.Play(ANIM_GROW, -1f, 1f);
        }

        public void PlayStowAnimation()
        {
            _animationPlayer.Play(ANIM_SHRINK, -1f, 1f);
        }

        private void UpdatePivotOffset()
        {
            RectPivotOffset = new Vector2(_isLeft ? RectSize.x : 0f, RectSize.y);
        }
    }
}