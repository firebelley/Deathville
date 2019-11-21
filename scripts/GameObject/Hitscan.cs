using Godot;
using GodotApiTools.Extension;

namespace Deathville.GameObject
{
    public class Hitscan : Projectile
    {
        private const int COLLISION_MASK = (1 << 18) | 1;

        private Line2D _line2d;
        private Light2D _light2d;
        private AnimationPlayer _animationPlayer;
        private Tween _tween;

        public override void _Ready()
        {
            _line2d = GetNode<Line2D>("Line2D");
            _light2d = _line2d.GetNode<Light2D>("Light2D");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _tween = GetNode<Tween>("Tween");
        }

        public override void _Process(float delta)
        {
            _animationPlayer.PlaybackSpeed = 1f / Engine.TimeScale;
            _tween.PlaybackSpeed = 1f / Engine.TimeScale;
        }

        public void Start(Vector2 chamberPos, Vector2 spawnPos, Vector2 toPos)
        {
            _direction = (toPos - chamberPos).Normalized();
            GlobalPosition = spawnPos + _direction * _range;
            var raycastResult = GetWorld2d().DirectSpaceState.Raycast(chamberPos, GlobalPosition, null, COLLISION_MASK, true, true);
            if (raycastResult != null)
            {
                RegisterHit(raycastResult);
            }
            UpdateTrail(spawnPos);
        }

        private void UpdateTrail(Vector2 spawnPos)
        {
            var len = (GlobalPosition - spawnPos).Length();
            _line2d.SetPointPosition(1, Vector2.Left * len);
            _line2d.Rotation = _direction.Angle();

            _light2d.Scale = new Vector2(len / 16f, 1f);
            _light2d.Position = Vector2.Left * len / 2f;

            _tween.InterpolateProperty(
                _line2d,
                "scale",
                Vector2.One,
                Vector2.Zero,
                .25f,
                Tween.TransitionType.Sine,
                Tween.EaseType.Out
            );
            _tween.Start();
        }
    }
}