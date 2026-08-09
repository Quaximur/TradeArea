using System;
using System.Collections.Generic;
using TradeArea.Data.Items;

namespace TradeArea.Repositories
{
    public class LocalItemConfigRepository : IItemConfigRepository
    {
        private readonly Dictionary<Guid, IItemConfig> _itemConfigs = new();

        public void AddItemConfig(Guid id, IItemConfig config)
        {
            if (id == Guid.Empty || config == null)
                return;

            _itemConfigs[id] = config;
        }

        public IItemConfig Get(Guid id)
        {
            return _itemConfigs.TryGetValue(id, out var config) ? config : null;
        }
    }
}