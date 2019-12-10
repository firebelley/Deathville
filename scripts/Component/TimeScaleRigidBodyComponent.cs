using Godot;

namespace Deathville.Component
{
    public class TimeScaleRigidBodyComponent : RigidBody2D
    {
        private float _prevTimeScale = 1f;

        private bool _ignoreTimeScale;

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            if (!_ignoreTimeScale) return;

            var downscaledVel = state.LinearVelocity * _prevTimeScale;
            LinearVelocity = downscaledVel / Engine.TimeScale;

            var gravityDiff = (state.TotalGravity / Engine.TimeScale) - state.TotalGravity;
            LinearVelocity += gravityDiff * (state.Step / Engine.TimeScale);

            _prevTimeScale = Engine.TimeScale;
        }

        public void IgnoreTimescale()
        {
            _ignoreTimeScale = true;
        }
    }
}