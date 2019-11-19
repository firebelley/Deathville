using Godot;

namespace Deathville.Component
{
    public class EntitySeparatorComponent : Area2D
    {
        private const float SEPARATION_FORCE = 1500f;

        [Export]
        private NodePath _velocityComponentPath;

        private VelocityComponent _velocityComponent;
        private KinematicBody2D _owner;

        public override void _Ready()
        {
            _owner = Owner as KinematicBody2D;
            _velocityComponent = GetNode<VelocityComponent>(_velocityComponentPath);
            Connect("area_entered", this, nameof(OnAreaEntered));
        }

        public override void _PhysicsProcess(float delta)
        {
            if (!_owner.IsOnFloor()) return;

            foreach (var area in GetOverlappingAreas())
            {
                if (area is EntitySeparatorComponent esc)
                {
                    var dir = new Vector2(Mathf.Sign(GlobalPosition.x - esc.GlobalPosition.x), 0f);
                    _velocityComponent.ApplyForce(dir, SEPARATION_FORCE * delta);
                }
            }
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area is EntitySeparatorComponent esc)
            {

            }
        }
    }
}