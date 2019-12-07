using Deathville.Util;
using Godot;

namespace Deathville.Component
{
    public class HealthComponent : Node
    {
        [Signal]
        public delegate void HealthChanged();
        [Signal]
        public delegate void HealthDepleted();

        [Export]
        private float _maxHealth;
        [Export]
        private NodePath _damageReceiverComponentPath;
        [Export]
        private float _damageOverride = 0f;

        public float CurrentHealth
        {
            get
            {
                return _currentHealth;
            }
            private set
            {
                _currentHealth = value;
                EmitSignal(nameof(HealthChanged));
                if (_currentHealth <= 0f)
                {
                    EmitSignal(nameof(HealthDepleted));
                }
            }
        }
        private float _currentHealth;

        public override void _Ready()
        {
            _currentHealth = _maxHealth;
            GetNode<DamageReceiverComponent>(_damageReceiverComponentPath).Connect(nameof(DamageReceiverComponent.DamageReceived), this, nameof(OnDamageReceived));
        }

        private void OnDamageReceived(ImpactData impactData)
        {
            CurrentHealth -= _damageOverride > 0f ? _damageOverride : impactData.Damage;
        }
    }
}