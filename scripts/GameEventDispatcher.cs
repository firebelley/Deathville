using Godot;

namespace Deathville
{
    public class GameEventDispatcher : Node
    {
        [Signal]
        public delegate void PlayerPositionUpdated(Vector2 globalPosition);
        [Signal]
        public delegate void WeaponFired();

        public static GameEventDispatcher Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }

        public static void DispatchPlayerPositionUpdated(Vector2 globalPosition)
        {
            Instance.EmitSignal(nameof(PlayerPositionUpdated), globalPosition);
        }

        public static void DispatchWeaponFired()
        {
            Instance.EmitSignal(nameof(WeaponFired));
        }
    }
}