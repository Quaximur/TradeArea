using DevKit.UI.MVVM;
using UnityEngine.UIElements;
using R3;
using DevKit.Utils;
using TradeArea.UI.Custom;
using Aucy.Contracts.OnionClient;

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

        protected override void OnInit()
        {
            _loadableList = Root.Q<LoadableListViewWidget>(_loadableListViewWidgetName);
            _loadableList.Init();
            
            _refreshMarketButton = Root.Q<Button>(_refreshMarketButtonName);

            _loadableList.ListView.bindItem = (x, i) =>
            {
                var itemRow = x.Q<ItemConfigRow>();
                itemRow.ItemName = ((ItemConfigDto)_loadableList.ListView.itemsSource[i]).Name;
            };
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