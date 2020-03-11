using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Common;
using LiveScore.Common.Extensions;
using LiveScore.Common.LangResources;
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
        private readonly ISearchService<ITeamProfile> searchService;
        private readonly IFavoriteService<ITeamProfile> favoriteService;
        private readonly Func<string, string> buildTeamLogoUrlFunc;
        private IList<ITeamProfile> favoriteTeams;
        private IList<ITeamProfile> trendingTeams;
        private string currentSearchKey;

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
            searchService = DependencyResolver.Resolve<ISearchService<ITeamProfile>>(CurrentSportId.ToString());
            favoriteService = DependencyResolver.Resolve<IFavoriteService<ITeamProfile>>(CurrentSportId.ToString());
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
            if (string.IsNullOrEmpty(currentSearchKey))
            {
                await LoadRecentSearchAndTrendingTeams();
            }
            else
            {
                await OnSearchTeam(currentSearchKey);
            }
        }

        public bool ShowTrendingHeader { get; private set; } = true;

        public DelegateAsyncCommand CancelSearchCommand { get; }

        public DelegateAsyncCommand<string> SearchTeamCommand { get; }

        public DelegateAsyncCommand<ITeamProfile> FavoriteCommand { get; }

        public DelegateAsyncCommand<ITeamProfile> OpenTeamDetailCommand { get; }

        public IReadOnlyCollection<ITeamProfile> TeamsItemSource { get; private set; }

        public IReadOnlyCollection<SearchListGroup> DefaultSearch { get; private set; }

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

            currentSearchKey = text;
            ShowTrendingHeader = false;
            var teams = (await teamService.SearchTeams(CurrentLanguage.DisplayName, text)).ToList();

            BuildSearchResultItemSource(teams);
        }

        private void BuildSearchResultItemSource(List<ITeamProfile> teams)
        {
            foreach (var team in teams)
            {
                team.LogoUrl = buildTeamLogoUrlFunc(team.LogoName);
                team.IsFavorite = favoriteTeams.Any(favoriteTeam => favoriteTeam.Id == team.Id);
            }
            TeamsItemSource = new List<ITeamProfile>(teams);
        }

        private async Task LoadRecentSearchAndTrendingTeams()
        {
            ShowTrendingHeader = false;

            var recentAndTrendingTeams = new List<SearchListGroup>();

            var recentSearch = GetRecentSearch();
            if (recentSearch.Any())
            {
                recentAndTrendingTeams.Add(recentSearch);
            }

            var trendingGroup = await GetTrendingTeams();
            recentAndTrendingTeams.Add(trendingGroup);

            DefaultSearch = new List<SearchListGroup>(recentAndTrendingTeams);
            ShowTrendingHeader = true;
        }

        private async Task<SearchListGroup> GetTrendingTeams()
        {
            trendingTeams ??= (await teamService.GetTrendingTeams(CurrentLanguage.DisplayName))?.ToList();
            var trendingGroup = new SearchListGroup();
            if (trendingTeams == null)
            {
                return trendingGroup;
            }

            foreach (var team in trendingTeams)
            {
                team.LogoUrl = buildTeamLogoUrlFunc(team.LogoName);
                team.IsFavorite = favoriteTeams.Any(favoriteTeam => favoriteTeam.Id == team.Id);
                trendingGroup.Add(team);
            }

            trendingGroup.Heading = AppResources.Trending;

            return trendingGroup;
        }

        private SearchListGroup GetRecentSearch()
        {
            var recentSearchTeams = searchService.GetRecentSearchKeys(RecentSearchLinit);

            if (recentSearchTeams.Any())
            {
                var recentSearch = new SearchListGroup
                {
                    Heading = AppResources.RecentSearch
                };
                foreach (var team in recentSearchTeams)
                {
                    team.IsFavorite = favoriteTeams.Any(favoriteTeam => favoriteTeam.Id == team.Id);
                    recentSearch.Add(team);
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

            foreach (var teamGroup in DefaultSearch)
            {
                foreach (var groupItem in teamGroup)
                {
                    if (groupItem.Id == team.Id)
                    {
                        groupItem.IsFavorite = team.IsFavorite;
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task OnTapTeam(ITeamProfile team)
        {
            searchService.UpdateRecent(team);

            await NavigationService.NavigateAsync(
                "FavoriteTeamMatchesView",
                new NavigationParameters
                {
                    {"Team", team}
                });
        }
    }
}