using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Progression;
using KingSmash.Services;
using KingSmash.Economy;
using KingSmash.Audio;
using KingSmash.UI.Components;

namespace KingSmash.UI.Screens
{
    public class UpgradeScreen : UIScreen
    {
        [Header("King Status")]
        [SerializeField] private TextMeshProUGUI _kingLevelLabel;
        [SerializeField] private TextMeshProUGUI _kingXpLabel;
        [SerializeField] private Slider          _kingXpBar;

        [Header("Currency")]
        [SerializeField] private TextMeshProUGUI _coinsLabel;

        [Header("Stat Rows")]
        [SerializeField] private UpgradeStatRow _powerRow;
        [SerializeField] private UpgradeStatRow _speedRow;
        [SerializeField] private UpgradeStatRow _smashRow;
        [SerializeField] private UpgradeStatRow _armorRow;

        [Header("Config")]
        [SerializeField] private KingUpgradeConfig _upgradeConfig;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Art")]
        [SerializeField] private GameObject _kingArtPlaceholder;

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup _screenCg;
        [SerializeField] private KSProgressBar _xpProgressBar;
        [SerializeField] private RectTransform _kingArtPanel;
        [SerializeField] private Animator _kingReactionAnimator;
        [SerializeField] private ParticleSystem _upgradeParticles;

        private KingUpgradeService     _upgradeService;
        private KingProgressionService _progressionService;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _powerRow?.SetupButton(OnUpgradePower);
            _speedRow?.SetupButton(OnUpgradeSpeed);
            _smashRow?.SetupButton(OnUpgradeSmash);
            _armorRow?.SetupButton(OnUpgradeArmor);
        }

        private void OnEnable()
        {
            KingUpgradeService.OnStatUpgraded    += OnStatUpgradedHandler;
            KingProgressionService.OnKingLevelUp += OnKingLevelUpHandler;
        }

        private void OnDisable()
        {
            KingUpgradeService.OnStatUpgraded    -= OnStatUpgradedHandler;
            KingProgressionService.OnKingLevelUp -= OnKingLevelUpHandler;
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
        }

        protected override void OnShow()
        {
            ResolveServices();

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.UpgradeScreenOpened);

