using Godot;

namespace Deathville.Component
{
    public class EntityAnimationComponent : AnimatedSprite
    {
        public const string ANIM_IDLE = "idle";
        public const string ANIM_RUN = "run";

        public override void _Ready()
        {

        }

        public void PlayAnimation(string anim)
        {
            Play(anim);
        }

        public void Flip(bool flipH)
        {
            FlipH = flipH;
        }
    }
}