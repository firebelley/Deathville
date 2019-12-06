using Deathville.Component;
using Godot;
using GodotApiTools.Extension;
using GodotApiTools.Util;

namespace Deathville.GameObject.Combat
{
    public class PhysicsProjectile : Projectile
    {
        private RigidBody2D _rigidBody;
        private Vector2 _prevPosition;

        public override void _Ready()
        {
            _rigidBody = GetNode<RigidBody2D>("RigidBody2D");
        }

        public override void _PhysicsProcess(float delta)
        {
            if (_prevPosition != Vector2.Zero)
            {
                _distanceTravelled += (_prevPosition - _rigidBody.GlobalPosition).ApproximateLength();
                if (_distanceTravelled >= Range)
                {
                    SpawnEffect();
                }
                else
                {
                    var raycastResult = GetWorld2d().DirectSpaceState.Raycast(_prevPosition, GlobalPosition, null, _rigidBody.CollisionMask, true, true);
                    if (raycastResult?.Collider is DamageReceiverComponent)
                    {
                        SpawnEffect();
                    }
                }
            }
            _prevPosition = _rigidBody.GlobalPosition;
        }

        public override void SetEnemy()
        {
            base.SetEnemy();
            _rigidBody.CollisionMask |= (1 << 19);
        }

        public override void SetPlayer()
        {
            base.SetPlayer();
            _rigidBody.CollisionMask |= (1 << 18);
        }

        public override void Start(Vector2 chamberPos, Vector2 spawnPos, Vector2 toPos)
        {
            var raycastResult = GetWorld2d().DirectSpaceState.Raycast(chamberPos, spawnPos, null, _rigidBody.CollisionMask, true, true);
            GlobalPosition = raycastResult != null ? raycastResult.Position : spawnPos;
            _direction = (toPos - chamberPos).Normalized();
            _rigidBody.LinearVelocity = _direction * Speed;
            _rigidBody.AngularVelocity = Main.RNG.RandfRange(-60f, 60f);
        }

        public override Node2D SpawnEffect(RaycastResult raycastResult = null)
        {
            var death = base.SpawnEffect(raycastResult);
            death.GlobalPosition = _rigidBody.GlobalPosition;
            Die();
            return death;
        }
    }
}