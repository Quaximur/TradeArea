using TradeArea.Data;
using TradeArea.Shared;
using Aucy.Contracts.OnionClient;
using System.Collections.Generic;
using DevKit.Utils;
using System;
using System.Linq;
using TradeArea.Data.Items;
using TradeArea.Repositories;

namespace TradeArea
{
    public class OrderHubReceiver : IOrderHubReceiver
    {
        private readonly AuctionModel _auctionModel;
        private readonly IItemConfigRepository _itemConfigRepository;

        public OrderHubReceiver(AuctionModel auctionModel, IItemConfigRepository itemConfigRepository)
        {
            _auctionModel = auctionModel;
            _itemConfigRepository = itemConfigRepository;
        }

        public void OnJoin(OwnLotsDto ownLots, OwnBidsDto ownBids, InventoryDto inventory)
        {
            if (inventory != null)
            {
                _auctionModel.OnMoneyChanged.Value = LoadingState.Success(inventory.Money);
                _auctionModel.OnUsedWeightChanged.Value = LoadingState.Success(inventory.UsedWeight);
                _auctionModel.OnWeightLimitChanged.Value = LoadingState.Success(inventory.WeightLimit);
                _auctionModel.OnUsedCapacityChanged.Value = LoadingState.Success(inventory.UsedCapacity);
                _auctionModel.OnCapacityLimitChanged.Value = LoadingState.Success(inventory.Capacity);

                if (inventory.ItemStacks != null && inventory.ItemStacks.Length > 0)
                {
                    FLogger.Log(inventory.ItemStacks[0].ItemConfig.Name);
                    var itemStacks = inventory.ItemStacks
                        .Where(x => x != null)
                        .Select(x => new ItemStack(_itemConfigRepository.Get(x.ItemConfig.Id), x.Quantity))
                        .ToList();
                    _auctionModel.OnInventoryChanged.Value = LoadingState.Success(itemStacks);
                }
                else
                {
                    FLogger.Log("Empty Inventory");
                    _auctionModel.OnInventoryChanged.Value = LoadingState.Success<List<ItemStack>>(new());
                }
            }
            else
            {
                FLogger.LogWarning("Inventory is null");
                _auctionModel.OnMoneyChanged.Value = LoadingState.Fail<long>();
                _auctionModel.OnInventoryChanged.Value = LoadingState.Fail<List<ItemStack>>();
            }

            if (ownLots != null && ownLots.Lots != null && ownLots.Lots.Count > 0)
            {
                FLogger.Log(ownLots.Lots[0].ItemConfig.Name);
                var lots = ownLots.Lots
                    .Where(x => x != null)
                    .Select(x => new OwnLot
                    {
                        Id = x.Id,
                        ItemConfig = _itemConfigRepository.Get(x.ItemConfig.Id),
                        UnitPrice = x.UnitPrice,
                        RemainingQuantity = x.RemainingQuantity,
                        TotalQuantity = x.TotalQuantity,
                        CreatedAt = x.CreatedAt,
                        ExtraSoldoutMoney = x.ExtraSoldoutMoney
                    })
                    .ToList();
                _auctionModel.OnOwnLotsChanged.Value = LoadingState.Success(lots);
            }
            else
            {
                FLogger.Log("Empty own lots");
                _auctionModel.OnOwnLotsChanged.Value = LoadingState.Success<List<OwnLot>>(new());
            }

            if (ownBids != null && ownBids.OwnBids != null && ownBids.OwnBids.Count > 0)
            {
                FLogger.Log(ownBids.OwnBids[0].ItemConfig.Name);
                var bids = ownBids.OwnBids
                    .Where(x => x != null)
                    .Select(x => new OwnBid
                    {
                        Id = x.Id,
                        ItemConfig = _itemConfigRepository.Get(x.ItemConfig.Id),
                        BidPrice = x.BidPrice,
                        RemainingQuantity = x.RemainingQuantity,
                        TotalQuantity = x.TotalQuantity,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList();
                _auctionModel.OnOwnBidsChanged.Value = LoadingState.Success(bids);
            }
            else
            {
                FLogger.Log("Empty own bids");
                _auctionModel.OnOwnBidsChanged.Value = LoadingState.Success<List<OwnBid>>(new());
            }
        }

        #region Lots
        public void OnLotCreated(LotCreatedDto lotCreated)
        {
            FLogger.LogGood($"Lot created: {lotCreated.LotId} {lotCreated.ItemConfig.Name}");

            _auctionModel.OnUsedWeightChanged.Value = LoadingState.Success(_auctionModel.OnUsedWeightChanged.Value.Value - lotCreated.ItemConfig.Weight * lotCreated.TotalQuantity);

            _auctionModel.OnOwnLotsChanged.Value.Value.Add(new OwnLot
            {
                Id = lotCreated.LotId,
                ItemConfig = _itemConfigRepository.Get(lotCreated.ItemConfig.Id),
                UnitPrice = lotCreated.UnitPrice,
                RemainingQuantity = lotCreated.TotalQuantity,
                TotalQuantity = lotCreated.TotalQuantity,
                CreatedAt = lotCreated.CreatedAt,
                ExtraSoldoutMoney = 0
            });

            _auctionModel.OnOwnLotsChanged.OnNext(_auctionModel.OnOwnLotsChanged.Value); // TODO: notify for RefreshItems only

            var itemStack = _auctionModel.OnInventoryChanged.Value.Value.FirstOrDefault(x => x.ItemConfig.Id == lotCreated.ItemConfig.Id);

            if (itemStack != null)
            {
                if (itemStack.Quantity <= lotCreated.TotalQuantity)
                {
                    _auctionModel.OnInventoryChanged.Value.Value.Remove(itemStack);
                    _auctionModel.OnUsedCapacityChanged.Value = LoadingState.Success(_auctionModel.OnUsedCapacityChanged.Value.Value - 1);
                }
                else
                {
                    itemStack.Quantity -= lotCreated.TotalQuantity;
                }

                _auctionModel.OnInventoryChanged.OnNext(_auctionModel.OnInventoryChanged.Value); // TODO: notify for RefreshItems only
            }
        }

        public void OnLotCollected(LotCollectedDto lotCollected)
        {
            FLogger.Log($"OnLotCollected");

            var ownLot = _auctionModel.OnOwnLotsChanged.Value.Value
                .FirstOrDefault(x => x.Id == lotCollected.LotId);

            if (ownLot != null)
            {
                var collectedQuantity = lotCollected.CollectedQuantity;
                if (ownLot.RemainingQuantity == 0 && ownLot.TotalQuantity == collectedQuantity)
                    _auctionModel.OnOwnLotsChanged.Value.Value.Remove(ownLot);
                else
                    ownLot.TotalQuantity -= collectedQuantity;

                _auctionModel.OnOwnLotsChanged.OnNext(_auctionModel.OnOwnLotsChanged.Value); // TODO: notify for RefreshItems only
            }
            else
            {
                FLogger.LogWarning("Lot to collect is not found");
            }

            AddMoney(lotCollected.ReturningMoney);
        }

        public void OnLotRemoved(LotRemovedDto lotRemoved)
        {
            FLogger.Log($"OnLotRemoved: {lotRemoved.LotId} {lotRemoved.ItemConfig?.Name}");

            _auctionModel.OnUsedWeightChanged.Value = LoadingState.Success(_auctionModel.OnUsedWeightChanged.Value.Value + lotRemoved.ItemConfig.Weight * lotRemoved.RemovedQuantity);

            var existingStack = _auctionModel.OnInventoryChanged.Value.Value
                .FirstOrDefault(x => x.ItemConfig.Id == lotRemoved.ItemConfig.Id);

            if (existingStack != null)
            {
                existingStack.Quantity += lotRemoved.RemovedQuantity;
            }
            else
            {
                var newStack = new ItemStack
                (
                    itemConfig: _itemConfigRepository.Get(lotRemoved.ItemConfig.Id),
                    quantity: lotRemoved.RemovedQuantity
                );

                _auctionModel.OnInventoryChanged.Value.Value.Add(newStack);
                _auctionModel.OnUsedCapacityChanged.Value = LoadingState.Success(_auctionModel.OnUsedCapacityChanged.Value.Value + 1);
            }

            _auctionModel.OnInventoryChanged.OnNext(_auctionModel.OnInventoryChanged.Value); // TODO: notify for RefreshItems only

            var ownLot = _auctionModel.OnOwnLotsChanged.Value.Value
                .FirstOrDefault(x => x.Id == lotRemoved.LotId);

            if (ownLot != null)
            {
                var removedQuantity = lotRemoved.RemovedQuantity;
                if (ownLot.RemainingQuantity == ownLot.TotalQuantity && ownLot.TotalQuantity == removedQuantity)
                {
                    _auctionModel.OnOwnLotsChanged.Value.Value.Remove(ownLot);
                }
                else
                {
                    ownLot.TotalQuantity -= removedQuantity;
                    ownLot.RemainingQuantity -= removedQuantity;
                }

                _auctionModel.OnOwnLotsChanged.OnNext(_auctionModel.OnOwnLotsChanged.Value);
            }
            else
            {
                FLogger.LogWarning("Lot to remove is not found");
            }

        }
        #endregion

        #region Bids
        public void OnBidCreated(BidCreatedDto bidCreated)
        {
            FLogger.LogGood($"Bid created: {bidCreated.BidId} {bidCreated.ItemConfig.Name}");

            _auctionModel.OnOwnBidsChanged.Value.Value.Add(new OwnBid
            {
                Id = bidCreated.BidId,
                ItemConfig = _itemConfigRepository.Get(bidCreated.ItemConfig.Id),
                BidPrice = bidCreated.BidPrice,
                RemainingQuantity = bidCreated.TotalQuantity,
                TotalQuantity = bidCreated.TotalQuantity,
                CreatedAt = bidCreated.CreatedAt
            });

            _auctionModel.OnOwnBidsChanged.OnNext(_auctionModel.OnOwnBidsChanged.Value); // TODO: notify for RefreshItems only

            RemoveMoney(bidCreated.BidPrice * bidCreated.TotalQuantity);
        }

        public void OnBidCollected(BidCollectedDto bidCollected)
        {
            FLogger.Log($"OnBidCollected");

            _auctionModel.OnUsedWeightChanged.Value = LoadingState.Success(_auctionModel.OnUsedWeightChanged.Value.Value + bidCollected.ItemConfig.Weight * bidCollected.CollectedQuantity);

            var existingStack = _auctionModel.OnInventoryChanged.Value.Value
                .FirstOrDefault(x => x.ItemConfig.Id == bidCollected.ItemConfig.Id);

            if (existingStack != null)
            {
                existingStack.Quantity += bidCollected.CollectedQuantity;
            }
            else
            {
                var newStack = new ItemStack
                (
                    itemConfig: _itemConfigRepository.Get(bidCollected.ItemConfig.Id),
                    quantity: bidCollected.CollectedQuantity
                );
                _auctionModel.OnInventoryChanged.Value.Value.Add(newStack);
                _auctionModel.OnUsedCapacityChanged.Value = LoadingState.Success(_auctionModel.OnUsedCapacityChanged.Value.Value + 1);
            }

            _auctionModel.OnInventoryChanged.OnNext(_auctionModel.OnInventoryChanged.Value); // TODO: notify for RefreshItems only

            var ownBid = _auctionModel.OnOwnBidsChanged.Value.Value
                .FirstOrDefault(x => x.Id == bidCollected.BidId);

            if (ownBid != null)
            {
                var collectedQuantity = bidCollected.CollectedQuantity;
                if (ownBid.RemainingQuantity == 0 && ownBid.TotalQuantity == collectedQuantity)
                {
                    _auctionModel.OnOwnBidsChanged.Value.Value.Remove(ownBid);
                }
                else
                {
                    ownBid.TotalQuantity -= collectedQuantity;
                }

                _auctionModel.OnOwnBidsChanged.OnNext(_auctionModel.OnOwnBidsChanged.Value); // TODO: notify for RefreshItems only
            }
            else
            {
                FLogger.LogWarning("Bid to collect is not found");
            }
        }

        public void OnBidRemoved(BidRemovedDto bidRemoved)
        {
            FLogger.LogGood($"Bid removed: {bidRemoved.BidId}. Money returned: {bidRemoved.ReturningMoney}");

            var ownBid = _auctionModel.OnOwnBidsChanged.Value.Value
                .FirstOrDefault(x => x.Id == bidRemoved.BidId);

            if (ownBid != null)
            {
                var removedQuantity = bidRemoved.RemovedQuantity;
                if (ownBid.RemainingQuantity == ownBid.TotalQuantity && ownBid.TotalQuantity == removedQuantity)
                {
                    _auctionModel.OnOwnBidsChanged.Value.Value.Remove(ownBid);
                }
                else
                {
                    ownBid.RemainingQuantity -= removedQuantity;
                    ownBid.TotalQuantity -= removedQuantity;
                }

                _auctionModel.OnOwnBidsChanged.OnNext(_auctionModel.OnOwnBidsChanged.Value); // TODO: notify for RefreshItems only
            }
            else
            {
                FLogger.LogWarning("Bid to collect is not found");
            }

            if (_auctionModel.OnOwnBidsChanged.Value.Value.TryRemoveFirstWhere(x => x.Id == bidRemoved.BidId))
                _auctionModel.OnOwnBidsChanged.OnNext(_auctionModel.OnOwnBidsChanged.Value);
            else
                FLogger.LogWarning("Bid for removing not found");

            AddMoney(bidRemoved.ReturningMoney);
        }
        #endregion

        #region Worker Events
        public void OnBought(Guid bidId, int boughtQuantity, long extraMoney)
        {
            // TODO: store events queue until full state fetch (OnJoin)
            FLogger.Log("OnBought");

            var ownBid = _auctionModel.OnOwnBidsChanged.Value.Value
                .FirstOrDefault(x => x.Id == bidId);

            if (ownBid != null)
            {
                ownBid.RemainingQuantity -= boughtQuantity;
                _auctionModel.OnOwnBidsChanged.OnNext(_auctionModel.OnOwnBidsChanged.Value); // TODO: notify for RefreshItems only
            }
            else
            {
                FLogger.LogWarning($"{nameof(OnBought)}: bid not found");
            }

            AddMoney(extraMoney);
        }

        public void OnSold(Guid lotId, int soldQuantity, long extraMoney)
        {
            // TODO: store events queue until full state fetch (OnJoin)
            FLogger.Log("OnSold");

            var ownLot = _auctionModel.OnOwnLotsChanged.Value.Value
                .FirstOrDefault(x => x.Id == lotId);

            if (ownLot != null)
            {
                ownLot.RemainingQuantity -= soldQuantity;
                ownLot.ExtraSoldoutMoney += extraMoney;
                _auctionModel.OnOwnLotsChanged.OnNext(_auctionModel.OnOwnLotsChanged.Value); // TODO: notify for RefreshItems only
            }
            else
            {
                FLogger.LogWarning($"{nameof(OnSold)}: lot not found");
            }
        }
        #endregion

        private void AddMoney(long moneyToAdd)
        {
            _auctionModel.OnMoneyChanged.Value = LoadingState.Success<long>(_auctionModel.OnMoneyChanged.Value.Value
                + moneyToAdd);
        }

        private void RemoveMoney(long moneyToRemove)
        {
            _auctionModel.OnMoneyChanged.Value = LoadingState.Success<long>(_auctionModel.OnMoneyChanged.Value.Value
                - moneyToRemove);
        }
    }
}