using Godot;

namespace Deathville.GameObject
{
    public class Weapon : Node2D
    {
        private Sprite _sprite;

        public override void _Ready()
        {
            _sprite = GetNode<Sprite>("Sprite");
        }

        public override void _Process(float delta)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var facingLeft = GetGlobalMousePosition().x < GlobalPosition.x;

            _sprite.Scale = facingLeft ? _sprite.Scale.Abs() * new Vector2(-1, 1) : _sprite.Scale.Abs() * Vector2.One;

            Rotation = (GlobalPosition - GetGlobalMousePosition()).Angle() - (!facingLeft ? Mathf.Pi : 0);

        }
    }
}