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
using UnityEngine.InputSystem;

namespace TradeArea.Infrastructure.Installers
{
    public class AuctionInstaller : ModuleInstaller
    {
        [SerializeField] private AuctionView _auctionViewPrefab;
        [SerializeField] private OrderBookView _orderBookViewPrefab;
        [SerializeField] private string _auctionActionName = "GameUI/Auction";
        [SerializeField] private InputActionAsset _inputActionAsset;

        private InputAction _auctionActionRef;
        private IViewBinder<AuctionViewModel> _auctionBinder;
        private bool _isAuctionOpened = false;

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
        }

        protected override async UniTask OnInitialize(CancellationToken token)
        {
            var userModel = Container.Resolve<UserModel>();
            
#if UNITY_EDITOR
            var channel = Container.Resolve<GrpcChannelx>();
            await CreateInventory(channel, userModel.OnTokenChanged.Value);
            // await CreateInventory(_devUserState2.Token);  // TEST 2nd user
#endif

            SubscribeAuctionKey();
        }

        private void SubscribeAuctionKey()
        {
            _auctionBinder = Container.Resolve<IViewBinder<AuctionViewModel>>();
            _inputActionAsset.Enable();
            _auctionActionRef = _inputActionAsset.FindAction(_auctionActionName);

            if (_auctionActionRef != null)
            {
                _auctionActionRef.started += ToggleAuctionView;
                FLogger.Log("Auction action binded");
            }
            else
            {
                FLogger.LogWarning("Auction action not found");
            }
        }

        private void ToggleAuctionView(InputAction.CallbackContext _)
        {
            if (!_isAuctionOpened)
                _auctionBinder.Open();
            else
                _auctionBinder.Close();

            _isAuctionOpened = !_isAuctionOpened;
        }

        private void UnsubscribeAuctionKey()
        {
            if (_auctionActionRef != null)
                _auctionActionRef.started -= ToggleAuctionView;
        }

        private async UniTask CreateInventory(GrpcChannelx channel, string token)
        {
            try
            {
                var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                    .WithHeaders(new Metadata { { "authorization", $"Bearer {token}" } })
                    .WithCancellationToken(_cts.Token);

                // TEST
                var inventoryDto = await inventoryClient.CreateInventory(
                    new CreateInventoryDto(capacity: 1000, weightLimit: 10000)
                );

                // TEST
                for (var i = 0; i < 7; i++)
                {
                    await inventoryClient.AddItemStack(
                        new AddItemStackDto(itemConfigId: Guid.Parse($"00000000-0000-0000-0000-00000000000{Random.Range(1, 10)}"),
                            itemConfig: null,
                            quantity: Random.Range(500, 1001))
                    );
                }

                FLogger.LogGood($"Inventory has been created (Capacity: {inventoryDto.Capacity}, WeightLimit: {inventoryDto.WeightLimit}).");
                await AddMoneyAsync(500_000_000, token);
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
            UnsubscribeAuctionKey();
            Disposes.ClearCts(ref _cts);
            base.OnDestroy();
        }
    }
}