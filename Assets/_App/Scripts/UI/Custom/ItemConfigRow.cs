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
            set => SetItemIcon(value);
        }


        [UxmlAttribute]
        public int Rarity
        {
            get => _rarity;
            set => SetRarity(value);
        }

        [UxmlAttribute]
        public int Grade
        {
            get => _grade;
            set => SetGrade(value);
        }

        private string _itemName;
        private Sprite _itemIcon;
        private int _rarity;
        private int _grade;

        private readonly Label _nameLabel;
        protected readonly VisualElement _startContainer;
        private readonly VisualElement _iconElement;

        public ItemConfigRow()
        {
            AddToClassList("item-config-row");

            _iconElement = new VisualElement();
            SetItemIcon(_itemIcon);
            _iconElement.AddToClassList("item-config-row__icon");
            Add(_iconElement);

            _startContainer = new VisualElement();
            _startContainer.AddToClassList("item-config-row__start-container");
            Add(_startContainer);

            _nameLabel = new Label(_itemName);
            _nameLabel.AddToClassList("item-config-row__name");
            _startContainer.Add(_nameLabel);


            SetRarity(_rarity);
            SetGrade(_grade);
        }

        public virtual void SetStub()
        {
            ItemName = "Unknown Item";
            ItemIcon = null;
            Rarity = 0;
            Grade = 0;
        }

        private void SetItemIcon(Sprite icon)
        {
            _itemIcon = icon;
            if (_iconElement != null)
                _iconElement.style.backgroundImage = icon != null ? Background.FromSprite(icon) : null;
        }

        public void SetRarity(int rarity)
        {
            _rarity = rarity;
            if (_iconElement != null)
            {
                var borderColor = rarity switch
                {
                    // Common - Soft silver/gray
                    0 => new Color(0.71f, 0.71f, 0.75f, 1f),

                    // Uncommon - Vibrant emerald green
                    1 => new Color(0.12f, 0.90f, 0.51f, 1f),

                    // Rare - Rich cerulean blue
                    2 => new Color(0.27f, 0.59f, 1.0f, 1f),

                    // Epic - Royal purple
                    3 => new Color(0.78f, 0.31f, 1.0f, 1f),

                    // Legendary - Warm golden orange
                    4 => new Color(1.0f, 0.71f, 0.20f, 1f),

                    // Mythic - Crimson red with gold tint
                    5 => new Color(1.0f, 0.24f, 0.24f, 1f),

                    // Artifact - Celestial teal
                    6 => new Color(0.0f, 1.0f, 0.82f, 1f),

                    _ => new Color(0.71f, 0.71f, 0.75f, 1f),
                };

                _iconElement.style.borderTopColor = borderColor;
                _iconElement.style.borderRightColor = borderColor;
                _iconElement.style.borderBottomColor = borderColor;
                _iconElement.style.borderLeftColor = borderColor;
            }
        }

        public void SetGrade(int grade)
        {
            _grade = grade;
        }
    }
}