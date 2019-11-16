using Deathville.GameObject;
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

        public void RegisterHit(Projectile projectile, RaycastResult raycastResult)
        {
            if (_sendEnemyStruckEvent)
            {
                GameEventDispatcher.DispatchEnemyStruck();
            }

            if (_hitEffect != null)
            {
                var effect = _hitEffect.Instance() as Node2D;
                Zone.Current.EffectsLayer.AddChild(effect);
                effect.Rotation = (raycastResult.ToPosition - raycastResult.FromPosition).Angle();
                GD.Print(effect.RotationDegrees);
                effect.GlobalPosition = raycastResult.Position;
            }

            EmitSignal(nameof(DamageReceived), 0f);
        }
    }
}