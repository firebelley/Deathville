using Deathville.GameObject.Combat;
using Godot;

namespace Deathville.Component
{
    public class ProjectileExplosionComponent : Area2D
    {
        [Export]
        private float _detonationDelay = 3f;
        private float _detonationTimer;

        private Projectile _owner;

        public override void _Ready()
        {
            if (Owner is Projectile p)
            {
                _owner = p;
                _owner.Connect(nameof(Projectile.Died), this, nameof(OnProjectileDied));
                _owner.Connect(nameof(Projectile.FactionChanged), this, nameof(OnFactionChanged));
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            _detonationTimer += delta / (_owner.IsPlayer ? Engine.TimeScale : 1f);
            if (_detonationTimer >= _detonationDelay)
            {
                _detonationDelay = float.MaxValue;
                _owner.SpawnEffect();
            }
        }

        private void OnProjectileDied()
        {
            foreach (var area in GetOverlappingAreas())
            {
                if (area is DamageReceiverComponent damageReceiverComponent)
                {
                    damageReceiverComponent.RegisterAreaOfEffect(_owner, GlobalPosition);
                }
            }
        }

        private void OnFactionChanged()
        {
            if (_owner.IsPlayer)
            {
                CollisionMask = 1 << 18;
            }
            else
            {
                CollisionMask = 1 << 19;
            }
        }
    }
}