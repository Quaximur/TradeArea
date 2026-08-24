using DevKit.UI.MVVM;
using UnityEngine.UIElements;
using R3;
using DevKit.Utils;
using TradeArea.UI.Custom;
using TradeArea.Data.Items;

namespace TradeArea.UI.AuctionSearch
{
    [UxmlElement]
    public partial class AuctionSearchWidget : ToolkitWidget<AuctionSearchViewModel>
    {
        [UxmlAttribute] private string _refreshMarketButtonName = "RefreshMarket";
        [UxmlAttribute] private string _loadableListViewWidgetName = "MarketList";

        private Button _refreshMarketButton;
        private LoadableListViewWidget _loadableList;

        private CompositeDisposable _disposables;

        public override void Show()
        {
            base.Show();
            _loadableList?.ListView.ScrollToItem(0);
        }

        protected override void OnInit()
        {
            _loadableList = Root.Q<LoadableListViewWidget>(_loadableListViewWidgetName);
            _loadableList.Init();

            _refreshMarketButton = Root.Q<Button>(_refreshMarketButtonName);

            _loadableList.ListView.makeItem = () =>
            {
                var row = new ItemConfigRow();
                return row;
            };

            _loadableList.ListView.bindItem = (e, i) =>
            {
                var itemRow = (ItemConfigRow)e;

                var sourceItem = _loadableList.ListView.itemsSource[i];
                if (sourceItem == null)
                {
                    FLogger.LogWarning<AuctionSearchWidget>($"ItemConfig is null at index {i}");
                    itemRow.SetStub();
                    
                    return;
                }

                var itemConfig = (IItemConfig)_loadableList.ListView.itemsSource[i];
                e.RegisterCallback<ClickEvent, IItemConfig>(OnItemClicked, itemConfig);

                itemRow.ItemName = itemConfig.Name;
                itemRow.ItemIcon = itemConfig.Icon;
                itemRow.Rarity = itemConfig.Rarity;
                itemRow.Grade = itemConfig.Grade;
            };

            _loadableList.ListView.unbindItem = (e, i) =>
            {
                e.UnregisterCallback<ClickEvent, IItemConfig>(OnItemClicked);
            };
        }

        private void OnItemClicked(ClickEvent _, IItemConfig itemConfig)
        {
            ViewModel.OpenBuyingView(itemConfig);
        }

        protected override void OnBind(AuctionSearchViewModel viewModel)
        {
            _disposables = new();

            viewModel.UpdateMarketItems();

            _refreshMarketButton.SubscribeCallback<ClickEvent>(_ => viewModel.UpdateMarketItems())
                .AddTo(_disposables);

            viewModel.OnMarketListChanged.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}