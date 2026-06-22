using System;
using System.Threading;
using Aucy.Contracts.OnionClient;
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
using TradeArea.UI.AuctionBuying;
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
        [SerializeField] private AuctionBuyingView _auctionBuyingViewPrefab;

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
            builder.RegisterMVVM<AuctionBuyingView, AuctionBuyingViewModel>(_auctionBuyingViewPrefab, 
                viewModelLifetime: Lifetime.Transient);

            // TODO: take user data from context
            builder.RegisterInstance(new UserModel(new UserState()
            {
                Name = "User 1",
                Token = Constants.UserTokens[Constants.UserIndex1],
                UserId = Constants.UserIds[Constants.UserIndex1],
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

                var inventoryDto = await inventoryClient.CreateInventory(new CreateInventoryDto
                {
                    Capacity = 1000,
                    WeightLimit = 10000
                });

                // TEST
                await inventoryClient.AddItemStack(new AddItemStackDto
                {
                    ItemConfig = new ItemConfigDto(
                    id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
                        name: "bread",
                        isUnary: false,
                        icon: "bread",
                        grade: null,
                        rarity: 0,
                        weight: 0.1f,
                        baseType: "consumable",
                        extra: null,
                        categoryPath: "food"),
                    Quantity = Random.Range(500, 1001)
                });

                await inventoryClient.AddItemStack(new AddItemStackDto
                {
                    ItemConfig = new ItemConfigDto(
                    id: Guid.Parse("00000000-0000-0000-0000-000000000002"),
                        name: "bread",
                        isUnary: false,
                        icon: "bread",
                        grade: null,
                        rarity: 0,
                        weight: 0.1f,
                        baseType: "consumable",
                        extra: null,
                        categoryPath: "food"),
                    Quantity = Random.Range(500, 1001)
                });

                FLogger.LogGood($"Inventory has been created (Capacity: {inventoryDto.Capacity}, WeightLimit: {inventoryDto.WeightLimit}).");

            }
            catch (Exception ex)
            {
                FLogger.LogError($"No Inventory has been created. {ex.Message}");
            }
        }

        protected override void OnDestroy()
        {
            Disposes.ClearCts(ref _cts);
            base.OnDestroy();
        }
    }
}