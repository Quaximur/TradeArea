using System.Collections.Generic;
using Aucy.Contracts.OnionClient;
using R3;
using TradeArea.Data.Items;
using TradeArea.Shared;

namespace TradeArea.Data
{
    public class AuctionModel
    {
        public readonly ReactiveProperty<LoadingState<long>> OnMoneyChanged = new();
        public readonly ReactiveProperty<LoadingState<List<ItemStack>>> OnInventoryChanged = new();
        public readonly ReactiveProperty<LoadingState<List<OwnLot>>> OnOwnLotsChanged = new();
        public readonly ReactiveProperty<LoadingState<List<OwnBidDto>>> OnOwnBidsChanged = new();
    }
}