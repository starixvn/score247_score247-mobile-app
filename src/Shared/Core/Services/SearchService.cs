namespace LiveScore.Core.Services
{
    using LiveScore.Common.Services;
    using System.Collections.Generic;
    using System.Linq;

    public interface ISearchService<T>
    {
        bool UpdateRecent(T obj);

        IEnumerable<T> GetRecentSearchKeys(int limit);
    }

    public abstract class SearchService<T> : ISearchService<T>
    {
        protected const string RecentSearchCacheKey = "RecentSearch";
        private const int LimitSearchKey = 100;

        protected string Key;
        protected readonly IUserSettingService userSettingService;
        protected IList<T> Objects;


        protected SearchService(
            string key,
            IDependencyResolver dependencyResolver)
        {
            Key = key;
            userSettingService = dependencyResolver.Resolve<IUserSettingService>();
        }

        public void Init()
        {
            Objects = GetRecentSearchKeys(LimitSearchKey).ToList();
        }

        public bool UpdateRecent(T obj)
        {
            if (!Objects.Contains(obj))
            {
                Objects.Remove(obj);
            }

            Objects.Add(obj);

            if (Objects.Count > LimitSearchKey)
            {
                Objects.RemoveAt(0);
            }

            userSettingService.AddOrUpdateValue(RecentSearchCacheKey + Key, Objects);

            return true;
        }

        public abstract IEnumerable<T> GetRecentSearchKeys(int limit);
    }
}