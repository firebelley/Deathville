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

        public void RegisterHit(Projectile projectile, RaycastResult raycastResult)
        {
            if (_sendEnemyStruckEvent)
            {
                GameEventDispatcher.DispatchEnemyStruck();
            }
            EmitSignal(nameof(DamageReceived), 0f);
        }
    }
}