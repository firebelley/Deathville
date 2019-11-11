using Godot;

namespace Deathville
{
    public class GameEventDispatcher : Node
    {
        [Signal]
        public delegate void PlayerPositionUpdated(Vector2 globalPosition);

        public static GameEventDispatcher Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }

        public static void DispatchPlayerPositionUpdated(Vector2 globalPosition)
        {
            Instance.EmitSignal(nameof(PlayerPositionUpdated), globalPosition);
        }
    }
}