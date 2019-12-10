using Deathville.Component;
using Godot;
using GodotApiTools.Extension;
using GodotApiTools.Util;

namespace Deathville.GameObject.Combat
{
    public class PhysicsProjectile : Projectile
    {
        private TimeScaleRigidBodyComponent _timescaleBody;
        private Vector2 _prevPosition;

        public override void _Ready()
        {
            _timescaleBody = GetNode<TimeScaleRigidBodyComponent>("TimeScaleRigidBodyComponent");
        }

        public override void _PhysicsProcess(float delta)
        {
            if (_prevPosition != Vector2.Zero)
            {
                _distanceTravelled += (_prevPosition - _timescaleBody.GlobalPosition).ApproximateLength();
                if (_distanceTravelled >= Range)
                {
                    SpawnEffect();
                }
                else
                {
                    var raycastResult = GetWorld2d().DirectSpaceState.Raycast(_prevPosition, GlobalPosition, null, _timescaleBody.CollisionMask, true, true);
                    if (raycastResult?.Collider is DamageReceiverComponent)
                    {
                        SpawnEffect();
                    }
                }
            }
            _prevPosition = _timescaleBody.GlobalPosition;
        }

        public override void SetEnemy()
        {
            base.SetEnemy();
            _timescaleBody.CollisionMask |= (1 << 19);
        }

        public override void SetPlayer()
        {
            base.SetPlayer();
            _timescaleBody.CollisionMask |= (1 << 18);
            _timescaleBody.IgnoreTimescale();
        }

        public override void Start(Vector2 chamberPos, Vector2 spawnPos, Vector2 toPos)
        {
            var raycastResult = GetWorld2d().DirectSpaceState.Raycast(chamberPos, spawnPos, null, _timescaleBody.CollisionMask, true, true);
            GlobalPosition = raycastResult != null ? raycastResult.Position : spawnPos;
            _direction = (toPos - chamberPos).Normalized();
            _timescaleBody.LinearVelocity = _direction * Speed;
            _timescaleBody.AngularVelocity = GetAngleMod(_direction) * 60f;
        }

        public override Node2D SpawnEffect(RaycastResult raycastResult = null)
        {
            var death = base.SpawnEffect(raycastResult);
            death.GlobalPosition = _timescaleBody.GlobalPosition;
            Die();
            return death;
        }

        private float GetAngleMod(Vector2 direction)
        {
            var mod = Mathf.Abs(direction.x);
            if (_direction.y > 0f)
            {
                return mod * 1f;
            }
            else
            {
                return mod * -1f;
            }
        }
    }
}