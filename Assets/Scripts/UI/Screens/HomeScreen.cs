using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Retention;
using KingSmash.UI.Components;

namespace KingSmash.UI.Screens
{
    public class HomeScreen : UIScreen
    {
        // ── Existing fields (preserved) ────────────────────────────────────────

        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI _kingLevelLabel;
        [SerializeField] private TextMeshProUGUI _coinsLabel;
        [SerializeField] private TextMeshProUGUI _gemsLabel;
        [SerializeField] private Button          _settingsButton;

        [Header("Primary")]
        [SerializeField] private Button _playButton;

        [Header("Bottom Nav")]
        [SerializeField] private Button _kingUpgradeButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _dailyButton;
        [SerializeField] private Button _missionsButton;

        [Header("Notification Badges")]
        [SerializeField] private GameObject _dailyBadge;
        [SerializeField] private GameObject _missionsBadge;
        [SerializeField] private GameObject _achievementsBadge;

        // ── M13 polish fields ─────────────────────────────────────────────

        [Header("M13 Polish")]
        [SerializeField] private KingSmashTheme  _themeConfig;
        [SerializeField] private RectTransform   _heroKingTransform;
        [SerializeField] private Transform       _playButtonTransform;
        [SerializeField] private CanvasGroup     _topBarCg;
        [SerializeField] private CanvasGroup     _bottomNavCg;

        // Tracked currency values for CountUp animation
        private long _displayedCoins;
        private long _displayedGems;

        private Coroutine _heroFloatCoroutine;
        private Coroutine _playPulseCoroutine;
        private Coroutine _badgePulseCoroutine;

        // ── Lifecycle ──────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _playButton?.onClick.AddListener(OnPlayClicked);
            _settingsButton?.onClick.AddListener(OnSettingsClicked);
            _kingUpgradeButton?.onClick.AddListener(OnKingUpgradeClicked);
            _shopButton?.onClick.AddListener(OnShopClicked);
            _dailyButton?.onClick.AddListener(OnDailyClicked);
            _missionsButton?.onClick.AddListener(OnMissionsClicked);
        }

        protected override void OnShow()
        {
            RefreshPlayerData();
            RefreshBadges();

            DailyRewardService.OnDailyClaimed  += HandleDailyClaimed;
            MissionService.OnMissionClaimed    += HandleMissionClaimed;

            ServiceLocator.TryGet<IAnalyticsService>(out var analytics);
            analytics?.LogEvent(AnalyticsEvents.MainMenuOpened);
            GameManager.Instance?.TransitionTo(GameState.MainMenu);

            StartCoroutine(EntranceAnimation());
            RestartIdleCoroutines();
        }

        private void OnDisable()
        {
            DailyRewardService.OnDailyClaimed  -= HandleDailyClaimed;
            MissionService.OnMissionClaimed    -= HandleMissionClaimed;

            StopIdleCoroutines();
        }

        // ── Event handlers (existing) ─────────────────────────────────────────

        private void HandleDailyClaimed(DailyRewardResult result)   => RefreshBadges();
        private void HandleMissionClaimed(string id, MissionClaimResult r) => RefreshBadges();

        // ── Data refresh (existing, preserved) ───────────────────────────────────

