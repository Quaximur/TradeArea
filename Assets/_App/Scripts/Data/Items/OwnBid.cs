using System;

namespace TradeArea.Data.Items
{
    public class OwnBid
    {
        public Guid Id { get; set; }
        public long BidPrice { get; set; }
        public int RemainingQuantity { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public IItemConfig ItemConfig { get; set; }

        public int BoughtQuantity => TotalQuantity - RemainingQuantity;
    }
}
