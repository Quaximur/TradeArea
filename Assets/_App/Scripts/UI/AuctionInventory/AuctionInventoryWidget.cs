using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using R3;
using TradeArea.Data.Items;
using TradeArea.UI.Custom;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionInventory
{
    [UxmlElement]
    public partial class AuctionInventoryWidget : ToolkitWidget<AuctionInventoryViewModel>
    {
        [UxmlAttribute] private string _moneyLabelName = "MoneyLabel";
        [UxmlAttribute] private string _loadableListViewWidgetName = null;
        [UxmlAttribute] private int _lotMarginTop = 4;
        [UxmlAttribute] private int _lotPaddingTop = 4;

        private Label _moneyLabel;
        private LoadableListViewWidget _loadableList;

        private CompositeDisposable _disposables;


        protected override void OnInit()
        {
            _moneyLabel = Root.Q<Label>(name: _moneyLabelName);
            _loadableList = Root.Q<LoadableListViewWidget>(_loadableListViewWidgetName);
            _loadableList.Init();
        }

        protected override void OnBind(AuctionInventoryViewModel viewModel)
        {
            _disposables = new();

            _loadableList.ListView.makeItem = () =>
            {
                var row = new ItemConfigRow();
                return row;
            };

            _loadableList.ListView.bindItem = (e, i) =>
            {
                var itemRow = (ItemConfigRow)e; // TODO: Replace with Element for ItemStack
                itemRow.style.marginTop = _lotMarginTop;
                itemRow.style.paddingTop = _lotPaddingTop;

                var sourceItem = _loadableList.ListView.itemsSource[i];
                if (sourceItem == null)
                {
                    FLogger.LogWarning<AuctionInventoryWidget>($"ItemStack is null at index {i}");
                    itemRow.ItemName = "Unknown";
                    itemRow.ItemIcon = null;
                    return;
                }

                var itemStack = (ItemStack)_loadableList.ListView.itemsSource[i];

                if (itemStack.ItemConfig == null)
                {
                    FLogger.LogWarning<AuctionInventoryWidget>($"ItemConfig is null for ItemStack at index {i}");
                    itemRow.ItemName = "Unknown";
                    itemRow.ItemIcon = null;
                    return;
                }

                e.RegisterCallback<ClickEvent, IItemConfig>(OnItemClicked, itemStack.ItemConfig);

                itemRow.ItemName = $"{itemStack.ItemConfig.Name} {itemStack.Quantity}";
                itemRow.ItemIcon = itemStack.ItemConfig.Icon;
            };

            _loadableList.ListView.unbindItem = (e, i) =>
            {
                e.UnregisterCallback<ClickEvent, IItemConfig>(OnItemClicked);
            };

            viewModel.ItemStacks.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);

            viewModel.OnMoneyChanged.Subscribe(x => // TODO: loading/error state
            {
                if (x.LoadingFlag == Shared.LoadingFlag.Success)
                    _moneyLabel.text = x.Value.ToString("N0");
            }).AddTo(_disposables);
        }

        private void OnItemClicked(ClickEvent _, IItemConfig itemConfig)
        {
            // Получаем ViewModel через поле класса или другим способом
            ViewModel.OpenSellingView(itemConfig);
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}