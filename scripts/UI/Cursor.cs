using Deathville.GameObject.Combat;
using Deathville.Singleton;
using Godot;

namespace Deathville.UI
{
    public class Cursor : CanvasLayer
    {
        private Node2D _node2d;
        private OverheatBar _overheatBar;

        public override void _Ready()
        {
            _node2d = GetNode<Node2D>("Node2D");
            _overheatBar = GetNode<OverheatBar>("Node2D/OverheatBar");
            Input.SetMouseMode(Input.MouseMode.Hidden);

            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerWeaponSwapped), this, nameof(OnPlayerWeaponSwapped));
        }

        public override void _Process(float delta)
        {
            _node2d.GlobalPosition = _node2d.GetGlobalMousePosition();
        }

        private void OnPlayerWeaponSwapped(Weapon weapon)
        {
            _overheatBar.ConnectWeapon(weapon);
        }
    }
}