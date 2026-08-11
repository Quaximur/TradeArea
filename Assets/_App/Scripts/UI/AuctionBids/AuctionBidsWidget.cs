using System;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using R3;
using TradeArea.Data.Items;
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
            _loadableList.ListView.bindItem = (e, i) =>
            {
                var ownBidRow = (OwnBidRow)e;
                ownBidRow.style.marginTop = _lotMarginTop;
                ownBidRow.style.paddingTop = _lotPaddingTop;

                var sourceItem = _loadableList.ListView.itemsSource[i];
                if (sourceItem == null)
                {
                    FLogger.LogWarning<AuctionBidsWidget>($"OwnBid is null at index {i}");
                    ownBidRow.SetStub();

                    return;
                }

                var ownBid = (OwnBid)_loadableList.ListView.itemsSource[i];
                var boughtQuantity = ownBid.BoughtQuantity;
                var remainingQuantity = ownBid.RemainingQuantity;
                var refund = remainingQuantity * ownBid.BidPrice;

                if (ownBid.ItemConfig == null)
                {
                    FLogger.LogWarning<AuctionBidsWidget>($"ItemConfig is null for OwnBid at index {i}");
                    ownBidRow.SetStub();

                    ownBidRow.SetStats(
                        boughtQuantity: boughtQuantity.ToString(),
                        totalQuantity: ownBid.TotalQuantity.ToString(),
                        bidPrice: ownBid.BidPrice.ToString(),
                        refund: refund.ToString()
                    );

                    return;
                }

                ownBidRow.ItemName = ownBid.ItemConfig.Name;
                ownBidRow.ItemIcon = ownBid.ItemConfig.Icon;
                ownBidRow.Rarity = ownBid.ItemConfig.Rarity;
                ownBidRow.Grade = ownBid.ItemConfig.Grade;

                ownBidRow.SetStats(
                    boughtQuantity: boughtQuantity.ToString(),
                    totalQuantity: ownBid.TotalQuantity.ToString(),
                    bidPrice: ownBid.BidPrice.ToString(),
                    refund: refund.ToString()
                );

                // Can only cancel if there are still unfilled orders
                ownBidRow.SetCollectButtonEnabled(boughtQuantity > 0);
                ownBidRow.SetRemoveButtonEnabled(remainingQuantity > 0);

                ownBidRow.CollectButton.RegisterCallback<ClickEvent, Guid>(OnCollectClicked, ownBid.Id);
                ownBidRow.RemoveButton.RegisterCallback<ClickEvent, Guid>(OnRemoveClicked, ownBid.Id);
            };

            _loadableList.ListView.unbindItem = (e, i) =>
           {
               var ownBidRow = (OwnBidRow)e;
               ownBidRow.CollectButton?.UnregisterCallback<ClickEvent, Guid>(OnCollectClicked);
               ownBidRow.RemoveButton?.UnregisterCallback<ClickEvent, Guid>(OnRemoveClicked);
           };

            viewModel.OwnBids.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }

        private void OnRemoveClicked(ClickEvent _, Guid bidId)
        {
            ViewModel.RemoveBidAsync(bidId).Forget();
        }

        private void OnCollectClicked(ClickEvent _, Guid bidId)
        {
            ViewModel.CollectBidAsync(bidId).Forget();
        }
    }
}