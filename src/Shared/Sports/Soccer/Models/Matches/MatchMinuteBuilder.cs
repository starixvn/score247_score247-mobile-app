using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiveScore.Common.Services;
using LiveScore.Core;
using LiveScore.Core.Converters;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Matches;

namespace LiveScore.Soccer.Models.Matches
{
    // TODO: Unit test will be written in Performance Enhancement branch
    public class MatchMinuteBuilder : IMatchMinuteBuilder
    {
        private static readonly ReadOnlyDictionary<MatchStatus, Tuple<byte, byte>> PeriodTimes
          = new ReadOnlyDictionary<MatchStatus, Tuple<byte, byte>>(
              new Dictionary<MatchStatus, Tuple<byte, byte>>
              {
                  [MatchStatus.FirstHalf] = new Tuple<byte, byte>(1, 45),
                  [MatchStatus.SecondHalf] = new Tuple<byte, byte>(46, 90),
                  [MatchStatus.FirstHalfExtra] = new Tuple<byte, byte>(91, 105),
                  [MatchStatus.SecondHalfExtra] = new Tuple<byte, byte>(106, 120)
              });

        private readonly ISettings settings;

        public MatchMinuteBuilder(ISettings settings)
        {
            this.settings = settings;
        }

        // TODO: Remove casting
        public string BuildMatchMinute(IMatch match)
            => BuildMatchMinute(match as SoccerMatch, DateTimeOffset.UtcNow);

        internal string BuildMatchMinute(SoccerMatch match, DateTimeOffset timeToViewMatch)
        {
            try
            {
                if (match == null || !PeriodTimes.ContainsKey(match.MatchStatus))
                {
                    return string.Empty;
                }

                var (periodStartMinute, periodEndMinute) = PeriodTimes[match.MatchStatus];

                // TODO: What if CurrentPeriodStartTime does not have data?
                var matchMinute = (int)(periodStartMinute + (timeToViewMatch - match.CurrentPeriodStartTime).TotalMinutes);

                // TODO: check InjuryTimeAnnounced > 0 for each period
                if ((match.LastTimelineType?.IsInjuryTimeShown == true) || GetAnnouncedInjuryTime(match) > 0)
                {
                    return BuildMinuteWithInjuryTime(matchMinute, periodEndMinute, match);
                }

                if (matchMinute >= periodEndMinute)
                {
                    matchMinute = periodEndMinute;
                }

                if (matchMinute < periodStartMinute)
                {
                    matchMinute = periodStartMinute;
                }

                return $"{matchMinute}'";
            }
            catch (Exception ex)
            {
                /// TODO: temporarily add custom message to log lib

                return string.Empty;
            }
        }

        private string BuildMinuteWithInjuryTime(int matchMinute, int periodEndMinute, SoccerMatch soccerMatch)
        {
            var annoucedInjuryTime = GetAnnouncedInjuryTime(soccerMatch);

            if (soccerMatch.InjuryTimeAnnounced > 0)
            {
                UpdateAnnouncedInjuryTime(soccerMatch);
                annoucedInjuryTime = soccerMatch.InjuryTimeAnnounced;
            }

            var currentInjuryTime = matchMinute - periodEndMinute;
            var displayInjuryTime = currentInjuryTime <= 0 ? 1 : currentInjuryTime;

            if (currentInjuryTime > annoucedInjuryTime)
            {
                displayInjuryTime = annoucedInjuryTime;
            }

            return $"{periodEndMinute}+{displayInjuryTime}'";
        }

        private int GetAnnouncedInjuryTime(SoccerMatch soccerMatch)
        {
            // TODO: Should move InjuryTimeAnnouced to backend for storing?
            var cachedInjuryTime = settings.Get(GetCacheKey(soccerMatch));

            return string.IsNullOrWhiteSpace(cachedInjuryTime) ? 0 : int.Parse(cachedInjuryTime);
        }

        public void UpdateAnnouncedInjuryTime(SoccerMatch soccerMatch)
            => settings.Set(GetCacheKey(soccerMatch), soccerMatch.InjuryTimeAnnounced.ToString());

        private static string GetCacheKey(SoccerMatch soccerMatch)
            => $"InjuryTimeAnnouced_{soccerMatch.Id}_{soccerMatch.MatchStatus.DisplayName}";
    }
}