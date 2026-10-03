using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KingSmash.Gameplay;
using KingSmash.Characters;
using KingSmash.Core;

namespace KingSmash.Levels
{
    [Serializable]
    public class TutorialHint
    {
        public string text;
        public float displayDuration = 2.5f;
        public enum Trigger { OnLevelStart, OnFirstAim, OnFirstLaunch, OnFirstImpact, OnQueenVisible, OnVictory }
        public Trigger trigger;
    }

    public class TutorialHintSystem : MonoBehaviour
    {
        [SerializeField] private bool _enabled = true;
        [SerializeField] private GameObject _hintPanel;
        [SerializeField] private TextMeshProUGUI _hintLabel;
        [SerializeField] private List<TutorialHint> _hints = new();

        private readonly HashSet<TutorialHint.Trigger> _shownTriggers = new();

        private void Awake()
        {
            _hintPanel?.SetActive(false);
        }

        private void OnEnable()
        {
            if (!_enabled) return;
            AimController.OnAimStarted      += HandleAimStarted;
            LaunchController.OnKingLaunched += HandleKingLaunched;
            KingProjectile.OnKingCollision  += HandleKingCollision;
            QueenController.OnQueenRescued  += HandleQueenRescued;
        }

        private void OnDisable()
        {
            AimController.OnAimStarted      -= HandleAimStarted;
            LaunchController.OnKingLaunched  -= HandleKingLaunched;
            KingProjectile.OnKingCollision   -= HandleKingCollision;
            QueenController.OnQueenRescued   -= HandleQueenRescued;
        }

        private void Start()
        {
            if (_enabled)
                ShowTrigger(TutorialHint.Trigger.OnLevelStart);
        }

        private void HandleAimStarted()                              => ShowTrigger(TutorialHint.Trigger.OnFirstAim);
        private void HandleKingLaunched(Vector2 _, float __)        => ShowTrigger(TutorialHint.Trigger.OnFirstLaunch);
        private void HandleKingCollision(KingProjectile _, UnityEngine.Collision2D __) => ShowTrigger(TutorialHint.Trigger.OnFirstImpact);
        private void HandleQueenRescued(QueenController _)           => ShowTrigger(TutorialHint.Trigger.OnVictory);

        private void ShowTrigger(TutorialHint.Trigger trigger)
        {
            if (!_enabled || _shownTriggers.Contains(trigger)) return;
            _shownTriggers.Add(trigger);

            foreach (var hint in _hints)
            {
                if (hint.trigger == trigger)
                {
                    StartCoroutine(DisplayHint(hint));
                    break;
                }
            }
        }

        private IEnumerator DisplayHint(TutorialHint hint)
        {
            if (_hintLabel  != null) _hintLabel.text = hint.text;
            _hintPanel?.SetActive(true);
            yield return new WaitForSecondsRealtime(hint.displayDuration);
            _hintPanel?.SetActive(false);
        }
    }
}
