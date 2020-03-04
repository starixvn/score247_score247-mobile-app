using LiveScore.Core;
using LiveScore.Soccer.Extensions;
using LiveScore.Soccer.Models.Matches;
using Prism.Navigation;

namespace LiveScore.Soccer.ViewModels.Matches.MatchDetails.Information.InfoItems
{
    public class VARItemViewModel : BaseItemViewModel
    {
        public VARItemViewModel(
            TimelineEvent timelineEvent,
            MatchInfo matchInfo,
            INavigationService navigationService,
            IDependencyResolver dependencyResolver)
            : base(timelineEvent, matchInfo, navigationService, dependencyResolver)
        {
        }

        public string HomeOutcome { get; private set; }
        public string AwayOutcome { get; private set; }

        public override BaseItemViewModel BuildData()
        {
            base.BuildData();

            if (TimelineEvent.OfHomeTeam())
            {
                BuildHomeInfo();
            }
            else
            {
                BuildAwayInfo();
            }

            return this;
        }

        private void BuildHomeInfo()
        {
            HomeOutcome = TimelineEvent.Outcome.CompiledDisplayName;
            VisibleHomeImage = true;
        }

        private void BuildAwayInfo()
        {
            AwayOutcome = TimelineEvent.Outcome.CompiledDisplayName;
            VisibleAwayImage = true;
        }
    }
}