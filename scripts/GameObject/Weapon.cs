using Deathville.GameObject;
using Godot;

namespace Deathville.Component
{
    public class Weapon : Node2D
    {
        private const string ANIM_FIRE = "fire";

        [Signal]
        public delegate void Fired();

        [Export]
        private PackedScene _projectileScene;

        [Export]
        private bool _isHitscan;

        [Export]
        private PackedScene _hitscanScene;

        [Export]
        private float _projectilesPerSecond = 10f;

        public bool IsFriendly;

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
        }

        public void AttemptFire(Vector2 atTarget)
        {
            if (_fireTime == 0f)
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