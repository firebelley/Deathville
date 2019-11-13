using Godot;

namespace Deathville.Component
{
    public class AIMovementComponent : Node
    {
        private Curve2D _curve2d = new Curve2D();
        private float _currentT = 0f;
        private float _lol = 0f;

        public override void _Ready()
        {
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerPositionUpdated), this, nameof(OnPlayerPositionUpdated));
            CallDeferred(nameof(Something), new Vector2(6, 6) * 16f);
        }

        public override void _PhysicsProcess(float delta)
        {
            _currentT += 25f * delta;
            _lol += delta;
            if (_curve2d.GetPointCount() > 0)
            {
                (Owner as Node2D).GlobalPosition = _curve2d.InterpolateBaked(_currentT);
            }
        }

        private void Something(Vector2 desiredPos)
        {
            var path = Zone.Current.Pathfinder.GetGlobalPath((Owner as Node2D).GlobalPosition, desiredPos);
            var line2d = new Line2D();
            _curve2d.ClearPoints();
            _currentT = 0;
            foreach (var point in path)
            {
                _curve2d.AddPoint(point);
                line2d.AddPoint(point);
            }

            line2d.Width = 1f;
            Zone.Current.EffectsLayer.AddChild(line2d);
        }

        private void OnPlayerPositionUpdated(Vector2 pos)
        {
            if (_lol > 3f)
            {
                Something(pos);
                _lol = 0f;
            }
        }
    }
}