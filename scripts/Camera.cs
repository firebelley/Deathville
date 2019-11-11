using Godot;

namespace Deathville
{
    public class Camera : Camera2D
    {
        private Vector2 _targetPos;

        public override void _Ready()
        {
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerPositionUpdated), this, nameof(OnPlayerPositionUpdated));
        }

        public override void _Process(float delta)
        {
            GlobalPosition = GlobalPosition.LinearInterpolate(_targetPos, 10 * delta / Engine.TimeScale);
        }

        private void OnPlayerPositionUpdated(Vector2 globalPosition)
        {
            _targetPos = globalPosition;
        }
    }
}