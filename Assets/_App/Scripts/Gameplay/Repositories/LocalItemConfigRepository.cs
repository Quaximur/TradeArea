using System.Collections.Generic;
using TradeArea.Data.Items;

namespace TradeArea.Repositories
{
    public class LocalItemConfigRepository : IItemConfigRepository
    {
        private readonly Dictionary<string, IItemConfig> _itemConfigs = new();

        public void AddItemConfig(string id, IItemConfig config)
        {
            if (string.IsNullOrEmpty(id) || config == null)
                return;

            _itemConfigs[id] = config;
        }

        public IItemConfig Get(string id)
        {
            return _itemConfigs.TryGetValue(id, out var config) ? config : null;
        }
    }
}