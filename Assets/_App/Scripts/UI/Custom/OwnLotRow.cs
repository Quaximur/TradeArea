using UnityEngine.UIElements;

namespace TradeArea.UI.Custom
{
    [UxmlElement]
    public partial class OwnLotRow : ItemConfigRow
    {
        public Button CollectButton => _collectButton;
        public Button RemoveButton => _removeButton;

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

        private readonly Label _soldQuantityDescLabel;
        private readonly Label _totalQuantityDescLabel;
        private readonly Label _unitPriceDescLabel;
        private readonly Label _extraSoldoutMoneyDescLabel;
        private readonly Label _revenueDescLabel;

        private readonly Label _soldQuantityLabel;
        private readonly Label _totalQuantityLabel;
        private readonly Label _unitPriceLabel;
        private readonly Label _extraSoldoutMoneyLabel;
        private readonly Label _revenueLabel;
        private readonly Label _revenueFormulaLabel;

        private readonly Button _collectButton;
        private readonly Button _removeButton;

        public OwnLotRow() : base()
        {
            AddToClassList("own-lot-row");

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

            var divider = new VisualElement();
            divider.AddToClassList("own-lot-row__divider");
            _container.Add(divider);

            var middleBlock = new VisualElement();
            middleBlock.AddToClassList("own-lot-row__middle");
            _container.Add(middleBlock);

            var middleTop = new VisualElement();
            middleTop.AddToClassList("own-lot-row__middle-top");
            middleBlock.Add(middleTop);

            var unitPriceCell = new VisualElement();
            unitPriceCell.AddToClassList("own-lot-row__stat");
            unitPriceCell.AddToClassList("own-lot-row__stat--unit-price");
            middleTop.Add(unitPriceCell);

            _unitPriceDescLabel = new Label();
            _unitPriceDescLabel.AddToClassList("own-lot-row__stat-desc");
            unitPriceCell.Add(_unitPriceDescLabel);

            _unitPriceLabel = new Label();
            _unitPriceLabel.AddToClassList("own-lot-row__stat-value");
            unitPriceCell.Add(_unitPriceLabel);

            var moneyIcon1 = new VisualElement();
            moneyIcon1.AddToClassList("silver-icon");
            unitPriceCell.Add(moneyIcon1);

            var extraMoneyCell = new VisualElement();
            extraMoneyCell.AddToClassList("own-lot-row__stat");
            extraMoneyCell.AddToClassList("own-lot-row__stat--extra-soldout-money");
            middleTop.Add(extraMoneyCell);

            _extraSoldoutMoneyDescLabel = new Label();
            _extraSoldoutMoneyDescLabel.AddToClassList("own-lot-row__stat-desc");
            extraMoneyCell.Add(_extraSoldoutMoneyDescLabel);

            _extraSoldoutMoneyLabel = new Label();
            _extraSoldoutMoneyLabel.AddToClassList("own-lot-row__stat-value");
            extraMoneyCell.Add(_extraSoldoutMoneyLabel);

            var moneyIcon2 = new VisualElement();
            moneyIcon2.AddToClassList("silver-icon");
            extraMoneyCell.Add(moneyIcon2);

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

            var moneyIcon3 = new VisualElement();
            moneyIcon3.AddToClassList("silver-icon");
            revenueRow.Add(moneyIcon3);

            var spacer = new VisualElement();
            spacer.AddToClassList("own-lot-row__spacer");
            _container.Add(spacer);

            var buttonGroup = new VisualElement();
            buttonGroup.AddToClassList("own-lot-row__button-group");
            _container.Add(buttonGroup);

            _collectButton = new Button { text = "Collect" };
            _collectButton.AddToClassList("item-config-row__control-button");
            _collectButton.AddToClassList("item-config-row__collect-button");
            buttonGroup.Add(_collectButton);

            _removeButton = new Button { text = "Remove" };
            _removeButton.AddToClassList("item-config-row__control-button");
            _removeButton.AddToClassList("item-config-row__remove-button");
            buttonGroup.Add(_removeButton);

            _soldQuantityDescLabel.text = "Sold";
            _totalQuantityDescLabel.text = "Orders";
            _unitPriceDescLabel.text = "Unit Price";
            _extraSoldoutMoneyDescLabel.text = "Bonus";
            _revenueDescLabel.text = "Revenue";
        }

        public override void SetStub()
        {
            base.SetStub();

            SetStats(
                soldQuantity: "0",
                totalQuantity: "0",
                unitPrice: "0",
                extraSoldoutMoney: "0",
                revenue: "0"
            );

            SetCollectButtonEnabled(false);
            SetRemoveButtonEnabled(false);
        }

        public void SetCollectButtonEnabled(bool isEnabled) =>
            _collectButton.SetEnabled(isEnabled);

        public void SetRemoveButtonEnabled(bool isEnabled) =>
            _removeButton.SetEnabled(isEnabled);

        public void SetStats(string soldQuantity, string totalQuantity,
            string unitPrice, string extraSoldoutMoney, string revenue)
        {
            _soldQuantityLabel.text = soldQuantity;
            _totalQuantityLabel.text = totalQuantity;
            _unitPriceLabel.text = unitPrice;
            _extraSoldoutMoneyLabel.text = extraSoldoutMoney;

            _revenueFormulaLabel.text = $"{soldQuantity} * {unitPrice} + {extraSoldoutMoney} = ";
            _revenueLabel.text = revenue;
        }

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
