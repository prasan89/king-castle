using UnityEngine;

namespace KingSmash.Audio
{
    [CreateAssetMenu(fileName = "AudioCue", menuName = "KingSmash/Audio/AudioCue")]
    public class AudioCue : ScriptableObject
    {
        [Header("Identity")]
        public SoundId id;
        public AudioCategory category;

        [Header("Clips")]
        public AudioClip[] variants;

        [Header("Playback")]
        [Range(0f, 1f)]
        public float volume = 1f;
        public float pitch = 1f;
        [Range(0f, 1f)]
        public float pitchVariance = 0.1f;
        public bool loop = false;

        [Header("Fades")]
        public float fadeInDuration = 0f;
        public float fadeOutDuration = 0f;

        /// <summary>
        /// Returns a random clip from the variants array, or null if empty.
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (variants == null || variants.Length == 0)
                return null;
            if (variants.Length == 1)
                return variants[0];
            return variants[Random.Range(0, variants.Length)];
        }

        /// <summary>
        /// Returns a pitch value with ±pitchVariance applied.
        /// </summary>
        public float GetRandomPitch()
        {
            return pitch + Random.Range(-pitchVariance, pitchVariance);
        }
    }
}
