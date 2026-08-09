namespace TradeArea.Data.Items
{
    public class ItemStack
    {
        public IItemConfig ItemConfig { get; set; }
        public int Quantity { get; set; }

        public ItemStack(IItemConfig itemConfig, int quantity)
        {
            ItemConfig = itemConfig;
            Quantity = quantity;
        }
    }
}