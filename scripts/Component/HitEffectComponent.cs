using Deathville.Environment;
using Deathville.Util;
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
        [Export]
        private PackedScene _hitEffect;

        private Node2D _shadedNode;
        private Tween _tween;
        private DamageReceiverComponent _damageReceiverComponent;

        public override void _Ready()
        {
            _shadedNode = GetNodeOrNull<Node2D>(_shadedNodePath ?? string.Empty);
            if (_shadedNode != null)
            {
                _shadedNode.Material = _shaderMaterial;
            }

            _tween = GetNode<Tween>("Tween");
            _damageReceiverComponent = GetNodeOrNull<DamageReceiverComponent>(_damageReceiverComponentPath ?? string.Empty);
            _damageReceiverComponent?.Connect(nameof(DamageReceiverComponent.DamageReceived), this, nameof(OnDamageReceived));
        }

        private void PlayHitShadeTween()
        {
            _tween.ResetAll();
            _tween.InterpolateProperty(
                _shaderMaterial,
                "shader_param/_hitShadePercent",
                1.0f,
                0f,
                .4f,
                Tween.TransitionType.Linear,
                Tween.EaseType.In
            );
            _tween.Start();
            _tween.PlaybackSpeed = 1f / Engine.TimeScale;
        }

        private void OnDamageReceived(ImpactData impactData)
        {
            if (_hitEffect != null)
            {
                var effect = _hitEffect.Instance() as Node2D;
                Zone.Current.EffectsLayer.AddChild(effect);
                effect.Rotation = impactData.Direction.Angle();

                Vector2? spawnPos = impactData.ImpactPosition ?? _damageReceiverComponent?.GlobalPosition;
                if (spawnPos != null)
                {
                    effect.GlobalPosition = (Vector2) spawnPos;
                }
            }
            PlayHitShadeTween();
        }
    }
}