using System.Collections.Generic;

namespace TradeArea.Data.Items
{
    public interface IItemConfigCollection
    {
        public IReadOnlyList<IItemConfig> AllData { get; }
    }
}