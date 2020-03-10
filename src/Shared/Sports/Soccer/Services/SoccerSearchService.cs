namespace LiveScore.Soccer.Services
{
    using LiveScore.Core;
    using LiveScore.Core.Enumerations;
    using LiveScore.Core.Services;

    public class SoccerSearchService : SearchService
    {
        public SoccerSearchService(IDependencyResolver dependencyResolver)
            : base(SportType.Soccer.DisplayName, dependencyResolver)
        {
        }
    }
}