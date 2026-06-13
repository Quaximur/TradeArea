using System;
using System.Collections.Generic;
using System.Threading;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using R3;
using TradeArea.Data;
using TradeArea.Shared;

namespace TradeArea.UI.AuctionSearch
{
    public class AuctionSearchViewModel : BaseViewModel
    {
        public Observable<IList<ItemConfigDto>> OnMarketListChanged => _onMarketListChanged;
        public Observable<LoadingFlag> OnMarketLoading => _marketLoading;

        private readonly UserModel _userModel;

        private readonly ReactiveProperty<IList<ItemConfigDto>> _onMarketListChanged = new();
        private readonly ReactiveProperty<LoadingFlag> _marketLoading = new(LoadingFlag.Loading);

        private CancellationTokenSource _cts = new();
        
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
                    FLogger.LogError<AuctionSearchViewModel>($"{nameof(GetMarketItemsAsync)}: {ex.Message}");

                    _onMarketListChanged.Value = Array.Empty<ItemConfigDto>();

                    _marketLoading.Value = LoadingFlag.Fail;
                }
            }
        }

        private Metadata GetAuthHeader()
        {
            return new Metadata { { "authorization", $"Bearer {_userModel.OnTokenChanged.Value}" } };
        }
    }
}