using System.Collections.Generic;
using Aucy.Contracts.OnionClient;
using R3;
using TradeArea.Shared;

namespace TradeArea.Data
{
    public class AuctionModel
    {
        public readonly ReactiveProperty<LoadingState<long>> OnMoneyChanged = new();
        public readonly ReactiveProperty<LoadingState<List<ItemStackDto>>> OnInventoryChanged = new();
        public readonly ReactiveProperty<LoadingState<List<OwnLotDto>>> OnOwnLotsChanged = new();
        public readonly ReactiveProperty<LoadingState<List<OwnBidDto>>> OnOwnBidsChanged = new();
    }
}