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
        [UxmlAttribute] private string _usedWeightLabelName = "UsedWeightLabel";
        [UxmlAttribute] private string _weightLimitLabelName = "WeightLimitLabel";
        [UxmlAttribute] private string _usedCapacityLabelName = "UsedCapacityLabel";
        [UxmlAttribute] private string _capacityLimitLabelName = "CapacityLimitLabel";
        [UxmlAttribute] private string _loadableListViewWidgetName = null;
        [UxmlAttribute] private int _lotMarginTop = 4;
        [UxmlAttribute] private int _lotPaddingTop = 4;

        private Label _moneyLabel;
        private Label _usedWeightLabel;
        private Label _weightLimitLabel;
        private Label _usedCapacityLabel;
        private Label _capacityLimitLabel;
        private LoadableListViewWidget _loadableList;

        private CompositeDisposable _disposables;


        protected override void OnInit()
        {
            _moneyLabel = Root.Q<Label>(name: _moneyLabelName);
            
            _usedWeightLabel = Root.Q<Label>(name: _usedWeightLabelName);
            _weightLimitLabel = Root.Q<Label>(name: _weightLimitLabelName);
            _usedCapacityLabel = Root.Q<Label>(name: _usedCapacityLabelName);
            _capacityLimitLabel = Root.Q<Label>(name: _capacityLimitLabelName);

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
                    itemRow.SetStub();

                    return;
                }

                var itemStack = (ItemStack)_loadableList.ListView.itemsSource[i];

                if (itemStack.ItemConfig == null)
                {
                    FLogger.LogWarning<AuctionInventoryWidget>($"ItemConfig is null for ItemStack at index {i}");
                    itemRow.SetStub();
                    itemRow.ItemName = $"Unknown {itemStack.Quantity}";

                    return;
                }

                e.RegisterCallback<ClickEvent, IItemConfig>(OnItemClicked, itemStack.ItemConfig);

                itemRow.ItemName = $"{itemStack.ItemConfig.Name} {itemStack.Quantity}";
                itemRow.ItemIcon = itemStack.ItemConfig.Icon;
                itemRow.Rarity = itemStack.ItemConfig.Rarity;
                itemRow.Grade = itemStack.ItemConfig.Grade;
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

            viewModel.OnUsedWeightChanged.Subscribe(x =>
            {
                if (x.LoadingFlag == Shared.LoadingFlag.Success)
                    _usedWeightLabel.text = x.Value.ToString("F2");
            }).AddTo(_disposables);

            viewModel.OnWeightLimitChanged.Subscribe(x =>
            {
                if (x.LoadingFlag == Shared.LoadingFlag.Success)
                    _weightLimitLabel.text = x.Value.ToString("F2");
            }).AddTo(_disposables);

            viewModel.OnUsedCapacityChanged.Subscribe(x =>
            {
                if (x.LoadingFlag == Shared.LoadingFlag.Success)
                    _usedCapacityLabel.text = x.Value.ToString("N0");
            }).AddTo(_disposables);

            viewModel.OnCapacityLimitChanged.Subscribe(x =>
            {
                if (x.LoadingFlag == Shared.LoadingFlag.Success)
                    _capacityLimitLabel.text = x.Value.ToString("N0");
            }).AddTo(_disposables);
        }

        private void OnItemClicked(ClickEvent _, IItemConfig itemConfig)
        {
            ViewModel.OpenSellingView(itemConfig);
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}