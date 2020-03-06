using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Akavache;
using Xamarin.Essentials;

namespace LiveScore.Common.Services
{
    public interface IUserSettingService
    {
        void AddOrUpdateValue<T>(string key, T value);

        T GetValueOrDefault<T>(string key, T defaultValue);

        IEnumerable<T> GetAll<T>();
    }

    public class UserSettingService : IUserSettingService
    {
        private const DateTimeKind DEFAULT_DATETIMEKIND = DateTimeKind.Local;

        private readonly IBlobCache UserAccount;

        public UserSettingService(IBlobCache userAccount = null)
        {
            Registrations.Start(AppInfo.Name);
            UserAccount = userAccount ?? BlobCache.UserAccount;
            UserAccount.ForcedDateTimeKind = DEFAULT_DATETIMEKIND;
        }

        public void AddOrUpdateValue<T>(string key, T value)
            => UserAccount.InsertObject(key, value).Wait();

        public T GetValueOrDefault<T>(string key, T defaultValue)
            => UserAccount.GetOrCreateObject(key, () => defaultValue).Wait();

        public IEnumerable<T> GetAll<T>() => UserAccount.GetAllObjects<T>().Wait();
    }
}
