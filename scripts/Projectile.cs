using Deathville.Component;
using Godot;
using GodotApiTools.Extension;
using GodotApiTools.Util;

namespace Deathville.GameObject
{
    public class Projectile : Node2D
    {
        public bool ObeyTimeScale = true;

        [Export]
        private float _speed = 500f;

        private ResourcePreloader _resourcePreloader;

        private Vector2 _direction;
        private int _hitCount = 0;
        private uint _collisionMask = 1;

        public override void _Ready()
        {
            _resourcePreloader = GetNode<ResourcePreloader>("ResourcePreloader");
        }

        public override void _PhysicsProcess(float delta)
        {
            var prevPos = GlobalPosition;
            GlobalPosition += _speed * delta * _direction / (ObeyTimeScale ? 1f : Engine.TimeScale);
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
        }

        public void SetEnemy()
        {
            _collisionMask |= (1 << 19);
        }

        public void RegisterHit(RaycastResult raycastResult)
        {
            if (raycastResult.Collider is DamageReceiverComponent drc)
            {
                drc.RegisterHit(this, raycastResult);
            }
            _hitCount++;
            if (_hitCount >= 1)
            {
                GlobalPosition = raycastResult.Position;
                Die(raycastResult);
            }
        }

        private void Die(RaycastResult raycastResult = null)
        {
            var death = _resourcePreloader.InstanceScene<Node2D>("BulletDeath");
            Zone.Current.EffectsLayer.AddChild(death);
            death.Rotation = (raycastResult == null ? _direction.Angle() - Mathf.Pi : raycastResult.Normal.Angle());
            death.GlobalPosition = GlobalPosition;
            QueueFree();
        }
    }
}