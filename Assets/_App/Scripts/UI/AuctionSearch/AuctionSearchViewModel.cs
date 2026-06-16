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
        public Observable<LoadingState<List<ItemConfigDto>>> OnMarketListChanged => _onMarketListChanged;

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;

        private readonly ReactiveProperty<LoadingState<List<ItemConfigDto>>> _onMarketListChanged = new();

        private CancellationTokenSource _cts = new();

        public AuctionSearchViewModel(AuctionModel auctionModel, UserModel userModel)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
        }
        
        public void UpdateMarketItems()
        {
            if (_onMarketListChanged.Value.LoadingFlag != LoadingFlag.Loading)
                GetMarketItemsAsync().Forget();

            async UniTaskVoid GetMarketItemsAsync()
            {
                _onMarketListChanged.Value = LoadingState.Loading<List<ItemConfigDto>>();

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
                        _onMarketListChanged.Value = LoadingState.Success(configs.ItemConfigs);
                    else
                        _onMarketListChanged.Value = LoadingState.Success<List<ItemConfigDto>>(null);
                }
                catch (Exception ex)
                {
                    FLogger.LogError<AuctionSearchViewModel>($"{nameof(GetMarketItemsAsync)}: {ex.Message}");

                    _onMarketListChanged.Value = LoadingState.Fail<List<ItemConfigDto>>();
                }
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