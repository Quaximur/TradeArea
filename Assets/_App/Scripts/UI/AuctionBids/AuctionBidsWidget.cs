using System;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
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

            _loadableList.ListView.makeItem = () => new OwnBidRow();
            _loadableList.ListView.bindItem = (x, i) =>
            {
                var bidRow = (OwnBidRow)x;
                bidRow.style.marginTop = _lotMarginTop;
                bidRow.style.paddingTop = _lotPaddingTop;

                var ownBid = (OwnBidDto)_loadableList.ListView.itemsSource[i];
                var boughtQuantity = ownBid.BoughtQuantity;
                var remainingQuantity = ownBid.RemainingQuantity;
                var refund = remainingQuantity * ownBid.BidPrice;

                bidRow.ItemName = ownBid.ItemConfig.Name;
                // bidRow.ItemIcon = ownBid.ItemConfig.Icon;

                bidRow.SetStats(
                    boughtQuantity: boughtQuantity.ToString(),
                    totalQuantity: ownBid.TotalQuantity.ToString(),
                    bidPrice: ownBid.BidPrice.ToString(),
                    remainingQuantity: remainingQuantity.ToString(),
                    refund: refund.ToString()
                );

                // Can only cancel if there are still unfilled orders
                bidRow.SetCollectButtonEnabled(boughtQuantity > 0);
                bidRow.SetRemoveButtonEnabled(remainingQuantity > 0);

                bidRow.RemoveButton.RegisterCallback<ClickEvent, Guid>(OnRemoveClicked, ownBid.Id);
            };

            viewModel.OwnBids.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }

        private void OnRemoveClicked(ClickEvent _, Guid bidId)
        {
            ViewModel.RemoveBid(bidId).Forget();
        }

        private void OnCollectClicked(ClickEvent _, Guid bidId)
        {
            ViewModel.CollectBid(bidId).Forget();
        }
    }
}