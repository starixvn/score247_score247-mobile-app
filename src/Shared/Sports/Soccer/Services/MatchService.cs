using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveScore.Common.Extensions;
using LiveScore.Common.Services;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Matches;
using LiveScore.Core.Services;
using LiveScore.Soccer.Models.Matches;
using MethodTimer;
using static LiveScore.Soccer.Services.SoccerApi;

namespace LiveScore.Soccer.Services
{
    public interface ISoccerMatchService
    {
        Task<MatchInfo> GetMatchAsync(string matchId, Language language);

        Task<MatchCoverage> GetMatchCoverageAsync(string matchId, Language language, bool forceFetchLatestData = false);

        Task<IEnumerable<MatchCommentary>> GetMatchCommentariesAsync(string matchId, Language language);

        Task<MatchStatistic> GetMatchStatisticAsync(string matchId, Language language);

        Task<MatchLineups> GetMatchLineupsAsync(string matchId, Language language);
    }

    public class MatchService : BaseService, IMatchService
    {
        public MatchService()
        {
        }

        [Time]
        public async Task<IEnumerable<IMatch>> GetMatchesByDateAsync(DateTime dateTime, Language language)
        {
            try
            {
                var random = new Random();
                var matches = new List<SoccerMatch>();

                for (var i = 0; i < 200; i++)
                {
                    var match = new SoccerMatch(random.Next(100000).ToString(),
                        DateTime.Now, DateTime.Now, random.Next(100000).ToString(),
                        "Champion League", "1", "Real Madrid", "2", "Barcelona", MatchStatus.FullTime,
                        MatchStatus.FullTime, 2, 2, "1", null, 0, 0, 0, 0, 0, 0, 0, "", 0,
                        EventType.MatchEnded, null, "", "Spain", DateTime.Now, false, 1, "", null, "", 0, "", "Champion League" + random.Next(100000));

                    matches.Add(match);
                }

                return matches;
            }
            catch (Exception ex)
            {
                HandleException(ex);

                return Enumerable.Empty<IMatch>();
            }
        }

        [Time]
        public async Task<MatchInfo> GetMatchAsync(string matchId, Language language)
        {
            return new MatchInfo();
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatchesAsync(Language language)
        {
            return Enumerable.Empty<IMatch>();
        }
    }
}