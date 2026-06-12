using System;
using System.Collections.Generic;
using System.Threading;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using R3;
using TradeArea.Data;
using TradeArea.Shared;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        public Observable<IList<ItemConfigDto>> OnMarketListChanged => _onMarketListChanged;
        public Observable<LoadingFlag> OnMarketLoading => _marketLoading;

        private readonly UserModel _userModel;

        private readonly ReactiveProperty<IList<ItemConfigDto>> _onMarketListChanged = new();
        private readonly ReactiveProperty<LoadingFlag> _marketLoading = new(LoadingFlag.Loading);

        private CancellationTokenSource _cts = new();

        public AuctionViewModel(UserModel userModel)
        {
            _userModel = userModel;
        }

        public void UpdateMarketItems()
        {
            if (_marketLoading.Value != LoadingFlag.Loading)
                GetMarketItemsAsync().Forget();

            async UniTaskVoid GetMarketItemsAsync()
            {
                _marketLoading.Value = LoadingFlag.Loading;

                try
                {
                    using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                    var searchClient = MagicOnionClient.Create<IItemSearchService>(channel)
                        .WithHeaders(GetAuthHeader())
                        .WithCancellationToken(_cts.Token);

                    var configs = await searchClient.GetItemConfigs(new GetItemConfigsDto(
                        categoryPathFilter: null,
                        nameFilter: null,
                        rarity: null,
                        minGrade: null,
                        maxGrade: null,
                        page: 1,
                        pageSize: 100));

                    if (configs != null && configs.ItemConfigs != null)
                        _onMarketListChanged.Value = configs.ItemConfigs;
                    else
                        _onMarketListChanged.Value = Array.Empty<ItemConfigDto>();

                    _marketLoading.Value = LoadingFlag.Success;
                }
                catch (Exception ex)
                {
                    FLogger.LogError<AuctionViewModel>($"{nameof(GetMarketItemsAsync)}: {ex.Message}");

                    _onMarketListChanged.Value = Array.Empty<ItemConfigDto>();

                    _marketLoading.Value = LoadingFlag.Fail;
                }
            }
        }

        public async UniTask<OwnLotsDto> GetOwnLotsAsync()
        {
            using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
            var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                .WithHeaders(GetAuthHeader())
                .WithCancellationToken(_cts.Token);

            return await auctionClient.GetOwnLots();
        }

        public async UniTask CreateLotAsync(Guid itemConfigId, long unitPrice, int quantity)
        {
            if (unitPrice <= 0 || quantity <= 0)
            {
                FLogger.Log<AuctionViewModel>("Unit price and quantity must be greater than zero.");
                return;
            }

            try
            {
                using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                    .WithHeaders(GetAuthHeader())
                    .WithCancellationToken(_cts.Token);

                var lot = await auctionClient.CreateLot(new CreateLotDto(itemConfigId, unitPrice, quantity));
                FLogger.Log<AuctionViewModel>($"{_userModel.OnNameChanged.Value} listed {lot.RemainingQuantity} item(s) at {lot.UnitPrice}.");
            }
            catch (Exception ex)
            {
                FLogger.LogError<AuctionViewModel>($"{nameof(CreateLotAsync)}: {ex.Message}");
            }
        }

        public async UniTask CollectLotAsync(Guid lotId, int quantity)
        {
            if (quantity <= 0)
            {
                FLogger.Log<AuctionViewModel>("Purchase quantity must be greater than zero.");
                return;
            }

            try
            {
                using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                    .WithHeaders(GetAuthHeader())
                    .WithCancellationToken(_cts.Token);

                await auctionClient.CollectLot(new CollectLotDto(lotId, quantity));
                FLogger.Log<AuctionViewModel>($"{_userModel.OnNameChanged.Value} purchased {quantity} item(s).");
            }
            catch (Exception ex)
            {
                FLogger.LogError<AuctionViewModel>($"{nameof(CollectLotAsync)}: {ex.Message}");
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
