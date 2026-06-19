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
using TradeArea.Shared;

namespace TradeArea.UI.AuctionInventory
{
    public class AuctionInventoryViewModel : BaseViewModel
    {
        /// <summary>
        /// Inventory items. After full collection loading starts diffs streaming.
        /// </summary>
        public Observable<LoadingState<List<ItemStackDto>>> ItemStacks => _auctionModel.OnInventoryChanged;

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;
        private readonly OrderHubService _orderHubService;

        private CancellationTokenSource _cts = new();

        public AuctionInventoryViewModel(AuctionModel auctionModel, UserModel userModel, 
            OrderHubService orderHubService)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
            _orderHubService = orderHubService;
        }

        public async UniTask TestSell(Guid itemConfigId)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<AuctionInventoryViewModel>("Order hub is null");
                return;
            }

            try
            {
                await _orderHubService.OrderHub.CreateLot(new CreateLotDto(itemConfigId, 2, 5))
                    .AsUniTask()
                    .AttachExternalCancellation(_cts.Token);
            }
            catch (Exception ex)
            {
                FLogger.LogWarning<AuctionInventoryViewModel>($"Failed to test sell: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        public void RefreshInventory()
        {
            // if (_itemStacks.Value.LoadingFlag != LoadingFlag.Loading)
            //     RefreshInventoryAsync().Forget();

            // async UniTask RefreshInventoryAsync()
            // {
            //     try
            //     {
            //         _itemStacks.Value = LoadingState.Loading<List<ItemStackDto>>();

            //         using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
            //         var inventoryLayoutClient = MagicOnionClient.Create<IInventoryLayoutService>(channel)
            //             .WithHeaders(GetAuthHeader())
            //             .WithCancellationToken(_cts.Token);

            //         var inventoryDto = await inventoryLayoutClient.GetOwnInventory(new GetInventoryDto
            //         {
            //             Page = 1,
            //             PageSize = 1000
            //         });

            //         if (inventoryDto != null && inventoryDto.ItemStacks != null)
            //         {
            //             var itemsCollection = new List<ItemStackDto>();

            //             foreach (var itemStack in inventoryDto.ItemStacks)
            //             {
            //                 if (itemStack != null)
            //                     itemsCollection.Add(itemStack);
            //                 else
            //                     FLogger.LogWarning<AuctionInventoryViewModel>("Null itemStack");
            //             }

            //             _itemStacks.Value = LoadingState.Success(itemsCollection);
            //         }
            //         else
            //         {
            //             FLogger.LogWarning<AuctionInventoryViewModel>("Null inventory");
            //             _itemStacks.Value = LoadingState.Success<List<ItemStackDto>>(null);
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         FLogger.LogError<AuctionInventoryViewModel>($"{nameof(RefreshInventoryAsync)}: {ex.Message}");

            //         _itemStacks.Value = LoadingState.Fail<List<ItemStackDto>>();
            //     }
            // }
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