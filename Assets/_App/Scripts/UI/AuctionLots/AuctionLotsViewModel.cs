using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using Grpc.Core;
using R3;
using TradeArea.Data;
using TradeArea.Shared;

namespace TradeArea.UI.AuctionLots
{
    public class AuctionLotsViewModel : BaseViewModel
    {
        public Observable<LoadingState<List<OwnLotDto>>> OwnLots => _auctionModel.OnOwnLotsChanged;

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;
        private readonly OrderHubService _orderHubService;

        private CancellationTokenSource _cts = new();

        public AuctionLotsViewModel(UserModel userModel, AuctionModel auctionModel,
            OrderHubService orderHubService)
        {
            _userModel = userModel;
            _auctionModel = auctionModel;
            _orderHubService = orderHubService;
        }

        public async UniTask TestRemoveLot(Guid lotId)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<AuctionLotsViewModel>("Order hub is null");
                return;
            }

            try
            {
                await _orderHubService.OrderHub.RemoveLot(new RemoveLotDto(lotId))
                    .AsUniTask()
                    .AttachExternalCancellation(_cts.Token);
            }
            catch (Exception ex)
            {
                FLogger.LogWarning<AuctionLotsViewModel>($"Failed to test remove lot: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        public async UniTask CollectLot(Guid lotId)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<AuctionLotsViewModel>("Order hub is null");
                return;
            }

            try
            {
                await _orderHubService.OrderHub
                    .CollectLot(new CollectLotDto(lotId,
                        _auctionModel.OnOwnLotsChanged.Value.Value.FirstOrDefault(x => x.Id == lotId).SoldQuantity))
                    .AsUniTask()
                    .AttachExternalCancellation(_cts.Token);
            }
            catch (Exception ex)
            {
                FLogger.LogWarning<AuctionLotsViewModel>($"Failed to test remove lot: {ex.Message} {ex.InnerException?.Message}");
            }
        }


        // public async UniTask CollectLotAsync(Guid lotId, int quantity)
        // {
        //     if (quantity <= 0)
        //     {
        //         FLogger.Log<AuctionLotsViewModel>("Purchase quantity must be greater than zero.");
        //         return;
        //     }

        //     try
        //     {
        //         using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
        //         var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
        //             .WithHeaders(GetAuthHeader())
        //             .WithCancellationToken(_cts.Token);

        //         await auctionClient.CollectLot(new CollectLotDto(lotId, quantity));
        //         FLogger.Log<AuctionLotsViewModel>($"{_userModel.OnNameChanged.Value} purchased {quantity} item(s).");
        //     }
        //     catch (Exception ex)
        //     {
        //         FLogger.LogError<AuctionLotsViewModel>($"{nameof(CollectLotAsync)}: {ex.Message}");
        //     }
        // }

        private Metadata GetAuthHeader()
        {
            return new Metadata { { "authorization", $"Bearer {_userModel.OnTokenChanged.Value}" } };
        }

        public override void Dispose()
        {
            Disposes.ClearCts(ref _cts);
            base.Dispose();
        }
    }
}