using Godot;

namespace Deathville.Component
{
    public class TimeScaleRigidBodyComponent : RigidBody2D
    {
        private float _prevTimeScale = 1f;

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            //2nd case
            var downscaled = state.LinearVelocity * _prevTimeScale;

            // 1st case
            LinearVelocity = downscaled / Engine.TimeScale;
            LinearVelocity += (state.TotalGravity / Engine.TimeScale) * state.Step * state.Step;
            _prevTimeScale = Engine.TimeScale;
        }

        public void IgnoreTimescale()
        {
            CustomIntegrator = true;
        }
    }
}