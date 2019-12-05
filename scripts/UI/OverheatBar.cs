using Deathville.GameObject.Combat;
using Deathville.Singleton;
using Godot;

namespace Deathville.UI
{
    public class OverheatBar : Control
    {
        [Export]
        private Color _overheatColor;
        [Export(PropertyHint.Enum, "Bar,Radial")]
        private int _progressType;

        private float _targetValue;

        private Color _defaultColor;
        private ProgressBar _progressBar;
        private TextureProgress _textureProgressBar;
        private StyleBoxFlat _styleBoxFlat;

        public override void _Ready()
        {
            _progressBar = GetNode<ProgressBar>("ProgressBar");
            _textureProgressBar = GetNode<TextureProgress>("TextureProgress");
            _styleBoxFlat = _progressBar.GetStylebox("fg") as StyleBoxFlat;
            _defaultColor = _styleBoxFlat.BgColor;

            _textureProgressBar.Visible = _progressType == 1;
            _progressBar.Visible = _progressType == 0;

            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerWeaponEquipped), this, nameof(OnPlayerWeaponEquipped));
        }

        public override void _Process(float delta)
        {
            Range range;
            if (_progressType == 0)
            {
                range = _progressBar;
            }
            else
            {
                range = _textureProgressBar;
            }
            range.Value = Mathf.Lerp((float) range.Value, _targetValue * 100f, 10f * delta / Engine.TimeScale);
            UpdateTint();
        }

        private void UpdateTint()
        {
            var color = _defaultColor.LinearInterpolate(_overheatColor, _targetValue);
            if (_progressType == 0)
            {
                _styleBoxFlat.BgColor = color;
            }
            else if (_progressType == 1)
            {
                _textureProgressBar.Modulate = color;
            }
        }

        private void OnPlayerWeaponEquipped(Weapon weapon)
        {
            weapon.Connect(nameof(Weapon.HeatChanged), this, nameof(OnWeaponHeatChanged));
        }

        private void OnWeaponHeatChanged(Weapon weapon)
        {
            _targetValue = weapon.CurrentHeat;
        }
    }
}