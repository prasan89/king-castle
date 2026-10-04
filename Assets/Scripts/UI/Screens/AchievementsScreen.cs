using UnityEngine;
using KingSmash.Core;
using KingSmash.Progression;
using KingSmash.Services;
using KingSmash.Audio;

namespace KingSmash.UI.Screens
{
    public class AchievementsScreen : UIScreen
    {
        [SerializeField] private UnityEngine.UI.ScrollRect _scrollRect;
        [SerializeField] private Transform                  _listRoot;
        [SerializeField] private Widgets.AchievementCardWidget _cardPrefab;

        protected override void OnShow()
        {
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.AchievementsOpened);

            PopulateList();
        }

        private void PopulateList()
        {
            if (_listRoot == null || _cardPrefab == null) return;

            foreach (Transform child in _listRoot)
                Destroy(child.gameObject);

            var config = UnityEngine.Resources.Load<AchievementConfig>("AchievementConfig");
            if (config == null) return;

            ISaveService save;
            if (!ServiceLocator.TryGet<ISaveService>(out save)) return;

            foreach (var def in config.Definitions)
            {
                var card = Instantiate(_cardPrefab, _listRoot);
                card.Bind(def, save.Current);
            }
        }
    }
}
