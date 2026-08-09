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

namespace TradeArea.UI.AuctionBids
{
    public class AuctionBidsViewModel : BaseViewModel
    {
        public Observable<LoadingState<List<OwnBid>>> OwnBids => _auctionModel.OnOwnBidsChanged;

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;
        private readonly OrderHubService _orderHubService;

        private CancellationTokenSource _cts = new();

        public AuctionBidsViewModel(UserModel userModel, AuctionModel auctionModel,
            OrderHubService orderHubService)
        {
            _userModel = userModel;
            _auctionModel = auctionModel;
            _orderHubService = orderHubService;
        }

        public async UniTask RemoveBidAsync(Guid bidId)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<AuctionBidsViewModel>("Order hub is null");
                return;
            }

            try
            {
                await _orderHubService.OrderHub.RemoveBid(new RemoveBidDto(bidId))
                    .AsUniTask()
                    .AttachExternalCancellation(_cts.Token);
            }
            catch (Exception ex)
            {
                FLogger.LogWarning<AuctionBidsViewModel>($"Failed to remove bid: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        public async UniTask CollectBidAsync(Guid bidId)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<AuctionBidsViewModel>("Order hub is null");
                return;
            }

            try
            {
                // TODO: take quantity from PopUp
                
                await _orderHubService.OrderHub
                    .CollectBid(new CollectBidDto(bidId,
                        _auctionModel.OnOwnBidsChanged.Value.Value.FirstOrDefault(x => x.Id == bidId).BoughtQuantity))
                    .AsUniTask()
                    .AttachExternalCancellation(_cts.Token);
            }
            catch (Exception ex)
            {
                FLogger.LogWarning<AuctionBidsViewModel>($"Failed to test remove lot: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        public override void Dispose()
        {
            Disposes.ClearCts(ref _cts);
            base.Dispose();
        }
    }
}