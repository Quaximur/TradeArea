using UnityEngine;
using UnityEngine.UIElements;

namespace TradeArea.UI.Custom
{
    [UxmlElement]
    public partial class ItemConfigRow : VisualElement
    {
        [UxmlAttribute]
        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                if (_nameLabel != null)
                    _nameLabel.text = value;
            }
        }

        [UxmlAttribute]
        public Sprite ItemIcon
        {
            get => _itemIcon;
            set
            {
                _itemIcon = value;
                if (_iconElement != null)
                    _iconElement.style.backgroundImage = value != null ? Background.FromSprite(value) : null;
            }
        }

        private string _itemName;
        private Sprite _itemIcon;

        private readonly Label _nameLabel;
        private readonly VisualElement _iconElement;

        public ItemConfigRow()
        {
            AddToClassList("item-config-row");

            _iconElement = new VisualElement();
            _iconElement.style.backgroundImage = _itemIcon != null ? Background.FromSprite(_itemIcon) : null;
            _iconElement.AddToClassList("item-config-row__icon");
            Add(_iconElement);

            _nameLabel = new Label(_itemName);
            _nameLabel.AddToClassList("item-config-row__name");
            Add(_nameLabel);
        }
    }
}