namespace KingSmash.Camera
{
    public interface ICameraEffectService
    {
        /// <summary>Trigger a shake using a pre-authored profile asset.</summary>
        void Shake(CameraShakeProfile profile);

        /// <summary>Trigger a shake with inline parameters — no profile asset needed.</summary>
        void ShakeImmediate(float duration, float magnitude);

        /// <summary>Cancel any in-progress shake immediately.</summary>
        void StopShake();
    }
}
