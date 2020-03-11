namespace LiveScore.Core.Services
{
    using LiveScore.Common.Services;
    using System.Collections.Generic;
    using System.Linq;

    public interface ISearchService<T>
    {
        bool UpdateRecent(T team);

        IEnumerable<T> GetRecentSearchKeys(int limit);
    }

    public class SearchService<T> : ISearchService<T>
    {
        protected readonly IUserSettingService userSettingService;

        protected string Key;

        private const int LimitSearchKey = 100;
        private const string RecentSearchCacheKey = "RecentSearch";

        public SearchService(
            string key,
            IDependencyResolver dependencyResolver)
        {
            Key = key;
            userSettingService = dependencyResolver.Resolve<IUserSettingService>();
        }

        public bool UpdateRecent(T team)
        {
            var recentSearches = userSettingService.GetValueOrDefault(RecentSearchCacheKey + Key, new List<T>());
            recentSearches.Remove(team);
            recentSearches.Add(team);
            if (recentSearches.Count > LimitSearchKey)
            {
                recentSearches.RemoveAt(0);
            }

            userSettingService.AddOrUpdateValue(RecentSearchCacheKey + Key, recentSearches);

            return true;
        }

        public IEnumerable<T> GetRecentSearchKeys(int limit)
        {
            var recentSearches = userSettingService.GetValueOrDefault(RecentSearchCacheKey + Key, new List<T>());
            recentSearches.Reverse();

            return recentSearches.Take(limit);
        }
    }
}