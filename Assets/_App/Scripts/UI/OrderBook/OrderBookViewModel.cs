using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM.Bases;
using TradeArea.Data;
using R3;
using Aucy.Contracts.OnionClient;
using DevKit.Utils;
using MagicOnion;
using TradeArea.Shared;
using MagicOnion.Client;
using Grpc.Core;

namespace TradeArea.UI.OrderBook
{
    public class OrderBookViewModel : ScreenViewModel
    {
        public Observable<bool> IsLoading => _isLoading;
        public Observable<string> Error => _error;
        public Observable<ItemConfigDto> ItemConfig => _itemConfig;
        public Observable<RawItemListingDto> OrderBook => _orderBook;

        public Observable<int> OnSellQuantityChanged => _sellQuantity;
        public Observable<long> OnSellPriceChanged => _sellPrice;

        public Observable<int> OnBuyQuantityChanged => _buyQuantity;
        public Observable<long> OnBuyPriceChanged => _buyPrice;

        // Non-null when the stream has faulted
        private readonly ReactiveProperty<string> _error = new();

        // True while the stream connection is being established
        private readonly ReactiveProperty<bool> _isLoading = new(false);

        // Currently displayed item — set externally before opening the window
        private readonly ReactiveProperty<ItemConfigDto> _itemConfig = new();

        // Latest order book snapshot from the stream
        private readonly ReactiveProperty<RawItemListingDto> _orderBook = new();

        private readonly ReactiveProperty<int> _sellQuantity = new(1);
        private readonly ReactiveProperty<long> _sellPrice = new();

        private readonly ReactiveProperty<int> _buyQuantity = new(1);
        private readonly ReactiveProperty<long> _buyPrice = new();

        private readonly AuctionModel _auctionModel;
        private readonly OrderHubService _orderHubService;
        private readonly UserModel _userModel;

        private CancellationTokenSource _streamCts;

        public OrderBookViewModel(AuctionModel auctionModel,
            UserModel userModel,
            OrderHubService orderHubService)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
            _orderHubService = orderHubService;
        }

        /// <summary>
        /// Call this before showing the window. Cancels any existing stream and
        /// starts a new one for the supplied item.
        /// </summary>
        public void StartItemStreaming(ItemConfigDto itemConfig)
        {
            _itemConfig.Value = itemConfig;

            _sellQuantity.Value = 1;
            _sellPrice.Value = itemConfig.MaxPrice;

            _buyQuantity.Value = 1;
            _buyPrice.Value = itemConfig.MinPrice;

            StartStreamAsync(itemConfig.Id).Forget();
        }

        public async UniTask CreateBidAsync()
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<OrderBookViewModel>("Order hub is null");
                return;
            }

            try
            {
                await _orderHubService.OrderHub.CreateBid(
                    new CreateBidDto(itemConfigId: _itemConfig.Value.Id,
                        bidPrice: _buyPrice.Value,
                        totalQuantity: _buyQuantity.Value)
                );
            }
            catch (Exception ex)
            {
                FLogger.LogError<OrderBookViewModel>($"{nameof(CreateBidAsync)}: {ex.Message}");
            }
        }

        public async UniTask CreateLotAsync()
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<OrderBookViewModel>("Order hub is null");
                return;
            }

            try
            {
                await _orderHubService.OrderHub.CreateLot(
                    new CreateLotDto(itemConfigId: _itemConfig.Value.Id,
                        unitPrice: _sellPrice.Value,
                        totalQuantity: _sellQuantity.Value)
                );
            }
            catch (Exception ex)
            {
                FLogger.LogError<OrderBookViewModel>($"{nameof(CreateLotAsync)}: {ex.Message}");
            }
        }

        public void SetSellPrice(long sellPrice)
        {
            _sellPrice.Value = Math.Clamp(sellPrice, _itemConfig.Value.MinPrice, _itemConfig.Value.MaxPrice);
        }

        public void SetSellQuantity(int sellQuantity)
        {
             _sellQuantity.Value = Math.Clamp(sellQuantity, 1, _itemConfig.Value.MaxSellQuantity);
        }

        public void SetBuyPrice(long buyPrice)
        {
            _buyPrice.Value = Math.Clamp(buyPrice, _itemConfig.Value.MinPrice, _itemConfig.Value.MaxPrice);
        }

        public void SetBuyQuantity(int buyQuantity)
        {
            _buyQuantity.Value = Math.Clamp(buyQuantity, 1, _itemConfig.Value.MaxBuyQuantity);
        }

        private async UniTaskVoid StartStreamAsync(Guid itemConfigId)
        {
            // Cancel previous stream
            _streamCts?.Cancel();
            _streamCts?.Dispose();
            _streamCts = new CancellationTokenSource();
            var ct = _streamCts.Token;

            _isLoading.Value = true;
            _error.Value = null;
            _orderBook.Value = null;

            try
            {
                try
                {
                    using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                    var itemListingClient = MagicOnionClient.Create<IItemListingService>(channel)
                        .WithHeaders(GetAuthHeader())
                        .WithCancellationToken(_streamCts.Token);

                    var stream = await itemListingClient.GetItemListingStream(itemConfigId);
                    _isLoading.Value = false;

                    await foreach (var listing in stream.ResponseStream.ReadAllAsync(ct))
                    {
                        _orderBook.Value = listing;
                    }
                }
                catch (Exception ex)
                {
                    FLogger.LogError<OrderBookViewModel>($"{nameof(StartStreamAsync)}: {ex.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                // Normal on window close or item switch — no error
            }
            catch (Exception ex)
            {
                _isLoading.Value = false;
                _error.Value = ex.Message;
            }
        }

        private Metadata GetAuthHeader()
        {
            return new Metadata { { "authorization", $"Bearer {_userModel.OnTokenChanged.Value}" } };
        }

        public override void Dispose()
        {
            _streamCts?.Cancel();
            _streamCts?.Dispose();
            _itemConfig.Dispose();
            _orderBook.Dispose();
            _isLoading.Dispose();
            _error.Dispose();

            base.Dispose();
        }
    }
}