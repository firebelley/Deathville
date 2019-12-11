using Deathville.GameObject;
using Deathville.Singleton;
using Godot;
using GodotApiTools.Extension;

namespace Deathville
{
    public class Camera : Camera2D
    {
        private const float SHAKE_DECAY = 10f;
        private const float MAX_OFFSET = 5f;
        private const float AIM_OFFSET = .1f;

        [Export]
        private Color _clearColor;

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
            _noise.Period = .2f;

            VisualServer.SetDefaultClearColor(_clearColor);

            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.WeaponFired), this, nameof(OnWeaponFired));
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.EnemyStruck), this, nameof(OnEnemyStruck));
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.CameraShaken), this, nameof(OnCameraShaken));
        }

        public override void _Process(float delta)
        {
            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            if (player != null)
            {
                var mouseVec = GetGlobalMousePosition() - player.GlobalPosition;
                _targetPos = player.GlobalPosition + mouseVec * AIM_OFFSET;
            }
            else
            {
                _targetPos = GetGlobalMousePosition();
            }
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

        private void OnWeaponFired()
        {
            Shake(.75f);
        }

        private void OnEnemyStruck()
        {
            Shake(1f);
        }

        private void OnCameraShaken(float magnitude)
        {
            Shake(magnitude);
        }
    }
}