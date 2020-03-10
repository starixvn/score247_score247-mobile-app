using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Com.OneSignal;
using MethodTimer;

namespace LiveScore.Common.Services
{
    public interface IUserService
    {
        Task<string> GetOrCreateUserIdAsync();

        string GetOrCreateDeviceId();
    }

    public class UserService : IUserService
    {
        private const string UserIdKey = "UserId";
        private const string DeviceIdKey = "DeviceId";

        private readonly IUserSettingService userSettingService;

        public UserService(IUserSettingService userSettingService)
        {
            this.userSettingService = userSettingService;
        }

        public string GetOrCreateDeviceId()
        {
            var deviceId = userSettingService.GetValueOrDefault(DeviceIdKey, default(string));

            if (deviceId == default)
            {
                deviceId = Guid.NewGuid().ToString();

                userSettingService.AddOrUpdateValue(DeviceIdKey, deviceId);
            }

            return deviceId;
        }

        [Time]
        public Task<string> GetOrCreateUserIdAsync()
        => Task.Run(() =>
        {
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