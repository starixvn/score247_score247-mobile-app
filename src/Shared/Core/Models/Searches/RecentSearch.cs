namespace LiveScore.Core.Models.Searches
{
    using System;

    public class RecentSearch
    {
        public RecentSearch(string key, DateTimeOffset searchDate)
        {
            Key = key;
            SearchDate = searchDate;
        }

        public string Key { get; }

        public DateTimeOffset SearchDate { get; }
    }
}