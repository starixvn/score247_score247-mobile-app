using LiveScore.Core.Converters;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Leagues;
using LiveScore.Core.Models.Matches;
using LiveScore.Core.Models.Teams;
using LiveScore.Core.Services;
using LiveScore.Soccer.Models.Matches;
using LiveScore.Soccer.Models.TimelineImages;
using LiveScore.Soccer.Services;
using LiveScore.Soccer.Services.Favorites;
using LiveScore.Soccer.ViewModels.Leagues;
using LiveScore.Soccer.ViewModels.Matches;
using LiveScore.Soccer.Views.Leagues;
using LiveScore.Soccer.Views.Matches;
using LiveScore.Soccer.Views.Matches.Templates;
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

            containerRegistry.RegisterSingleton<IFavoriteService<ILeague>, FavoriteLeagueService>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<IFavoriteService<IMatch>, FavoriteMatchService>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<IFavoriteService<ITeamProfile>, FavoriteTeamService>(SportType.Soccer.Value.ToString());

            containerRegistry.RegisterSingleton<IHubService, SoccerHubService>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<IMatchService, MatchService>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<ITeamService, TeamService>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<ILeagueService, LeagueService>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<INewsService, NewsService>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<ITeamService, TeamService>(SportType.Soccer.Value.ToString());

            containerRegistry.RegisterSingleton<DataTemplate, MatchDataTemplate>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<IMatchDisplayStatusBuilder, MatchStatusBuilder>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<IMatchMinuteBuilder, MatchMinuteBuilder>(SportType.Soccer.Value.ToString());
            containerRegistry.RegisterSingleton<ISearchService, SoccerSearchService>(SportType.Soccer.Value.ToString());

            containerRegistry.RegisterSingleton<ISoccerMatchService, MatchService>();

            containerRegistry.RegisterSingleton<ITimelineEventImageBuilder, DefaultEventImageBuilder>();
            containerRegistry.RegisterSingleton<ITimelineEventImageBuilder, ScoreChangeImageBuilder>(
                EventType.ScoreChange.Value.ToString());
            containerRegistry.RegisterSingleton<ITimelineEventImageBuilder, PenaltyShootOutImageBuilder>(
                EventType.PenaltyShootout.Value.ToString());
        }
    }
}