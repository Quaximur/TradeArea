using TradeArea.Data;
using TradeArea.Shared;
using Aucy.Contracts.OnionClient;
using System.Collections.Generic;
using DevKit.Utils;
using System;
using System.Linq;
using TradeArea.Data.Items;
using TradeArea.Repositories;
using TradeArea.Shared.Notifications;

namespace TradeArea
{
    public class OrderHubReceiver : IOrderHubReceiver
    {
        private readonly AuctionModel _auctionModel;
        private readonly IItemConfigRepository _itemConfigRepository;
        private readonly INotificationService _notificationService;

        public OrderHubReceiver(
            AuctionModel auctionModel,
            IItemConfigRepository itemConfigRepository,
            INotificationService notificationService)
        {
            _auctionModel = auctionModel;
            _itemConfigRepository = itemConfigRepository;
            _notificationService = notificationService;
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
                    FLogger.Log(inventory.ItemStacks[0].ItemConfigId.ToString());
                    var itemStacks = inventory.ItemStacks
                        .Where(x => x != null)
                        .Select(x => new ItemStack(_itemConfigRepository.Get(x.ItemConfigId), x.Quantity))
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
                FLogger.Log(ownLots.Lots[0].ItemConfigId.ToString());
                var lots = ownLots.Lots
                    .Where(x => x != null)
                    .Select(x => new OwnLot
                    {
                        Id = x.Id,
                        ItemConfig = _itemConfigRepository.Get(x.ItemConfigId),
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
                FLogger.Log(ownBids.OwnBids[0].ItemConfigId.ToString());
                var bids = ownBids.OwnBids
                    .Where(x => x != null)
                    .Select(x => new OwnBid
                    {
                        Id = x.Id,
                        ItemConfig = _itemConfigRepository.Get(x.ItemConfigId),
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
            FLogger.LogGood($"Lot created: {lotCreated.LotId} x{lotCreated.TotalQuantity}");
            _notificationService.Show(
                $"Sell order created: {_itemConfigRepository.Get(lotCreated.ItemConfigId).Name} x{lotCreated.TotalQuantity}.",
                NotificationType.Success);

            _auctionModel.OnUsedWeightChanged.Value = LoadingState.Success(_auctionModel.OnUsedWeightChanged.Value.Value + lotCreated.WeightDiff);

            _auctionModel.OnOwnLotsChanged.Value.Value.Add(new OwnLot
            {
                Id = lotCreated.LotId,
                ItemConfig = _itemConfigRepository.Get(lotCreated.ItemConfigId),
                UnitPrice = lotCreated.UnitPrice,
                RemainingQuantity = lotCreated.TotalQuantity,
                TotalQuantity = lotCreated.TotalQuantity,
                CreatedAt = lotCreated.CreatedAt,
                ExtraSoldoutMoney = 0
            });

            _auctionModel.OnOwnLotsChanged.OnNext(_auctionModel.OnOwnLotsChanged.Value); // TODO: notify for RefreshItems only

            var itemStack = _auctionModel.OnInventoryChanged.Value.Value.FirstOrDefault(x => 
                x.ItemConfig.Id == lotCreated.ItemConfigId);

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
            _notificationService.Show(
                $"Sale proceeds collected: {lotCollected.ReturningMoney}.",
                NotificationType.Success);

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
            FLogger.Log($"OnLotRemoved: {lotRemoved.LotId} x{lotRemoved.RemovedQuantity}");
            _notificationService.Show(
                $"Sell order removed: {_itemConfigRepository.Get(lotRemoved.ItemConfigId).Name} x{lotRemoved.RemovedQuantity}.",
                NotificationType.Info);

            _auctionModel.OnUsedWeightChanged.Value = LoadingState.Success(_auctionModel.OnUsedWeightChanged.Value.Value + lotRemoved.WeightDiff);

            var existingStack = _auctionModel.OnInventoryChanged.Value.Value
                .FirstOrDefault(x => x.ItemConfig.Id == lotRemoved.ItemConfigId);

            if (existingStack != null)
            {
                existingStack.Quantity += lotRemoved.RemovedQuantity;
            }
            else
            {
                var newStack = new ItemStack
                (
                    itemConfig: _itemConfigRepository.Get(lotRemoved.ItemConfigId),
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
            FLogger.LogGood($"Bid created: {bidCreated.BidId} x{bidCreated.TotalQuantity}");
            _notificationService.Show(
                $"Buy order created: {_itemConfigRepository.Get(bidCreated.ItemConfigId).Name} x{bidCreated.TotalQuantity}.",
                NotificationType.Success);

            _auctionModel.OnOwnBidsChanged.Value.Value.Add(new OwnBid
            {
                Id = bidCreated.BidId,
                ItemConfig = _itemConfigRepository.Get(bidCreated.ItemConfigId),
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
            _notificationService.Show(
                $"Items collected: {_itemConfigRepository.Get(bidCollected.ItemConfigId).Name} x{bidCollected.CollectedQuantity}.",
                NotificationType.Success);

            _auctionModel.OnUsedWeightChanged.Value = LoadingState.Success(_auctionModel.OnUsedWeightChanged.Value.Value + bidCollected.WeightDiff);

            var existingStack = _auctionModel.OnInventoryChanged.Value.Value
                .FirstOrDefault(x => x.ItemConfig.Id == bidCollected.ItemConfigId);

            if (existingStack != null)
            {
                existingStack.Quantity += bidCollected.CollectedQuantity;
            }
            else
            {
                var newStack = new ItemStack
                (
                    itemConfig: _itemConfigRepository.Get(bidCollected.ItemConfigId),
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
            _notificationService.Show(
                $"Buy order removed. Refunded: {bidRemoved.ReturningMoney}.",
                NotificationType.Info);

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
                FLogger.LogWarning("Bid to remove is not found");
            }

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
                _notificationService.Show(
                    $"Bought: {ownBid.ItemConfig.Name} x{boughtQuantity}.",
                    NotificationType.Success);
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
                _notificationService.Show(
                    $"Sold: {ownLot.ItemConfig.Name} x{soldQuantity}.",
                    NotificationType.Success);
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
