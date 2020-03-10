using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Core;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Favorites;
using LiveScore.Core.Models.Leagues;
using LiveScore.Soccer.Models.Leagues;

namespace LiveScore.Soccer.Services.Favorites
{
    public class FavoriteLeagueService : SoccerFavoriteService<ILeague>
    {
        private const int LeagueLimit = 30;

        public FavoriteLeagueService(
            IDependencyResolver dependencyResolver,
            SoccerApi.FavoriteApi favoriteApi = null)
                : base("Leagues", LeagueLimit, dependencyResolver, favoriteApi)
        {
            Init();

            Task.Run(() => Sync(
                addedFavorites: Objects.Select(league => new Favorite(league.Id, FavoriteType.LeagueValue, league.Name)).ToList()
            )).ConfigureAwait(false);

            OnRemovedFavorite += HandleRemoveFavorite;
            OnAddedFavorite += HandleAddFavorite;
        }

        private Task HandleRemoveFavorite(ILeague league)
            => Task.Run(() => Sync(
                removedFavorites: new List<Favorite> { new Favorite(league.Id, FavoriteType.LeagueValue, league.Name) }));

        private Task HandleAddFavorite(ILeague league)
            => Task.Run(() => Sync(
                addedFavorites: new List<Favorite> { new Favorite(league.Id, FavoriteType.LeagueValue, league.Name) }));

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types

        public override void UpdateCache()
        {
            userSettingService.AddOrUpdateValue(Key, Objects.Select(obj => obj as League).ToList());
        }

#pragma warning restore S3215 // "interface" instances should not be cast to concrete types

        public override IList<ILeague> LoadCache()
            => userSettingService.GetValueOrDefault(Key, Enumerable.Empty<League>()).Select(l => l as ILeague).ToList();
    }
}