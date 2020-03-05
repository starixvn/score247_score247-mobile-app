using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Common.Extensions;
using LiveScore.Common.LangResources;
using LiveScore.Core;
using LiveScore.Core.Controls.TabStrip;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Matches;
using LiveScore.Core.PubSubEvents.Matches;
using LiveScore.Soccer.Extensions;
using LiveScore.Soccer.Models.Matches;
using LiveScore.Soccer.Services;
using LiveScore.Soccer.ViewModels.Matches.MatchDetails.Information.InfoItems;
using MethodTimer;
using Prism.Events;
using Prism.Navigation;
using Xamarin.Forms;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Soccer.Tests")]

namespace LiveScore.Soccer.ViewModels.Matches.MatchDetails.Information
{
    public class InformationViewModel : TabItemViewModel, IDisposable
    {
        private const string SpectatorNumberFormat = "0,0";
        private readonly ISoccerMatchService matchInfoService;
        private readonly IEventAggregator eventAggregator;
        private readonly IMatch match;
        private bool disposed;

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
            IsBusy = true;
        }

        public DelegateAsyncCommand RefreshCommand { get; }

        public bool IsRefreshing { get; set; }

        public MatchInfo MatchInfo { get; private set; }

        public string DisplayEventDate
            => MatchInfo?.Match?.EventDate.ToDateTimeWithoutSecond();

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

        public override void Destroy()
        {
            base.Destroy();

            Dispose();
        }

        internal async Task LoadMatchInfoData()
        {
            try
            {
                await LoadDataAsync(LoadMatchDetail);
            }
            catch (Exception ex)
            {
                await LoggingService.LogExceptionAsync(ex);
            }
        }

        private void SubscribeEvents()
        {
            if (match.EventStatus.IsNotStarted || match.EventStatus.IsLive)
            {
                eventAggregator
                    .GetEvent<MatchEventPubSubEvent>()
                    .Subscribe(OnReceivedMatchEvent);

                EventAggregator
                   .GetEvent<MatchEventRemovedPubSubEvent>()
                   .Subscribe(OnReceivedMatchEventRemoved);
            }
        }

        private void UnsubscribeEvents()
        {
            eventAggregator
                .GetEvent<MatchEventPubSubEvent>()
                .Unsubscribe(OnReceivedMatchEvent);

            EventAggregator
                .GetEvent<MatchEventRemovedPubSubEvent>()
                .Unsubscribe(OnReceivedMatchEventRemoved);
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types

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

#pragma warning restore S3215 // "interface" instances should not be cast to concrete types

#pragma warning disable S3168 // "async" methods should not return "void"

        protected internal async void OnReceivedMatchEventRemoved(IMatchEventRemovedMessage message)
        {
            if (match.Id != message.MatchId || MatchInfo == null)
            {
                return;
            }

            MatchInfo = await matchInfoService
                .GetMatchAsync(match.Id, CurrentLanguage, match.EventDate)
                .ConfigureAwait(false);

            BuildInfoItems(MatchInfo);
        }

#pragma warning restore S3168 // "async" methods should not return "void"

        private async Task LoadMatchDetail()
        {
            MatchInfo = await matchInfoService
                .GetMatchAsync(match.Id, CurrentLanguage, match.EventDate);

            BuildInfoItems(MatchInfo);

            IsRefreshing = false;
        }

        private void BuildInfoItems(MatchInfo matchInfo)
        {
            if (matchInfo?.TimelineEvents == null)
            {
                return;
            }

            matchInfo.UpdateTimelineEvents(matchInfo.TimelineEvents?
                    .OrderByDescending(t => t.MatchTime)
                    .ThenByDescending(t => t.StoppageTime)
                    .ThenByDescending(t => t.Time));

            var timelineEvents = matchInfo.TimelineEvents?
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
                InfoItemViewModels = null;
                MatchInfo = null;
            }

            disposed = true;
        }
    }
}