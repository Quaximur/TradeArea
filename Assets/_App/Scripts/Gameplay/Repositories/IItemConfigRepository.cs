using TradeArea.Data.Items;

namespace TradeArea.Repositories
{
    public interface IItemConfigRepository
    {
        public IItemConfig Get(string id);
    }
}