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

        IEnumerable<ITeamProfile> GetRecentSearchKeys(int limit);
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
            var recentSearches = userSettingService.GetValueOrDefault(RecentSearchCacheKey + Key, new List<ITeamProfile>());
            recentSearches = recentSearches.Where(searchKey => searchKey.Id != team.Id).ToList();
            recentSearches.Add(team);
            if (recentSearches.Count > LimitSearchKey)
            {
                recentSearches.RemoveAt(0);
            }

            userSettingService.AddOrUpdateValue(RecentSearchCacheKey + Key, JsonConvert.SerializeObject(recentSearches));

            return true;
        }

        public IEnumerable<ITeamProfile> GetRecentSearchKeys(int limit)
        {
            var recentSearches = userSettingService.GetValueOrDefault(RecentSearchCacheKey + Key, new List<ITeamProfile>());
            recentSearches.Reverse();

            return recentSearches.Take(limit);
        }
    }
}