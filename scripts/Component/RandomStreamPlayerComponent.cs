using Godot;
using GodotApiTools.Extension;

namespace Deathville.Component
{
    public class RandomStreamPlayerComponent : AudioStreamPlayer2D
    {
        [Export]
        private Godot.Collections.Array<AudioStream> _streams;
        [Export]
        private float _pitchDifference = .1f;
        [Export]
        private bool _scaleWithTimeScale;

        private float _basePitch = 1f;

        public override void _Ready()
        {
            SetProcess(_scaleWithTimeScale);
        }

        public override void _Process(float delta)
        {
            PitchScale = _basePitch * Mathf.Clamp(Engine.TimeScale, .5f, 1f);
        }

        public void PlayAudio()
        {
            _basePitch = 1f + Main.RNG.RandfRange(-_pitchDifference, _pitchDifference);
            if (_streams != null && _streams.Count > 0)
            {
                var idx = Main.RNG.RandiRange(0, _streams.Count - 1);
                Stream = _streams[idx];
                PitchScale = _basePitch;
                Play();
            }
        }
    }
}