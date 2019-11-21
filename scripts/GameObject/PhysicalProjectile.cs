using Godot;
using GodotApiTools.Extension;
using GodotApiTools.Util;

namespace Deathville.GameObject
{
    public class PhysicalProjectile : Projectile
    {
        [Export]
        private float _speed = 500f;

        private uint _collisionMask = 1;
        private bool _obeyTimeScale = true;

        public override void _PhysicsProcess(float delta)
        {
            var prevPos = GlobalPosition;
            GlobalPosition += _speed * delta * _direction / (_obeyTimeScale ? 1f : Engine.TimeScale);
            var raycastResult = GetWorld2d().DirectSpaceState.Raycast(prevPos, GlobalPosition, null, _collisionMask, true, true);

            if (raycastResult != null)
            {
                RegisterHit(raycastResult);
            }
        }

        public void Start(Vector2 chamberPos, Vector2 spawnPos, Vector2 toPos)
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

        public void SetFriendly()
        {
            _collisionMask |= (1 << 18);
            _obeyTimeScale = false;
        }

        public void SetEnemy()
        {
            _collisionMask |= (1 << 19);
        }

        public override void Die(RaycastResult raycastResult = null)
        {
            base.Die(raycastResult);
            QueueFree();
        }
    }
}