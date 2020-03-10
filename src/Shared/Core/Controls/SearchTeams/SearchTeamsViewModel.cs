using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Common;
using LiveScore.Common.Extensions;
using LiveScore.Common.LangResources;
using LiveScore.Core.Models.Searches;
using LiveScore.Core.Models.Teams;
using LiveScore.Core.Services;
using LiveScore.Core.ViewModels;
using Prism.Navigation;

namespace LiveScore.Core.Controls.SearchTeams
{
    public class SearchTeamsViewModel : ViewModelBase
    {
        private const int RecentSearchLinit = 3;
        private readonly ITeamService teamService;
        private readonly ISearchService searchService;
        private readonly IFavoriteService<ITeamProfile> favoriteService;
        private readonly Func<string, string> buildTeamLogoUrlFunc;
        private IList<ITeamProfile> favoriteTeams;
        private IList<ITeamProfile> trendingTeams;

        public SearchTeamsViewModel(
            INavigationService navigationService,
            IDependencyResolver dependencyResolver)
            : base(navigationService, dependencyResolver)
        {
            CancelSearchCommand = new DelegateAsyncCommand(OnCancelSearch);
            SearchTeamCommand = new DelegateAsyncCommand<string>(OnSearchTeam);
            FavoriteCommand = new DelegateAsyncCommand<ITeamProfile>(OnFavoriteTeam);
            OpenTeamDetailCommand = new DelegateAsyncCommand<ITeamProfile>(OnTapTeam);
            buildTeamLogoUrlFunc = DependencyResolver.Resolve<Func<string, string>>(FuncNameConstants.BuildTeamLogoUrlFuncName);
            teamService = DependencyResolver.Resolve<ITeamService>(CurrentSportId.ToString());
            searchService = DependencyResolver.Resolve<ISearchService>(CurrentSportId.ToString());
            favoriteService = DependencyResolver.Resolve<IFavoriteService<ITeamProfile>>(CurrentSportId.ToString());

            DefaultSearch = new ObservableCollection<SearchListGroup>();
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(() => LoadDataAsync(LoadTeamData));
        }

        public override Task OnNetworkReconnectedAsync() => LoadDataAsync(LoadTeamData);

        public override async void OnResumeWhenNetworkOK() => await LoadDataAsync(LoadTeamData);

        private async Task LoadTeamData()
        {
            favoriteTeams = favoriteService.GetAll();
            await LoadRecentSearchAndTrendingTeams();
        }

        public bool? ShowTrendingHeader { get; private set; } = true;

        public DelegateAsyncCommand CancelSearchCommand { get; }

        public DelegateAsyncCommand<string> SearchTeamCommand { get; }

        public DelegateAsyncCommand<ITeamProfile> FavoriteCommand { get; }

        public DelegateAsyncCommand<ITeamProfile> OpenTeamDetailCommand { get; }

        public IReadOnlyCollection<ITeamProfile> TeamsItemSource { get; private set; }

        public IList<RecentSearch> RecentSearches { get; private set; }

        public ObservableCollection<SearchListGroup> DefaultSearch { get; private set; }

        public async Task OnCancelSearch()
        {
            await NavigationService.GoBackAsync(animated: false);
        }

        private async Task OnSearchTeam(string text)
        {
            if (text.Length < 2)
            {
                if (text.Length == 0)
                {
                    await LoadRecentSearchAndTrendingTeams();
                }

                return;
            }

            ShowTrendingHeader = null;

            var team = (await teamService.SearchTeams(CurrentLanguage.DisplayName, text)).ToList();

            BuildSearchResultItemSource(team);
        }

        private void BuildSearchResultItemSource(List<ITeamProfile> teams)
        {
            DefaultSearch = new ObservableCollection<SearchListGroup>();
            var searchResults = new SearchListGroup();
            searchResults.Heading = "Results";
            foreach (var team in teams)
            {
                team.LogoUrl = buildTeamLogoUrlFunc(team.LogoName);
                team.IsFavorite = favoriteTeams.Any(favoriteTeam => favoriteTeam.Id == team.Id);
                searchResults.Add(new SearchItem(team));
            }
            DefaultSearch.Add(searchResults);
        }

        private async Task LoadRecentSearchAndTrendingTeams()
        {
            ShowTrendingHeader = true;
            DefaultSearch = new ObservableCollection<SearchListGroup>();

            var recentSearch = GetRecentSearch();
            if (recentSearch.Any())
            {
                DefaultSearch.Add(recentSearch);
            }

            var trendingGroup = await GetTrendingTeams();
            DefaultSearch.Add(trendingGroup);
        }

        private async Task<SearchListGroup> GetTrendingTeams()
        {
            trendingTeams ??= (await teamService.GetTrendingTeams(CurrentLanguage.DisplayName))?.ToList();

            var trendingGroup = new SearchListGroup();
            foreach (var team in trendingTeams)
            {
                team.LogoUrl = buildTeamLogoUrlFunc(team.LogoName);
                team.IsFavorite = favoriteTeams.Any(favoriteTeam => favoriteTeam.Id == team.Id);
                trendingGroup.Add(new SearchItem(team));
            }

            trendingGroup.Heading = AppResources.Trending;

            return trendingGroup;
        }

        private SearchListGroup GetRecentSearch()
        {
            var recentSearchKeys = searchService.GetRecentSearchKeys(RecentSearchLinit);

            if (recentSearchKeys.Any())
            {
                var recentSearch = new SearchListGroup();
                recentSearch.Heading = AppResources.RecentSearch;
                foreach (var key in recentSearchKeys)
                {
                    recentSearch.Add(new SearchItem(key.Key));
                }

                return recentSearch;
            }

            return new SearchListGroup();
        }

        private Task OnFavoriteTeam(ITeamProfile team)
        {
            if (team.IsFavorite)
            {
                favoriteService.Remove(team);
                team.IsFavorite = false;
            }
            else
            {
                favoriteService.Add(team);
                team.IsFavorite = true;
            }

            return Task.CompletedTask;
        }

        private async Task OnTapTeam(ITeamProfile team)
        {
            await NavigationService.NavigateAsync(
                "FavoriteTeamMatchesView",
                new NavigationParameters
                {
                    {"Team", team}
                });
        }
    }
}