using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Common.Extensions;
using LiveScore.Common.Services;
using LiveScore.Core;
using LiveScore.Core.Extensions;
using LiveScore.Core.Models.Matches;
using LiveScore.Core.PubSubEvents.Matches;
using LiveScore.Core.PubSubEvents.Teams;
using LiveScore.Core.ViewModels;
using MethodTimer;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using Device = Xamarin.Forms.Device;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LiveScore.Tests")]

namespace LiveScore.Features.Score.ViewModels
{
    public class ScoreMatchesViewModel : MatchesViewModel
    {
        private const int EnableLoadOnDemainMatchThreshold = 150;
        private const int DefaultFirstLoadMatchItemCount = 8;
        private const int DefaultLoadingMatchItemCountOnScrolling = 16;

        [Time]
        public ScoreMatchesViewModel(
            DateTime viewDate,
            INavigationService navigationService,
            IDependencyResolver dependencyResolver,
            IEventAggregator eventAggregator)
            : base(navigationService, dependencyResolver, eventAggregator)
        {
            ViewDate = viewDate;
            RemainingMatchItemSource = new ObservableCollection<IGrouping<MatchGroupViewModel, MatchViewModel>>();
            LoadMoreCommand = new DelegateCommand(OnLoadMore);
        }

        public DateTime ViewDate { get; }

        public bool IsActive { get; set; }

        public bool IsLoadMore { get; private set; }

        public bool IsNotLoadMore => !IsLoadMore;

        public ObservableCollection<IGrouping<MatchGroupViewModel, MatchViewModel>> RemainingMatchItemSource { get; protected set; }

        public DelegateCommand LoadMoreCommand { get; protected set; }

        public override void OnResumeWhenNetworkOK()
        {
            base.OnResumeWhenNetworkOK();

            if (ViewDate.IsTodayOrYesterday())
            {
                SubscribeEvents();
                Task.Run(() => LoadDataAsync(UpdateMatchesAsync, false));
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            if (ViewDate.IsTodayOrYesterday())
            {
                SubscribeEvents();
            }

            if (FirstLoad)
            {
                FirstLoad = false;
                LoadDataAsync(LoadMatchesAsync).ConfigureAwait(false);
            }
            else
            {
                if (ViewDate.IsTodayOrYesterday())
                {
                    Task.Run(() => LoadDataAsync(UpdateMatchesAsync, false));
                }
            }
        }

        protected override void OnReceivedMatchEvent(IMatchEventMessage payload)
        {
            if (payload?.SportId != CurrentSportId)
            {
                return;
            }
            base.OnReceivedMatchEvent(payload);

            RemainingMatchItemSource?.UpdateMatchItemEvent(payload.MatchEvent);
        }

        protected override void OnReceivedTeamStatistic(ITeamStatisticsMessage payload)
        {
            if (payload?.SportId != CurrentSportId)
            {
                return;
            }
            base.OnReceivedTeamStatistic(payload);

            RemainingMatchItemSource?.UpdateMatchItemStatistics(
                payload.MatchId,
                payload.IsHome,
                payload.TeamStatistic);
        }

        private void OnLoadMore()
        {
            if (RemainingMatchItemSource?.Any() != true)
            {
                return;
            }

            IsLoadMore = true;
            IsBusy = true;

            Task.Delay(1000)
                .ContinueWith(_ =>
                {
                    var matchItems = RemainingMatchItemSource.Take(DefaultLoadingMatchItemCountOnScrolling);

                    RemainingMatchItemSource
                        = new ObservableCollection<IGrouping<MatchGroupViewModel, MatchViewModel>>(
                            RemainingMatchItemSource.Skip(DefaultLoadingMatchItemCountOnScrolling));

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MatchItemsSource.AddItems(matchItems);

                        IsBusy = false;
                    });
                });
        }

        protected override async Task UpdateMatchesAsync()
        {
            await base.UpdateMatchesAsync();

            if (MatchItemsSource == null || !MatchItemsSource.Any())
            {
                RemainingMatchItemSource?.Clear();
            }
        }

        protected override void InitializeMatchItems(IEnumerable<IMatch> matches)
        {
            var matchItemViewModels = matches.Select(match => new MatchViewModel(
                match,
                matchStatusBuilder,
                matchMinuteBuilder,
                EventAggregator));

            var matchItems
                = matchItemViewModels.GroupBy(item => new MatchGroupViewModel(item.Match, buildFlagUrlFunc, NavigationService, CurrentSportId));

            if (matchItemViewModels.Count() > EnableLoadOnDemainMatchThreshold)
            {
                var loadItems = matchItems.Take(DefaultFirstLoadMatchItemCount);

                RemainingMatchItemSource
                    = new ObservableCollection<IGrouping<MatchGroupViewModel, MatchViewModel>>(matchItems.Skip(DefaultFirstLoadMatchItemCount));

                Device.BeginInvokeOnMainThread(()
                    => MatchItemsSource = new ObservableCollection<IGrouping<MatchGroupViewModel, MatchViewModel>>(loadItems));
            }
            else
            {
                Device.BeginInvokeOnMainThread(()
                    => MatchItemsSource = new ObservableCollection<IGrouping<MatchGroupViewModel, MatchViewModel>>(matchItems));
            }
        }

        protected override void UpdateMatchItems(IEnumerable<IMatch> matches)
        {
            try
            {
                if ((MatchItemsSource == null || MatchItemsSource.Count == 0) && matches?.Any() == true)
                {
                    InitializeMatchItems(matches);
                    return;
                }

                var loadedMatches = matches
                    .Where(m => MatchItemsSource.Any(l => l.Any(lm => lm.Match?.Id == m.Id)));

                Device.BeginInvokeOnMainThread(() =>
                    MatchItemsSource.UpdateMatchItems(
                        loadedMatches,
                        matchStatusBuilder,
                        matchMinuteBuilder,
                        EventAggregator,
                        buildFlagUrlFunc,
                        NavigationService,
                        CurrentSportId));

                var remainingMatches = matches.Except(loadedMatches);

                RemainingMatchItemSource?
                    .UpdateMatchItems(
                        remainingMatches,
                        matchStatusBuilder,
                        matchMinuteBuilder,
                        EventAggregator,
                        buildFlagUrlFunc,
                        NavigationService,
                        CurrentSportId);
            }
            catch (Exception ex)
            {
            }
        }

        protected override Task<IEnumerable<IMatch>> LoadMatchesFromServiceAsync()
            => matchService.GetMatchesByDateAsync(ViewDate, CurrentLanguage);
    }
}