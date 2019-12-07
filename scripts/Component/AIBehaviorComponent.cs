using Deathville.GameObject;
using Deathville.Util;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.Component
{
    public class AIBehaviorComponent : Node
    {
        [Export]
        private NodePath _aiMovementComponentPath;
        [Export]
        private NodePath _damageReceiverComponentPath;
        [Export]
        private float _aggroRange = 100f;

        public bool Aggressive { get; private set; } = true;

        private Node2D _owner;
        private AIMovementComponent _aiMovementComponent;
        private DamageReceiverComponent _damageReceiverComponent;

        public override void _Ready()
        {
            _owner = Owner as Node2D;
            _aiMovementComponent = GetNodeOrNull<AIMovementComponent>(_aiMovementComponentPath ?? string.Empty);
            _damageReceiverComponent = GetNodeOrNull<DamageReceiverComponent>(_damageReceiverComponentPath ?? string.Empty);
            _damageReceiverComponent?.Connect(nameof(DamageReceiverComponent.DamageReceived), this, nameof(OnDamageReceived));
        }

        public override void _PhysicsProcess(float delta)
        {
            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            if (Aggressive || (player != null && _owner.GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < _aggroRange * _aggroRange))
            {
                _aiMovementComponent.TargetPosition = player.GlobalPosition;
                Aggressive = true;
            }
        }

        private void OnDamageReceived(ImpactData impactData)
        {
            Aggressive = true;
        }
    }
}