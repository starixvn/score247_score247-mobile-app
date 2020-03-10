namespace LiveScore.Core.Controls.SearchTeams
{
    using System.Collections.Generic;

    public class SearchListGroup : List<SearchItem>
    {
        public string Heading { get; set; }
        public List<SearchItem> SearchItems => this;
    }
}