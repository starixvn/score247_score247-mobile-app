namespace LiveScore.Core.Controls.SearchTeams
{
    using System.Collections.ObjectModel;

    public class SearchListGroup : ObservableCollection<SearchItem>
    {
        public string Heading { get; set; }
        public ObservableCollection<SearchItem> SearchItems => this;
    }
}