namespace LiveScore.Core.Models.Searches
{
    using LiveScore.Core.Models.Teams;
    using System;

    public class RecentSearch : ITeamProfile
    {
        public RecentSearch(ITeamProfile team, DateTimeOffset searchDate)
        {
            Id = team.Id;
            Name = team.Name;
            Country = team.Country;
            CountryCode = team.CountryCode;
            Abbreviation = team.Abbreviation;
            LogoName = team.LogoName;
            LogoUrl = team.LogoUrl;
            IsFavorite = team.IsFavorite;
            SearchDate = searchDate;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Country { get; private set; }

        public string CountryCode { get; private set; }

        public string Abbreviation { get; private set; }

        public string LogoName { get; private set; }

        public string LogoUrl { get; set; }

        public bool IsFavorite { get; set; }

        public DateTimeOffset SearchDate { get; }
    }
}