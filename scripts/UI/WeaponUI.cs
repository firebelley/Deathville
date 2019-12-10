using Deathville.GameObject.Combat;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.UI
{
    public class WeaponUI : VBoxContainer
    {

        private const string ANIM_GROW = "grow";
        private const string ANIM_SHRINK = "shrink";
        private const string ANIM_DEFAULT = "default";

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
        private AnimationPlayer _swapAnimationPlayer;
        private AnimationPlayer _hotAnimationPlayer;

        public override void _Ready()
        {
            _overheatBar = GetNode<OverheatBar>("OverheatBar");
            _textureRect = GetNode<TextureRect>("TextureRect");
            _hotLabel = GetNode<Label>("TextureRect/HotLabel");
            _swapAnimationPlayer = GetNode<AnimationPlayer>("SwapAnimationPlayer");
            _hotAnimationPlayer = GetNode<AnimationPlayer>("HotAnimationPlayer");

            _hotLabel.Hide();
            UpdatePivotOffset();
        }

        public override void _Process(float delta)
        {
            _swapAnimationPlayer.PlaybackSpeed = 1f / Engine.TimeScale;
            _hotAnimationPlayer.PlaybackSpeed = 1f / Engine.TimeScale;
        }

        public void ConnectWeapon(Weapon weapon)
        {
            DisconnectWeapon();
            _currentWeapon = null;
            _textureRect.Texture = null;
            _currentWeapon = weapon;
            _textureRect.Texture = _currentWeapon.Sprite.Texture;
            _overheatBar.ConnectWeapon(_currentWeapon);

            weapon.Connect(nameof(Weapon.Overheated), this, nameof(OnWeaponOverheated));
            weapon.Connect(nameof(Weapon.Cooled), this, nameof(OnWeaponCooled));
        }

        public void PlaySwapAnimation()
        {
            _swapAnimationPlayer.Play(ANIM_GROW, -1f, 1f);
        }

        public void PlayStowAnimation()
        {
            _swapAnimationPlayer.Play(ANIM_SHRINK, -1f, 1f);
        }

        private void DisconnectWeapon()
        {
            if (IsInstanceValid(_currentWeapon))
            {
                this.DisconnectAllSignals(_currentWeapon);
            }
        }

        private void UpdatePivotOffset()
        {
            RectPivotOffset = new Vector2(_isLeft ? RectSize.x : 0f, RectSize.y);
        }

        private void OnWeaponOverheated(Weapon weapon)
        {
            _hotAnimationPlayer.Play(ANIM_DEFAULT);
        }

        private void OnWeaponCooled(Weapon weapon)
        {
            if (_hotAnimationPlayer.IsPlaying())
            {
                _hotAnimationPlayer.Seek(1f, true);
            }
            _hotAnimationPlayer.Stop();
        }
    }
}