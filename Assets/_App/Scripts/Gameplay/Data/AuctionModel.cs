using System.Collections.Generic;
using Aucy.Contracts.OnionClient;
using R3;
using TradeArea.Shared;

namespace TradeArea.Data
{
    public class AuctionModel
    {
        public readonly ReactiveProperty<LoadingState<List<ItemConfigDto>>> OnMarketListChanged = new();
        public readonly ReactiveProperty<LoadingState<List<ItemStackDto>>> OnInventoryChanged = new();
    }
}