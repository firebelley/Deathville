using Deathville.GameObject.Combat;
using Deathville.Singleton;
using Deathville.Util;
using Godot;
using GodotApiTools.Util;

namespace Deathville.Component
{
    public class DamageReceiverComponent : Area2D
    {
        [Signal]
        public delegate void DamageReceived(ImpactData impactData);

        [Export]
        private bool _sendEnemyStruckEvent;

        private VelocityComponent _velocityComponent;

        public void RegisterRaycastHit(Projectile projectile, RaycastResult raycastResult)
        {
            RegisterHit(ImpactData.FromRaycastProjectile(projectile, raycastResult));
        }

        public void RegisterAreaOfEffect(Projectile projectile, Vector2 sourcePosition)
        {
            RegisterHit(ImpactData.FromAreaOfEffectProjectile(projectile, sourcePosition));
        }

        private void RegisterHit(ImpactData impactData)
        {
            if (_sendEnemyStruckEvent)
            {
                GameEventDispatcher.DispatchEnemyStruck();
            }
            EmitSignal(nameof(DamageReceived), impactData);
        }
    }
}