        public void RefreshBadges()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)
                || !ServiceLocator.TryGet<MissionConfig>(out var missionConfig)
                || !ServiceLocator.TryGet<AchievementConfig>(out var achConfig)
                || !ServiceLocator.TryGet<DailyRewardService>(out var dailyService))
            {
                _dailyBadge?.SetActive(false);
                _missionsBadge?.SetActive(false);
                _achievementsBadge?.SetActive(false);
                return;
            }

            bool hasDaily        = dailyService.CanClaimToday();
            bool hasMission      = NotificationBadgeService.HasClaimableMission(save, missionConfig);
            bool hasAchievement  = NotificationBadgeService.HasClaimableAchievement(save, achConfig);

            _dailyBadge?.SetActive(hasDaily);
            _missionsBadge?.SetActive(hasMission);
            _achievementsBadge?.SetActive(hasAchievement);

            bool anyBadge = hasDaily || hasMission || hasAchievement;
            if (anyBadge) RestartBadgePulse();
        }

        public void RefreshPlayerData()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var data = save.Current;

            if (_kingLevelLabel != null) _kingLevelLabel.text = $"Lv {data.kingLevel}";

            long newCoins = data.coins;
            long newGems  = data.gems;

            if (gameObject.activeInHierarchy && _coinsLabel != null && newCoins != _displayedCoins)
            {
                StopCoroutineIfRunning(ref _displayedCoins);
                StartCoroutine(CountUpCoins(_displayedCoins, newCoins));
            }
            else if (_coinsLabel != null)
            {
                _coinsLabel.text = FormatNumber(newCoins);
            }

            if (gameObject.activeInHierarchy && _gemsLabel != null && newGems != _displayedGems)
            {
                StartCoroutine(CountUpGems(_displayedGems, newGems));
            }
            else if (_gemsLabel != null)
            {
                _gemsLabel.text = newGems.ToString();
            }

            _displayedCoins = newCoins;
            _displayedGems  = newGems;
        }

        // ── Button click handlers (existing, with sound added) ─────────────────

        private void OnPlayClicked()
        {
            PlaySound(Audio.SoundId.ButtonConfirm);
            if (_playButton != null)
                StartCoroutine(UIAnimationController.ButtonPress(_playButton.transform));
            ScreenManager.Instance.Show<WorldMapScreen>();
        }

        private void OnSettingsClicked()
        {
            PlaySound(Audio.SoundId.ButtonClick);
            ScreenManager.Instance.Show<SettingsScreen>();
        }

        private void OnKingUpgradeClicked()
        {
            PlaySound(Audio.SoundId.ButtonClick);
            ScreenManager.Instance.Show<UpgradeScreen>();
        }

        private void OnShopClicked()
        {
            PlaySound(Audio.SoundId.ButtonClick);
            ScreenManager.Instance.Show<ShopScreen>();
        }

        private void OnDailyClicked()
        {
            PlaySound(Audio.SoundId.PopupOpen);
            ScreenManager.Instance.Show<DailyRewardsScreen>();
        }

        private void OnMissionsClicked()
        {
            PlaySound(Audio.SoundId.PopupOpen);
            ScreenManager.Instance.Show<MissionsScreen>();
        }

        // ── M13 Animation: Entrance ──────────────────────────────────────────

        private IEnumerator EntranceAnimation()
        {
            float dur = _themeConfig != null ? _themeConfig.durationNormal : 0.25f;

            // Prepare top bar off-screen above
            if (_topBarCg != null)
            {
                var rt = _topBarCg.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 end   = rt.anchoredPosition;
                    Vector2 start = end + Vector2.up * 100f;
                    rt.anchoredPosition = start;
                    _topBarCg.alpha = 0f;
                    StartCoroutine(SlideAndFade(_topBarCg, rt, start, end, dur));
                }
                else
                {
                    _topBarCg.alpha = 0f;
                    StartCoroutine(UIAnimationController.Fade(_topBarCg, 0f, 1f, dur));
                }
            }

            // Prepare bottom nav off-screen below
            if (_bottomNavCg != null)
            {
                var rt = _bottomNavCg.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 end   = rt.anchoredPosition;
                    Vector2 start = end + Vector2.down * 100f;
                    rt.anchoredPosition = start;
                    _bottomNavCg.alpha = 0f;
                    StartCoroutine(SlideAndFade(_bottomNavCg, rt, start, end, dur));
                }
                else
                {
                    _bottomNavCg.alpha = 0f;
                    StartCoroutine(UIAnimationController.Fade(_bottomNavCg, 0f, 1f, dur));
                }
            }

            // Hero bounce
            if (_heroKingTransform != null)
                StartCoroutine(UIAnimationController.BounceReveal(_heroKingTransform,
                    _themeConfig != null ? _themeConfig.durationBounce : 0.35f));

            yield return new WaitForSecondsRealtime(dur + 0.1f);

            // Play button attention pulse after entrance
            if (_playButtonTransform != null)
                StartCoroutine(UIAnimationController.BounceReveal(_playButtonTransform, 0.3f));
        }

        // ── M13 Animation: Idle ────────────────────────────────────────────

        private void RestartIdleCoroutines()
        {
            StopIdleCoroutines();
            if (_heroKingTransform != null)
                _heroFloatCoroutine = StartCoroutine(HeroFloat());
            _playPulseCoroutine = StartCoroutine(PlayButtonPeriodPulse());
        }

        private void StopIdleCoroutines()
        {
            if (_heroFloatCoroutine != null) { StopCoroutine(_heroFloatCoroutine); _heroFloatCoroutine = null; }
            if (_playPulseCoroutine != null) { StopCoroutine(_playPulseCoroutine); _playPulseCoroutine = null; }
            if (_badgePulseCoroutine != null) { StopCoroutine(_badgePulseCoroutine); _badgePulseCoroutine = null; }
        }

        private IEnumerator HeroFloat()
        {
            if (_heroKingTransform == null) yield break;
            float originY   = _heroKingTransform.anchoredPosition.y;
            float amplitude = 8f;
            float period    = 3f;
            float t         = 0f;
            while (true)
            {
                t += Time.deltaTime;
                var pos = _heroKingTransform.anchoredPosition;
                pos.y = originY + Mathf.Sin(t * (Mathf.PI * 2f / period)) * amplitude;
                _heroKingTransform.anchoredPosition = pos;
                yield return null;
            }
        }

        private IEnumerator PlayButtonPeriodPulse()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(5f);
                Transform target = _playButtonTransform != null ? _playButtonTransform
                    : (_playButton != null ? _playButton.transform : null);
                if (target != null)
                    yield return StartCoroutine(PulseScale(target, 1.08f, 0.25f));
            }
        }

        private void RestartBadgePulse()
        {
            if (_badgePulseCoroutine != null) { StopCoroutine(_badgePulseCoroutine); }
            _badgePulseCoroutine = StartCoroutine(BadgePulseLoop());
        }

        private IEnumerator BadgePulseLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(3f);
                PulseBadge(_dailyBadge);
                PulseBadge(_missionsBadge);
                PulseBadge(_achievementsBadge);
            }
        }

        private void PulseBadge(GameObject badge)
        {
            if (badge != null && badge.activeSelf)
                StartCoroutine(PulseScale(badge.transform, 1.25f, 0.2f));
        }

        // ── Currency CountUp ────────────────────────────────────────────────

        private IEnumerator CountUpCoins(long from, long to)
        {
            if (_coinsLabel == null) yield break;
            float dur = _themeConfig != null ? _themeConfig.durationCountUp : 0.8f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                long val = (long)Mathf.Lerp(from, to, elapsed / dur);
                _coinsLabel.text = FormatNumber(val);
                yield return null;
            }
            _coinsLabel.text = FormatNumber(to);
        }

        private IEnumerator CountUpGems(long from, long to)
        {
            if (_gemsLabel == null) yield break;
            float dur = _themeConfig != null ? _themeConfig.durationCountUp : 0.8f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                long val = (long)Mathf.Lerp(from, to, elapsed / dur);
                _gemsLabel.text = val.ToString();
                yield return null;
            }
            _gemsLabel.text = to.ToString();
        }

        // ── Utilities ────────────────────────────────────────────────────

        private IEnumerator SlideAndFade(CanvasGroup cg, RectTransform rt, Vector2 from, Vector2 to, float dur)
        {
            float elapsed = 0f;
            cg.alpha = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                rt.anchoredPosition = Vector2.Lerp(from, to, t);
                cg.alpha = t;
                yield return null;
            }
            rt.anchoredPosition = to;
            cg.alpha = 1f;
        }

        private IEnumerator PulseScale(Transform target, float peakScale, float duration)
        {
            if (target == null) yield break;
            Vector3 original = target.localScale;
            float half = duration * 0.5f;
            float e = 0f;
            while (e < half)
            {
                e += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(original, original * peakScale, e / half);
                yield return null;
            }
            e = 0f;
            while (e < half)
            {
                e += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(original * peakScale, original, e / half);
                yield return null;
            }
            target.localScale = original;
        }

        private static void PlaySound(Audio.SoundId id)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(id);
        }

        private static string FormatNumber(long n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000f:F1}M";
            if (n >= 1_000)     return $"{n / 1_000f:F1}K";
            return n.ToString();
        }

        // Dummy to suppress unused variable warning for pattern usage
        private void StopCoroutineIfRunning(ref long _) { }

        private void OnDestroy()
        {
            _playButton?.onClick.RemoveListener(OnPlayClicked);
            _settingsButton?.onClick.RemoveListener(OnSettingsClicked);
            _kingUpgradeButton?.onClick.RemoveListener(OnKingUpgradeClicked);
            _shopButton?.onClick.RemoveListener(OnShopClicked);
            _dailyButton?.onClick.RemoveListener(OnDailyClicked);
            _missionsButton?.onClick.RemoveListener(OnMissionsClicked);
        }
    }
}
