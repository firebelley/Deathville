using Deathville.Singleton;
using Godot;

namespace Deathville.UI
{
    public class PlayerHealth : HBoxContainer
    {
        private Control _healthSegments;
        public override void _Ready()
        {
            _healthSegments = GetNode<Control>("Background/HealthSegments");
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerHealthChanged), this, nameof(OnPlayerHealthChanged));
        }

        private void OnPlayerHealthChanged(float currentHealth)
        {
            foreach (var child in _healthSegments.GetChildren())
            {
                if (child is Control control)
                {
                    if (control.GetIndex() >= currentHealth)
                    {
                        control.Hide();
                    }
                    else
                    {
                        control.Show();
                    }
                }
            }
        }
    }
}