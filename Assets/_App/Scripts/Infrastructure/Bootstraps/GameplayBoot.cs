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
using R3;
using TradeArea.Data;
using TradeArea.Infrastructure.Contexts;
using TradeArea.Shared;
using TradeArea.UI.Auction;
using TradeArea.UI.AuctionBids;
using TradeArea.UI.AuctionInventory;
using TradeArea.UI.AuctionLots;
using TradeArea.UI.AuctionOrders;
using TradeArea.UI.AuctionSearch;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace TradeArea.Infrastructure.Bootstraps
{
    public class GameplayBoot : Bootstrap
    {
        [SerializeField] private AuctionView _auctionViewPrefab;

        private GameplayEnterContext _gameplayEnterContext;
        private CancellationTokenSource _cts = new();

        protected override void Configure(IContainerBuilder builder)
        {
            //// REGISTRATIONS

            builder.Register<Subject<Unit>>(Lifetime.Singleton)
                .Keyed(Constants.GAMEPLAY_EXIT_SIGNAL);

            builder.Register<Subject<Unit>>(Lifetime.Singleton)
                .Keyed(Constants.GAMEPLAY_RESTART_SIGNAL);
                
            builder.Register<AuctionSearchViewModel>(Lifetime.Transient);
            builder.Register<AuctionInventoryViewModel>(Lifetime.Transient);
            builder.Register<AuctionLotsViewModel>(Lifetime.Transient);
            builder.Register<AuctionBidsViewModel>(Lifetime.Transient);
            builder.Register<AuctionOrdersViewModel>(Lifetime.Transient);
            builder.Register<AuctionModel>(Lifetime.Singleton);

            builder.RegisterMVVM<AuctionView, AuctionViewModel>(_auctionViewPrefab);

            // TODO: take user data from context
            builder.RegisterInstance(new UserModel(new UserState()
            {
                Name = "User 1",
                Token = Constants.UserTokens[Constants.UserIndex1],
                UserId = Constants.UserIds[Constants.UserIndex1],
            }));

            ////
        }

        public async UniTask<Observable<GameplayExitContext>> BootAsync(GameplayEnterContext gameplayEnterContext)
        {
            _gameplayEnterContext = gameplayEnterContext;

            Build();

            //// INITIALIZATION

            var userModel = Container.Resolve<UserModel>();

            await CreateInventory(userModel.OnTokenChanged.Value);
            await CreateInventory(Constants.UserTokens[Constants.UserIndex2]);  // TEST 2nd user

            Container.Resolve<IViewBinder<AuctionViewModel>>().Open();

            ////

            var exitSignal = Container.Resolve<Subject<Unit>>(Constants.GAMEPLAY_EXIT_SIGNAL);
            var gameplayExitSignal = new Subject<GameplayExitContext>();
            var mainMenuEnterContext = new MainMenuEnterContext();

            var gameplayExitContext = new GameplayExitContext(mainMenuEnterContext);

            exitSignal.Take(1)
                .Subscribe(_ =>
                {
                    gameplayExitSignal.OnNext(gameplayExitContext);
                })
                .AddTo(_disposables);

            return await UniTask.FromResult<Observable<GameplayExitContext>>(gameplayExitSignal);
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