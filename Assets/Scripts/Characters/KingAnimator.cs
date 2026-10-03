using UnityEngine;

namespace KingSmash.Characters
{
    // Animator parameter hash wrappers — keeps KingController decoupled from string names.
    [RequireComponent(typeof(Animator))]
    public class KingAnimator : MonoBehaviour
    {
        private Animator _animator;

        private static readonly int _stateHash    = Animator.StringToHash("KingState");
        private static readonly int _impactHash   = Animator.StringToHash("Impact");
        private static readonly int _victoryHash  = Animator.StringToHash("Victory");
        private static readonly int _defeatedHash = Animator.StringToHash("Defeated");

        private void Awake() => _animator = GetComponent<Animator>();

        public void SetState(KingState state)
        {
            _animator.SetInteger(_stateHash, (int)state);
            if (state == KingState.Impact)   _animator.SetTrigger(_impactHash);
            if (state == KingState.Victory)  _animator.SetTrigger(_victoryHash);
            if (state == KingState.Defeated) _animator.SetTrigger(_defeatedHash);
        }
    }
}
