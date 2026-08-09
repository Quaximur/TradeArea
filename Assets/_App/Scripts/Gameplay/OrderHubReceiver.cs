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
                if (inventory.ItemStacks != null && inventory.ItemStacks.Length > 0)
                {
                    FLogger.Log(inventory.ItemStacks[0].ItemConfig.Name);
                    var itemStacks = inventory.ItemStacks.Select(x => new ItemStack(_itemConfigRepository.Get(x.ItemConfig.Id.ToString()), x.Quantity))
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
                var lots = ownLots.Lots.Select(x => new OwnLot()).ToList(); // TODO: map fields
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
                _auctionModel.OnOwnBidsChanged.Value = LoadingState.Success(ownBids.OwnBids);
            }
            else
            {
                FLogger.Log("Empty own bids");
                _auctionModel.OnOwnBidsChanged.Value = LoadingState.Success<List<OwnBidDto>>(new());
            }
        }

        #region Lots
        public void OnLotCreated(LotCreatedDto lotCreated)
        {
            FLogger.LogGood($"Lot created: {lotCreated.LotId} {lotCreated.ItemConfig.Name}");

            _auctionModel.OnOwnLotsChanged.Value.Value.Add(new OwnLotDto(id: lotCreated.LotId,
                ownerUserId: default,
                itemConfigId: lotCreated.ItemConfig.Id,
                unitPrice: lotCreated.UnitPrice,
                remainingQuantity: lotCreated.TotalQuantity,
                totalQuantity: lotCreated.TotalQuantity,
                createdAt: lotCreated.CreatedAt,
                itemConfig: lotCreated.ItemConfig,
                extraSoldoutMoney: 0));

            _auctionModel.OnOwnLotsChanged.OnNext(_auctionModel.OnOwnLotsChanged.Value); // TODO: notify for RefreshItems only

            // TODO: decrease UsedWeight and mb UsedCapacity
            var itemStack = _auctionModel.OnInventoryChanged.Value.Value.FirstOrDefault(x => x.ItemConfig.Id == lotCreated.ItemConfig.Id);
            if (itemStack != null)
            {
                itemStack.Quantity -= lotCreated.TotalQuantity;
                if (itemStack.Quantity <= 0)
                    _auctionModel.OnInventoryChanged.Value.Value.Remove(itemStack);

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

            // 1. Находим существующий стек с таким же ItemConfig в инвентаре
            var existingStack = _auctionModel.OnInventoryChanged.Value.Value
                .FirstOrDefault(x => x.ItemConfig.Id == lotRemoved.ItemConfig.Id);

            if (existingStack != null)
            {
                // Если стек существует - увеличиваем количество
                existingStack.Quantity += lotRemoved.RemovedQuantity;
            }
            else
            {
                // Если стека нет - создаем новый
                var newStack = new ItemStackDto
                (
                    itemConfig: lotRemoved.ItemConfig,
                    quantity: lotRemoved.RemovedQuantity
                );
                _auctionModel.OnInventoryChanged.Value.Value.Add(newStack);
            }

            // 2. Обновляем UsedWeight
            // _auctionModel.OnInventoryChanged.Value.UsedWeight +=
            //     lotRemoved.ItemConfig.Weight * lotRemoved.RemovedQuantity;

            // 3. Обновляем UsedCapacity (если был создан новый стек)
            // if (existingStack == null)
            // {
            //     _auctionModel.OnInventoryChanged.Value.UsedCapacity += 1;
            // }

            // 4. Удаляем лот из списка своих лотов
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

            // 5. Уведомляем об изменении инвентаря
            _auctionModel.OnInventoryChanged.OnNext(_auctionModel.OnInventoryChanged.Value);
        }
        #endregion

        #region Bids
        public void OnBidCreated(BidCreatedDto bidCreated)
        {
            FLogger.LogGood($"Bid created: {bidCreated.BidId} {bidCreated.ItemConfig.Name}");

            _auctionModel.OnOwnBidsChanged.Value.Value.Add(new OwnBidDto(id: bidCreated.BidId,
                bidderUserId: default,
                itemConfigId: bidCreated.ItemConfig.Id,
                bidPrice: bidCreated.BidPrice,
                remainingQuantity: bidCreated.TotalQuantity,
                totalQuantity: bidCreated.TotalQuantity,
                createdAt: bidCreated.CreatedAt,
                itemConfig: bidCreated.ItemConfig));

            _auctionModel.OnOwnBidsChanged.OnNext(_auctionModel.OnOwnBidsChanged.Value); // TODO: notify for RefreshItems only

            RemoveMoney(bidCreated.BidPrice * bidCreated.TotalQuantity);
        }

        public void OnBidCollected(BidCollectedDto bidCollected)
        {
            FLogger.Log($"OnBidCollected");

            var existingStack = _auctionModel.OnInventoryChanged.Value.Value
                .FirstOrDefault(x => x.ItemConfig.Id == bidCollected.ItemConfig.Id);

            if (existingStack != null)
            {
                // Если стек существует - увеличиваем количество
                existingStack.Quantity += bidCollected.CollectedQuantity;
            }
            else
            {
                // Если стека нет - создаем новый
                var newStack = new ItemStackDto
                (
                    itemConfig: bidCollected.ItemConfig,
                    quantity: bidCollected.CollectedQuantity
                );
                _auctionModel.OnInventoryChanged.Value.Value.Add(newStack);
            }

            _auctionModel.OnInventoryChanged.OnNext(_auctionModel.OnInventoryChanged.Value); // TODO: notify for RefreshItems only

            // TODO: update UsedWeight

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

            // 1. Удаляем ставку из списка своих лотов
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