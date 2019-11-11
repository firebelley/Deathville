using System;
using Godot;

namespace Deathville.Component
{
    public class DamageReceiverComponent : Area2D
    {
        public override void _Ready()
        {
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        private void OnBodyEntered(PhysicsBody2D physicsBody2D)
        {
            GD.Print("oof");
        }
    }
}