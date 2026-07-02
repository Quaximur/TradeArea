using UnityEngine;
using UnityEngine.UIElements;

namespace TradeArea.UI.Custom
{
    [UxmlElement]
    public partial class OrderBookLine : VisualElement
    {
        private readonly Label _bidLabel;
        private readonly Label _priceLabel;
        private readonly Label _lotLabel;
        
        private readonly VisualElement _bidBackground;
        private readonly VisualElement _lotBackground;

        public OrderBookLine()
        {
            AddToClassList("order-book__line");

            var lotContainer = new VisualElement();
            lotContainer.AddToClassList("order-book__lot-container");
            Add(lotContainer);

            _lotBackground = new VisualElement();
            _lotBackground.AddToClassList("order-book__bg-lot");
            lotContainer.Add(_lotBackground);

            _lotLabel = new Label();
            _lotLabel.AddToClassList("order-book__column-lot");
            lotContainer.Add(_lotLabel);

            _priceLabel = new Label();
            _priceLabel.AddToClassList("order-book__column-price");
            Add(_priceLabel);

            var bidContainer = new VisualElement();
            bidContainer.AddToClassList("order-book__bid-container");
            Add(bidContainer);

            _bidBackground = new VisualElement();
            _bidBackground.AddToClassList("order-book__bg-bid");
            bidContainer.Add(_bidBackground);

            _bidLabel = new Label();
            _bidLabel.AddToClassList("order-book__column-bid");
            bidContainer.Add(_bidLabel);
        }

        public void Setup(long minPrice, long maxPrice, long bidQuantity, long lotQuantity, float bidFillRatio, float lotFillRatio)
        {
            _priceLabel.text = minPrice == maxPrice ? minPrice.ToString() : $"{minPrice}-{maxPrice}";

            _bidLabel.text = bidQuantity > 0 ? bidQuantity.ToString() : string.Empty;
            _lotLabel.text = lotQuantity > 0 ? lotQuantity.ToString() : string.Empty;

            _bidBackground.style.width = Length.Percent(Mathf.Clamp01(bidFillRatio) * 100f);
            if (bidFillRatio <= 0f)
                _bidBackground.style.display = DisplayStyle.None;
            else
                _bidBackground.style.display = DisplayStyle.Flex;
            
            _lotBackground.style.width = Length.Percent(Mathf.Clamp01(lotFillRatio) * 100f);
            if (lotFillRatio <= 0f)
                _lotBackground.style.display = DisplayStyle.None;
            else
                _lotBackground.style.display = DisplayStyle.Flex;
        }
    }
}