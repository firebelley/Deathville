using Godot;

namespace Deathville.Component
{
    public class TimeScaleRigidBodyComponent : RigidBody2D
    {
        private float _prevTimeScale = 1f;

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            //2nd case
            var downscaledVel = state.LinearVelocity * _prevTimeScale;

            // 1st case
            LinearVelocity = downscaledVel / Engine.TimeScale;

            var downscaledAngular = state.AngularVelocity * _prevTimeScale;
            AngularVelocity = downscaledAngular / Engine.TimeScale;

            var downscaledGravity = state.TotalGravity * _prevTimeScale;
            var upscaledGravity = downscaledGravity / Engine.TimeScale;

            LinearVelocity += upscaledGravity * (state.Step / Engine.TimeScale);

            _prevTimeScale = Engine.TimeScale;
        }

        public void IgnoreTimescale()
        {
            CustomIntegrator = true;
        }
    }
}