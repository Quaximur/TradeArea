using System;

namespace TradeArea.Shared.Notifications
{
    public interface INotificationService
    {
        public event Action<NotificationMessage> NotificationRequested;

        public void Show(string text, NotificationType type = NotificationType.Info);
        public void ShowError(Exception exception);
    }
}
