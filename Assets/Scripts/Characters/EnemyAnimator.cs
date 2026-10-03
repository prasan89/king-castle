using UnityEngine;

namespace KingSmash.Characters
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator _animator;

        private static readonly int _hitSmallHash  = Animator.StringToHash("HitSmall");
        private static readonly int _hitMedHash    = Animator.StringToHash("HitMedium");
        private static readonly int _hitLargeHash  = Animator.StringToHash("HitLarge");
        private static readonly int _deathHash     = Animator.StringToHash("Death");
        private static readonly int _isAliveHash   = Animator.StringToHash("IsAlive");

        private void Awake() => _animator = GetComponent<Animator>();

        public void PlayHitReaction(HitReactionSize size)
        {
            switch (size)
            {
                case HitReactionSize.Small:  _animator.SetTrigger(_hitSmallHash); break;
                case HitReactionSize.Medium: _animator.SetTrigger(_hitMedHash);   break;
                case HitReactionSize.Large:  _animator.SetTrigger(_hitLargeHash); break;
            }
        }

        public void PlayDeath()
        {
            _animator.SetBool(_isAliveHash, false);
            _animator.SetTrigger(_deathHash);
        }
    }
}
