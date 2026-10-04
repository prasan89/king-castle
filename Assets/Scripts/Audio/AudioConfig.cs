using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "KingSmash/Audio/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private List<AudioCue> _cues = new List<AudioCue>();

        public List<AudioCue> Cues => _cues;

        /// <summary>
        /// Returns the AudioCue matching the given SoundId via linear search.
        /// Returns null if not found.
        /// </summary>
        public AudioCue GetCue(SoundId id)
        {
            for (int i = 0; i < _cues.Count; i++)
            {
                if (_cues[i] != null && _cues[i].id == id)
                    return _cues[i];
            }
            return null;
        }
    }
}
