using UnityEngine.UIElements;

namespace TradeArea.UI.Custom
{
    [UxmlElement]
    public partial class OwnBidRow : ItemConfigRow
    {
        public Button CollectButton => _collectButton;
        public Button RemoveButton => _removeButton;

        // ── Inspector-editable descriptions ───────────────────────────────────

        [UxmlAttribute]
        public string BoughtQuantityDescription
        {
            get => _boughtQuantityDescLabel.text;
            set => _boughtQuantityDescLabel.text = value;
        }

        [UxmlAttribute]
        public string TotalQuantityDescription
        {
            get => _totalQuantityDescLabel.text;
            set => _totalQuantityDescLabel.text = value;
        }

        [UxmlAttribute]
        public string BidPriceDescription
        {
            get => _bidPriceDescLabel.text;
            set => _bidPriceDescLabel.text = value;
        }

        [UxmlAttribute]
        public string RemainingQuantityDescription
        {
            get => _remainingQuantityDescLabel.text;
            set => _remainingQuantityDescLabel.text = value;
        }

        [UxmlAttribute]
        public string RefundDescription
        {
            get => _refundDescLabel.text;
            set => _refundDescLabel.text = value;
        }

        // ── Description labels (inspector) ────────────────────────────────────

        private readonly Label _boughtQuantityDescLabel;
        private readonly Label _totalQuantityDescLabel;
        private readonly Label _bidPriceDescLabel;
        private readonly Label _remainingQuantityDescLabel;
        private readonly Label _refundDescLabel;

        // ── Value labels (runtime) ────────────────────────────────────────────

        private readonly Label _boughtQuantityLabel;
        private readonly Label _totalQuantityLabel;
        private readonly Label _bidPriceLabel;
        private readonly Label _remainingQuantityLabel;
        private readonly Label _refundFormulaLabel;
        private readonly Label _refundValueLabel;

        private readonly Button _collectButton;
        private readonly Button _removeButton;

        public OwnBidRow() : base()
        {
            AddToClassList("own-bid-row");

            // Left: Bought / Total inline row (injected into base _startContainer)
            var quantityRow = new VisualElement();
            quantityRow.AddToClassList("own-bid-row__quantity-row");
            _startContainer.Add(quantityRow);

            (_boughtQuantityDescLabel, _boughtQuantityLabel) =
                CreateInlineCell(quantityRow, "own-bid-row__bought");

            var slash = new Label("/");
            slash.AddToClassList("own-bid-row__slash");
            quantityRow.Add(slash);

            (_totalQuantityDescLabel, _totalQuantityLabel) =
                CreateInlineCell(quantityRow, "own-bid-row__total");

            // Vertical divider
            var divider = new VisualElement();
            divider.AddToClassList("own-bid-row__divider");
            Add(divider);

            // Middle block
            var middleBlock = new VisualElement();
            middleBlock.AddToClassList("own-bid-row__middle");
            Add(middleBlock);

            // Top: Bid Price + Remaining Qty
            var middleTop = new VisualElement();
            middleTop.AddToClassList("own-bid-row__middle-top");
            middleBlock.Add(middleTop);

            var bidPriceCell = new VisualElement();
            bidPriceCell.AddToClassList("own-bid-row__stat");
            bidPriceCell.AddToClassList("own-bid-row__stat--bid-price");
            middleTop.Add(bidPriceCell);

            _bidPriceDescLabel = new Label();
            _bidPriceDescLabel.AddToClassList("own-bid-row__stat-desc");
            bidPriceCell.Add(_bidPriceDescLabel);

            _bidPriceLabel = new Label();
            _bidPriceLabel.AddToClassList("own-bid-row__stat-value");
            bidPriceCell.Add(_bidPriceLabel);

            var moneyIcon1 = new VisualElement();
            moneyIcon1.AddToClassList("silver-icon");
            bidPriceCell.Add(moneyIcon1);

            (_remainingQuantityDescLabel, _remainingQuantityLabel) =
                CreateStatCell(middleTop, "own-bid-row__stat--remaining-quantity");

            // Bottom: Refund formula + emphasized refund value
            var refundRow = new VisualElement();
            refundRow.AddToClassList("own-bid-row__refund-row");
            middleBlock.Add(refundRow);

            _refundDescLabel = new Label("Refund");
            _refundDescLabel.AddToClassList("own-bid-row__stat-desc");
            refundRow.Add(_refundDescLabel);

            _refundFormulaLabel = new Label();
            _refundFormulaLabel.AddToClassList("own-bid-row__refund-formula");
            refundRow.Add(_refundFormulaLabel);

            _refundValueLabel = new Label();
            _refundValueLabel.AddToClassList("own-bid-row__refund-value");
            refundRow.Add(_refundValueLabel);

            var moneyIcon2 = new VisualElement();
            moneyIcon2.AddToClassList("silver-icon");
            refundRow.Add(moneyIcon2);

            // Spacer — pushes button to the right edge
            var spacer = new VisualElement();
            spacer.AddToClassList("own-bid-row__spacer");
            Add(spacer);

            // Buttons
            var buttonGroup = new VisualElement();
            buttonGroup.AddToClassList("own-bid-row__button-group");
            Add(buttonGroup);

            _collectButton = new Button { text = "Collect" };
            _collectButton.AddToClassList("item-config-row__control-button");
            _collectButton.AddToClassList("item-config-row__collect-button");
            buttonGroup.Add(_collectButton);

            _removeButton = new Button { text = "Remove" };
            _removeButton.AddToClassList("item-config-row__control-button");
            _removeButton.AddToClassList("item-config-row__remove-button");
            buttonGroup.Add(_removeButton);

            // Default descriptions — override in inspector
            _boughtQuantityDescLabel.text = "Bought";
            _totalQuantityDescLabel.text = "Orders";
            _bidPriceDescLabel.text = "Bid Price";
            _remainingQuantityDescLabel.text = "Remaining";
            _refundDescLabel.text = "Refund";
        }

