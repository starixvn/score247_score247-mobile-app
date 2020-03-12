using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Common;
using LiveScore.Common.Extensions;
using LiveScore.Common.LangResources;
using LiveScore.Core;
using LiveScore.Core.Controls.TabStrip;
using LiveScore.Core.Controls.TabStrip.EventArgs;
using LiveScore.Core.Converters;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Matches;
using LiveScore.Core.NavigationParams;
using LiveScore.Core.PubSubEvents.Matches;
using LiveScore.Core.PubSubEvents.Teams;
using LiveScore.Core.Services;
using LiveScore.Core.ViewModels;
using LiveScore.Soccer.Enumerations;
using LiveScore.Soccer.Models.Matches;
using LiveScore.Soccer.Services;
using LiveScore.Soccer.ViewModels.Leagues.LeagueDetails.Table;
using LiveScore.Soccer.ViewModels.Matches.MatchDetails.HeadToHead;
using LiveScore.Soccer.ViewModels.Matches.MatchDetails.Information;
using LiveScore.Soccer.ViewModels.Matches.MatchDetails.LineUps;
using LiveScore.Soccer.ViewModels.Matches.MatchDetails.Statistics;
using LiveScore.Soccer.ViewModels.Matches.MatchDetails.TrackerCommentary;
using LiveScore.Soccer.Views.Leagues.Templates.LeagueDetails.Table;
using LiveScore.Soccer.Views.Matches.Templates.MatchDetails.HeadToHead;
using LiveScore.Soccer.Views.Matches.Templates.MatchDetails.Information;
using LiveScore.Soccer.Views.Matches.Templates.MatchDetails.LineUps;
using LiveScore.Soccer.Views.Matches.Templates.MatchDetails.Statistics;
using LiveScore.Soccer.Views.Matches.Templates.MatchDetails.TrackerCommentary;
using MethodTimer;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using Rg.Plugins.Popup.Contracts;
using Xamarin.Forms;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Soccer.Tests")]

namespace LiveScore.Soccer.ViewModels.Matches
{
    public class MatchDetailViewModel : ViewModelBase, IDisposable
    {
        private static readonly DataTemplate infoTemplate = new InformationTemplate();
        private static readonly DataTemplate h2hTemplate = new H2HTemplate();
        private static readonly DataTemplate lineupsTemplate = new LineUpsTemplate();
        private static readonly DataTemplate statisticsTemplate = new StatisticsTemplate();
        private static readonly DataTemplate trackerTemplate = new TrackerCommentaryTemplate();
        private static readonly DataTemplate tableTemplate = new TableTemplate();

        private readonly IMatchDisplayStatusBuilder matchStatusConverter;
        private readonly IMatchMinuteBuilder matchMinuteConverter;
        private readonly Func<string, string> buildFlagUrlFunc;
        private readonly IFavoriteService<IMatch> favoriteService;
        private readonly IPopupNavigation popupNavigation;
        private MatchDetailFunction selectedTabItem;
        private readonly ISoccerMatchService soccerMatchService;
        private IDictionary<MatchDetailFunction, TabItemViewModel> tabItemViewModels;
        private string currentMatchId;
        private DateTimeOffset currentMatchEventDate;
        private MatchStatus currentMatchStatus;
        private bool firstLoad = true;
        private bool disposed;

        public MatchDetailViewModel(
            INavigationService navigationService,
            IDependencyResolver dependencyResolver,
            IEventAggregator eventAggregator,
            IPopupNavigation popupNavigation)
            : base(navigationService, dependencyResolver, eventAggregator)
        {
            soccerMatchService = DependencyResolver.Resolve<ISoccerMatchService>();
            matchStatusConverter = DependencyResolver.Resolve<IMatchDisplayStatusBuilder>(CurrentSportId.ToString());
            matchMinuteConverter = DependencyResolver.Resolve<IMatchMinuteBuilder>(CurrentSportId.ToString());
            buildFlagUrlFunc = DependencyResolver.Resolve<Func<string, string>>(FuncNameConstants.BuildFlagUrlFuncName);
            favoriteService = DependencyResolver.Resolve<IFavoriteService<IMatch>>(CurrentSportId.ToString());
            this.popupNavigation = popupNavigation;

            FunctionTabTappedCommand = new DelegateCommand<TabStripItemTappedEventArgs>(OnFunctionTabTapped);
        }

        public MatchViewModel MatchViewModel { get; private set; }

        public string CountryFlag { get; private set; }

        public string DisplayEventDate { get; private set; }

        public string DisplaySecondLeg { get; private set; }

        public bool IsResultOnly { get; private set; }

        public IReadOnlyList<TabItemViewModel> TabItems { get; private set; }

        public DelegateCommand<TabStripItemTappedEventArgs> FunctionTabTappedCommand { get; }

