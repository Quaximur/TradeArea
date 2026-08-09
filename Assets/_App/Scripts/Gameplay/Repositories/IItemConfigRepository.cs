using System;
using TradeArea.Data.Items;

namespace TradeArea.Repositories
{
    public interface IItemConfigRepository
    {
        public IItemConfig Get(Guid id);
    }
}