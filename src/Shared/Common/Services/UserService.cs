using System.Diagnostics;
using System.Threading.Tasks;
using Com.OneSignal;
using MethodTimer;

namespace LiveScore.Common.Services
{
    public interface IUserService
    {
        Task<string> GetOrCreateUserIdAsync();
    }

    public class UserService : IUserService
    {
        private const string UserIdKey = "UserId";

        private readonly IUserSettingService userSettingService;

        public UserService(IUserSettingService userSettingService)
        {
            this.userSettingService = userSettingService;
        }

        [Time]
        public Task<string> GetOrCreateUserIdAsync()
        => Task.Run(() => {
                var userId = userSettingService.GetValueOrDefault(UserIdKey, default(string));

                if (userId == default)
                {

                    OneSignal.Current.IdsAvailable((playerID, pushToken) =>
                    {
                        userId = playerID;

                        userSettingService.AddOrUpdateValue(UserIdKey, userId);
                    });


                }

                Debug.WriteLine($"OneSignalUserId {userId}");

                return userId;
            });
        
    }
}