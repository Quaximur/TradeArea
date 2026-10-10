using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        public Observable<IReadOnlyList<NotificationMessage>> Notifications => _notifications;

        private readonly INotificationService _notificationService;
        private readonly NotificationSettings _settings;
        private readonly ReactiveProperty<IReadOnlyList<NotificationMessage>> _notifications =
            new(Array.Empty<NotificationMessage>());
        private readonly List<NotificationMessage> _activeNotifications = new();
        private readonly CancellationTokenSource _lifetimeCts = new();

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
            AddNotificationAsync(notification).Forget();
        }

        private async UniTaskVoid AddNotificationAsync(NotificationMessage notification)
        {
            await UniTask.SwitchToMainThread();

            if (_lifetimeCts.IsCancellationRequested)
                return;

            _activeNotifications.Add(notification);
            PublishNotifications();

            RemoveAfterDelayAsync(notification.Id, _lifetimeCts.Token).Forget();
        }

        private async UniTaskVoid RemoveAfterDelayAsync(Guid notificationId, CancellationToken token)
        {
            var cancelled = await UniTask.Delay(
                    TimeSpan.FromSeconds(_settings.DisplayDurationSeconds),
                    DelayType.UnscaledDeltaTime,
                    PlayerLoopTiming.Update,
                    token)
                .SuppressCancellationThrow();

            if (cancelled)
                return;

            _activeNotifications.RemoveAll(x => x.Id == notificationId);
            PublishNotifications();
        }

        private void PublishNotifications()
        {
            _notifications.Value = _activeNotifications.ToArray();
        }

        public override void Dispose()
        {
            _notificationService.NotificationRequested -= OnNotificationRequested;
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _notifications.Dispose();
            base.Dispose();
        }
    }
}
