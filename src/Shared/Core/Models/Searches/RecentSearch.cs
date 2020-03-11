namespace LiveScore.Core.Models.Searches
{
    using LiveScore.Core.Models.Teams;
    using System;

    public class RecentSearch
    {
        public RecentSearch(ITeamProfile team, DateTimeOffset searchDate)
        {
            Team = team;
            SearchDate = searchDate;
        }

        public ITeamProfile Team { get; }

        public DateTimeOffset SearchDate { get; }
    }
}