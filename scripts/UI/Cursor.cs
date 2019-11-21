using Godot;

namespace Deathville.UI
{
    public class Cursor : CanvasLayer
    {
        private Sprite _sprite;

        public override void _Ready()
        {
            _sprite = GetNode<Sprite>("Sprite");
            Input.SetMouseMode(Input.MouseMode.Hidden);
        }

        public override void _Process(float delta)
        {
            _sprite.GlobalPosition = _sprite.GetGlobalMousePosition();
        }
    }
}