using DevKit.UI.MVVM.Bases;
using R3;
using TradeArea.Shared.Notifications;

namespace TradeArea.UI.Notifications
{
    public sealed class NotificationSettings
    {
        public float DisplayDurationSeconds { get; }

        public NotificationSettings(float displayDurationSeconds)
        {
            DisplayDurationSeconds = displayDurationSeconds;
        }
    }

    public sealed class NotificationViewModel : ScreenViewModel
    {
        public Observable<NotificationMessage> OnNotification => _notificationSignal;
        public float DisplayDurationSeconds => _settings.DisplayDurationSeconds;

        private readonly INotificationService _notificationService;
        private readonly NotificationSettings _settings;
        private readonly Subject<NotificationMessage> _notificationSignal = new();

        public NotificationViewModel(
            INotificationService notificationService,
            NotificationSettings settings)
        {
            _notificationService = notificationService;
            _settings = settings;
            _notificationService.NotificationRequested += OnNotificationRequested;
        }

        private void OnNotificationRequested(NotificationMessage notification)
        {
            _notificationSignal.OnNext(notification);
        }

        public override void Dispose()
        {
            _notificationService.NotificationRequested -= OnNotificationRequested;
            _notificationSignal.Dispose();
            base.Dispose();
        }
    }
}
