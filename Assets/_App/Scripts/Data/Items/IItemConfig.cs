using System;
using System.Collections.Generic;
using UnityEngine;

namespace TradeArea.Data.Items
{
    public interface IItemConfig
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsUnary { get; }
        public Sprite Icon { get; }
        public int Grade { get; }
        public int Rarity { get; }
        public float Weight { get; }
        public string BaseType { get; }
        public IList<string> Extra { get; }
        public string CategoryPath { get; }
        public long MinPrice { get; }
        public long MaxPrice { get; }
        public int MaxSellQuantity { get; }
        public int MaxBuyQuantity { get; }
    }
}