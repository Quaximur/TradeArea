using TradeArea.Data;
using TradeArea.Shared;
using Aucy.Contracts.OnionClient;
using System.Collections.Generic;
using DevKit.Utils;
using System;
using System.Linq;

namespace TradeArea
{
    public class OrderHubReceiver : IOrderHubReceiver
    {
        private readonly AuctionModel _auctionModel;

        public OrderHubReceiver(AuctionModel auctionModel)
        {
            _auctionModel = auctionModel;
        }

        public void OnJoin(OwnLotsDto ownLots, OwnBidsDto ownBids, InventoryDto inventory)
        {
            if (inventory != null && inventory.ItemStacks != null && inventory.ItemStacks.Length > 0)
            {
                FLogger.Log(inventory.ItemStacks[0].ItemConfig.Name);
                _auctionModel.OnInventoryChanged.Value = LoadingState.Success(inventory.ItemStacks.ToList());
            }
            else
            {
                FLogger.Log("Empty Inventory");
                _auctionModel.OnInventoryChanged.Value = LoadingState.Success<List<ItemStackDto>>(new());
            }

            if (ownLots != null && ownLots.Lots != null && ownLots.Lots.Count > 0)
            {
                FLogger.Log(ownLots.Lots[0].ItemConfig.Name);
                _auctionModel.OnOwnLotsChanged.Value = LoadingState.Success(ownLots.Lots);
            }
            else
            {
                FLogger.Log("Empty own lots");
                _auctionModel.OnOwnLotsChanged.Value = LoadingState.Success<List<OwnLotDto>>(new());
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
                createdAt: default, // TODO: add CreatedAt
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
        }

        public void OnLotRemoved(LotRemovedDto lotRemoved)
        {
            FLogger.LogGood($"Lot removed: {lotRemoved.LotId} {lotRemoved.ItemConfig?.Name}");

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
                var newStack = new ItemStackDto{
                    ItemConfig = lotRemoved.ItemConfig,
                    Quantity = lotRemoved.RemovedQuantity
                };
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
                _auctionModel.OnOwnLotsChanged.Value.Value.Remove(ownLot);
                _auctionModel.OnOwnLotsChanged.OnNext(_auctionModel.OnOwnLotsChanged.Value);
            }

            // 5. Уведомляем об изменении инвентаря
            _auctionModel.OnInventoryChanged.OnNext(_auctionModel.OnInventoryChanged.Value);
        }
        #endregion

        #region Bids
        public void OnBidCreated(BidCreatedDto bidCreated)
        {
        }

        public void OnBidCollected(BidCollectedDto bidCollected)
        {
        }

        public void OnBidRemoved(BidRemovedDto bidRemoved)
        {
        }
        #endregion

        #region Worker Events
        public void OnBought(Guid bidId, int boughtQuantity, long extraMoney)
        {
            // TODO: store events queue until full state fetch (OnJoin)
            FLogger.Log("OnBought");
        }

        public void OnSold(Guid lotId, int soldQuantity, long extraMoney)
        {
            // TODO: store events queue until full state fetch (OnJoin)
            FLogger.Log("OnSold");
        }
        #endregion
    }
}