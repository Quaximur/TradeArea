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
using TradeArea.Shared.Notifications;

namespace TradeArea.UI.AuctionSearch
{
    public class AuctionSearchViewModel : BaseViewModel
    {
        public Observable<LoadingState<List<IItemConfig>>> OnMarketListChanged => _onMarketListChanged;

        private readonly UserModel _userModel;
        private readonly IViewBinder<OrderBookViewModel> _orderBookBinder;
        private readonly IItemConfigRepository _itemConfigRepository;
        private readonly INotificationService _notificationService;
        private readonly ReactiveProperty<LoadingState<List<IItemConfig>>> _onMarketListChanged = new();
        private readonly GrpcChannelx _channel;

        private CancellationTokenSource _cts = new();

        public AuctionSearchViewModel(
            UserModel userModel,
            IViewBinder<OrderBookViewModel> orderBookBinder,
            IItemConfigRepository itemConfigRepository,
            INotificationService notificationService,
            GrpcChannelx channel)
        {
            _userModel = userModel;
            _orderBookBinder = orderBookBinder;
            _itemConfigRepository = itemConfigRepository;
            _notificationService = notificationService;
            _channel = channel;
        }

        public void UpdateMarketItems()
        {
            // TODO: request dto and search refreshing.
            // + if currentDto equals newDto then return
            // else Cancel old request and send the new one.

            Disposes.ResetCts(ref _cts);
            GetMarketItemsAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid GetMarketItemsAsync(CancellationToken currentSearchToken)
        {
            while (!currentSearchToken.IsCancellationRequested)
            {
                _onMarketListChanged.Value = LoadingState.Loading<List<IItemConfig>>();

                try
                {
                    var searchClient = MagicOnionClient.Create<IItemSearchService>(_channel)
                        .WithHeaders(GetAuthHeader())
                        .WithCancellationToken(currentSearchToken);

                    var configs = await searchClient.GetItemConfigs(new GetItemConfigsDto(
                        categoryPathFilter: null,
                        nameFilter: null,
                        rarity: null,
                        minGrade: null,
                        maxGrade: null,
                        page: 1,
                        pageSize: 100));

                    // operation was cancelled. Guard from late responses
                    if (currentSearchToken.IsCancellationRequested)
                        return;

                    if (configs != null && configs.ItemConfigs != null)
                    {
                        var itemConfigs = configs.ItemConfigs
                            .Where(x => x != null)
                            .Select(itemConfigDto => _itemConfigRepository.Get(itemConfigDto.Id))
                            .ToList();

                        _onMarketListChanged.Value = LoadingState.Success(itemConfigs);
                    }
                    else
                    {
                        _onMarketListChanged.Value = LoadingState.Success<List<IItemConfig>>(null);
                    }

                    // Success. Exiting
                    return;
                }
                catch (OperationCanceledException)
                {
                    return; // token was cancelled, exit cleanly
                }
                catch (Exception ex)
                {
                    FLogger.LogError<AuctionSearchViewModel>($"{nameof(GetMarketItemsAsync)}: {ex.Message}");
                    
                    // operation was cancelled. Guard from late responses
                    if (currentSearchToken.IsCancellationRequested)
                        return;

                    _notificationService.ShowError(ex);
                    
                    _onMarketListChanged.Value = LoadingState.Fail<List<IItemConfig>>();
                }

                // Wait before retrying (won't delay if token is cancelled)
                if (!currentSearchToken.IsCancellationRequested)
                    await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: currentSearchToken)
                        .SuppressCancellationThrow();
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
