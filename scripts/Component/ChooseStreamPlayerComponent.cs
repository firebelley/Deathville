using System.Collections.Generic;
using System.Linq;
using Deathville.Util;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.Component
{
    public class ChooseStreamPlayerComponent : AudioStreamPlayer2D
    {
        [Export]
        private bool _randomOrder = true;
        [Export]
        private float _pitchDifference = .1f;
        [Export]
        private NodePath _alwaysPlayPath;
        [Export]
        private NodePath _damageReceiverComponentPath;

        private Queue<int> _streamIndices = new Queue<int>();
        private AudioStreamPlayer2D _alwaysPlay;

        public override void _Ready()
        {
            if (_alwaysPlayPath != null)
            {
                _alwaysPlay = GetNode<AudioStreamPlayer2D>(_alwaysPlayPath);
            }
            if (_damageReceiverComponentPath != null)
            {
                GetNode<DamageReceiverComponent>(_damageReceiverComponentPath).Connect(nameof(DamageReceiverComponent.DamageReceived), this, nameof(OnDamageReceived));
            }
            foreach (var child in GetChildren())
            {
                if (child is AudioStreamPlayer2D player)
                {
                    player.Bus = Bus;
                    player.VolumeDb = VolumeDb;
                    player.Attenuation = Attenuation;
                    player.MaxDistance = MaxDistance;
                }
            }
        }

        public void PlayAudio()
        {
            if (_streamIndices.Count == 0)
            {
                PopulateStreamIndices();
            }

            if (_streamIndices.Count == 0)
            {
                return;
            }

            var idx = _streamIndices.Dequeue();
            GetChild<AudioStreamPlayer2D>(idx).PlayWithPitchRange(1f - _pitchDifference, 1f + _pitchDifference);

            if (_alwaysPlay != null)
            {
                _alwaysPlay.PlayWithPitchRange(1f - _pitchDifference, 1f + _pitchDifference);
            }
        }

        private void PopulateStreamIndices()
        {
            _streamIndices.Clear();
            if (GetChildCount() > 0)
            {
                var indices = Enumerable.Range(0, GetChildCount());
                if (_randomOrder)
                {
                    indices = indices.OrderBy(x => Main.RNG.Randf());
                }
                foreach (var index in indices)
                {
                    if (_alwaysPlay?.GetIndex() == index) continue;
                    _streamIndices.Enqueue(index);
                }
            }
        }

        private void OnDamageReceived(ImpactData impactData)
        {
            PlayAudio();
        }
    }
}