        [Time]
        public override async void Initialize(INavigationParameters parameters)
        {
            IMatch match = null;
            bool isInitFromNotification = false;
            
            if (parameters?["Match"] is IMatch)
            {
                match = parameters["Match"] as IMatch;
                
            }
            else if (parameters?["Id"] is string matchId)
            {
                currentMatchEventDate = DateTimeOffset.Parse(parameters["EventDate"].ToString());
                var matchInfo = await GetMatch(matchId);

                Debug.WriteLine($"Match EventDate {currentMatchEventDate.ToString()}");

                match = matchInfo?.Match;
                isInitFromNotification = true;
            }

            if (match != null)
            {
                await InitializeGeneralInfoAndTabs(match, isInitFromNotification);
            }
        }

        private async Task InitializeGeneralInfoAndTabs(IMatch match, bool isInitFromNotification)
        {
            currentMatchId = match.Id;
            currentMatchEventDate = match.EventDate;
            currentMatchStatus = match.EventStatus;
            IsResultOnly = !(match.Coverage?.Live ?? true);
            BuildGeneralInfo(match);
            CountryFlag = buildFlagUrlFunc(match.CountryCode);

            await Task.Run(async () =>
            {
                var tabModels = GenerateTabItemViewModels(match);

                Device.BeginInvokeOnMainThread(() => TabItems = new List<TabItemViewModel>(tabModels));

                await Task.Delay(200).ContinueWith(_ => LoadSelectedTab());
            });

            if (isInitFromNotification)
            {
                SubscribeEvents();
                MatchViewModel?.BuildFavorite();
            }
        }

        private void LoadSelectedTab()
        {
            if (selectedTabItem != null)
            {
                tabItemViewModels[selectedTabItem]?.OnAppearing();
            }
        }

        public override async void OnAppearing()
        {
            if (!firstLoad && (currentMatchStatus?.IsLive == true))
            {
                var matchInfo = await GetMatch(currentMatchId);

                if (matchInfo?.Match != null)
                {
                    BuildViewModel(matchInfo.Match);
                }

                LoadSelectedTab();
            }
            
            firstLoad = false;

            SubscribeEvents();
            MatchViewModel?.BuildFavorite();
        }

        public override async void OnResumeWhenNetworkOK()
        {
            if (selectedTabItem != null)
            {
                tabItemViewModels[selectedTabItem]?.OnResumeWhenNetworkOK();
            }

            var matchInfo = await GetMatch(currentMatchId);

            if (matchInfo?.Match != null)
            {
                BuildViewModel(matchInfo.Match);
            }

            SubscribeEvents();
        }

        public override void OnSleep()
        {
            if (selectedTabItem != null)
            {
                tabItemViewModels[selectedTabItem]?.OnSleep();
            }

            UnSubscribeEvents();
        }

        public override void OnDisappearing()
        {
            if (tabItemViewModels != null && selectedTabItem != null)
            {
                tabItemViewModels[selectedTabItem]?.OnDisappearing();
            }

            UnSubscribeEvents();
        }

