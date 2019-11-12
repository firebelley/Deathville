using Deathville.GameObject;
using Godot;
using GodotApiTools.Util;

namespace Deathville.Component
{
    public class DamageReceiverComponent : Area2D
    {
        [Signal]
        public delegate void DamageReceived(float damage);

        public override void _Ready()
        {

        }

        public void RegisterHit(Projectile projectile, RaycastResult raycastResult)
        {
            EmitSignal(nameof(DamageReceived), 0f);
        }
    }
}