            RefreshAll();
            StartCoroutine(PlayShowAnimation());
        }

        private IEnumerator PlayShowAnimation()
        {
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return UIAnimationController.Fade(_screenCg, 0f, 1f, 0.2f);
            }

            if (_kingArtPanel != null)
            {
                Vector2 orig = _kingArtPanel.anchoredPosition;
                _kingArtPanel.anchoredPosition = orig + Vector2.left * 120f;
                StartCoroutine(UIAnimationController.SlideIn(_kingArtPanel, -120f, 0.3f));
            }

            var rows = new Transform[] {
                _powerRow != null ? _powerRow.transform : null,
                _speedRow != null ? _speedRow.transform : null,
                _smashRow != null ? _smashRow.transform : null,
                _armorRow != null ? _armorRow.transform : null
            };

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null) continue;
                int captured = i;
                StartCoroutine(DelayedSlideInFromRight(rows[captured], captured * 0.06f));
            }

            if (_kingXpLabel != null && _progressionService != null)
            {
                long currentXP = _progressionService.KingXP;
                long xpForNext = _progressionService.XPRequiredForNextLevel(_progressionService.KingLevel);
                yield return UIAnimationController.CountUp(_kingXpLabel, 0L, currentXP, 0.6f, "", $" / {xpForNext:N0} XP");
            }
        }

        private static IEnumerator DelayedSlideInFromRight(Transform target, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            var rt = target as RectTransform ?? target.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 orig = rt.anchoredPosition;
                rt.anchoredPosition = orig + Vector2.right * 80f;
                yield return UIAnimationController.SlideIn(rt, 80f, 0.25f);
            }
        }

        private void OnStatUpgradedHandler(KingStat stat, int oldLevel, int newLevel, float oldVal, float newVal, long cost)
        {
            RefreshAll();

            UpgradeStatRow affectedRow = stat switch
            {
                KingStat.Power       => _powerRow,
                KingStat.Speed       => _speedRow,
                KingStat.SmashRadius => _smashRow,
                KingStat.Armor       => _armorRow,
                _                    => null
            };

            if (affectedRow != null)
                StartCoroutine(UIAnimationController.BounceReveal(affectedRow.transform, 0.30f));

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.Upgrade);

            ToastService.ShowMessage("UPGRADED!");

            if (_upgradeParticles != null)
                StartCoroutine(BriefParticles());
        }

        private IEnumerator BriefParticles()
        {
            _upgradeParticles.Play();
            yield return new WaitForSeconds(1.2f);
            _upgradeParticles.Stop();
        }

        private void OnKingLevelUpHandler(int oldLevel, int newLevel, KingStats stats)
        {
            RefreshKingLevelDisplay();

            if (_kingArtPanel != null)
                StartCoroutine(UIAnimationController.BounceReveal(_kingArtPanel, 0.35f));
            else if (_kingArtPlaceholder != null)
                StartCoroutine(UIAnimationController.BounceReveal(_kingArtPlaceholder.transform, 0.35f));

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.Reward);
        }

        private void OnUpgradePower() => TryUpgrade(KingStat.Power);
        private void OnUpgradeSpeed() => TryUpgrade(KingStat.Speed);
        private void OnUpgradeSmash() => TryUpgrade(KingStat.SmashRadius);
        private void OnUpgradeArmor() => TryUpgrade(KingStat.Armor);

        private void TryUpgrade(KingStat stat)
        {
            if (_upgradeService == null)
            {
                GameLogger.Warning("UpgradeScreen", "KingUpgradeService not available.");
                return;
            }

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.ButtonClick);

            var result = _upgradeService.TryUpgradeStat(stat);

            if (result != UpgradeResult.Success)
            {
                GameLogger.Warning("UpgradeScreen", $"Upgrade failed for {stat}: {result}");
                ShakeCoinsLabel();
                ToastService.ShowError("Not enough coins");
            }
        }

        private void RefreshAll()
        {
            RefreshCoinsDisplay();
            RefreshKingLevelDisplay();
            RefreshStatRow(_powerRow, KingStat.Power,       "Power");
            RefreshStatRow(_speedRow, KingStat.Speed,       "Speed");
            RefreshStatRow(_smashRow, KingStat.SmashRadius, "Smash Radius");
            RefreshStatRow(_armorRow, KingStat.Armor,       "Armor");
        }

        private void RefreshCoinsDisplay()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            if (_coinsLabel != null)
                _coinsLabel.text = save.Current.coins.ToString("N0");
        }

        private void RefreshKingLevelDisplay()
        {
            if (_progressionService == null) return;

            int  kingLevel  = _progressionService.KingLevel;
            long currentXP  = _progressionService.KingXP;
            long xpForNext  = _progressionService.XPRequiredForNextLevel(kingLevel);
            bool isMaxLevel = kingLevel >= _progressionService.MaxKingLevel;

            if (_kingLevelLabel != null)
                _kingLevelLabel.text = $"King Lv {kingLevel}";

            if (isMaxLevel)
            {
                if (_kingXpLabel != null) _kingXpLabel.text = "MAX";
                if (_kingXpBar   != null) _kingXpBar.value  = 1f;
                _xpProgressBar?.SetProgress(1f, animated: true);
                _xpProgressBar?.SetLabel("MAX");
            }
            else
            {
                if (_kingXpLabel != null)
                    _kingXpLabel.text = $"{currentXP:N0} / {xpForNext:N0} XP";

                float xpFraction = xpForNext > 0 ? Mathf.Clamp01((float)currentXP / xpForNext) : 0f;

                if (_kingXpBar != null)
                    _kingXpBar.value = xpFraction;

                _xpProgressBar?.SetProgress(xpFraction, animated: true);
                _xpProgressBar?.SetLabel($"{currentXP:N0} / {xpForNext:N0}");
            }
        }

        private void RefreshStatRow(UpgradeStatRow row, KingStat stat, string displayName)
        {
            if (row == null) return;
            if (_upgradeService == null)
            {
                row.Refresh(displayName, 1, 0, 0);
                return;
            }

            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            long playerCoins = save.Current.coins;

            var preview = _upgradeService.GetUpgradePreview(stat);
            row.Refresh(displayName, preview.currentLevel, preview.cost, playerCoins);
        }

        private void ShakeCoinsLabel()
        {
            if (_coinsLabel != null)
                StartCoroutine(UIAnimationController.Shake(_coinsLabel.transform, 8f, 0.3f));
        }

        private void OnBackClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.Back);
            ScreenManager.Instance.Back();
        }

        private void ResolveServices()
        {
            ServiceLocator.TryGet<KingUpgradeService>(out _upgradeService);
            ServiceLocator.TryGet<KingProgressionService>(out _progressionService);

            if (_upgradeService == null)
                GameLogger.Warning("UpgradeScreen", "KingUpgradeService not registered in ServiceLocator.");
            if (_progressionService == null)
                GameLogger.Warning("UpgradeScreen", "KingProgressionService not registered in ServiceLocator.");
        }
    }
}
