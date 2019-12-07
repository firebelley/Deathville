using Deathville.Component;
using Deathville.Environment;
using Deathville.Singleton;
using Godot;
using GodotApiTools.Util;

namespace Deathville.GameObject.Combat
{
    public abstract class Projectile : Node2D
    {
        [Signal]
        public delegate void Died();
        [Signal]
        public delegate void FactionChanged();

        [Export]
        protected PackedScene _deathScene;
        [Export]
        protected bool _deathIsExplosion;
        [Export]
        private float _explosionCameraShake = 1f;

        public float Speed;
        public float Range;
        public float Force;
        public float Damage = 1f;
        public bool IsPlayer { get; private set; }

        protected Vector2 _direction;
        protected float _distanceTravelled;

        private int _hitCount = 0;

        public void RegisterHit(RaycastResult raycastResult)
        {
            if (raycastResult.Collider is DamageReceiverComponent drc)
            {
                drc.RegisterRaycastHit(this, raycastResult);
            }
            _hitCount++;
            if (_hitCount >= 1)
            {
                GlobalPosition = raycastResult.Position;
                SpawnEffect(raycastResult);
            }
        }

        public virtual Node2D SpawnEffect(RaycastResult raycastResult = null)
        {
            if (_deathScene == null) return null;
            var death = _deathScene.Instance() as Node2D;
            Zone.Current.EffectsLayer.AddChild(death);
            death.Rotation = (raycastResult == null ? _direction.Angle() - Mathf.Pi : raycastResult.Normal.Angle());
            death.GlobalPosition = GlobalPosition;

            if (_deathIsExplosion)
            {
                GameEventDispatcher.DispatchCameraShaken(_explosionCameraShake);
            }
            return death;
        }

        public virtual void Die()
        {
            EmitSignal(nameof(Died));
            QueueFree();
        }

        public abstract void Start(Vector2 chamberPos, Vector2 spawnPos, Vector2 toPos);

        public virtual void SetPlayer()
        {
            IsPlayer = true;
            EmitSignal(nameof(FactionChanged));
        }

        public virtual void SetEnemy()
        {
            IsPlayer = false;
            EmitSignal(nameof(FactionChanged));
        }
    }
}