using DevKit.UI.MVVM.Bases;
using TradeArea.UI.AuctionInventory;
using TradeArea.UI.AuctionSearch;
using TradeArea.UI.AuctionOrders;
using TradeArea.UI.AuctionLots;
using TradeArea.UI.AuctionBids;
using TradeArea.Data;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        public AuctionSearchViewModel AuctionSearchViewModel { get; private set; }
        public AuctionInventoryViewModel AuctionInventoryViewModel { get; private set; }
        public AuctionOrdersViewModel AuctionOrdersViewModel { get; private set; }
        public AuctionLotsViewModel AuctionLotsViewModel { get; private set; }
        public AuctionBidsViewModel AuctionBidsViewModel { get; private set; }

        private readonly AuctionModel _auctionModel;
        private readonly OrderHubService _orderHubService;

        public AuctionViewModel(AuctionModel auctionModel,
            OrderHubService orderHubService,
            AuctionSearchViewModel auctionSearchViewModel,
            AuctionInventoryViewModel auctionInventoryViewModel,
            AuctionOrdersViewModel auctionOrdersViewModel,
            AuctionLotsViewModel auctionLotsViewModel,
            AuctionBidsViewModel auctionBidsViewModel)
        {
            _auctionModel = auctionModel;
            _orderHubService = orderHubService;

            AuctionOrdersViewModel = auctionOrdersViewModel;
            AuctionInventoryViewModel = auctionInventoryViewModel;
            AuctionSearchViewModel = auctionSearchViewModel;
            AuctionLotsViewModel = auctionLotsViewModel;
            AuctionBidsViewModel = auctionBidsViewModel;
        }

        public void StartMessaging()
        {
            _orderHubService.StartMessaging();
        }

        public override void Dispose()
        {
            _orderHubService?.Dispose();
            
            AuctionSearchViewModel?.Dispose();
            AuctionInventoryViewModel?.Dispose();
            AuctionOrdersViewModel?.Dispose();
            AuctionLotsViewModel?.Dispose();
            AuctionBidsViewModel?.Dispose();

            base.Dispose();
        }
    }
}
