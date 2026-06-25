using System;
using System.Threading;
using Aucy.Contracts.OnionServer;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using TradeArea.Data;
using TradeArea.Shared;
using TradeArea.UI.Auction;
using TradeArea.UI.AuctionBids;
using TradeArea.UI.OrderBook;
using TradeArea.UI.AuctionInventory;
using TradeArea.UI.AuctionLots;
using TradeArea.UI.AuctionOrders;
using TradeArea.UI.AuctionSearch;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace TradeArea.Infrastructure.Installers
{
    public class AuctionInstaller : ModuleInstaller
    {
        [SerializeField] private AuctionView _auctionViewPrefab;
        [SerializeField] private OrderBookView _orderBookViewPrefab;
        [SerializeField] private UserState _devUserState;

        private CancellationTokenSource _cts = new();

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<AuctionSearchViewModel>(Lifetime.Transient);
            builder.Register<AuctionInventoryViewModel>(Lifetime.Transient);
            builder.Register<AuctionLotsViewModel>(Lifetime.Transient);
            builder.Register<AuctionBidsViewModel>(Lifetime.Transient);
            builder.Register<AuctionOrdersViewModel>(Lifetime.Transient);

            builder.Register<OrderHubService>(Lifetime.Singleton);
            builder.Register<AuctionModel>(Lifetime.Singleton);

            builder.RegisterMVVM<AuctionView, AuctionViewModel>(_auctionViewPrefab, 
                viewModelLifetime: Lifetime.Transient);
            builder.RegisterMVVM<OrderBookView, OrderBookViewModel>(_orderBookViewPrefab, 
                viewModelLifetime: Lifetime.Transient);

            // TODO: take user data from context
            builder.RegisterInstance(new UserModel(new UserState()
            {
                Name = _devUserState.Name,
                Token = _devUserState.Token,
                UserId = _devUserState.UserId,
            }));
        }

        protected override async UniTask OnInitialize(CancellationToken token)
        {
            var userModel = Container.Resolve<UserModel>();

            await CreateInventory(userModel.OnTokenChanged.Value);
            await CreateInventory(Constants.UserTokens[Constants.UserIndex2]);  // TEST 2nd user

            Container.Resolve<IViewBinder<AuctionViewModel>>().Open(); // TODO: opening control
        }

        private async UniTask CreateInventory(string token)
        {
            try
            {
                using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                    .WithHeaders(new Metadata { { "authorization", $"Bearer {token}" } })
                    .WithCancellationToken(_cts.Token);

                // TEST
                var inventoryDto = await inventoryClient.CreateInventory(
                    new CreateInventoryDto(capacity: 1000, weightLimit: 10000)
                );

                // TEST
                await inventoryClient.AddItemStack(
                    new AddItemStackDto(itemConfigId: Guid.Parse("00000000-0000-0000-0000-000000000001"), 
                        itemConfig: null, 
                        quantity: Random.Range(500, 1001))
                );

                await inventoryClient.AddItemStack(
                    new AddItemStackDto(itemConfigId: Guid.Parse("00000000-0000-0000-0000-000000000002"), 
                        itemConfig: null, 
                        quantity: Random.Range(500, 1001))
                );

                FLogger.LogGood($"Inventory has been created (Capacity: {inventoryDto.Capacity}, WeightLimit: {inventoryDto.WeightLimit}).");
                await AddMoneyAsync(10000, token);
            }
            catch (Exception ex)
            {
                FLogger.LogError($"Inventory initializatio error. {ex.Message}");
            }
        }

        public async UniTask AddMoneyAsync(long amount, string token)
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
                    .WithHeaders(new Metadata { { "authorization", $"Bearer {token}" } })
                    .WithCancellationToken(_cts.Token);

                await inventoryClient.AddMoney(amount);
                FLogger.LogGood<AuctionInventoryViewModel>($"Added {amount} funds with token {token}.");
            }
            catch (Exception ex)
            {
                FLogger.LogError($"{nameof(AddMoneyAsync)}: {ex.Message}");
            }
        }

        protected override void OnDestroy()
        {
            Disposes.ClearCts(ref _cts);
            base.OnDestroy();
        }
    }
}