using DevKit.UI.MVVM.Bases;
using MagicOnion;
using MagicOnion.Client;
using Aucy.Contracts.OnionContracts;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using System;
using Random = UnityEngine.Random;
using DevKit.Utils;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        // 31.05.26
        private const string TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMmMyZjJmNzEtYWEwZi00NmYxLTgzOWYtOTc2NGEzNGI0Zjc2IiwiZXhwIjoxNzgwMjcwOTg1fQ.bzEzlsxktqxDTTdt5dvhTUM837Hx5adq9YSYwTIqWME";
        // private const string USER_ID_1 = "2c2f2f71-aa0f-46f1-839f-9764a34b4f76";

        public AuctionViewModel()
        {
            CreateInventoryTest().Forget();
        }

        public async UniTask CreateInventoryTest()
        {
            try
            {
                using var channel = GrpcChannelx.ForAddress("http://localhost:8765");

                var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                    .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

                var inventoryDto = await inventoryClient.CreateInventory(new CreateInventoryDto()
                {
                    Capacity = 1000,
                    WeightLimit = 10000,
                });

                for (var i = 0; i < 10; i++)
                {
                    var isUnary = Random.Range(0, 2) == 0;
                    var itemStackDto = new AddItemStackDto()
                    {
                        ItemConfig = new ItemConfigDto(
                            id: Guid.NewGuid(),
                            name: isUnary ? $"Item_{i + 1}_Unary" : $"Item_{i + 1}",
                            isUnary: isUnary,
                            icon: $"item_icon_{i + 1}",
                            grade: Random.Range(0, 5),
                            rarity: Random.Range(0, 5),
                            weight: Random.Range(1, 20),
                            baseType: isUnary ? "weapon" : Random.Range(0, 2) == 0 ? "material" : "consumable",
                            extra: null,
                            categoryPath: "common"
                        ),
                        Quantity = isUnary ? 1 : Random.Range(1, 101)
                    };

                    await inventoryClient.AddItemStack(itemStackDto);

                    FLogger.LogGood($"Added item to inventory (ItemId: {itemStackDto.ItemConfig.Id}, Name: {itemStackDto.ItemConfig.Name}, Quantity: {itemStackDto.Quantity})");
                }

                FLogger.LogGood($"Inventory created successfully (Capacity: {inventoryDto.Capacity}, WeightLimit: {inventoryDto.WeightLimit})");
            }
            catch (Exception ex)
            {
                FLogger.LogError($"Failed to create inventory: {ex}");
                return;
            }
        }

        public async UniTask CreateLotTest()
        {
            try
            {
                using var channel = GrpcChannelx.ForAddress("http://localhost:8765");

                var auctionClient = MagicOnionClient.Create<IAuctionService>(channel)
                    .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

                var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                    .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

                var inventoryDto = await inventoryClient.GetOwnInventory(new GetInventoryDto()
                {
                    Page = 1,
                    PageSize = 1000
                });


                if (inventoryDto == null || inventoryDto.ItemStacks == null && inventoryDto.ItemStacks.Length <= 0)
                {
                    FLogger.LogWarning("No items found in inventory.");
                    return;
                }

                FLogger.Log($"Paginated items count: {inventoryDto.ItemStacks.Length}");

                var randomItemStack = inventoryDto.ItemStacks[Random.Range(0, inventoryDto.ItemStacks.Length)];

                var lotDto = await auctionClient.CreateLot(new CreateLotDto
                (
                    itemConfig: randomItemStack.ItemConfig,
                    unitPrice: Random.Range(1_000, 1_000_001),
                    totalQuantity: Random.Range(1, randomItemStack.Quantity + 1)
                ));

                FLogger.LogGood($"Lot created successfully (LotId: {lotDto.Id}, ItemConfigId: {lotDto.ItemConfigId}, UnitPrice: {lotDto.UnitPrice}, RemainingQuantity: {lotDto.RemainingQuantity})");
            }
            catch (Exception ex)
            {
                FLogger.LogError($"Failed to create lot: {ex}");
                return;
            }
        }

        public async UniTask RefreshOwnLotsTest()
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");

            var auctionClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

            var lots = await auctionClient.GetOwnLots();
            if (lots != null && lots.Lots != null && lots.Lots.Count > 0)
            {
                foreach (var lot in lots.Lots)
                    FLogger.LogGood($"LotId: {lot.Id}, ItemName: {lot.ItemConfig.Name}, UnitPrice: {lot.UnitPrice}, Quantity: {lot.RemainingQuantity}/{lot.TotalQuantity}");
            }
            else
            {
                FLogger.LogGood("No lots found.");
            }
        }

        public async UniTask RefreshListingTest()
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var auctionClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

            try
            {
                var itemConfigs = await auctionClient.GetItemConfigs(new GetItemConfigsDto(
                categoryPathFilter: null,
                nameFilter: null,
                rarity: null,
                minGrade: null,
                maxGrade: null,
                page: 1,
                pageSize: 100
            ));

                if (itemConfigs != null && itemConfigs.ItemConfigs != null && itemConfigs.ItemConfigs.Count > 0)
                {
                    foreach (var item in itemConfigs.ItemConfigs)
                        FLogger.LogGood($"ItemConfigId: {item.Id}, ItemName: {item.Name}");
                }
                else
                {
                    FLogger.LogGood("No items found.");
                }
            }
            catch (Exception ex)
            {
                FLogger.LogError($"Failed to fetch item configs: {ex}");
                return;
            }
        }
    }
}