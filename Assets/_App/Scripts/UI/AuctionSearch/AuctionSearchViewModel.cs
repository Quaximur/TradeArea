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
using TradeArea.Repositories;
using TradeArea.Shared;
using TradeArea.UI.OrderBook;
using System.Linq;
using TradeArea.Data.Items;

namespace TradeArea.UI.AuctionSearch
{
    public class AuctionSearchViewModel : BaseViewModel
    {
        public Observable<LoadingState<List<IItemConfig>>> OnMarketListChanged => _onMarketListChanged;

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;
        private readonly IViewBinder<OrderBookViewModel> _orderBookBinder;
        private readonly IItemConfigRepository _itemConfigRepository;
        private readonly ReactiveProperty<LoadingState<List<IItemConfig>>> _onMarketListChanged = new();

        private CancellationTokenSource _cts = new();

        public AuctionSearchViewModel(AuctionModel auctionModel,
            UserModel userModel,
            IViewBinder<OrderBookViewModel> orderBookBinder,
            IItemConfigRepository itemConfigRepository)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
            _orderBookBinder = orderBookBinder;
            _itemConfigRepository = itemConfigRepository;
        }

        public void UpdateMarketItems()
        {
            if (_onMarketListChanged.Value.LoadingFlag != LoadingFlag.Loading)
                GetMarketItemsAsync().Forget();

            async UniTaskVoid GetMarketItemsAsync()
            {
                _onMarketListChanged.Value = LoadingState.Loading<List<IItemConfig>>();

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
                    {
                        var itemConfigs = configs.ItemConfigs.Select(itemConfigDto => _itemConfigRepository.Get(itemConfigDto.Id.ToString()))
                            .Where(x => x != null)
                            .ToList();

                        _onMarketListChanged.Value = LoadingState.Success(itemConfigs);
                    }
                    else
                    {
                        _onMarketListChanged.Value = LoadingState.Success<List<IItemConfig>>(null);
                    }
                }
                catch (Exception ex)
                {
                    FLogger.LogError<AuctionSearchViewModel>($"{nameof(GetMarketItemsAsync)}: {ex.Message}");

                    _onMarketListChanged.Value = LoadingState.Fail<List<IItemConfig>>();
                }
            }
        }

        public void OpenBuyingView(IItemConfig itemConfig)
        {
            FLogger.Log<AuctionSearchViewModel>($"Opening {nameof(OrderBookView)} for itemConfigId: {itemConfig.Id}");

            var orderBookViewModel = _orderBookBinder.Open();
            orderBookViewModel.StartItemStreaming(itemConfig);
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