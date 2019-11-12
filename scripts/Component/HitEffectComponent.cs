using Godot;

namespace Deathville.Component
{
    public class HitEffectComponent : Node
    {
        [Export]
        private ShaderMaterial _shaderMaterial;
        [Export]
        private NodePath _shadedNodePath;
        [Export]
        private NodePath _damageReceiverComponentPath;

        private Node2D _shadedNode;
        private Tween _tween;

        public override void _Ready()
        {
            _shadedNode = GetNodeOrNull<Node2D>(_shadedNodePath ?? string.Empty);
            if (_shadedNode != null)
            {
                _shadedNode.Material = _shaderMaterial;
            }

            _tween = GetNode<Tween>("Tween");
            GetNodeOrNull<DamageReceiverComponent>(_damageReceiverComponentPath ?? string.Empty)?.Connect(nameof(DamageReceiverComponent.DamageReceived), this, nameof(OnDamageReceived));
        }

        private void PlayHitShadeTween()
        {
            _tween.ResetAll();
            _tween.InterpolateProperty(
                _shaderMaterial,
                "shader_param/_hitShadePercent",
                1.0f,
                0f,
                .3f,
                Tween.TransitionType.Linear,
                Tween.EaseType.In
            );
            _tween.Start();
            _tween.PlaybackSpeed = 1f / Engine.TimeScale;
        }

        private void OnDamageReceived(float damage)
        {
            PlayHitShadeTween();
        }
    }
}