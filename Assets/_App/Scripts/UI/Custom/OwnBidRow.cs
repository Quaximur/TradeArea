using UnityEngine.UIElements;

namespace TradeArea.UI.Custom
{
    [UxmlElement]
    public partial class OwnBidRow : ItemConfigRow
    {
        public Button CollectButton => _collectButton;
        public Button RemoveButton => _removeButton;

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
        public string RefundDescription
        {
            get => _refundDescLabel.text;
            set => _refundDescLabel.text = value;
        }

        private readonly Label _boughtQuantityDescLabel;
        private readonly Label _totalQuantityDescLabel;
        private readonly Label _bidPriceDescLabel;
        private readonly Label _refundDescLabel;

        private readonly Label _boughtQuantityLabel;
        private readonly Label _totalQuantityLabel;
        private readonly Label _bidPriceLabel;
        private readonly Label _refundFormulaLabel;
        private readonly Label _refundValueLabel;

        private readonly Button _collectButton;
        private readonly Button _removeButton;

        public OwnBidRow() : base()
        {
            AddToClassList("own-bid-row");

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

            var divider = new VisualElement();
            divider.AddToClassList("own-bid-row__divider");
            Add(divider);

            var middleBlock = new VisualElement();
            middleBlock.AddToClassList("own-bid-row__middle");
            Add(middleBlock);

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

            var spacer = new VisualElement();
            spacer.AddToClassList("own-bid-row__spacer");
            Add(spacer);

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

            _boughtQuantityDescLabel.text = "Bought";
            _totalQuantityDescLabel.text = "Orders";
            _bidPriceDescLabel.text = "Bid Price";
            _refundDescLabel.text = "Refund";
        }

        public override void SetStub()
        {
            base.SetStub();

            SetStats(
                boughtQuantity: "0",
                totalQuantity: "0",
                bidPrice: "0",
                refund: "0"
            );

            SetCollectButtonEnabled(false);
            SetRemoveButtonEnabled(false);
        }

        public void SetCollectButtonEnabled(bool isEnabled) =>
            _collectButton.SetEnabled(isEnabled);

        public void SetRemoveButtonEnabled(bool isEnabled) =>
            _removeButton.SetEnabled(isEnabled);

        public void SetStats(string boughtQuantity, string totalQuantity,
            string bidPrice, string refund)
        {
            _boughtQuantityLabel.text = boughtQuantity;
            _totalQuantityLabel.text = totalQuantity;
            _bidPriceLabel.text = bidPrice;

            _refundFormulaLabel.text = $"({totalQuantity} - {boughtQuantity}) * {bidPrice} = ";
            _refundValueLabel.text = refund;
        }


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
