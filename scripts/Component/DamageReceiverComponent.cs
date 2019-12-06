using Deathville.Environment;
using Deathville.GameObject.Combat;
using Deathville.Singleton;
using Godot;
using GodotApiTools.Util;

namespace Deathville.Component
{
    public class DamageReceiverComponent : Area2D
    {
        [Signal]
        public delegate void DamageReceived(float damage);

        [Export]
        private bool _sendEnemyStruckEvent;
        [Export]
        private PackedScene _hitEffect;

        public void RegisterRaycastHit(Projectile projectile, RaycastResult raycastResult)
        {
            RegisterHit(projectile);
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
            RegisterHit(projectile);
        }

        private void RegisterHit(Projectile projectile)
        {
            if (_sendEnemyStruckEvent)
            {
                GameEventDispatcher.DispatchEnemyStruck();
            }
            EmitSignal(nameof(DamageReceived), 1f);
        }
    }
}