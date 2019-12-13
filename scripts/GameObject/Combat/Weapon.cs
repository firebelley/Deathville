using Deathville.Component;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.GameObject.Combat
{
    public class Weapon : Node2D
    {
        private const float OVERHEAT_DECAY_DELAY = 2f;
        private const string ANIM_FIRE = "fire";

        [Signal]
        public delegate void Fired();
        [Signal]
        public delegate void HeatChanged(Weapon weapon);
        [Signal]
        public delegate void Overheated(Weapon weapon);
        [Signal]
        public delegate void Cooled(Weapon weapon);

        [Export]
        private float _projectilesPerSecond = 10f;

        [Export(PropertyHint.Range, "0,1")]
        private float _heatPerShot = .1f;
        [Export]
        private float _heatDecayPerSecond = .5f;
        [Export]
        private float _heatDecayDelay = .25f;
        [Export]
        private float _heatDecayMultiplier = 1.5f;

        public bool IsPlayer;
        public float CurrentHeat
        {
            get
            {
                return _currentHeat;
            }
            private set
            {
                _currentHeat = value;
                EmitSignal(nameof(HeatChanged), this);
            }
        }

        public Sprite Sprite
        {
            get
            {
                return _sprite;
            }
        }

        private float _currentHeat;
        private float _decayTracker = 0f;
        private bool _canDecay = true;
        private bool _overheated = false;

        private float _timeToNextShot = 0f;

        private AnimationPlayer _animationPlayer;
        private Sprite _sprite;
        private Position2D _chamberPosition;

        private ProjectileSpawnerComponent _projectileSpawnerComponent;
        private ChooseStreamPlayerComponent _chooseStreamPlayerComponent;

        public override void _Ready()
        {
            _projectileSpawnerComponent = this.GetFirstNodeOfType<ProjectileSpawnerComponent>();
            _chooseStreamPlayerComponent = this.GetFirstNodeOfType<ChooseStreamPlayerComponent>();

            _chamberPosition = GetNode<Position2D>("ChamberPosition");
            _sprite = GetNode<Sprite>("Sprite");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            Connect(nameof(Fired), this, nameof(OnFired));
            _projectileSpawnerComponent.Connect(nameof(ProjectileSpawnerComponent.ProjectileSpawned), this, nameof(OnProjectileSpawned));
        }

        public override void _Process(float delta)
        {
            _timeToNextShot = Mathf.Clamp(_timeToNextShot - delta / (IsPlayer ? Engine.TimeScale : 1f), 0f, float.MaxValue);
            if (IsPlayer)
            {
                _animationPlayer.PlaybackSpeed = 1f / Engine.TimeScale;
            }

            if (_overheated)
            {
                UpdateCooldownOverheated();
            }
            else
            {
                UpdateCooldownStandard();
            }
        }

        public void AttemptFire(Vector2 atTarget)
        {
            if (!_overheated && _timeToNextShot == 0f && CurrentHeat < 1f)
            {
                _projectileSpawnerComponent?.Spawn(IsPlayer, atTarget);
            }
        }

        private void UpdateCooldownStandard()
        {
            if (IsPlayer)
            {
                var delta = GetProcessDeltaTime();
                _decayTracker = Mathf.Clamp(_decayTracker - delta / Engine.TimeScale, 0f, float.MaxValue);
                _canDecay = _decayTracker == 0f;
                if (CurrentHeat > 0f && _canDecay)
                {
                    var deltaDecay = _heatDecayPerSecond * delta / Engine.TimeScale;
                    var divisor = CurrentHeat * _heatDecayMultiplier;
                    var totalDelta = Mathf.Clamp(deltaDecay / divisor, deltaDecay, float.MaxValue);
                    CurrentHeat = Mathf.Clamp(CurrentHeat - totalDelta, 0f, 1f);
                }
            }
        }

        private void UpdateCooldownOverheated()
        {
            if (IsPlayer)
            {
                var delta = GetProcessDeltaTime();
                _decayTracker = Mathf.Clamp(_decayTracker - delta / Engine.TimeScale, 0f, float.MaxValue);
                _canDecay = _decayTracker == 0f;
                if (CurrentHeat > 0f && _canDecay)
                {
                    CurrentHeat = Mathf.Clamp(CurrentHeat - _heatDecayPerSecond * delta / Engine.TimeScale, 0f, 1f);
                    if (CurrentHeat == 0f)
                    {
                        _overheated = false;
                        EmitSignal(nameof(Cooled), this);
                    }
                }
            }
        }

        private void OnFired()
        {
            if (_animationPlayer.IsPlaying())
            {
                _animationPlayer.Seek(2f, true);
            }
            _animationPlayer.Play(ANIM_FIRE);
        }

        private void OnProjectileSpawned()
        {
            _timeToNextShot = 1f / _projectilesPerSecond;
            if (IsPlayer)
            {
                CurrentHeat = Mathf.Clamp(CurrentHeat + _heatPerShot, 0f, 1f);
                _decayTracker = _heatDecayDelay;
                if (CurrentHeat == 1f)
                {
                    _overheated = true;
                    _decayTracker = OVERHEAT_DECAY_DELAY;
                }
            }

            _chooseStreamPlayerComponent.PlayAudio();

            EmitSignal(nameof(Fired));
            if (_overheated)
            {
                EmitSignal(nameof(Overheated), this);
            }
        }
    }
}