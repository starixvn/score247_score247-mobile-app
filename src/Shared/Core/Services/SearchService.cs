namespace LiveScore.Core.Services
{
    using LiveScore.Common.Services;
    using LiveScore.Core.Models.Searches;
    using LiveScore.Core.Models.Teams;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface ISearchService
    {
        bool UpdateRecent(ITeamProfile team);

        IEnumerable<RecentSearch> GetRecentSearchKeys(int limit);
    }

    public class SearchService : ISearchService
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

        public bool UpdateRecent(ITeamProfile team)
        {
            var recentSearches = userSettingService.GetValueOrDefault(RecentSearchCacheKey + Key, new List<RecentSearch>());
            recentSearches = recentSearches.Where(searchKey => searchKey.Id != team.Id).ToList();
            recentSearches.Add(new RecentSearch(team, DateTime.UtcNow));
            if (recentSearches.Count > LimitSearchKey)
            {
                var oldestSearchKey = recentSearches.OrderBy(key => key.SearchDate).FirstOrDefault();
                recentSearches.Remove(oldestSearchKey);
            }

            userSettingService.AddOrUpdateValue(RecentSearchCacheKey + Key, JsonConvert.SerializeObject(recentSearches));

            return true;
        }

        public IEnumerable<RecentSearch> GetRecentSearchKeys(int limit)
        {
            var recentSearches = userSettingService.GetValueOrDefault(RecentSearchCacheKey + Key, new List<RecentSearch>());

            return recentSearches.OrderBy(key => key.SearchDate).Take(limit);
        }
    }
}