using Godot;

namespace Deathville.Component
{
    public class EntityDeleterComponent : VisibilityNotifier2D
    {
        public override void _PhysicsProcess(float delta)
        {
            if (!IsOnScreen())
            {
                GetParent().QueueFree();
            }
        }
    }
}