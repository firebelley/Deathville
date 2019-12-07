using Deathville.Environment;
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
        private NodePath _velocityComponentPath;
        [Export]
        private bool _sendEnemyStruckEvent;
        [Export]
        private PackedScene _hitEffect;

        private VelocityComponent _velocityComponent;

        public override void _Ready()
        {
            _velocityComponent = _velocityComponentPath != null ? GetNode<VelocityComponent>(_velocityComponentPath) : null;
        }

        public void RegisterRaycastHit(Projectile projectile, RaycastResult raycastResult)
        {
            RegisterHit(ImpactData.FromRaycastProjectile(projectile, raycastResult));
            if (_hitEffect != null)
            {
                var effect = _hitEffect.Instance() as Node2D;
                Zone.Current.EffectsLayer.AddChild(effect);
                effect.Rotation = (raycastResult.ToPosition - raycastResult.FromPosition).Angle();
                effect.GlobalPosition = raycastResult.Position;
            }
        }

        public void RegisterAreaOfEffect(Projectile projectile, Vector2 sourcePosition)
        {
            RegisterHit(ImpactData.FromAreaOfEffectProjectile(projectile, sourcePosition));
        }

        private void RegisterHit(ImpactData impactData)
        {
            if (_velocityComponent != null)
            {
                var dir = impactData.Direction;
                if (dir == Vector2.Zero)
                {
                    dir = (GlobalPosition - impactData.SourcePosition).Normalized();
                }
                _velocityComponent.ApplyForce(dir, impactData.Force);
            }

            if (_sendEnemyStruckEvent)
            {
                GameEventDispatcher.DispatchEnemyStruck();
            }
            EmitSignal(nameof(DamageReceived), impactData);
        }
    }
}