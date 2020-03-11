using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Core;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Favorites;
using LiveScore.Core.Models.Teams;
using LiveScore.Soccer.Models.Teams;

namespace LiveScore.Soccer.Services.Favorites
{
    public class FavoriteTeamService : SoccerFavoriteService<ITeamProfile>
    {
        private const int TeamLimit = 30;

        public FavoriteTeamService(
            IDependencyResolver dependencyResolver,
            SoccerApi.FavoriteApi favoriteApi = null)
                : base(
                    "Teams",
                    TeamLimit,
                    dependencyResolver,
                    favoriteApi)
        {
            Init();

            OnRemovedFavorite = PublishRemovedEvent;
            OnAddedFavorite = PublishAddedEvent;
        }

        private Task PublishRemovedEvent(ITeamProfile team)
        {
            return Task.Run(() => Sync(removedFavorites: new List<Favorite> { new Favorite(team.Id, FavoriteType.TeamValue, team.Name) }));
        }

        private Task PublishAddedEvent(ITeamProfile team)
        {
            return Task.Run(() => Sync(addedFavorites: new List<Favorite>
            {
                new Favorite(team.Id, FavoriteType.TeamValue, team.Name)
            }));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types

        public override void UpdateCache()
        {
            userSettingService.AddOrUpdateValue(Key, Objects.Select(obj => obj as TeamProfile).ToList());
        }

#pragma warning restore S3215 // "interface" instances should not be cast to concrete types

        public override IList<ITeamProfile> LoadCache()
            => userSettingService
                .GetValueOrDefault(Key, Enumerable.Empty<TeamProfile>())
                .Select(team => team as ITeamProfile)
                .ToList();
    }
}