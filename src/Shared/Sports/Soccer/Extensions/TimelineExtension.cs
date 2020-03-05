using System.Linq;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Matches;
using LiveScore.Soccer.Models.Matches;

namespace LiveScore.Soccer.Extensions
{
    public static class TimelineExtension
    {
        private static readonly EventType[] DetailInfoEventTypes = {
            EventType.ScoreChange,
            EventType.PenaltyMissed,
            EventType.YellowCard,
            EventType.RedCard,
            EventType.YellowRedCard,
            EventType.PenaltyShootout,
            EventType.BreakStart,
            EventType.MatchEnded,
            EventType.Substitution,
            EventType.VarOver
        };

        public static readonly EventType[] HighlightSoccerEventTypes = {
                EventType.ScoreChange,
                EventType.PenaltyMissed,
                EventType.YellowCard,
                EventType.RedCard,
                EventType.YellowRedCard,
                EventType.Substitution,
                EventType.VarOver
            };

        public static bool IsHalfTimeBreak(this TimelineEvent timeline)
            => timeline?.Type == EventType.BreakStart && timeline?.PeriodType == PeriodType.Pause;

        public static bool IsAwaitingExtraTimeBreak(this TimelineEvent timeline)
            => timeline?.Type == EventType.BreakStart && timeline?.PeriodType == PeriodType.AwaitingExtraTime;

        public static bool IsAwaitingPenaltiesBreak(this TimelineEvent timeline)
            => timeline?.Type == EventType.BreakStart && timeline?.PeriodType == PeriodType.AwaitingPenalties;

        public static bool IsNotExtraTimeHalfTimeBreak(this TimelineEvent timeline)
            => timeline?.Type == EventType.BreakStart && timeline?.PeriodType != PeriodType.ExtraTimeHalfTime;

        public static bool IsExtraTimeHalfTimeBreak(this TimelineEvent timeline)
            => timeline?.Type == EventType.BreakStart && timeline?.PeriodType == PeriodType.ExtraTimeHalfTime;

        public static bool IsMatchEndedFullTime(this TimelineEvent timeline, IMatch match)
            => timeline?.Type == EventType.MatchEnded && match?.MatchStatus?.IsEnded == true;

        public static bool IsMatchEndedExtraTime(this TimelineEvent timeline, IMatch match)
           => timeline?.Type == EventType.MatchEnded && match?.MatchStatus?.IsAfterExtraTime == true;

        public static bool IsMatchEndAfterPenalty(this TimelineEvent timeline, IMatch match)
           => timeline?.Type == EventType.MatchEnded && match?.MatchStatus?.IsAfterPenalties == true;

        public static bool OfHomeTeam(this TimelineEvent timeline)
            => timeline?.Team == "home";

        public static bool IsDetailInfoEvent(this TimelineEvent timeline)
            => DetailInfoEventTypes.Contains(timeline.Type);

        public static bool IsHighlightEvent(this EventType type)
            => HighlightSoccerEventTypes.Contains(type);
    }
}