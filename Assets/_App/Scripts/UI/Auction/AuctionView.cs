using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using R3;
using TradeArea.UI.AuctionBids;
using TradeArea.UI.AuctionInventory;
using TradeArea.UI.AuctionLots;
using TradeArea.UI.AuctionSearch;
using UnityEngine;
using UnityEngine.UIElements;

namespace TradeArea.UI.Auction
{
    public class AuctionView : AttachableToolkitWindow<AuctionViewModel>
    {
        [SerializeField] private string _searchTabName = "SearchTab";
        [SerializeField] private string _inventoryTabName = "InventoryTab";
        [SerializeField] private string _bidsTabName = "BidsTab";
        [SerializeField] private string _lotsTabName = "LotsTab";

        private AuctionSearchWidget _auctionSearchWidget;
        private AuctionInventoryWidget _auctionInventoryWidget;
        private AuctionLotsWidget _auctionLotsWidget;
        private AuctionBidsWidget _auctionBidsWidget;

        private Tab _searchTab;
        private Tab _inventoryTab;
        private Tab _bidsTab;
        private Tab _lotsTab;

        private DisposableBag _disposables;

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

            _searchTab = Root.Q<Tab>(_searchTabName);
            _inventoryTab = Root.Q<Tab>(_inventoryTabName);
            _bidsTab = Root.Q<Tab>(_bidsTabName);
            _lotsTab = Root.Q<Tab>(_lotsTabName);
        }

        protected override void OnBind(AuctionViewModel viewModel)
        {
            base.OnBind(viewModel);

            viewModel.StartMessaging();

            _disposables = new();

            _auctionSearchWidget.Bind(viewModel.AuctionSearchViewModel);
            _auctionInventoryWidget.Bind(viewModel.AuctionInventoryViewModel);
            _auctionLotsWidget.Bind(viewModel.AuctionLotsViewModel);
            _auctionBidsWidget.Bind(viewModel.AuctionBidsViewModel);

            _searchTab.SubscribeCallback<ChangeEvent<bool>>(x =>
            {
                if (x.newValue)
                    _auctionSearchWidget.Show();
            }).AddTo(ref _disposables);

            _inventoryTab.SubscribeCallback<ChangeEvent<bool>>(x =>
            {
                if (x.newValue)
                {
                    _auctionInventoryWidget.Show();
                    _auctionBidsWidget.Show();
                    _auctionLotsWidget.Show();
                }
            }).AddTo(ref _disposables);;

            _bidsTab.SubscribeCallback<ChangeEvent<bool>>(x =>
            {
                if (x.newValue)
                    _auctionBidsWidget.Show();
            }).AddTo(ref _disposables);;

            _lotsTab.SubscribeCallback<ChangeEvent<bool>>(x =>
            {
                if (x.newValue)
                    _auctionLotsWidget.Show();
            }).AddTo(ref _disposables);;
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);

            _auctionSearchWidget?.Dispose();
            _auctionInventoryWidget?.Dispose();
            _auctionLotsWidget?.Dispose();
            _auctionBidsWidget?.Dispose();

            base.Dispose();
        }
    }
}
