using Deathville.GameObject.Combat;
using Godot;

namespace Deathville.Singleton
{
    public class GameEventDispatcher : Node
    {
        [Signal]
        public delegate void WeaponFired();
        [Signal]
        public delegate void EnemyStruck();
        [Signal]
        public delegate void PlayerHealthChanged(float currentHealth);
        [Signal]
        public delegate void PlayerWeaponEquipped(Weapon weapon);
        [Signal]
        public delegate void CameraShaken(float magnitude);

        public static GameEventDispatcher Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }

        public static void DispatchWeaponFired()
        {
            Instance.EmitSignal(nameof(WeaponFired));
        }

        public static void DispatchEnemyStruck()
        {
            Instance.EmitSignal(nameof(EnemyStruck));
        }

        public static void DispatchPlayerHealthChanged(float currentHealth)
        {
            Instance.EmitSignal(nameof(PlayerHealthChanged), currentHealth);
        }

        public static void DispatchPlayerWeaponEquipped(Weapon weapon)
        {
            Instance.EmitSignal(nameof(PlayerWeaponEquipped), weapon);
        }

        public static void DispatchCameraShaken(float magnitude)
        {
            Instance.EmitSignal(nameof(CameraShaken), magnitude);
        }
    }
}