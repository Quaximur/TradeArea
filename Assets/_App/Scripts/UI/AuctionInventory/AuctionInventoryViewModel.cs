using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aucy.Contracts.OnionClient;
using Aucy.Contracts.OnionServer;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using R3;
using TradeArea.Data;
using TradeArea.Data.Items;
using TradeArea.Shared;
using TradeArea.UI.OrderBook;

namespace TradeArea.UI.AuctionInventory
{
    public class AuctionInventoryViewModel : BaseViewModel
    {
        /// <summary>
        /// Inventory items. After full collection loading starts diffs streaming.
        /// </summary>
        public Observable<LoadingState<List<ItemStack>>> ItemStacks => _auctionModel.OnInventoryChanged;
        public Observable<LoadingState<long>> OnMoneyChanged => _auctionModel.OnMoneyChanged;

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;
        private readonly OrderHubService _orderHubService;
        private readonly IViewBinder<OrderBookViewModel> _orderBookBinder;

        private CancellationTokenSource _cts = new();

        public AuctionInventoryViewModel(AuctionModel auctionModel, 
            UserModel userModel, 
            OrderHubService orderHubService, 
            IViewBinder<OrderBookViewModel> orderBookBinder)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
            _orderHubService = orderHubService;
            _orderBookBinder = orderBookBinder;
        }

        public void OpenSellingView(IItemConfig itemConfig)
        {
            FLogger.Log<AuctionInventoryViewModel>($"Opening {nameof(OrderBookView)} for itemConfigId: {itemConfig.Id}");
            
            var orderBookViewModel = _orderBookBinder.Open();
            orderBookViewModel.StartItemStreaming(itemConfig);
        }

        public async UniTask AddMoneyAsync(long amount)
        {
            if (amount <= 0)
            {
                FLogger.Log<AuctionInventoryViewModel>("Funds amount must be greater than zero.");
                return;
            }

            try
            {
                using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                    .WithHeaders(GetAuthHeader())
                    .WithCancellationToken(_cts.Token);

                await inventoryClient.AddMoney(amount);
                FLogger.Log<AuctionInventoryViewModel>($"Added {amount} funds to {_userModel.OnNameChanged.Value}.");
            }
            catch (Exception ex)
            {
                FLogger.LogError<AuctionInventoryViewModel>($"{nameof(AddMoneyAsync)}: {ex.Message}");
            }
        }

        public async UniTask RemoveMoneyAsync(long amount)
        {
            if (amount <= 0)
            {
                FLogger.Log<AuctionInventoryViewModel>("Funds amount must be greater than zero.");
                return;
            }

            try
            {
                using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                    .WithHeaders(GetAuthHeader())
                    .WithCancellationToken(_cts.Token);

                await inventoryClient.RemoveMoney(amount);
                FLogger.Log<AuctionInventoryViewModel>($"Removed {amount} money from {_userModel.OnNameChanged.Value}.");
            }
            catch (Exception ex)
            {
                FLogger.LogError<AuctionInventoryViewModel>($"{nameof(RemoveMoneyAsync)}: {ex.Message}");
            }
        }

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