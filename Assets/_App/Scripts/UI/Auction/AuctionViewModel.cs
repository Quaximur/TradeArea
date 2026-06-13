using DevKit.UI.MVVM.Bases;
using TradeArea.UI.AuctionInventory;
using TradeArea.UI.AuctionSearch;
using TradeArea.UI.AuctionOrders;
using TradeArea.UI.AuctionLots;
using TradeArea.UI.AuctionBids;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        public AuctionSearchViewModel AuctionSearchViewModel { get; private set; }
        public AuctionInventoryViewModel AuctionInventoryViewModel { get; private set; }
        public AuctionOrdersViewModel AuctionOrdersViewModel { get; private set; }
        public AuctionLotsViewModel AuctionLotsViewModel { get; private set; }
        public AuctionBidsViewModel AuctionBidsViewModel { get; private set; }

        public AuctionViewModel(AuctionSearchViewModel auctionSearchViewModel,
            AuctionInventoryViewModel auctionInventoryViewModel,
            AuctionOrdersViewModel auctionOrdersViewModel,
            AuctionLotsViewModel auctionLotsViewModel,
            AuctionBidsViewModel auctionBidsViewModel)
        {
            AuctionOrdersViewModel = auctionOrdersViewModel;
            AuctionInventoryViewModel = auctionInventoryViewModel;
            AuctionSearchViewModel = auctionSearchViewModel;
            AuctionLotsViewModel = auctionLotsViewModel;
            AuctionBidsViewModel = auctionBidsViewModel;
        }

        public override void Dispose()
        {
            AuctionSearchViewModel?.Dispose();
            AuctionInventoryViewModel?.Dispose();
            AuctionOrdersViewModel?.Dispose();

            base.Dispose();
        }
    }
}
