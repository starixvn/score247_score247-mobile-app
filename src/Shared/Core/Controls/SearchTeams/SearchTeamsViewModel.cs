using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Common;
using LiveScore.Common.Extensions;
using LiveScore.Core.Models.Teams;
using LiveScore.Core.Services;
using LiveScore.Core.ViewModels;
using Prism.Navigation;

namespace LiveScore.Core.Controls.SearchTeams
{
    public class SearchTeamsViewModel : ViewModelBase
    {
        private readonly ITeamService teamService;
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
            await LoadTrendingTeams();
        }

        public bool? ShowTrendingHeader { get; private set; } = true;

        public DelegateAsyncCommand CancelSearchCommand { get; }

        public DelegateAsyncCommand<string> SearchTeamCommand { get; }

        public DelegateAsyncCommand<ITeamProfile> FavoriteCommand { get; }

        public DelegateAsyncCommand<ITeamProfile> OpenTeamDetailCommand { get; }

        public IReadOnlyCollection<ITeamProfile> TeamsItemSource { get; private set; }

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
                    await LoadTrendingTeams();
                }

                return;
            }

            ShowTrendingHeader = null;

            var team = (await teamService.SearchTeams(CurrentLanguage.DisplayName, text)).ToList();

            BuildTeamItemSource(team);
        }

        private async Task LoadTrendingTeams()
        {
            ShowTrendingHeader = true;

            trendingTeams ??= (await teamService.GetTrendingTeams(CurrentLanguage.DisplayName))?.ToList();

            BuildTeamItemSource(trendingTeams);
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

        private void BuildTeamItemSource(IList<ITeamProfile> teams)
        {
            var teamList = teams?.ToList();

            if (teamList?.Any() != true)
            {
                TeamsItemSource = new List<ITeamProfile>();
                return;
            }

            teamList.ForEach(team =>
            {
                team.LogoUrl = buildTeamLogoUrlFunc(team.LogoName);
                team.IsFavorite = favoriteTeams.Any(favoriteTeam => favoriteTeam.Id == team.Id);
            });

            TeamsItemSource = teamList;
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