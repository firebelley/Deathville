using Deathville.GameObject;
using Godot;

namespace Deathville.Component
{
    public class DamageReceiverComponent : Area2D
    {
        [Signal]
        public delegate void DamageReceived(float damage);

        public override void _Ready()
        {
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        private void OnBodyEntered(PhysicsBody2D physicsBody2D)
        {
            if (physicsBody2D is Projectile p)
            {
                p.RegisterHit();
                EmitSignal(nameof(DamageReceived), 0f);
            }
        }
    }
}