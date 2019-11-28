using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Common.Extensions;
using LiveScore.Common.LangResources;
using LiveScore.Core;
using LiveScore.Core.Controls.TabStrip;
using LiveScore.Core.Models.Matches;
using LiveScore.Core.PubSubEvents.Matches;
using LiveScore.Soccer.Extensions;
using LiveScore.Soccer.Models.Matches;
using LiveScore.Soccer.Services;
using LiveScore.Soccer.ViewModels.MatchDetails.Information.InfoItems;
using MethodTimer;
using Prism.Events;
using Prism.Navigation;
using Xamarin.Forms;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Soccer.Tests")]

namespace LiveScore.Soccer.ViewModels.MatchDetails.Information
{
    public class InformationViewModel : TabItemViewModel
    {
        private const string SpectatorNumberFormat = "0,0";
        private readonly ISoccerMatchService matchInfoService;
        private readonly IEventAggregator eventAggregator;
        private readonly IMatch match;

        public InformationViewModel(
            IMatch match,
            INavigationService navigationService,
            IDependencyResolver dependencyResolver,
            IEventAggregator eventAggregator,
            DataTemplate dataTemplate)
            : base(navigationService, dependencyResolver, dataTemplate, eventAggregator, AppResources.Info)
        {
            this.match = match;
            this.eventAggregator = eventAggregator;
            matchInfoService = DependencyResolver.Resolve<ISoccerMatchService>();
            RefreshCommand = new DelegateAsyncCommand(async () => await LoadDataAsync(LoadMatchDetail, false));
        }

        public DelegateAsyncCommand RefreshCommand { get; }

        public bool IsRefreshing { get; set; }

        public MatchInfo MatchInfo { get; private set; }

        public string DisplayEventDate
            => MatchInfo?.Match?.EventDate.ToFullLocalDateTime();

        public string DisplayAttendance
            => MatchInfo?.Attendance > 0 ? MatchInfo.Attendance.ToString(SpectatorNumberFormat) : string.Empty;

        public string DisplayVenue
            => MatchInfo?.Venue?.Name;

        public string DisplayReferee => MatchInfo?.Referee;

        public ObservableCollection<BaseItemViewModel> InfoItemViewModels { get; private set; }

        public override Task OnNetworkReconnectedAsync() => LoadMatchInfoData();

        public override async void OnResumeWhenNetworkOK()
        {
            SubscribeEvents();
            await LoadMatchInfoData();
        }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            SubscribeEvents();
            await LoadMatchInfoData();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();

            UnsubscribeEvents();
        }

        public override void OnSleep() => UnsubscribeEvents();

        internal async Task LoadMatchInfoData()
        {
            try
            {
                await LoadDataAsync(LoadMatchDetail).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }
        }

        private void SubscribeEvents()
        {
            if (match.EventStatus.IsNotStarted || match.EventStatus.IsLive)
            {
                eventAggregator
                    .GetEvent<MatchEventPubSubEvent>()
                    .Subscribe(OnReceivedMatchEvent);
            }
        }

        private void UnsubscribeEvents()
        {
            eventAggregator
                .GetEvent<MatchEventPubSubEvent>()
                .Unsubscribe(OnReceivedMatchEvent);
        }

        [Time]
        protected internal void OnReceivedMatchEvent(IMatchEventMessage matchEventMessage)
        {
            if (matchEventMessage.SportId != CurrentSportId
                || matchEventMessage.MatchEvent.MatchId != match.Id
                || MatchInfo == null)
            {
                return;
            }

            MatchInfo.Match.UpdateResult(matchEventMessage.MatchEvent.MatchResult);

            if (MatchInfo.TimelineEvents == null)
            {
                MatchInfo.UpdateTimelineEvents(new List<TimelineEvent>());
            }

            MatchInfo.UpdateTimelineEvents(MatchInfo
                .TimelineEvents
                .Concat(new List<TimelineEvent> {
                    matchEventMessage.MatchEvent.Timeline as TimelineEvent
                }));

            BuildInfoItems(MatchInfo);
        }

        private async Task LoadMatchDetail()
        {
            MatchInfo = await matchInfoService
                .GetMatchAsync(match.Id, CurrentLanguage)
                .ConfigureAwait(false);

            BuildInfoItems(MatchInfo);

            IsRefreshing = false;
        }

        private void BuildInfoItems(MatchInfo matchInfo)
        {
            if (matchInfo.TimelineEvents == null)
            {
                return;
            }

            matchInfo
                .UpdateTimelineEvents(matchInfo.FilterPenaltyEvents()?
                .OrderByDescending(t => t.MatchTime)
                .ThenByDescending(t => t.Time));

            var timelineEvents = matchInfo.TimelineEvents
                .Where(t => t.IsDetailInfoEvent())
                .Distinct()
                .ToList();

            if (timelineEvents != null)
            {
                InfoItemViewModels = new ObservableCollection<BaseItemViewModel>(
                    timelineEvents.Select(t =>
                        BaseItemViewModel.CreateInstance(t, MatchInfo, NavigationService, DependencyResolver).BuildData()));
            }
        }
    }
}