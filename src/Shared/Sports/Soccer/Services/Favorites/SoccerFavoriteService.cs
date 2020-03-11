using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LiveScore.Common.Services;
using LiveScore.Core;
using LiveScore.Core.Models.Favorites;
using LiveScore.Core.Services;
using MethodTimer;

namespace LiveScore.Soccer.Services.Favorites
{
    public abstract class SoccerFavoriteService<T> : FavoriteService<T>
    {
        private readonly IApiService apiService;
        private readonly SoccerApi.FavoriteApi favoriteApi;
        private readonly IUserService userService;
        private readonly ISettings settings;

        private string userId;
        private string deviceId;

        protected SoccerFavoriteService(
            string key,
            int limitation,
            IDependencyResolver dependencyResolver,
            SoccerApi.FavoriteApi favoriteApi = null)
            : base(key, limitation, dependencyResolver)
        {
            apiService = dependencyResolver.Resolve<IApiService>();
            userService = dependencyResolver.Resolve<IUserService>();
            settings = dependencyResolver.Resolve<ISettings>();
            this.favoriteApi = favoriteApi ?? apiService.GetApi<SoccerApi.FavoriteApi>();
            SyncFunc = SyncToServer;
        }

        [Time]
        private async Task<bool> SyncToServer(IList<Favorite> addedFavorites, IList<Favorite> removedFavorites)
        {
            try
            {
                Debug.WriteLine("SyncToServer");

                deviceId = userService.GetOrCreateDeviceId();
                userId = await GetUserIdAsync();
                var syncUserFavorite = new SyncUserFavorite(
                    new UserFavorite(userId, deviceId, addedFavorites),
                    new UserFavorite(userId, deviceId, removedFavorites));

                return await apiService.Execute(()
                    => favoriteApi.Sync(syncUserFavorite, settings.CurrentLanguage.DisplayName));
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "Method", nameof(SyncToServer)}
                };

                HandleException(ex, properties);

                return false;
            }
        }

        private async Task<string> GetUserIdAsync()
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                var generatedUserId = (await userService.GetOrCreateUserIdAsync());

                if (generatedUserId == null)
                {
                    throw new InvalidOperationException("Cannot generate UserId");
                }

                return generatedUserId;
            }

            return userId;
        }
    }
}