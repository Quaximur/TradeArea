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
using TradeArea.UI.ConfirmPopUp;
using DevKit.UI.MVVM;
using TradeArea.Data.Items;

namespace TradeArea.UI.OrderBook
{
    public class OrderBookViewModel : ScreenViewModel
    {
        public Observable<bool> IsLoading => _isLoading;
        public Observable<string> Error => _error;
        public Observable<IItemConfig> ItemConfig => _itemConfig;
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
        private readonly ReactiveProperty<IItemConfig> _itemConfig = new();

        // Latest order book snapshot from the stream
        private readonly ReactiveProperty<RawItemListingDto> _orderBook = new();

        private readonly ReactiveProperty<int> _sellQuantity = new(1);
        private readonly ReactiveProperty<long> _sellPrice = new();

        private readonly ReactiveProperty<int> _buyQuantity = new(1);
        private readonly ReactiveProperty<long> _buyPrice = new();

        private readonly AuctionModel _auctionModel;
        private readonly OrderHubService _orderHubService;
        private readonly UserModel _userModel;
        private readonly GrpcChannelx _channel;
        private readonly IViewBinder<ConfirmPopUpViewModel> _popUpBinder;

        private CancellationTokenSource _streamCts;
        private IDisposable _confirmPopUpSubscription;

        public OrderBookViewModel(AuctionModel auctionModel,
            UserModel userModel,
            OrderHubService orderHubService,
            GrpcChannelx channel,
            IViewBinder<ConfirmPopUpViewModel> popUpBinder)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
            _orderHubService = orderHubService;
            _channel = channel;
            _popUpBinder = popUpBinder;
        }

        /// <summary>
        /// Call this before showing the window. Cancels any existing stream and
        /// starts a new one for the supplied item.
        /// </summary>
        public void StartItemStreaming(IItemConfig itemConfig)
        {
            _itemConfig.Value = itemConfig;

            _sellQuantity.Value = 1;
            _sellPrice.Value = itemConfig.MaxPrice;

            _buyQuantity.Value = 1;
            _buyPrice.Value = itemConfig.MinPrice;

            StartStreamAsync(itemConfig.Id).Forget();
        }

        public void CreateLot()
        {
            OpenConfirmPipe(
                () => CreateLotAsync(new CreateLotDto(itemConfigId: _itemConfig.Value.Id,
                        unitPrice: _sellPrice.Value,
                        totalQuantity: _sellQuantity.Value)).Forget(),
                title: "sell_item",
                message: $"Wanna sell {_itemConfig.Value.Name} (x{_sellQuantity.Value}) with unit price {_sellPrice.Value} (Total: {_sellQuantity.Value * _sellPrice.Value})?");
        }

        public void CreateBid()
        {
            OpenConfirmPipe(
                () => CreateBidAsync(new CreateBidDto(itemConfigId: _itemConfig.Value.Id,
                        bidPrice: _buyPrice.Value,
                        totalQuantity: _buyQuantity.Value)).Forget(),
                title: "buy_item",
                message: $"Wanna buy {_itemConfig.Value.Name} (x{_buyQuantity.Value}) with unit price {_buyPrice.Value} (Total: {_buyQuantity.Value * _buyPrice.Value})?");
        }

        private async UniTask CreateLotAsync(CreateLotDto createLotDto)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<OrderBookViewModel>("Order hub is null");
                return;
            }

            try
            {
                await _orderHubService.OrderHub.CreateLot(createLotDto);
            }
            catch (Exception ex)
            {
                FLogger.LogError<OrderBookViewModel>($"{nameof(CreateLotAsync)}: {ex.Message}");
            }
        }

        private async UniTask CreateBidAsync(CreateBidDto createBidDto)
        {
            if (_orderHubService.OrderHub == null)
            {
                FLogger.LogWarning<OrderBookViewModel>("Order hub is null");
                return;
            }

            try
            {
                await _orderHubService.OrderHub.CreateBid(createBidDto);
            }
            catch (Exception ex)
            {
                FLogger.LogError<OrderBookViewModel>($"{nameof(CreateBidAsync)}: {ex.Message}");
            }
        }

        public void SetSellPrice(long sellPrice)
        {
            _sellPrice.OnNext(Math.Clamp(sellPrice, _itemConfig.Value.MinPrice, _itemConfig.Value.MaxPrice));
        }

        public void SetSellQuantity(int sellQuantity)
        {
            _sellQuantity.OnNext(Math.Clamp(sellQuantity, 1, _itemConfig.Value.MaxSellQuantity));
        }

        public void SetBuyPrice(long buyPrice)
        {
            _buyPrice.OnNext(Math.Clamp(buyPrice, _itemConfig.Value.MinPrice, _itemConfig.Value.MaxPrice));
        }

        public void SetBuyQuantity(int buyQuantity)
        {
            _buyQuantity.OnNext(Math.Clamp(buyQuantity, 1, _itemConfig.Value.MaxBuyQuantity));
        }

        private async UniTaskVoid StartStreamAsync(Guid itemConfigId)
        {
            // Cancel previous stream
            Disposes.ResetCts(ref _streamCts);
            var cancelToken = _streamCts.Token;

            _isLoading.Value = true;
            _error.Value = null;
            _orderBook.Value = null;

            try
            {
                var itemListingClient = MagicOnionClient.Create<IItemListingService>(_channel)
                    .WithHeaders(GetAuthHeader())
                    .WithCancellationToken(cancelToken);

                var stream = await itemListingClient.GetItemListingStream(itemConfigId);
                _isLoading.Value = false;

                await foreach (var listing in stream.ResponseStream.ReadAllAsync(cancelToken))
                {
                    _orderBook.Value = listing;
                }
            }
            catch (RpcException rpce) when (rpce.StatusCode == StatusCode.Cancelled)
            {
                FLogger.Log<OrderBookViewModel>($"{nameof(StartStreamAsync)}: {rpce.Message}");
            }
            catch (Exception ex)
            {
                FLogger.LogError<OrderBookViewModel>($"{nameof(StartStreamAsync)}: {ex.Message}");
                _isLoading.Value = false;
                _error.Value = ex.Message;
            }
        }

        private void OpenConfirmPipe(
            Action onConfirm,
            string title,
            string message,
            string confirmText = "confirm",
            string cancelText = "cancel")
        {
            var errorPopUpViewModel = _popUpBinder.Open();
            errorPopUpViewModel.Setup(
                title: title,
                message: message,
                confirmText: confirmText,
                cancelText: cancelText
            );

            _confirmPopUpSubscription = errorPopUpViewModel.OnConfirmed.Take(1).Subscribe(_ => onConfirm());
        }

        private Metadata GetAuthHeader()
        {
            return new Metadata { { "authorization", $"Bearer {_userModel.OnTokenChanged.Value}" } };
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _streamCts);
            Disposes.ClearDispose(ref _confirmPopUpSubscription);

            base.Dispose();
        }
    }
}