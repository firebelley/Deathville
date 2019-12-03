using Deathville.Environment;
using Godot;

namespace Deathville.GameObject
{
    public class Weapon : Node2D
    {
        private const string ANIM_FIRE = "fire";

        [Signal]
        public delegate void Fired();
        [Signal]
        public delegate void HeatChanged(Weapon weapon);

        [Export]
        private PackedScene _projectileScene;

        [Export]
        private bool _isHitscan;

        [Export]
        private PackedScene _hitscanScene;

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

        public bool IsFriendly;
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

        private float _currentHeat;
        private float _decayTracker = 0f;
        private bool _canDecay = true;

        private Position2D _muzzlePosition;
        private Position2D _chamberPosition;

        private Timer _fireTimer;
        private AnimationPlayer _animationPlayer;

        private float _fireTime = 0f;

        public override void _Ready()
        {
            _fireTimer = GetNode<Timer>("FireTimer");
            _muzzlePosition = GetNode<Position2D>("MuzzlePosition");
            _chamberPosition = GetNode<Position2D>("ChamberPosition");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            Connect(nameof(Fired), this, nameof(OnFired));
        }

        public override void _Process(float delta)
        {
            _fireTime = Mathf.Clamp(_fireTime - delta / (IsFriendly ? Engine.TimeScale : 1f), 0f, float.MaxValue);
            UpdateCooldown();
        }

        public void AttemptFire(Vector2 atTarget)
        {
            if (_fireTime == 0f && CurrentHeat < 1f)
            {
                Fire(atTarget);
                _fireTime = 1f / _projectilesPerSecond;
            }
        }

        public void Fire(Vector2 atTarget)
        {
            if (!_isHitscan)
            {
                CreatePhysicalProjectile(atTarget);
            }
            else
            {
                CreateHitscanProjectile(atTarget);
            }

            if (IsFriendly)
            {
                CurrentHeat = Mathf.Clamp(CurrentHeat + _heatPerShot, 0f, 1f);
                _decayTracker = _heatDecayDelay;
            }
        }

        private void CreatePhysicalProjectile(Vector2 atTarget)
        {
            if (_projectileScene == null) return;

            var projectile = _projectileScene.Instance() as PhysicalProjectile;
            Zone.Current.EffectsLayer.AddChild(projectile);
            if (IsFriendly)
            {
                projectile.SetFriendly();
            }
            else
            {
                projectile.SetEnemy();
            }

            projectile.Start(_chamberPosition.GlobalPosition, _muzzlePosition.GlobalPosition, atTarget);
            EmitSignal(nameof(Fired));
        }

        private void CreateHitscanProjectile(Vector2 atTarget)
        {
            if (_hitscanScene == null) return;
            var projectile = _hitscanScene.Instance() as Hitscan;
            Zone.Current.EffectsLayer.AddChild(projectile);
            projectile.Start(_chamberPosition.GlobalPosition, _muzzlePosition.GlobalPosition, atTarget);
            EmitSignal(nameof(Fired));
        }

        private void UpdateCooldown()
        {
            if (IsFriendly)
            {
                var delta = GetProcessDeltaTime();
                _decayTracker = Mathf.Clamp(_decayTracker - delta / Engine.TimeScale, 0f, _heatDecayDelay);
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

        private void OnFired()
        {
            if (_animationPlayer.IsPlaying())
            {
                _animationPlayer.Seek(2f, true);
            }
            _animationPlayer.Play(ANIM_FIRE);
        }
    }
}