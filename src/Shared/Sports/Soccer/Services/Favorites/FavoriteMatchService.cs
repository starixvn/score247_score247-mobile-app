using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Core;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Extensions;
using LiveScore.Core.Models.Favorites;
using LiveScore.Core.Models.Matches;
using LiveScore.Soccer.Models.Matches;

namespace LiveScore.Soccer.Services.Favorites
{
    public class FavoriteMatchService : SoccerFavoriteService<IMatch>
    {
        public FavoriteMatchService(
            IDependencyResolver dependencyResolver,
            SoccerApi.FavoriteApi favoriteApi = null)
                : base(
                    "Matches",
                    99,
                    dependencyResolver,
                    favoriteApi)
        {
            CleanUpAndInit();

            OnAddedFavorite += HandleAddFavorite;
            OnRemovedFavorite += HandleRemoveFavorite;
        }

        public override IList<IMatch> GetAll()
            => base.GetAll().Where(match => match.IsEnableFavorite()).ToList();

        public override bool IsFavorite(IMatch obj)
        {
            if (obj.IsEnableFavorite())
            {
                return base.IsFavorite(obj);
            }

            RemoveFromCache(obj);

            return false;
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types

        public override void UpdateCache()
        {
            userSettingService.AddOrUpdateValue(
                Key,
                Objects.Select(obj => obj as SoccerMatch).ToList());
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types

        public override IList<IMatch> LoadCache() =>
            userSettingService.GetValueOrDefault(
                Key,
                Enumerable.Empty<SoccerMatch>()).Select(l => l as IMatch).ToList();

        private Task HandleAddFavorite(IMatch match)
            => Task.Run(() => Sync(
                addedFavorites: new List<Favorite> { new Favorite(match.Id, FavoriteType.MatchValue) }
            ));

        private Task HandleRemoveFavorite(IMatch match)
            => Task.Run(() => Sync(
               removedFavorites: new List<Favorite> { new Favorite(match.Id, FavoriteType.MatchValue) }
           ));

        private void CleanUpAndInit()
        {
            Init();

            var matches = LoadCache();
            var disableMatches = matches.Where(match => !match.IsEnableFavorite()).ToList();
            var enableMatches = matches.Except(disableMatches).ToList();

            userSettingService.AddOrUpdateValue(Key, enableMatches.Select(obj => obj as SoccerMatch).ToList());
            Objects = enableMatches;

            AddedFavorites.AddRange(enableMatches.Select(match => new Favorite(match.Id, FavoriteType.MatchValue)).ToList());
            RemovedFavorites.AddRange(disableMatches.Select(match => new Favorite(match.Id, FavoriteType.MatchValue)).ToList());
        }
    }
}