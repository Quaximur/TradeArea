using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using R3;
using TradeArea.Shared.Notifications;
using UnityEngine;
using UnityEngine.UIElements;

namespace TradeArea.UI.Notifications
{
    public sealed class NotificationView : AttachableToolkitScreen<NotificationViewModel>
    {
        [SerializeField] private string _notificationContainerName = "NotificationContainer";

        private VisualElement _notificationContainer;

        private DisposableBag _disposables;
        private CancellationTokenSource _lifetimeCts;

        protected override void OnInit()
        {
            base.OnInit();
            _notificationContainer = Root.Q<VisualElement>(_notificationContainerName);
        }

        protected override void OnBind(NotificationViewModel viewModel)
        {
            base.OnBind(viewModel);

            _disposables = new DisposableBag();
            _lifetimeCts = new CancellationTokenSource();
            viewModel.OnNotification
                .Subscribe(AddNotification)
                .AddTo(ref _disposables);
        }

        private void AddNotification(NotificationMessage notification)
        {
            AddNotificationAsync(notification, _lifetimeCts.Token).Forget();
        }

        private async UniTaskVoid AddNotificationAsync(NotificationMessage notification, CancellationToken token)
        {
            await UniTask.SwitchToMainThread();

            if (token.IsCancellationRequested)
                return;

            var row = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            row.AddToClassList("notification__item");
            row.AddToClassList(GetTypeClass(notification.Type));

            var label = new Label(notification.Text)
            {
                pickingMode = PickingMode.Ignore
            };
            label.AddToClassList("notification__text");

            row.Add(label);
            _notificationContainer.Add(row);

            var cancelled = await UniTask.Delay(
                    TimeSpan.FromSeconds(ViewModel.DisplayDurationSeconds),
                    DelayType.UnscaledDeltaTime,
                    PlayerLoopTiming.Update,
                    token)
                .SuppressCancellationThrow();

            if (!cancelled)
                row.RemoveFromHierarchy();
        }

        private static string GetTypeClass(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => "notification__item--success",
                NotificationType.Error => "notification__item--error",
                _ => "notification__item--info"
            };
        }

        public override void Dispose()
        {
            Disposes.ClearCts(ref _lifetimeCts);
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}
