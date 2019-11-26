using Deathville.Component;
using Deathville.Environment;
using Godot;
using GodotApiTools.Util;

namespace Deathville.GameObject
{
    public abstract class Projectile : Node2D
    {
        [Export]
        protected PackedScene _deathScene;
        [Export]
        protected float _range = 250f;

        private int _hitCount = 0;

        protected Vector2 _direction;

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

        public virtual void Die(RaycastResult raycastResult = null)
        {
            var death = _deathScene.Instance() as Node2D;
            Zone.Current.EffectsLayer.AddChild(death);
            death.Rotation = (raycastResult == null ? _direction.Angle() - Mathf.Pi : raycastResult.Normal.Angle());
            death.GlobalPosition = GlobalPosition;
        }
    }
}