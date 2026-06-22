using Aucy.Contracts.OnionClient;
using DevKit.UI.MVVM;
using R3;
using TradeArea.UI.Custom;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionBids
{
    [UxmlElement]
    public partial class AuctionBidsWidget : ToolkitWidget<AuctionBidsViewModel>
    {
        [UxmlAttribute] private string _loadableListViewWidgetName = null;
        [UxmlAttribute] private int _lotMarginTop = 4;
        [UxmlAttribute] private int _lotPaddingTop = 4;

        private LoadableListViewWidget _loadableList;

        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            _loadableList = Root.Q<LoadableListViewWidget>(_loadableListViewWidgetName);
            _loadableList.Init();
        }

        protected override void OnBind(AuctionBidsViewModel viewModel)
        {
            _disposables = new();

            _loadableList.ListView.makeItem = () =>
            {
                var row = new ItemConfigRow();
                return row;
            };

            _loadableList.ListView.bindItem = (x, i) =>
            {
                var itemRow = (ItemConfigRow)x; // TODO: Replace with Element for ItemStack
                itemRow.style.marginTop = _lotMarginTop;
                itemRow.style.paddingTop = _lotPaddingTop;
                
                var ownBid = (OwnBidDto)_loadableList.ListView.itemsSource[i];
                var boughtQuantity = ownBid.BoughtQuantity;
                itemRow.ItemName = $"{ownBid.ItemConfig.Name} | Bought: {boughtQuantity} | Orders: {ownBid.TotalQuantity}) | Fund: {ownBid.RemainingQuantity * ownBid.BidPrice}$";

            };

            viewModel.OwnBids.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }
    }
}