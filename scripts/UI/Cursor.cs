using Godot;

namespace Deathville.UI
{
    public class Cursor : CanvasLayer
    {
        private Node2D _node2d;

        public override void _Ready()
        {
            _node2d = GetNode<Node2D>("Node2D");
            Input.SetMouseMode(Input.MouseMode.Hidden);
        }

        public override void _Process(float delta)
        {
            _node2d.GlobalPosition = _node2d.GetGlobalMousePosition();
        }
    }
}