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
using ObservableCollections;
using R3;
using TradeArea.Data;
using TradeArea.Shared;

namespace TradeArea.UI.AuctionInventory
{
    public class AuctionInventoryViewModel : BaseViewModel
    {
        public IObservableCollection<ItemStackDto> ItemStacks => _itemStacks;
        public Observable<LoadingFlag> OnInventoryLoading => _inventoryLoading;

        private readonly UserModel _userModel;

        private readonly ObservableList<ItemStackDto> _itemStacks = new();
        private readonly ReactiveProperty<LoadingFlag> _inventoryLoading = new(LoadingFlag.Loading);

        private CancellationTokenSource _cts = new();

        public AuctionInventoryViewModel(UserModel userModel)
        {
            _userModel = userModel;
        }

        public void RefreshInventory()
        {
            if (_inventoryLoading.Value != LoadingFlag.Loading)
                RefreshInventoryAsync().Forget();

            async UniTask RefreshInventoryAsync()
            {
                try
                {
                    _inventoryLoading.Value = LoadingFlag.Loading;

                    using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                    var inventoryLayoutClient = MagicOnionClient.Create<IInventoryLayoutService>(channel)
                        .WithHeaders(GetAuthHeader())
                        .WithCancellationToken(_cts.Token);

                    var inventoryDto = await inventoryLayoutClient.GetOwnInventory(new GetInventoryDto
                    {
                        Page = 1,
                        PageSize = 1000
                    });

                    _itemStacks.Clear();
                    if (inventoryDto != null && inventoryDto.ItemStacks != null)
                    {
                        foreach (var itemStack in inventoryDto.ItemStacks)
                        {
                            if (itemStack != null)
                                _itemStacks.Add(itemStack);
                            else
                                FLogger.LogWarning<AuctionInventoryViewModel>("Null itemStack");
                        }
                    }
                    else
                    {
                        FLogger.LogWarning<AuctionInventoryViewModel>("Null inventory");
                    }

                    _inventoryLoading.Value = LoadingFlag.Success;
                }
                catch (Exception ex)
                {
                    FLogger.LogError<AuctionInventoryViewModel>($"{nameof(RefreshInventoryAsync)}: {ex.Message}");

                    _inventoryLoading.Value = LoadingFlag.Fail;
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