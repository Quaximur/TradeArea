using System.Linq;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using R3;
using UnityEngine.UIElements;

namespace TradeArea.UI.OrderBook
{
    public class OrderBookView : AttachableToolkitWindow<OrderBookViewModel>
    {
        // Header
        private Label _itemNameLabel;
        private Label _itemCategoryLabel;

        // Order book
        private VisualElement _lotsBookContainer;
        private VisualElement _bidsBookContainer;
        private Label _loadingLabel;
        private Label _errorLabel;

        // Buy form
        private IntegerField _buyQuantityField; // TODO: input field
        private LongField _buyPriceField;
        private Label _buyTotalLabel;
        private Button _placeBidButton;

        // Sell form
        private IntegerField _sellQuantityField;
        private LongField _sellPriceField;
        private Label _sellTotalLabel;
        private Button _placeLotButton;

        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            base.OnInit();

            // Header
            _itemNameLabel = Root.Q<Label>("ItemName"); // TODO: serialize names
            _itemCategoryLabel = Root.Q<Label>("ItemCategory");

            // Order book panels
            _lotsBookContainer = Root.Q<VisualElement>("LotsBook");
            _bidsBookContainer = Root.Q<VisualElement>("BidsBook");
            _loadingLabel = Root.Q<Label>("LoadingLabel");
            _errorLabel = Root.Q<Label>("ErrorLabel");

            // Buy form
            _buyQuantityField = Root.Q<IntegerField>("BuyQuantity");
            _buyPriceField = Root.Q<LongField>("BuyPrice");
            _buyTotalLabel = Root.Q<Label>("BuyTotal");
            _placeBidButton = Root.Q<Button>("PlaceBidButton");

            // Sell form
            _sellQuantityField = Root.Q<IntegerField>("SellQuantity");
            _sellPriceField = Root.Q<LongField>("SellPrice");
            _sellTotalLabel = Root.Q<Label>("SellTotal");
            _placeLotButton = Root.Q<Button>("PlaceLotButton");
        }

        protected override void OnBind(OrderBookViewModel viewModel)
        {
            base.OnBind(viewModel);

            _disposables = new();

            // Item header
            viewModel.ItemConfig
                .Where(x => x != null)
                .Subscribe(item =>
                {
                    _itemNameLabel.text = item.Name;
                    _itemCategoryLabel.text = item.CategoryPath;
                })
                .AddTo(_disposables);

            // Loading / error states
            viewModel.IsLoading
                .Subscribe(loading =>
                {
                    _loadingLabel.style.display =
                        loading ? DisplayStyle.Flex : DisplayStyle.None;
                })
                .AddTo(_disposables);

            viewModel.Error
                .Subscribe(err =>
                {
                    var hasError = !string.IsNullOrEmpty(err);
                    _errorLabel.style.display =
                        hasError ? DisplayStyle.Flex : DisplayStyle.None;
                    if (hasError) _errorLabel.text = $"Stream error: {err}";
                })
                .AddTo(_disposables);

            // Order book — rebuild rows whenever a new snapshot arrives
            viewModel.OrderBook
                .Where(x => x != null)
                .Subscribe(RebuildOrderBook)
                .AddTo(_disposables);

            viewModel.OnSellQuantityChanged.Subscribe(x =>
            {
                _sellQuantityField.SetValueWithoutNotify(x);
                RefreshSellTotal();
            }).AddTo(_disposables);
            
            viewModel.OnSellPriceChanged.Subscribe(x =>
            {
                _sellPriceField.SetValueWithoutNotify(x);
                RefreshSellTotal();
            }).AddTo(_disposables);

            viewModel.OnBuyQuantityChanged.Subscribe(x =>
            {
                _buyQuantityField.SetValueWithoutNotify(x);
                RefreshBuyTotal();
            }).AddTo(_disposables);

            viewModel.OnBuyPriceChanged.Subscribe(x =>
            {
                _buyPriceField.SetValueWithoutNotify(x);
                RefreshBuyTotal();
            }).AddTo(_disposables);

            _sellQuantityField.RegisterValueChangedCallback<int>(x => viewModel.SetSellQuantity(x.newValue));
            _sellPriceField.RegisterValueChangedCallback<long>(x => viewModel.SetSellPrice(x.newValue));

            _buyQuantityField.RegisterValueChangedCallback<int>(x => viewModel.SetBuyQuantity(x.newValue));
            _buyPriceField.RegisterValueChangedCallback<long>(x => viewModel.SetBuyPrice(x.newValue));

            // Place Bid button
            _placeBidButton.RegisterCallback<ClickEvent>(_ =>
            {
                viewModel.CreateBidAsync().Forget();
            });

            // Place Lot button
            _placeLotButton.RegisterCallback<ClickEvent>(_ =>
            {
                viewModel.CreateLotAsync().Forget();
            });
        }

        private void RebuildOrderBook(RawItemListingDto listing)
        {
            _lotsBookContainer.Clear();
            _bidsBookContainer.Clear();

            var levels = listing.Levels
                .OrderBy(l => l.MinPrice)
                .ToArray();

            foreach (var level in levels)
            {
                var row = CreateBookRow(
                    minPrice: level.MinPrice,
                   maxPrice: level.MaxPrice,
                    quantity: level.LotQuantity,
                    isSell: true);

                _lotsBookContainer.Add(row);

                var row2 = CreateBookRow(
                   minPrice: level.MinPrice,
                   maxPrice: level.MaxPrice,
                   quantity: level.BidQuantity,
                   isSell: false);

                _bidsBookContainer.Add(row2);
            }
        }

        /// <summary>
        /// Creates a single order book row.
        /// isSell=true → price in red (asks); isSell=false → price in green (bids).
        /// </summary>
        private static VisualElement CreateBookRow(long minPrice, long maxPrice, long quantity, bool isSell)
        {
            var row = new VisualElement();
            row.AddToClassList("order-book__book-row");

            var priceText = minPrice == maxPrice ? minPrice.ToString() : $"{minPrice}-{maxPrice}";

            var priceLabel = new Label(priceText);
            priceLabel.AddToClassList("order-book__book-price");
            priceLabel.AddToClassList(isSell
                ? "order-book__book-price--ask"
                : "order-book__book-price--bid");

            var qtyLabel = new Label(quantity.ToString());
            qtyLabel.AddToClassList("order-book__book-qty");

            row.Add(priceLabel);
            row.Add(qtyLabel);

            return row;
        }

        private void RefreshBuyTotal()
        {
            _buyTotalLabel.text = $"{_buyQuantityField.value * _buyPriceField.value}";
        }

        private void RefreshSellTotal()
        {
            _sellTotalLabel.text = $"{_sellQuantityField.value * _sellPriceField.value}";
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}