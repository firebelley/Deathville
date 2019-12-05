using Godot;
using GodotApiTools.Extension;

namespace Deathville.GameObject.Combat
{
    public class PhysicsProjectile : Projectile
    {
        private RigidBody2D _rigidBody;
        private bool _isPlayer;

        public override void _Ready()
        {
            _rigidBody = GetNode<RigidBody2D>("RigidBody2D");
        }

        public override void _PhysicsProcess(float delta)
        {
            _distanceTravelled += _rigidBody.LinearVelocity.LengthSquared() * delta;
            if (_distanceTravelled >= Range * Range)
            {
                Die();
            }

            var dir = _rigidBody.LinearVelocity.Normalized();
            _rigidBody.SetAxisVelocity(_rigidBody.LinearVelocity / Engine.TimeScale);
        }

        public override void SetEnemy()
        {
            _rigidBody.CollisionMask |= (1 << 19);
        }

        public override void SetPlayer()
        {
            _isPlayer = true;
            _rigidBody.CollisionMask |= (1 << 18);
        }

        public override void Start(Vector2 chamberPos, Vector2 spawnPos, Vector2 toPos)
        {
            var raycastResult = GetWorld2d().DirectSpaceState.Raycast(chamberPos, spawnPos, null, _rigidBody.CollisionMask, true, true);
            GlobalPosition = raycastResult != null ? raycastResult.Position : spawnPos;
            _direction = (toPos - chamberPos).Normalized();
            _rigidBody.LinearVelocity = _direction * Speed;
        }
    }
}