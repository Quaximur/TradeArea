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

        // Description labels (set via inspector)
        private readonly Label _soldQuantityDescLabel;
        private readonly Label _totalQuantityDescLabel;
        private readonly Label _unitPriceDescLabel;
        private readonly Label _extraSoldoutMoneyDescLabel;
        private readonly Label _revenueDescLabel;

        // Value labels (set at runtime)
        private readonly Label _soldQuantityLabel;
        private readonly Label _totalQuantityLabel;
        private readonly Label _unitPriceLabel;
        private readonly Label _extraSoldoutMoneyLabel;
        private readonly Label _revenueLabel;

        private readonly Button _collectButton;
        private readonly Button _removeButton;

        public OwnLotRow() : base()
        {
            AddToClassList("own-lot-row");

            var statsContainer = new VisualElement();
            statsContainer.AddToClassList("own-lot-row__stats");
            Add(statsContainer);

            (_soldQuantityDescLabel, _soldQuantityLabel) =
                CreateStatCell(statsContainer, "own-lot-row__stat--sold-quantity");

            (_totalQuantityDescLabel, _totalQuantityLabel) =
                CreateStatCell(statsContainer, "own-lot-row__stat--total-quantity");

            (_unitPriceDescLabel, _unitPriceLabel) =
                CreateStatCell(statsContainer, "own-lot-row__stat--unit-price");

            (_extraSoldoutMoneyDescLabel, _extraSoldoutMoneyLabel) =
                CreateStatCell(statsContainer, "own-lot-row__stat--extra-soldout-money");

            (_revenueDescLabel, _revenueLabel) =
                CreateStatCell(statsContainer, "own-lot-row__stat--revenue");

            _collectButton = new Button { text = "Collect" };
            _collectButton.AddToClassList("own-lot-row__control-button");
            _collectButton.AddToClassList("own-lot-row__collect-button");
            Add(_collectButton);

            _removeButton = new Button { text = "Remove" };
            _removeButton.AddToClassList("own-lot-row__control-button");
            _removeButton.AddToClassList("own-lot-row__remove-button");
            Add(_removeButton);

            // Default description text — override in inspector
            _soldQuantityDescLabel.text = "Sold";
            _totalQuantityDescLabel.text = "Total Qty";
            _unitPriceDescLabel.text = "Unit Price";
            _extraSoldoutMoneyDescLabel.text = "Bonus";
            _revenueDescLabel.text = "Revenue";
        }

        public void SetCollectButtonEnabled(bool isEnabled)
        {
            _collectButton.SetEnabled(isEnabled);
        }

        public void SetRemoveButtonEnabled(bool isEnabled)
        {
            _removeButton.SetEnabled(isEnabled);
        }

        public void SetStats(string soldQuantity, string totalQuantity,
            string unitPrice, string extraSoldoutMoney, string revenue)
        {
            _soldQuantityLabel.text = soldQuantity;
            _totalQuantityLabel.text = totalQuantity;
            _unitPriceLabel.text = unitPrice;
            _extraSoldoutMoneyLabel.text = extraSoldoutMoney;
            _revenueLabel.text = $"{soldQuantity} * {unitPrice} + {extraSoldoutMoney} = {revenue}$";
        }


        private static (Label descLabel, Label valueLabel) CreateStatCell(VisualElement container, string modifierClass)
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
    }
}