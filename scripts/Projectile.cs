using Godot;
using GodotApiTools.Extension;

namespace Deathville.GameObject
{
    public class Projectile : RigidBody2D
    {
        public bool ObeyTimeScale = true;

        [Export]
        private float _speed = 500f;

        private ResourcePreloader _resourcePreloader;

        private Vector2 _direction;
        private int _hitCount = 0;
        private Vector2 _contactPosition;

        public override void _Ready()
        {
            _resourcePreloader = GetNode<ResourcePreloader>("ResourcePreloader");
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            if (state.GetContactCount() > 0)
            {
                _contactPosition = state.GetContactLocalPosition(0);
            }
        }

        public void Start(Vector2 spawnPos, Vector2 toPos)
        {
            GlobalPosition = spawnPos;

            _direction = (toPos - GlobalPosition).Normalized();
            Rotation = _direction.Angle();
            LinearVelocity = CalculateLinearVelocity();
        }

        public void SetFriendly()
        {
            CollisionLayer = 1 << 2;
            CollisionMask |= (1 << 18);
        }

        public void SetEnemy()
        {
            CollisionLayer = 1 << 1;
            CollisionMask |= (1 << 19);
        }

        public void RegisterHit()
        {
            _hitCount++;
            if (_hitCount >= 1)
            {
                Die();
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            LinearVelocity = CalculateLinearVelocity();
        }

        private Vector2 CalculateLinearVelocity()
        {
            return _speed * _direction / (ObeyTimeScale ? 1f : Engine.TimeScale);
        }

        private void Die()
        {
            var death = _resourcePreloader.InstanceScene<Node2D>("BulletDeath");
            Zone.Current.EffectsLayer.AddChild(death);
            death.Rotation = _direction.Angle() - Mathf.Pi;
            death.GlobalPosition = _contactPosition != Vector2.Zero ? _contactPosition : GlobalPosition;
            QueueFree();
        }

        private void OnBodyEntered(PhysicsBody2D body2D)
        {
            Die();
        }
    }
}