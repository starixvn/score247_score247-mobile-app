namespace LiveScore.Core.Controls.SearchTeams
{
    using LiveScore.Core.Models.Teams;

    public class SearchItem
    {
        private const string DefaultSearchIcon = "images/common/search.png";

        public SearchItem(ITeamProfile team)
        {
            Team = team;
            SearchValue = team.Name;
            LogoUrl = team.LogoUrl;
            IsFavorite = team.IsFavorite;
            IsShowFavorite = true;
        }

        public SearchItem(string recentSearch)
        {
            Team = null;
            SearchValue = recentSearch;
            LogoUrl = DefaultSearchIcon;
            IsShowFavorite = false;
        }

        public ITeamProfile Team { get; }

        public string SearchValue { get; }

        public string LogoUrl { get; }

        public bool IsFavorite { get; }

        public bool IsShowFavorite { get; }
    }
}