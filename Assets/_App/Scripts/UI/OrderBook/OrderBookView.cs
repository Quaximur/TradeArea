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
        private TextField _buyQuantityField; // TODO: input field
        private TextField _buyPriceField;
        private Label _buyTotalLabel;
        private Button _placeBidButton;

        // Sell form
        private TextField _sellQuantityField;
        private TextField _sellPriceField;
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
            _buyQuantityField = Root.Q<TextField>("BuyQuantity");
            _buyPriceField = Root.Q<TextField>("BuyPrice");
            _buyTotalLabel = Root.Q<Label>("BuyTotal");
            _placeBidButton = Root.Q<Button>("PlaceBidButton");

            // Sell form
            _sellQuantityField = Root.Q<TextField>("SellQuantity");
            _sellPriceField = Root.Q<TextField>("SellPrice");
            _sellTotalLabel = Root.Q<Label>("SellTotal");
            _placeLotButton = Root.Q<Button>("PlaceLotButton");

            // Wire input → total labels before ViewModel is bound
            _buyQuantityField.RegisterValueChangedCallback(_ => RefreshBuyTotal());
            _buyPriceField.RegisterValueChangedCallback(_ => RefreshBuyTotal());
            _sellQuantityField.RegisterValueChangedCallback(_ => RefreshSellTotal());
            _sellPriceField.RegisterValueChangedCallback(_ => RefreshSellTotal());
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

            // Place Bid button
            _placeBidButton.clicked += () =>
            {
                if (!TryParseInt(_buyQuantityField.value, out var qty)) return;
                if (!TryParseLong(_buyPriceField.value, out var price)) return;
                viewModel.CreateBidAsync(qty, price).Forget();
            };

            // Place Lot button
            // _placeLotButton.clicked += () =>
            // {
            //     if (!TryParseInt(_sellQuantityField.value, out var qty)) return;
            //     if (!TryParseLong(_sellPriceField.value, out var price)) return;
            //     viewModel.CreateLotAsync(qty, price).Forget();
            // };
        }

        private void RebuildOrderBook(RawItemListingDto listing)
        {
            _lotsBookContainer.Clear();
            _bidsBookContainer.Clear();

            // Lots: cheapest first (ascending) — buyers scan from top
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
            if (TryParseLong(_buyQuantityField.value, out var qty) &&
                TryParseLong(_buyPriceField.value, out var price))
                _buyTotalLabel.text = $"Total: {qty * price}$";
            else
                _buyTotalLabel.text = "Total: —";
        }

        private void RefreshSellTotal()
        {
            if (TryParseLong(_sellQuantityField.value, out var qty) &&
                TryParseLong(_sellPriceField.value, out var price))
                _sellTotalLabel.text = $"Total: {qty * price}$";
            else
                _sellTotalLabel.text = "Total: —";
        }

        private static bool TryParseLong(string value, out long result) =>
            long.TryParse(value, out result) && result > 0;

        private static bool TryParseInt(string value, out int result) =>
            int.TryParse(value, out result) && result > 0;

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}