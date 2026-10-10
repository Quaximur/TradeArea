using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using TradeArea.Shared.Notifications;
using TradeArea.UI.Notifications;
using UnityEngine;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    public sealed class NotificationInstaller : ModuleInstaller
    {
        [SerializeField] private NotificationView _notificationViewPrefab;
        [SerializeField, Min(0.1f)] private float _displayDurationSeconds = 5f;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<NotificationService>(Lifetime.Singleton)
                .As<INotificationService>();
            builder.RegisterInstance(new NotificationSettings(_displayDurationSeconds));
            builder.RegisterMVVM<NotificationView, NotificationViewModel>(_notificationViewPrefab);
        }

        protected override UniTask OnInitialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Container.Resolve<IViewBinder<NotificationViewModel>>().Open();
            return UniTask.CompletedTask;
        }
    }
}