        public override async Task OnNetworkReconnectedAsync()
        {
            if (tabItemViewModels != null && selectedTabItem != null)
            {
                await tabItemViewModels[selectedTabItem]?.OnNetworkReconnectedAsync();
            }

            var matchInfo = await GetMatch(currentMatchId);

            if (matchInfo?.Match != null)
            {
                BuildViewModel(matchInfo.Match);
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            foreach (var item in TabItems)
            {
                item.Destroy();
            }

            Dispose();
        }

        private void SubscribeEvents()
        {
            if (MatchViewModel?.Match == null)
            {
                return;
            }

            if (MatchViewModel.Match.EventStatus.IsLive || MatchViewModel.Match.EventStatus.IsNotStarted)
            {
                EventAggregator?
                    .GetEvent<MatchEventPubSubEvent>()
                    .Subscribe(OnReceivedMatchEvent);

                EventAggregator?
                    .GetEvent<MatchEventRemovedPubSubEvent>()
                    .Subscribe(OnReceivedMatchEventRemoved);

                EventAggregator?
                    .GetEvent<TeamStatisticPubSubEvent>()
                    .Subscribe(OnReceivedTeamStatistic);
            }
        }

        private void UnSubscribeEvents()
        {
            EventAggregator?
                .GetEvent<MatchEventPubSubEvent>()
                .Unsubscribe(OnReceivedMatchEvent);

            EventAggregator?
                .GetEvent<MatchEventRemovedPubSubEvent>()
                .Unsubscribe(OnReceivedMatchEventRemoved);

            EventAggregator?
                .GetEvent<TeamStatisticPubSubEvent>()
                .Unsubscribe(OnReceivedTeamStatistic);
        }

        protected internal void OnReceivedMatchEvent(IMatchEventMessage payload)
        {
            var match = MatchViewModel?.Match;

            if (match == null || payload.SportId != CurrentSportId || payload.MatchEvent.MatchId != match.Id)
            {
                return;
            }

            MatchViewModel.OnReceivedMatchEvent(payload.MatchEvent);

            BuildGeneralInfo(match);
        }

        protected internal void OnReceivedTeamStatistic(ITeamStatisticsMessage payload)
        {
            if (payload.SportId != CurrentSportId || MatchViewModel == null || MatchViewModel.Match.Id != payload.MatchId)
            {
                return;
            }

            MatchViewModel.OnReceivedTeamStatistic(payload.IsHome, payload.TeamStatistic);
        }

#pragma warning disable S3168 // "async" methods should not return "void"

        protected internal async void OnReceivedMatchEventRemoved(IMatchEventRemovedMessage payload)
        {
            if (MatchViewModel == null || MatchViewModel.Match.Id != payload.MatchId)
            {
                return;
            }

            var matchInfo = await GetMatch(currentMatchId);

            if (matchInfo?.Match != null)
            {
                BuildViewModel(matchInfo.Match);
            }
        }

#pragma warning restore S3168 // "async" methods should not return "void"

        private void BuildGeneralInfo(IMatch match)
        {
            BuildViewModel(match);
            BuildSecondLeg(match);

            DisplayEventDate = match.EventDate.ToDayMonth().ToUpperInvariant();
        }

        private void BuildSecondLeg(IMatch match)
        {
            if (!(match is SoccerMatch soccerMatch))
            {
                return;
            }

            var winnerId = soccerMatch.AggregateWinnerId;

            if (!string.IsNullOrWhiteSpace(winnerId) && soccerMatch.EventStatus.IsClosed)
            {
                DisplaySecondLeg = $"{AppResources.SecondLeg} {soccerMatch.AggregateHomeScore} - {soccerMatch.AggregateAwayScore}";
            }
        }

        private void BuildViewModel(IMatch match)
            => MatchViewModel = new MatchViewModel(match, matchStatusConverter, matchMinuteConverter, EventAggregator, favoriteService);

        private IEnumerable<TabItemViewModel> GenerateTabItemViewModels(IMatch match)
        {
            tabItemViewModels = new Dictionary<MatchDetailFunction, TabItemViewModel>
            {
                [MatchDetailFunction.Info] = new InformationViewModel(match, NavigationService, DependencyResolver, EventAggregator, infoTemplate),
                [MatchDetailFunction.Tracker] = new TrackerCommentaryViewModel(match.Id, match.Coverage, match.EventDate, NavigationService, DependencyResolver, EventAggregator, trackerTemplate),
                [MatchDetailFunction.H2H] = new H2HViewModel(match, NavigationService, DependencyResolver, EventAggregator, h2hTemplate),
                [MatchDetailFunction.Lineups] = new LineupsViewModel(match.Id, match.EventDate, NavigationService, DependencyResolver, EventAggregator, lineupsTemplate),
                [MatchDetailFunction.Stats] = new StatisticsViewModel(match.Id, match.EventDate, NavigationService, DependencyResolver, EventAggregator, statisticsTemplate),
            };

            if (match.LeagueHasStandings)
            {
                tabItemViewModels.Add(MatchDetailFunction.Table, new TableViewModel(
                    NavigationService,
                    DependencyResolver,
                    new LeagueDetailNavigationParameter(match.LeagueId,
                        match.LeagueGroupName,
                        match.LeagueOrder,
                        match.CountryCode,
                        match.IsInternationalLeague,
                        match.LeagueRoundGroup,
                        match.LeagueSeasonId,
                        true),
                    tableTemplate,
                    homeTeamId: match.HomeTeamId,
                    awayTeamId: match.AwayTeamId,
                    highlightTeamName: true,
                    showLeagueBlock: false));
            }

            Title = tabItemViewModels.First().Key.DisplayName;
            selectedTabItem = tabItemViewModels.First().Key;

            return (from function in Enumeration.GetAll<MatchDetailFunction>()
                    where tabItemViewModels.ContainsKey(function)
                    select tabItemViewModels[function]).ToList();
        }

        private void OnFunctionTabTapped(TabStripItemTappedEventArgs args)
        {
            if (args != null)
            {
                selectedTabItem = Enumeration.FromValue<MatchDetailFunction>(args.Index);
            }
        }

        private Task<MatchInfo> GetMatch(string id)
            => soccerMatchService.GetMatchAsync(id, CurrentLanguage, currentMatchEventDate);

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                TabItems = null;
                MatchViewModel = null;
            }

            disposed = true;
        }
    }
}