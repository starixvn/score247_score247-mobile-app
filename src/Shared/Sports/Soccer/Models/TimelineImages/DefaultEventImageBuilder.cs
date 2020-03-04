using System.Collections.Generic;
using LiveScore.Core.Enumerations;

namespace LiveScore.Soccer.Models.TimelineImages
{
    public class DefaultEventImageBuilder : ITimelineEventImageBuilder
    {
        private static readonly IDictionary<EventType, string> EventImages = new Dictionary<EventType, string>
        {
            { EventType.YellowCard, Enumerations.Images.YellowCard.Value },
            { EventType.YellowRedCard, Enumerations.Images.RedYellowCard.Value },
            { EventType.RedCard, Enumerations.Images.RedCard.Value },
            { EventType.PenaltyMissed, Enumerations.Images.MissPenaltyGoal.Value },
            { EventType.Substitution, Enumerations.Images.Substitution.Value },
            { EventType.ScoreChangeByOwnGoal, Enumerations.Images.ScoreChangeByOwnGoal.Value },
            { EventType.SubstitutionIn, Enumerations.Images.SubstitutionIn.Value },
            { EventType.SubstitutionOut, Enumerations.Images.SubstitutionOut.Value },
            { EventType.VideoAssistantRefereeOver, Enumerations.Images.SubstitutionOut.Value }
        };

        public string BuildImageSource(TimelineEventImage timelineEvent)
            => EventImages.ContainsKey(timelineEvent.Type) ? EventImages[timelineEvent.Type] : string.Empty;
    }
}