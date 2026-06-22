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
        private readonly AuctionModel _auctionModel;
        private readonly OrderHubService _orderHubService;

        // Currently displayed item — set externally before opening the window
        public Observable<ItemConfigDto> ItemConfig => _itemConfig;
        private readonly ReactiveProperty<ItemConfigDto> _itemConfig = new();

        // Latest order book snapshot from the stream
        public Observable<RawItemListingDto> OrderBook => _orderBook;
        private readonly ReactiveProperty<RawItemListingDto> _orderBook = new();

        // True while the stream connection is being established
        public Observable<bool> IsLoading => _isLoading;
        private readonly ReactiveProperty<bool> _isLoading = new(false);

        // Non-null when the stream has faulted
        public Observable<string> Error => _error;
        private readonly ReactiveProperty<string> _error = new();

        private CancellationTokenSource _streamCts;

        private readonly UserModel _userModel;

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
        public void SetItem(ItemConfigDto itemConfig)
        {
            _itemConfig.Value = itemConfig;
            StartStreamAsync(itemConfig.Id).Forget();
        }

        public async UniTask CreateBidAsync(int quantity, long bidPrice)
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
                        bidPrice: bidPrice,
                        totalQuantity: quantity)
                );
            }
            catch (Exception ex)
            {
                FLogger.LogError<OrderBookViewModel>($"{nameof(CreateBidAsync)}: {ex.Message}");
            }
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