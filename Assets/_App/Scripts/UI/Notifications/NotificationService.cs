using System;
using Grpc.Core;
using TradeArea.Shared.Notifications;

namespace TradeArea.UI.Notifications
{
    public sealed class NotificationService : INotificationService
    {
        public event Action<NotificationMessage> NotificationRequested;

        public void Show(string text, NotificationType type = NotificationType.Info)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            NotificationRequested?.Invoke(new NotificationMessage(text, type));
        }

        public void ShowError(Exception exception)
        {
            if (exception == null)
                return;

            var rpcException = FindRpcException(exception);
            var message = rpcException?.Status.Detail;

            if (string.IsNullOrWhiteSpace(message))
                message = exception.Message;

            Show(message, NotificationType.Error);
        }

        private static RpcException FindRpcException(Exception exception)
        {
            while (exception != null)
            {
                if (exception is RpcException rpcException)
                    return rpcException;

                exception = exception.InnerException;
            }

            return null;
        }
    }
}
