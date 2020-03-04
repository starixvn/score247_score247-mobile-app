using System;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Models.Matches;
using LiveScore.Soccer.Models.Teams;
using MessagePack;

namespace LiveScore.Soccer.Models.Matches
{
    /// <summary>
    /// TODO: God object, why dont we have specified TimelineEvents, eg: Score Events, Card events???
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class TimelineEvent : ITimelineEvent
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public EventType Type { get; set; }

        public DateTimeOffset Time { get; set; }

        public byte MatchTime { get; set; }

        public string StoppageTime { get; set; }

        public string MatchClock { get; set; }

        public string Team { get; set; }

        public byte Period { get; set; }

        public PeriodType PeriodType { get; set; }

        public byte HomeScore { get; set; }

        public byte AwayScore { get; set; }

        public GoalScorer GoalScorer { get; set; }

        public Player Assist { get; set; }

        public Player Player { get; set; }

        public byte InjuryTimeAnnounced { get; set; }

        public Player HomeShootoutPlayer { get; set; }

        public bool IsHomeShootoutScored { get; set; }

        public Player AwayShootoutPlayer { get; set; }

        public bool IsAwayShootoutScored { get; set; }

        public byte ShootoutHomeScore { get; set; }

        public byte ShootoutAwayScore { get; set; }

        public bool IsHome { get; set; }

        public Player PlayerIn { get; set; }

        public Player PlayerOut { get; set; }

        public OutcomeType Outcome { get; set; }

        public DescriptionType Description { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is TimelineEvent actualObj))
            {
                return false;
            }

            return Id == actualObj.Id;
        }

        public override int GetHashCode() => Id?.GetHashCode() ?? 0;
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class GoalScorer
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Method { get; set; }
    }
}