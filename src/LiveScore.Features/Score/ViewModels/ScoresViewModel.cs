using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LiveScore.Common.Extensions;
using LiveScore.Core;
using LiveScore.Core.ViewModels;
using PanCardView.EventArgs;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation;

namespace LiveScore.Features.Score.ViewModels
{
    public class ScoresViewModel : ViewModelBase
    {
        private const byte LiveDateBarItemIndex = 0;
        private const byte TodayDateBarItemIndex = 3;
        private bool secondLoad;

        public ScoresViewModel(
            INavigationService navigationService,
            IDependencyResolver dependencyResolver,
            IEventAggregator eventAggregator) : base(navigationService, dependencyResolver, eventAggregator)

        {
            ScoreItemAppearedCommand = new DelegateCommand<ItemAppearedEventArgs>(OnScoreItemAppeared);
            ScoreItemDisappearingCommand = new DelegateCommand<ItemDisappearingEventArgs>(OnScoreItemDisappearing);
            ClickSearchCommand = new DelegateAsyncCommand(OnClickSearchAsync);

            InitScoreItemSources();
        }

        public byte RangeOfDays { get; } = 2;

        public ObservableCollection<ScoreMatchesViewModel> ScoreItemSources { get; private set; }

        public ScoreMatchesViewModel SelectedScoreItem { get; set; }

        public DelegateCommand<ItemAppearedEventArgs> ScoreItemAppearedCommand { get; }

        public DelegateCommand<ItemDisappearingEventArgs> ScoreItemDisappearingCommand { get; }

        public DelegateAsyncCommand ClickSearchCommand { get; }

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
        }

        public override Task OnNetworkReconnectedAsync() => SelectedScoreItem?.OnNetworkReconnectedAsync();

        public override void OnSleep() => SelectedScoreItem?.OnSleep();

        public override async void OnAppearing()
        {
            var todayItem = ScoreItemSources[TodayDateBarItemIndex];

            if (todayItem?.ViewDate != DateTime.Today)
            {
                await NavigateToHomeAsync().ConfigureAwait(false);
            }
            else
            {
                SelectedScoreItem?.OnAppearing();
            }
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
            else
            {
                ScoreItemSources[LiveDateBarItemIndex].OnAppearing();
            }

            secondLoad = true;
        }

        private void OnScoreItemDisappearing(ItemDisappearingEventArgs args)
        {
            if (args.Index >= 0)
            {
                var oldItemSource = ScoreItemSources[args.Index];

                oldItemSource.IsActive = false;
                oldItemSource.OnDisappearing();
            }
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
            }
        }
    }
}