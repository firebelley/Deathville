using Deathville.GameObject;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.Component
{
    public class AIAimComponent : Node2D
    {
        [Export]
        private NodePath _weaponSocketComponentPath;
        [Export]
        private NodePath _aiBehaviorComponentPath;

        private WeaponSocketComponent _weaponSocketComponent;
        private AIBehaviorComponent _aiBehaviorComponent;

        public override void _Ready()
        {
            _weaponSocketComponent = GetNode<WeaponSocketComponent>(_weaponSocketComponentPath);
            _aiBehaviorComponent = GetNode<AIBehaviorComponent>(_aiBehaviorComponentPath);
        }

        public override void _Process(float delta)
        {
            if (_aiBehaviorComponent.Aggressive)
            {
                var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
                if (player != null)
                {
                    _weaponSocketComponent.AimWeapon(player.GlobalPosition);
                    _weaponSocketComponent.Weapon?.AttemptFire(player.GlobalPosition);
                }
            }
            else
            {
                _weaponSocketComponent.ResetWeaponAim();
            }
        }
    }
}