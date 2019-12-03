using Deathville.GameObject;
using Deathville.Singleton;
using Godot;

namespace Deathville.UI
{
    public class OverheatBar : Control
    {
        [Export]
        private Color _overheatColor;

        private float _targetValue;

        private Color _defaultColor;
        private ProgressBar _progressBar;
        private StyleBoxFlat _styleBoxFlat;

        public override void _Ready()
        {
            _progressBar = GetNode<ProgressBar>("ProgressBar");
            _styleBoxFlat = _progressBar.GetStylebox("fg") as StyleBoxFlat;
            _defaultColor = _styleBoxFlat.BgColor;

            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerWeaponEquipped), this, nameof(OnPlayerWeaponEquipped));
        }

        public override void _Process(float delta)
        {
            _progressBar.Value = Mathf.Lerp((float) _progressBar.Value, _targetValue * 100f, 10f * delta / Engine.TimeScale);
            _styleBoxFlat.BgColor = _defaultColor.LinearInterpolate(_overheatColor, _targetValue);
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