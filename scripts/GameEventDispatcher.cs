using Godot;

namespace Deathville
{
    public class GameEventDispatcher : Node
    {
        [Signal]
        public delegate void WeaponFired();
        [Signal]
        public delegate void EnemyStruck();

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
    }
}