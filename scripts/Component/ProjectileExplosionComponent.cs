using Deathville.GameObject.Combat;
using Godot;

namespace Deathville.Component
{
    public class ProjectileExplosionComponent : Area2D
    {
        [Export]
        private float _detonationDelay = 3f;

        private Projectile _owner;

        public override void _Ready()
        {
            if (Owner is Projectile p)
            {
                _owner = p;
                _owner.Connect(nameof(Projectile.Died), this, nameof(OnProjectileDied));
                _owner.Connect(nameof(Projectile.FactionChanged), this, nameof(OnFactionChanged));
            }
            var timer = GetNode<Timer>("DetonationTimer");
            timer.WaitTime = _detonationDelay;
            timer.Start();
            timer.Connect("timeout", this, nameof(OnTimerTimeout));
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

        private void OnTimerTimeout()
        {
            _owner?.SpawnEffect();
        }
    }
}