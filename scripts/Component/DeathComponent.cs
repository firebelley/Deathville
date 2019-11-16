using Deathville.GameObject;
using Godot;

namespace Deathville.Component
{
    public class DeathComponent : Node
    {
        [Export]
        private NodePath _healthComponentPath;

        [Export]
        private NodePath _velocityComponentPath;

        [Export]
        private PackedScene _ragdollScene;

        private VelocityComponent _velocityComponent;
        private Node2D _owner;

        public override void _Ready()
        {
            _owner = Owner as Node2D;
            _velocityComponent = GetNodeOrNull<VelocityComponent>(_velocityComponentPath ?? string.Empty);
            GetNodeOrNull<HealthComponent>(_healthComponentPath ?? string.Empty)?.Connect(nameof(HealthComponent.HealthDepleted), this, nameof(OnHealthDepleted));
        }

        private void OnHealthDepleted()
        {
            var ragdoll = _ragdollScene.Instance() as Ragdoll;
            Zone.Current.EntitiesLayer.AddChild(ragdoll);
            ragdoll.GlobalPosition = _owner.GlobalPosition;

            if (_velocityComponent != null)
            {
                ragdoll.ApplyVelocity(_velocityComponent.Velocity);
            }

            _owner.QueueFree();
        }
    }
}