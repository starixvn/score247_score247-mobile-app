namespace LiveScore.Soccer.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using LiveScore.Core;
    using LiveScore.Core.Enumerations;
    using LiveScore.Core.Models.Teams;
    using LiveScore.Core.Services;
    using LiveScore.Soccer.Models.Teams;

    public class SoccerSearchService : SearchService<ITeamProfile>
    {
        public SoccerSearchService(IDependencyResolver dependencyResolver)
            : base(SportType.Soccer.DisplayName, dependencyResolver)
        {
            Init();
        }

        public override IEnumerable<ITeamProfile> GetRecentSearchKeys(int limit)
        {
            var recentSearches = userSettingService.GetValueOrDefault(RecentSearchCacheKey + Key, new List<TeamProfile>());
            recentSearches.Reverse();

            return recentSearches.Take(limit);
        }
    }
}