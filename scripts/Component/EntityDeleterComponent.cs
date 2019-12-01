using Godot;

namespace Deathville.Component
{
    public class EntityDeleterComponent : VisibilityNotifier2D
    {
        private bool _oneFramePassed = false;

        public override void _PhysicsProcess(float delta)
        {
            if (!_oneFramePassed)
            {
                _oneFramePassed = true;
                return;
            }

            if (!IsOnScreen())
            {
                GetParent().QueueFree();
            }
        }
    }
}