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
using UnityEngine;

namespace TradeArea.UI.Auction
{
    public class OrderHubReceiver : IOrderHubReceiver
    {
        public void OnBidCollected(BidCollectedDto bidCollected)
        {
            throw new NotImplementedException();
        }

        public void OnBidCreated(BidCreatedDto bidCreated)
        {
            throw new NotImplementedException();
        }

        public void OnBidRemoved(BidRemovedDto bidRemoved)
        {
            throw new NotImplementedException();
        }

        public void OnBought(Guid bidId, int boughtQuantity, long extraMoney)
        {
            FLogger.Log("OnBought");
        }

        public void OnJoin(OwnLotsDto ownLots, OwnBidsDto ownBids, InventoryDto inventory)
        {
            FLogger.LogGood($"Auction pulled: {JsonUtility.ToJson(ownLots)} {JsonUtility.ToJson(ownBids)} {JsonUtility.ToJson(inventory)}");
        }

        public void OnLotCollected(LotCollectedDto lotCollected)
        {
            throw new NotImplementedException();
        }

        public void OnLotCreated(LotCreatedDto lotCreated)
        {
            FLogger.LogGood($"Lot created: {lotCreated.LotId} {lotCreated.ItemConfig.Name}");
        }

        public void OnLotRemoved(LotRemovedDto lotRemoved)
        {
            throw new NotImplementedException();
        }

        public void OnSold(Guid lotId, int soldQuantity, long extraMoney)
        {
            FLogger.Log("OnSold");
        }
    }

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
            GetMarketItemsAsync().Forget();

            async UniTaskVoid GetMarketItemsAsync()
            {
                _auctionModel.OnMarketListChanged.Value = LoadingState.Loading<List<ItemConfigDto>>();
                _auctionModel.OnInventoryChanged.Value = LoadingState.Loading<List<ItemStackDto>>();

                try
                {
                    _channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                    var receiver = new OrderHubReceiver();
                    var client = await StreamingHubClient.ConnectAsync<IOrderHub, IOrderHubReceiver>(_channel, receiver, 
                        cancellationToken: _cts.Token, 
                        option: new CallOptions().WithHeaders(GetAuthHeader())).AsUniTask();

                    // var searchClient = MagicOnionClient.Create<IItemSearchService>(channel)
                    //     .WithHeaders(GetAuthHeader())
                    //     .WithCancellationToken(_cts.Token);

                    // var configs = await searchClient.GetItemConfigs(new GetItemConfigsDto(
                    //     categoryPathFilter: null,
                    //     nameFilter: null,
                    //     rarity: null,
                    //     minGrade: null,
                    //     maxGrade: null,
                    //     page: 1,
                    //     pageSize: 100));

                    // if (configs != null && configs.ItemConfigs != null)
                    //     _auctionModel.OnMarketListChanged.Value = LoadingState.Success(configs.ItemConfigs);
                    // else
                    //     _auctionModel.OnMarketListChanged.Value = LoadingState.Success<List<ItemConfigDto>>(null);
                }
                catch (Exception ex)
                {
                    FLogger.LogError<AuctionSearchViewModel>($"{nameof(GetMarketItemsAsync)}: {ex.Message}");

                    _auctionModel.OnMarketListChanged.Value = LoadingState.Fail<List<ItemConfigDto>>();
                    _auctionModel.OnInventoryChanged.Value = LoadingState.Fail<List<ItemStackDto>>();
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

            base.Dispose();
        }
    }
}
