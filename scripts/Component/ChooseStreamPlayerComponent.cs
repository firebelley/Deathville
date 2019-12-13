using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotApiTools.Extension;

namespace Deathville.Component
{
    public class ChooseStreamPlayerComponent : AudioStreamPlayer
    {
        [Export]
        private bool _randomOrder = true;
        [Export]
        private float _pitchDifference;
        [Export]
        private Array<AudioStream> _streams;
        [Export]
        private AudioStream _alwaysPlayStream;

        private Queue<int> _streamIndices = new Queue<int>();

        public override void _Ready()
        {
            if (_streams != null)
            {
                foreach (var stream in _streams)
                {
                    var player = new AudioStreamPlayer();
                    AddChild(player);
                    player.Stream = stream;
                    player.Bus = Bus;
                }
                if (_alwaysPlayStream != null)
                {
                    var player = new AudioStreamPlayer();
                    AddChild(player);
                    player.Stream = _alwaysPlayStream;
                    player.Bus = Bus;
                }
            }
        }

        public void PlayAudio()
        {
            if (!HasStreams()) return;

            if (_streamIndices.Count == 0)
            {
                PopulateStreamIndices();
            }

            var idx = _streamIndices.Dequeue();
            GetChild<AudioStreamPlayer>(idx).PlayWithPitchRange(1f - _pitchDifference, 1f + _pitchDifference);

            if (_alwaysPlayStream != null)
            {
                GetChild<AudioStreamPlayer>(_streams.Count).PlayWithPitchRange(1f - _pitchDifference, 1f + _pitchDifference);
            }
        }

        private void PopulateStreamIndices()
        {
            _streamIndices.Clear();
            if (HasStreams())
            {
                var indices = Enumerable.Range(0, _streams.Count);
                if (_randomOrder)
                {
                    indices = indices.OrderBy(x => Main.RNG.Randf());
                }
                foreach (var index in indices)
                {
                    _streamIndices.Enqueue(index);
                }
            }
        }

        private bool HasStreams()
        {
            return _streams != null && _streams.Count > 0;
        }
    }
}