namespace LiveScore.Core.Controls.SearchTeams
{
    using LiveScore.Core.Models.Teams;
    using System.Collections.ObjectModel;

    public class SearchListGroup : ObservableCollection<ITeamProfile>
    {
        public string Heading { get; set; }
        public ObservableCollection<ITeamProfile> SearchItems => this;
    }
}