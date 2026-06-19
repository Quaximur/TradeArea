using System;
using System.Collections.Generic;
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

        public void CreateLot()
        {

            // TEST: create lot for bread after 5s
            // UniTask.Create(async (_) =>
            // {
            //     try
            //     {
            //         await UniTask.Delay(5000);
            //         await client.CreateBid(new CreateBidDto(Guid.Parse("00000000-0000-0000-0000-000000000001"), 4, 2));
            //     }
            //     catch (Exception ex)
            //     {
            //         FLogger.LogWarning($"Failed to create test lot. {ex.Message}");
            //     }
            // }, cancellationToken: _cts.Token).Forget();
        }
        // public async UniTask<OwnLotsDto> GetOwnLotsAsync()
        // {
        //     using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
        //     var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
        //         .WithHeaders(GetAuthHeader())
        //         .WithCancellationToken(_cts.Token);

        //     return await auctionClient.GetOwnLots();
        // }

        // public async UniTask CreateLotAsync(Guid itemConfigId, long unitPrice, int quantity)
        // {
        //     if (unitPrice <= 0 || quantity <= 0)
        //     {
        //         FLogger.Log<AuctionLotsViewModel>("Unit price and quantity must be greater than zero.");
        //         return;
        //     }

        //     try
        //     {
        //         using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
        //         var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
        //             .WithHeaders(GetAuthHeader())
        //             .WithCancellationToken(_cts.Token);

        //         var lot = await auctionClient.CreateLot(new CreateLotDto(itemConfigId, unitPrice, quantity));
        //         FLogger.Log<AuctionLotsViewModel>($"{_userModel.OnNameChanged.Value} listed {lot.RemainingQuantity} item(s) at {lot.UnitPrice}.");
        //     }
        //     catch (Exception ex)
        //     {
        //         FLogger.LogError<AuctionLotsViewModel>($"{nameof(CreateLotAsync)}: {ex.Message}");
        //     }
        // }

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