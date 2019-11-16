using Deathville.GameObject;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.Component
{
    public class AIAimComponent : Node2D
    {
        [Export]
        private NodePath _weaponSocketComponentPath;

        private WeaponSocketComponent _weaponSocketComponent;

        public override void _Ready()
        {
            _weaponSocketComponent = GetNode<WeaponSocketComponent>(_weaponSocketComponentPath);
        }

        public override void _Process(float delta)
        {
            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            if (player != null)
            {
                _weaponSocketComponent.AimWeapon(player.GlobalPosition);
            }
        }
    }
}