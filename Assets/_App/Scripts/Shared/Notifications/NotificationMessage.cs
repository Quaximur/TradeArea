using System;

namespace TradeArea.Shared.Notifications
{
    public sealed class NotificationMessage
    {
        public Guid Id { get; }
        public string Text { get; }
        public NotificationType Type { get; }

        public NotificationMessage(string text, NotificationType type)
        {
            Id = Guid.NewGuid();
            Text = text;
            Type = type;
        }
    }
}