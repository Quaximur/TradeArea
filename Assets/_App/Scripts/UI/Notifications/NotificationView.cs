using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using R3;
using TradeArea.Shared.Notifications;
using UnityEngine.UIElements;

namespace TradeArea.UI.Notifications
{
    public sealed class NotificationView : AttachableToolkitScreen<NotificationViewModel>
    {
        private VisualElement _notificationContainer;
        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            base.OnInit();
            _notificationContainer = Root.Q<VisualElement>("NotificationContainer");
        }

        protected override void OnBind(NotificationViewModel viewModel)
        {
            base.OnBind(viewModel);

            _disposables = new CompositeDisposable();
            viewModel.Notifications
                .Subscribe(RebuildNotifications)
                .AddTo(_disposables);
        }

        private void RebuildNotifications(System.Collections.Generic.IReadOnlyList<NotificationMessage> notifications)
        {
            _notificationContainer.Clear();

            foreach (var notification in notifications)
            {
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
            }
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
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}
