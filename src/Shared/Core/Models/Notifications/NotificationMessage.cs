using System;
using LiveScore.Core.Enumerations;

namespace LiveScore.Core.Models.Notifications
{
    public class NotificationMessage
    {
        public const string NotificationMessageCachedKey = "NotificationMessage";

        public NotificationMessage(
            string sportId,
            string id,
            string type,
            string eventDate,
            string title = null,
            string message = null)
        {
            SportType = Enumeration.FromValue<SportType>(Convert.ToByte(sportId));
            Id = id;
            Type = Enumeration.FromValue<NotificationType>(Convert.ToByte(type));
            EventDate = DateTimeOffset.Parse(eventDate);
            Title = title;
            Message = message;
        }

        public SportType SportType { get; }

        public string Id { get; }

        public NotificationType Type { get; }

        public string Title { get; }

        public string Message { get; }

        public DateTimeOffset EventDate { get; }
    }
}