using System;
using System.Collections.Generic;
using System.Threading;
using Aucy.Contracts.OnionClient;
using Aucy.Contracts.OnionServer;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using ObservableCollections;
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
        public Observable<LoadingState<List<ItemStackDto>>> ItemStacks => _itemStacks;

        private readonly UserModel _userModel;

        private readonly ReactiveProperty<LoadingState<List<ItemStackDto>>> _itemStacks = new();

        private CancellationTokenSource _cts = new();

        public AuctionInventoryViewModel(UserModel userModel)
        {
            _userModel = userModel;
        }

        public void RefreshInventory()
        {
            if (_itemStacks.Value.LoadingFlag != LoadingFlag.Loading)
                RefreshInventoryAsync().Forget();

            async UniTask RefreshInventoryAsync()
            {
                try
                {
                    _itemStacks.Value = LoadingState.Loading<List<ItemStackDto>>();

                    using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                    var inventoryLayoutClient = MagicOnionClient.Create<IInventoryLayoutService>(channel)
                        .WithHeaders(GetAuthHeader())
                        .WithCancellationToken(_cts.Token);

                    var inventoryDto = await inventoryLayoutClient.GetOwnInventory(new GetInventoryDto
                    {
                        Page = 1,
                        PageSize = 1000
                    });

                    if (inventoryDto != null && inventoryDto.ItemStacks != null)
                    {
                        var itemsCollection = new List<ItemStackDto>();

                        foreach (var itemStack in inventoryDto.ItemStacks)
                        {
                            if (itemStack != null)
                                itemsCollection.Add(itemStack);
                            else
                                FLogger.LogWarning<AuctionInventoryViewModel>("Null itemStack");
                        }

                        _itemStacks.Value = LoadingState.Success(itemsCollection);
                    }
                    else
                    {
                        FLogger.LogWarning<AuctionInventoryViewModel>("Null inventory");
                        _itemStacks.Value = LoadingState.Success<List<ItemStackDto>>(null);
                    }
                }
                catch (Exception ex)
                {
                    FLogger.LogError<AuctionInventoryViewModel>($"{nameof(RefreshInventoryAsync)}: {ex.Message}");

                    _itemStacks.Value = LoadingState.Fail<List<ItemStackDto>>();
                }
            }
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