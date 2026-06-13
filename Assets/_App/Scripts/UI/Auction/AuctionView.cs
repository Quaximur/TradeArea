using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using R3;
using TradeArea.UI.AuctionBids;
using TradeArea.UI.AuctionInventory;
using TradeArea.UI.AuctionLots;
// using TradeArea.UI.AuctionOrders;
using TradeArea.UI.AuctionSearch;
using UnityEngine.UIElements;

namespace TradeArea.UI.Auction
{
    public class AuctionView : AttachableToolkitWindow<AuctionViewModel>
    {
        private AuctionSearchWidget _auctionSearchWidget;
        private AuctionInventoryWidget _auctionInventoryWidget;
        private AuctionLotsWidget _auctionLotsWidget;
        private AuctionBidsWidget _auctionBidsWidget;
        // private AuctionOrdersWidget _auctionOrdersWidget;

        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            base.OnInit();

            _auctionSearchWidget = Root.Q<AuctionSearchWidget>();
            _auctionSearchWidget.Init();

            _auctionInventoryWidget = Root.Q<AuctionInventoryWidget>();
            _auctionInventoryWidget.Init();

            _auctionLotsWidget = Root.Q<AuctionLotsWidget>();
            _auctionLotsWidget.Init();

            _auctionBidsWidget = Root.Q<AuctionBidsWidget>();
            _auctionBidsWidget.Init();

            // _auctionOrdersWidget = Root.Q<AuctionOrdersWidget>();
            // _auctionOrdersWidget.Init();
        }

        protected override void OnBind(AuctionViewModel viewModel)
        {
            base.OnBind(viewModel);

            _disposables = new();

            _auctionSearchWidget.Bind(viewModel.AuctionSearchViewModel);
            _auctionInventoryWidget.Bind(viewModel.AuctionInventoryViewModel);
            _auctionLotsWidget.Bind(viewModel.AuctionLotsViewModel);
            _auctionBidsWidget.Bind(viewModel.AuctionBidsViewModel);
            // _auctionOrdersWidget.Bind(viewModel.AuctionOrdersViewModel);
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);

            _auctionSearchWidget?.Dispose();
            _auctionInventoryWidget?.Dispose();
            _auctionLotsWidget?.Dispose();
            _auctionBidsWidget?.Dispose();
            // _auctionOrdersWidget?.Dispose();

            base.Dispose();
        }
    }
}
