using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using R3;
using TradeArea.Data;
using TradeArea.Data.Items;
using TradeArea.Shared;
using TradeArea.Shared.Notifications;

namespace TradeArea.UI.AuctionLots
{
    public class AuctionLotsViewModel : BaseViewModel
    {
        public Observable<LoadingState<List<OwnLot>>> OwnLots => _auctionModel.OnOwnLotsChanged;

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;
        private readonly OrderHubService _orderHubService;
        private readonly INotificationService _notificationService;

        private CancellationTokenSource _cts = new();

        public AuctionLotsViewModel(UserModel userModel, AuctionModel auctionModel,
            OrderHubService orderHubService,
            INotificationService notificationService)
        {
            _userModel = userModel;
            _auctionModel = auctionModel;
            _orderHubService = orderHubService;
            _notificationService = notificationService;
        }

        public async UniTask RemoveLotAsync(Guid lotId)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<AuctionLotsViewModel>("Order hub is null");
                _notificationService.Show("Order service is unavailable.", NotificationType.Error);
                return;
            }

            try
            {
                await _orderHubService.OrderHub.RemoveLot(new RemoveLotDto(lotId))
                    .AsUniTask()
                    .AttachExternalCancellation(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                FLogger.LogWarning<AuctionLotsViewModel>($"Failed to test remove lot: {ex.Message} {ex.InnerException?.Message}");
                _notificationService.ShowError(ex);
            }
        }

        public async UniTask CollectLotAsync(Guid lotId)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<AuctionLotsViewModel>("Order hub is null");
                _notificationService.Show("Order service is unavailable.", NotificationType.Error);
                return;
            }

            try
            {
                // TODO: take quantity from PopUp

                await _orderHubService.OrderHub
                    .CollectLot(new CollectLotDto(lotId,
                        _auctionModel.OnOwnLotsChanged.Value.Value.FirstOrDefault(x => x.Id == lotId).SoldQuantity))
                    .AsUniTask()
                    .AttachExternalCancellation(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                FLogger.LogWarning<AuctionLotsViewModel>($"Failed to test remove lot: {ex.Message} {ex.InnerException?.Message}");
                _notificationService.ShowError(ex);
            }
        }

        public override void Dispose()
        {
            Disposes.ClearCts(ref _cts);
            base.Dispose();
        }
    }
}
