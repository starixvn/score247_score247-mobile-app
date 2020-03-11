using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using LiveScore.Common.Extensions;
using LiveScore.Core;
using LiveScore.Core.Events;
using LiveScore.Core.Models.Leagues;
using LiveScore.Core.Models.Matches;
using LiveScore.Core.Models.Teams;
using LiveScore.Core.Services;
using LiveScore.Core.ViewModels;
using PanCardView.EventArgs;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using Xamarin.Forms;

namespace LiveScore.Features.Score.ViewModels
{
    public class ScoresViewModel : ViewModelBase
    {
        private const byte LiveDateBarItemIndex = 0;
        private const byte TodayDateBarItemIndex = 3;
        private readonly IMatchService matchService;
        private readonly IFavoriteService<IMatch> favoriteMatchService;
        private readonly IFavoriteService<ILeague> favoriteLeagueService;
        private readonly IFavoriteService<ITeamProfile> favoriteTeamService;
        private readonly ISettingsService settingsService;
        private bool secondLoad;

        public ScoresViewModel(
            INavigationService navigationService,
            IDependencyResolver dependencyResolver,
            IEventAggregator eventAggregator) : base(navigationService, dependencyResolver, eventAggregator)
        {
            IsActive = true;
            ScoreItemAppearedCommand = new DelegateCommand<ItemAppearedEventArgs>(OnScoreItemAppeared);
            ScoreItemDisappearingCommand = new DelegateCommand<ItemDisappearingEventArgs>(OnScoreItemDisappearing);
            ClickSearchCommand = new DelegateAsyncCommand(OnClickSearchAsync);
            OpenSearchTeamCommand = new DelegateAsyncCommand(OpenPopupSearchTeam);
            matchService = DependencyResolver.Resolve<IMatchService>(CurrentSportId.ToString());
            InitScoreItemSources();

            settingsService = DependencyResolver.Resolve<ISettingsService>();
            favoriteMatchService = DependencyResolver.Resolve<IFavoriteService<IMatch>>(CurrentSportId.ToString());
            favoriteLeagueService = DependencyResolver.Resolve<IFavoriteService<ILeague>>(CurrentSportId.ToString());
            favoriteTeamService = DependencyResolver.Resolve<IFavoriteService<ITeamProfile>>(CurrentSportId.ToString());
        }

        public byte RangeOfDays { get; } = 2;

        public ObservableCollection<ScoreMatchesViewModel> ScoreItemSources { get; private set; }

        public ScoreMatchesViewModel SelectedScoreItem { get; set; }

        public DelegateCommand<ItemAppearedEventArgs> ScoreItemAppearedCommand { get; }

        public DelegateCommand<ItemDisappearingEventArgs> ScoreItemDisappearingCommand { get; }

        public DelegateAsyncCommand ClickSearchCommand { get; }

        public DelegateAsyncCommand OpenSearchTeamCommand { get; }

        public override async void OnResumeWhenNetworkOK()
        {
            var todayItem = ScoreItemSources[TodayDateBarItemIndex];

            if (todayItem?.ViewDate != DateTime.Today)
            {
                await NavigateToHomeAsync().ConfigureAwait(false);
            }
            else
            {
                SelectedScoreItem?.OnResumeWhenNetworkOK();
            }

            GetLiveMatchesCount();
        }

        public override Task OnNetworkReconnectedAsync() => SelectedScoreItem?.OnNetworkReconnectedAsync();

        public override void OnSleep() => SelectedScoreItem?.OnSleep();

        public override async void OnAppearing()
        {
            if (!IsActive)
            {
                return;
            }

            var todayItem = ScoreItemSources[TodayDateBarItemIndex];

            if (todayItem?.ViewDate != DateTime.Today)
            {
                await NavigateToHomeAsync().ConfigureAwait(false);
            }
            else
            {
                SelectedScoreItem?.OnAppearing();
                GetLiveMatchesCount();
            }

            Debug.WriteLine("ScoresViewModel OnAppearing");
        }

        public override void OnDisappearing()
        {
            SelectedScoreItem?.OnDisappearing();
        }

        private void OnScoreItemAppeared(ItemAppearedEventArgs args)
        {
            if (SelectedScoreItem == null)
            {
                return;
            }

            SelectedScoreItem.IsActive = true;

            if (secondLoad)
            {
                SelectedScoreItem.OnAppearing();
            }

            secondLoad = true;

            EventAggregator
              .GetEvent<FinishInitializationEvent>()
              .Subscribe(OnFinishInitialization);
        }

        private void GetLiveMatchesCount()
        {
            Task.Run(async () =>
            {
                var liveMatchCount = await matchService.GetLiveMatchesCountAsync();

                Device.BeginInvokeOnMainThread(()
                    => ScoreItemSources[LiveDateBarItemIndex].HasData = liveMatchCount > 0);
            });
        }

        private void OnScoreItemDisappearing(ItemDisappearingEventArgs args)
        {
            if (args.Index >= 0)
            {
                var oldItemSource = ScoreItemSources[args.Index];

                oldItemSource.IsActive = false;
                oldItemSource.OnDisappearing();
            }

            EventAggregator
                .GetEvent<FinishInitializationEvent>()
                .Unsubscribe(OnFinishInitialization);
        }

        private void InitScoreItemSources()
        {
            ScoreItemSources = new ObservableCollection<ScoreMatchesViewModel>
            {
                new LiveMatchesViewModel(NavigationService, DependencyResolver, EventAggregator)
            };

            for (var i = -RangeOfDays; i <= RangeOfDays; i++)
            {
                ScoreItemSources.Add(
                    new ScoreMatchesViewModel(DateTime.Today.AddDays(i), NavigationService, DependencyResolver, EventAggregator));
            }

            ScoreItemSources.Add(
                new CalendarMatchesViewModel(NavigationService, DependencyResolver, EventAggregator));

            SelectedScoreItem = ScoreItemSources[TodayDateBarItemIndex];
        }

        private async Task OnClickSearchAsync()
        {
            var navigated = await NavigationService
                .NavigateAsync("SearchNavigationPage/SearchView", useModalNavigation: true)
                .ConfigureAwait(false);

            if (!navigated.Success)
            {
                await LoggingService.LogExceptionAsync(navigated.Exception).ConfigureAwait(false);
            }
        }

        private async Task OpenPopupSearchTeam()
        {
            await NavigationService.NavigateAsync("NavigationPage/SearchTeamsView", useModalNavigation: true, animated: false);
        }

        private void OnFinishInitialization()
        {
            Task.Run(async () =>
            {
                Debug.WriteLine("OnFinishInitialization start sync");

                await favoriteMatchService.Sync();
                await favoriteLeagueService.Sync();
                await favoriteTeamService.Sync();
                await settingsService.SyncNotification();
            });
        }
    }
}