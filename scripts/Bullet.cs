using Godot;

namespace Deathville.GameObject
{
    public class Bullet : RigidBody2D
    {
        public bool ObeyTimeScale = true;

        [Export]
        private float _speed = 500f;

        private Vector2 _direction;

        public override void _Ready()
        {
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        public void Start(Vector2 spawnPos, Vector2 toPos)
        {
            GlobalPosition = spawnPos;

            _direction = (toPos - GlobalPosition).Normalized();
            Rotation = _direction.Angle();
            LinearVelocity = CalculateLinearVelocity();
        }

        public override void _PhysicsProcess(float delta)
        {
            LinearVelocity = CalculateLinearVelocity();
        }

        private Vector2 CalculateLinearVelocity()
        {
            return _speed * _direction / (ObeyTimeScale ? 1f : Engine.TimeScale);
        }

        private void OnBodyEntered(PhysicsBody2D body2D)
        {
            QueueFree();
        }
    }
}