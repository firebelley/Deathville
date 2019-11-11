using Godot;

namespace Deathville
{
    public class Camera : Camera2D
    {
        private const float SHAKE_DECAY = 5f;
        private const float MAX_OFFSET = 10f;

        private Vector2 _targetPos;
        private OpenSimplexNoise _noise;
        private float _currentSampleX;
        private float _currentSampleY;
        private float _shakeMagnitude;

        public override void _Ready()
        {
            _noise = new OpenSimplexNoise();
            _noise.Seed = (int) Main.RNG.Randi();
            _noise.Octaves = 4;
            _noise.Period = .1f;

            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerPositionUpdated), this, nameof(OnPlayerPositionUpdated));
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.WeaponFired), this, nameof(OnWeaponFired));
        }

        public override void _Process(float delta)
        {
            GlobalPosition = GlobalPosition.LinearInterpolate(_targetPos, 10 * delta / Engine.TimeScale);
            UpdateShake();
        }

        private void Shake(float magnitude)
        {
            if (_shakeMagnitude < magnitude)
            {
                _shakeMagnitude = magnitude;
            }
        }

        private void UpdateShake()
        {
            _currentSampleX = Mathf.Wrap(_currentSampleX + (GetProcessDeltaTime() / Engine.TimeScale), 0f, 100f);
            _currentSampleY = Mathf.Wrap(_currentSampleY + (GetProcessDeltaTime() / Engine.TimeScale), 0f, 100f);
            var sampleX = _noise.GetNoise1d(_currentSampleX) * MAX_OFFSET * 2f;
            var sampleY = _noise.GetNoise2d(_currentSampleX, _currentSampleY) * MAX_OFFSET * 2f;
            Offset = new Vector2(sampleX, sampleY) * _shakeMagnitude * _shakeMagnitude;
            _shakeMagnitude = Mathf.Clamp(_shakeMagnitude - (GetProcessDeltaTime() / Engine.TimeScale) * SHAKE_DECAY, 0f, 1f);
        }

        private void OnPlayerPositionUpdated(Vector2 globalPosition)
        {
            _targetPos = globalPosition;
        }

        private void OnWeaponFired()
        {
            Shake(.5f);
        }
    }
}