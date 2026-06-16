using System.Collections.Generic;
using Ami.BroAudio.Data;
using UnityEngine;

namespace Ami.BroAudio.Runtime
{
    public class ShuffleClipStrategy : IClipSelectionStrategy
    {
        private readonly List<BroAudioClip> _remaining = new List<BroAudioClip>();

        public IBroAudioClip SelectClip(BroAudioClip[] clips, ClipSelectionContext context, out int index)
        {
            if (clips == null || clips.Length == 0)
            {
                index = -1;
                return null;
            }

            RefillIfNeeded(clips);

            int remainingIndex = Random.Range(0, _remaining.Count);
            BroAudioClip selected = _remaining[remainingIndex];
            _remaining.RemoveAt(remainingIndex);

            index = System.Array.IndexOf(clips, selected);
            return selected;
        }

        private void RefillIfNeeded(BroAudioClip[] clips)
        {
            if (_remaining.Count > 0)
                return;

            _remaining.Clear();

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null && clips[i].IsSet)
                {
                    _remaining.Add(clips[i]);
                }
            }
        }

        public void Reset()
        {
            _remaining.Clear();
        }
    }
}