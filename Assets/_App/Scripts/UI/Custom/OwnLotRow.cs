using UnityEngine.UIElements;

namespace TradeArea.UI.Custom
{
    [UxmlElement]
    public partial class OwnLotRow : ItemConfigRow
    {
        public Button CollectButton => _collectButton;
        public Button RemoveButton => _removeButton;

        // ── Inspector-editable descriptions ───────────────────────────────────

        [UxmlAttribute]
        public string SoldQuantityDescription
        {
            get => _soldQuantityDescLabel.text;
            set => _soldQuantityDescLabel.text = value;
        }

        [UxmlAttribute]
        public string TotalQuantityDescription
        {
            get => _totalQuantityDescLabel.text;
            set => _totalQuantityDescLabel.text = value;
        }

        [UxmlAttribute]
        public string UnitPriceDescription
        {
            get => _unitPriceDescLabel.text;
            set => _unitPriceDescLabel.text = value;
        }

        [UxmlAttribute]
        public string ExtraSoldoutMoneyDescription
        {
            get => _extraSoldoutMoneyDescLabel.text;
            set => _extraSoldoutMoneyDescLabel.text = value;
        }

        [UxmlAttribute]
        public string RevenueDescription
        {
            get => _revenueDescLabel.text;
            set => _revenueDescLabel.text = value;
        }

        // ── Description labels (inspector) ────────────────────────────────────

        private readonly Label _soldQuantityDescLabel;
        private readonly Label _totalQuantityDescLabel;
        private readonly Label _unitPriceDescLabel;
        private readonly Label _extraSoldoutMoneyDescLabel;
        private readonly Label _revenueDescLabel;

        // ── Value labels (runtime) ────────────────────────────────────────────

        private readonly Label _soldQuantityLabel;
        private readonly Label _totalQuantityLabel;
        private readonly Label _unitPriceLabel;
        private readonly Label _extraSoldoutMoneyLabel;
        private readonly Label _revenueLabel;
        private readonly Label _revenueFormulaLabel;

        private readonly Button _collectButton;
        private readonly Button _removeButton;

        // ── Constructor ───────────────────────────────────────────────────────

        public OwnLotRow() : base()
        {
            AddToClassList("own-lot-row");

            // Sold / Total inline row (left block, below name)
            var quantityRow = new VisualElement();
            quantityRow.AddToClassList("own-lot-row__quantity-row");
            _startContainer.Add(quantityRow);

            (_soldQuantityDescLabel, _soldQuantityLabel) =
                CreateInlineCell(quantityRow, "own-lot-row__sold");

            var slash = new Label("/");
            slash.AddToClassList("own-lot-row__slash");
            quantityRow.Add(slash);

            (_totalQuantityDescLabel, _totalQuantityLabel) =
                CreateInlineCell(quantityRow, "own-lot-row__total");

            // Vertical divider
            var divider = new VisualElement();
            divider.AddToClassList("own-lot-row__divider");
            Add(divider);

            // Middle block — fixed width, not flex-grow
            var middleBlock = new VisualElement();
            middleBlock.AddToClassList("own-lot-row__middle");
            Add(middleBlock);

            // Top: Unit Price + Bonus
            var middleTop = new VisualElement();
            middleTop.AddToClassList("own-lot-row__middle-top");
            middleBlock.Add(middleTop);

            (_unitPriceDescLabel, _unitPriceLabel) =
                CreateStatCell(middleTop, "own-lot-row__stat--unit-price");

            (_extraSoldoutMoneyDescLabel, _extraSoldoutMoneyLabel) =
                CreateStatCell(middleTop, "own-lot-row__stat--extra-soldout-money");

            // Bottom: Revenue formula + emphasized value
            var revenueRow = new VisualElement();
            revenueRow.AddToClassList("own-lot-row__revenue-row");
            middleBlock.Add(revenueRow);

            _revenueDescLabel = new Label("Revenue");
            _revenueDescLabel.AddToClassList("own-lot-row__stat-desc");
            revenueRow.Add(_revenueDescLabel);

            _revenueFormulaLabel = new Label();
            _revenueFormulaLabel.AddToClassList("own-lot-row__revenue-formula");
            revenueRow.Add(_revenueFormulaLabel);

            _revenueLabel = new Label();
            _revenueLabel.AddToClassList("own-lot-row__revenue-value");
            revenueRow.Add(_revenueLabel);

            // Spacer — pushes buttons to the right edge
            var spacer = new VisualElement();
            spacer.AddToClassList("own-lot-row__spacer");
            Add(spacer);

            // Buttons
            var buttonGroup = new VisualElement();
            buttonGroup.AddToClassList("own-lot-row__button-group");
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
            _soldQuantityDescLabel.text      = "Sold";
            _totalQuantityDescLabel.text     = "Total";
            _unitPriceDescLabel.text         = "Unit Price";
            _extraSoldoutMoneyDescLabel.text = "Bonus";
            _revenueDescLabel.text           = "Revenue";
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void SetCollectButtonEnabled(bool isEnabled) =>
            _collectButton.SetEnabled(isEnabled);

        public void SetRemoveButtonEnabled(bool isEnabled) =>
            _removeButton.SetEnabled(isEnabled);

        public void SetStats(string soldQuantity, string totalQuantity,
            string unitPrice, string extraSoldoutMoney, string revenue)
        {
            _soldQuantityLabel.text      = soldQuantity;
            _totalQuantityLabel.text     = totalQuantity;
            _unitPriceLabel.text         = unitPrice;
            _extraSoldoutMoneyLabel.text = extraSoldoutMoney;

            _revenueFormulaLabel.text = $"{soldQuantity} * {unitPrice} + {extraSoldoutMoney} =";
            _revenueLabel.text        = $"{revenue}$";
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static (Label descLabel, Label valueLabel) CreateStatCell(
            VisualElement container, string modifierClass)
        {
            var cell = new VisualElement();
            cell.AddToClassList("own-lot-row__stat");
            cell.AddToClassList(modifierClass);
            container.Add(cell);

            var desc = new Label();
            desc.AddToClassList("own-lot-row__stat-desc");
            cell.Add(desc);

            var value = new Label();
            value.AddToClassList("own-lot-row__stat-value");
            cell.Add(value);

            return (desc, value);
        }

        private static (Label descLabel, Label valueLabel) CreateInlineCell(
            VisualElement container, string modifierClass)
        {
            var cell = new VisualElement();
            cell.AddToClassList("own-lot-row__inline-cell");
            cell.AddToClassList(modifierClass);
            container.Add(cell);

            var desc = new Label();
            desc.AddToClassList("own-lot-row__inline-desc");
            cell.Add(desc);

            var value = new Label();
            value.AddToClassList("own-lot-row__inline-value");
            cell.Add(value);

            return (desc, value);
        }
    }
}
