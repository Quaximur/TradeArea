using DevKit.UI.MVVM.Bases;
using TradeArea.UI.AuctionInventory;
using TradeArea.UI.AuctionSearch;
using TradeArea.UI.AuctionOrders;
using TradeArea.UI.AuctionLots;
using TradeArea.UI.AuctionBids;
using TradeArea.Data;
using Cysharp.Threading.Tasks;
using TradeArea.Shared;
using Aucy.Contracts.OnionClient;
using System.Collections.Generic;
using MagicOnion;
using MagicOnion.Client;
using Grpc.Core;
using DevKit.Utils;
using System.Threading;
using System;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        public AuctionSearchViewModel AuctionSearchViewModel { get; private set; }
        public AuctionInventoryViewModel AuctionInventoryViewModel { get; private set; }
        public AuctionOrdersViewModel AuctionOrdersViewModel { get; private set; }
        public AuctionLotsViewModel AuctionLotsViewModel { get; private set; }
        public AuctionBidsViewModel AuctionBidsViewModel { get; private set; }

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;

        private CancellationTokenSource _cts = new();
        private GrpcChannelx _channel = null;

        public AuctionViewModel(AuctionModel auctionModel,
            UserModel userModel,
            AuctionSearchViewModel auctionSearchViewModel,
            AuctionInventoryViewModel auctionInventoryViewModel,
            AuctionOrdersViewModel auctionOrdersViewModel,
            AuctionLotsViewModel auctionLotsViewModel,
            AuctionBidsViewModel auctionBidsViewModel)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;

            AuctionOrdersViewModel = auctionOrdersViewModel;
            AuctionInventoryViewModel = auctionInventoryViewModel;
            AuctionSearchViewModel = auctionSearchViewModel;
            AuctionLotsViewModel = auctionLotsViewModel;
            AuctionBidsViewModel = auctionBidsViewModel;
        }

        public void StartPolling()
        {
            PollAuctionStateAsync().Forget();

            async UniTaskVoid PollAuctionStateAsync()
            {
                _auctionModel.OnInventoryChanged.Value = LoadingState.Loading<List<ItemStackDto>>();
                _auctionModel.OnOwnLotsChanged.Value = LoadingState.Loading<List<OwnLotDto>>();
                _auctionModel.OnOwnBidsChanged.Value = LoadingState.Loading<List<OwnBidDto>>();

                try
                {
                    _channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                    var receiver = new OrderHubReceiver(_auctionModel);
                    var client = await StreamingHubClient.ConnectAsync<IOrderHub, IOrderHubReceiver>(_channel, receiver,
                        cancellationToken: _cts.Token,
                        option: new CallOptions().WithHeaders(GetAuthHeader())).AsUniTask();

                    // TEST: create lot for bread after 5s
                    UniTask.Create(async (_) =>
                    {
                        try
                        {
                            await UniTask.Delay(5000);
                            await client.CreateLot(new CreateLotDto(Guid.Parse("00000000-0000-0000-0000-000000000001"), 3, 1));
                        }
                        catch (Exception ex)
                        {
                            FLogger.LogWarning($"Failed to create test lot. {ex.Message}");
                        }
                    }, cancellationToken: _cts.Token).Forget();
                }
                catch (Exception ex)
                {
                    FLogger.LogError<AuctionSearchViewModel>($"{nameof(PollAuctionStateAsync)}: {ex.Message}");

                    _auctionModel.OnInventoryChanged.Value = LoadingState.Fail<List<ItemStackDto>>();
                    _auctionModel.OnOwnLotsChanged.Value = LoadingState.Fail<List<OwnLotDto>>();
                    _auctionModel.OnOwnBidsChanged.Value = LoadingState.Fail<List<OwnBidDto>>();
                }
            }
        }

        private Metadata GetAuthHeader()
        {
            return new Metadata { { "authorization", $"Bearer {_userModel.OnTokenChanged.Value}" } };
        }

        public override void Dispose()
        {
            AuctionSearchViewModel?.Dispose();
            AuctionInventoryViewModel?.Dispose();
            AuctionOrdersViewModel?.Dispose();

            Disposes.ClearDispose(ref _channel);
            Disposes.ClearDispose(ref _cts);

            base.Dispose();
        }
    }
}
