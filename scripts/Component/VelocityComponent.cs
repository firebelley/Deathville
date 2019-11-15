using System;
using Godot;

namespace Deathville.Component
{
    public class VelocityComponent : Node
    {
        private const float ACCELERATION = 2000f;
        private const float DECELERATION = 1500f;
        private const float GRAVITY = 800f;

        [Export]
        private bool _obeyTimeScale = true;

        public Vector2 Velocity
        {
            get
            {
                return _velocity;
            }
        }
        private Vector2 _velocity;
        private KinematicBody2D _owner;

        public override void _Ready()
        {
            _owner = Owner as KinematicBody2D;
        }

        public void Accelerate(Vector2 dir, float maxSpeed)
        {
            var mod = dir.x < 0f ? -1f : 1f;
            if (dir.x == 0f)
            {
                mod = 0f;
            }
            _velocity.x += ACCELERATION * GetProcessDeltaTime() * mod / GetTimeScale();
            var abs = Math.Abs(_velocity.x);
            var x = Mathf.Clamp(abs, 0f, maxSpeed);
            _velocity.x = x * Mathf.Sign(_velocity.x);
        }

        public void Decelerate()
        {
            var abs = Mathf.Abs(_velocity.x);
            var x = Mathf.Clamp(abs - DECELERATION * GetProcessDeltaTime() / GetTimeScale(), 0f, float.MaxValue);
            _velocity.x = x * Mathf.Sign(_velocity.x);
        }

        public void Jump(float speed)
        {
            _velocity.y = -speed;
        }

        public void ApplyGravity(float accelerator = 1f)
        {
            var deltaGrav = GRAVITY * GetProcessDeltaTime() / GetTimeScale();
            _velocity.y += deltaGrav * accelerator;
        }

        public void MoveWithSnap()
        {
            _velocity = _owner.MoveAndSlideWithSnap(_velocity / GetTimeScale(), Vector2.Down, Vector2.Up) * GetTimeScale();
        }

        public void Move()
        {
            _velocity = _owner.MoveAndSlide(_velocity / GetTimeScale(), Vector2.Up) * GetTimeScale();
        }

        private float GetTimeScale()
        {
            return _obeyTimeScale ? 1f : Engine.TimeScale;
        }
    }
}