using LiveScore.Core.Converters;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Services;
using LiveScore.Soccer.Models.Matches;
using LiveScore.Soccer.Models.TimelineImages;
using LiveScore.Soccer.Services;
using LiveScore.Soccer.ViewModels.Leagues;
using LiveScore.Soccer.ViewModels.MatchDetails;
using LiveScore.Soccer.Views;
using LiveScore.Soccer.Views.Leagues;
using LiveScore.Soccer.Views.Templates;
using Prism.Ioc;
using Prism.Modularity;
using Xamarin.Forms;

namespace LiveScore.Soccer
{
    public class SoccerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // OnInitialized
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MatchDetailView, MatchDetailViewModel>(
                nameof(MatchDetailView) + SportType.Soccer.Value);

            containerRegistry.RegisterForNavigation<LeagueDetailView, LeagueDetailViewModel>(
                 nameof(LeagueDetailView) + SportType.Soccer.Value);

            containerRegistry.RegisterSingleton<IHubService, SoccerHubService>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<IMatchService, MatchService>(SportType.Soccer.Value.ToString());

            containerRegistry.RegisterSingleton<DataTemplate, MatchDataTemplate>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<IMatchDisplayStatusBuilder, MatchStatusBuilder>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<IMatchMinuteBuilder, MatchMinuteBuilder>(SportType.Soccer.Value.ToString());

            containerRegistry.RegisterSingleton<ITimelineEventImageBuilder, DefaultEventImageBuilder>();
            containerRegistry.RegisterSingleton<ITimelineEventImageBuilder, ScoreChangeImageBuilder>(
                EventType.ScoreChange.Value.ToString());
            containerRegistry.RegisterSingleton<ITimelineEventImageBuilder, PenaltyShootOutImageBuilder>(
                EventType.PenaltyShootout.Value.ToString());
        }
    }
}