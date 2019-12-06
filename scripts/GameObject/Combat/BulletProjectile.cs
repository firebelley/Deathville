using Godot;
using GodotApiTools.Extension;
using GodotApiTools.Util;

namespace Deathville.GameObject.Combat
{
    public class BulletProjectile : Projectile
    {
        private uint _collisionMask = 1;
        private bool _obeyTimeScale = true;

        public override void _PhysicsProcess(float delta)
        {
            var prevPos = GlobalPosition;
            var deltaDist = Speed * delta / (_obeyTimeScale ? 1f : Engine.TimeScale);
            GlobalPosition += deltaDist * _direction;
            _distanceTravelled += deltaDist;
            var raycastResult = GetWorld2d().DirectSpaceState.Raycast(prevPos, GlobalPosition, null, _collisionMask, true, true);

            if (raycastResult != null)
            {
                RegisterHit(raycastResult);
            }
            else if (_distanceTravelled >= Range)
            {
                Die();
            }
        }

        public override void Start(Vector2 chamberPos, Vector2 spawnPos, Vector2 toPos)
        {
            var raycastResult = GetWorld2d().DirectSpaceState.Raycast(chamberPos, spawnPos, null, _collisionMask, true, true);
            if (raycastResult != null)
            {
                RegisterHit(raycastResult);
            }
            else
            {
                GlobalPosition = spawnPos;
                _direction = (GlobalPosition - chamberPos).Normalized();
                Rotation = _direction.Angle();
            }
        }

        public override void SetPlayer()
        {
            base.SetPlayer();
            _collisionMask |= (1 << 18);
            _obeyTimeScale = false;
        }

        public override void SetEnemy()
        {
            base.SetEnemy();
            _collisionMask |= (1 << 19);
        }

        public override Node2D SpawnEffect(RaycastResult raycastResult = null)
        {
            var death = base.SpawnEffect(raycastResult);
            Die();
            return death;
        }
    }
}