        public void SetCollectButtonEnabled(bool isEnabled) =>
            _collectButton.SetEnabled(isEnabled);

        public void SetRemoveButtonEnabled(bool isEnabled) =>
            _removeButton.SetEnabled(isEnabled);

        /// <param name="boughtQuantity">Units already bought.</param>
        /// <param name="totalQuantity">Total units ordered.</param>
        /// <param name="bidPrice">Price per unit.</param>
        /// <param name="remainingQuantity">Units not yet filled (refundable).</param>
        /// <param name="refund">Computed refund = remainingQuantity * bidPrice.</param>
        public void SetStats(string boughtQuantity, string totalQuantity,
            string bidPrice, string remainingQuantity, string refund)
        {
            _boughtQuantityLabel.text = boughtQuantity;
            _totalQuantityLabel.text = totalQuantity;
            _bidPriceLabel.text = bidPrice;
            _remainingQuantityLabel.text = remainingQuantity;

            _refundFormulaLabel.text = $"{remainingQuantity} * {bidPrice} = "; // TODO: remove it
            _refundValueLabel.text = refund;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static (Label descLabel, Label valueLabel) CreateStatCell(
            VisualElement container, string modifierClass)
        {
            var cell = new VisualElement();
            cell.AddToClassList("own-bid-row__stat");
            cell.AddToClassList(modifierClass);
            container.Add(cell);

            var desc = new Label();
            desc.AddToClassList("own-bid-row__stat-desc");
            cell.Add(desc);

            var value = new Label();
            value.AddToClassList("own-bid-row__stat-value");
            cell.Add(value);

            return (desc, value);
        }

        private static (Label descLabel, Label valueLabel) CreateInlineCell(
            VisualElement container, string modifierClass)
        {
            var cell = new VisualElement();
            cell.AddToClassList("own-bid-row__inline-cell");
            cell.AddToClassList(modifierClass);
            container.Add(cell);

            var desc = new Label();
            desc.AddToClassList("own-bid-row__inline-desc");
            cell.Add(desc);

            var value = new Label();
            value.AddToClassList("own-bid-row__inline-value");
            cell.Add(value);

            return (desc, value);
        }
    }
}
