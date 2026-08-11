using System.Collections.Generic;
using R3;
using TradeArea.Data.Items;
using TradeArea.Shared;

namespace TradeArea.Data
{
    public class AuctionModel
    {
        public readonly ReactiveProperty<LoadingState<long>> OnMoneyChanged = new();
        public readonly ReactiveProperty<LoadingState<float>> OnUsedWeightChanged = new();
        public readonly ReactiveProperty<LoadingState<float>> OnWeightLimitChanged = new();
        public readonly ReactiveProperty<LoadingState<int>> OnUsedCapacityChanged = new();
        public readonly ReactiveProperty<LoadingState<int>> OnCapacityLimitChanged = new();
        public readonly ReactiveProperty<LoadingState<List<ItemStack>>> OnInventoryChanged = new();
        public readonly ReactiveProperty<LoadingState<List<OwnLot>>> OnOwnLotsChanged = new();
        public readonly ReactiveProperty<LoadingState<List<OwnBid>>> OnOwnBidsChanged = new();
    }
}