using Godot;
using GodotApiTools.Extension;

namespace Deathville.Component
{
    public class StreamPlayerComponent : AudioStreamPlayer2D
    {
        [Export]
        private float _pitchDifference = .1f;
        [Export]
        private bool _scaleWithTimeScale;

        private float _basePitch = 1f;

        public override void _Ready()
        {
            SetProcess(_scaleWithTimeScale);
            if (Autoplay)
            {
                _basePitch = 1f + Main.RNG.RandfRange(-_pitchDifference, _pitchDifference);
                PitchScale = _basePitch;
            }
        }

        public override void _Process(float delta)
        {
            PitchScale = _basePitch * Mathf.Clamp(Engine.TimeScale, .5f, 1f);
        }

        public void PlayAudio()
        {
            this.PlayWithPitchRange(1f - _pitchDifference, 1f + _pitchDifference);
        }
    }
}