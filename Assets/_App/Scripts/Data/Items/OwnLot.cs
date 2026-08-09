using System;

namespace TradeArea.Data.Items
{
    public class OwnLot
    {
        public Guid Id { get; set; }
        public long UnitPrice { get; set; }
        public int RemainingQuantity { get; set; }
        public int SoldQuantity => TotalQuantity - RemainingQuantity;

        public int TotalQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public IItemConfig ItemConfig { get; set; }
        public long ExtraSoldoutMoney { get; set; }
    }
}