using Deathville.Util;
using Godot;

namespace Deathville.Component
{
    public class ImpactHandlerComponent : Node
    {
        [Export]
        private NodePath _velocityComponentPath;
        [Export]
        private NodePath _damageReceiverComponentPath;

        private VelocityComponent _velocityComponent;
        private DamageReceiverComponent _damageReceiverComponent;

        public override void _Ready()
        {
            _velocityComponent = _velocityComponentPath != null ? GetNode<VelocityComponent>(_velocityComponentPath) : null;
            _damageReceiverComponent = _damageReceiverComponentPath != null ? GetNode<DamageReceiverComponent>(_damageReceiverComponentPath) : null;
            _damageReceiverComponent?.Connect(nameof(DamageReceiverComponent.DamageReceived), this, nameof(OnDamageReceived));
        }

        private void OnDamageReceived(ImpactData impactData)
        {
            if (_velocityComponent != null)
            {
                var dir = impactData.Direction;
                if (dir == Vector2.Zero)
                {
                    dir = (_damageReceiverComponent.GlobalPosition - impactData.SourcePosition).Normalized();
                }
                _velocityComponent.ApplyKnockback(dir, impactData.Force);
            }
        }
    }
}