using System;
using System.Collections.Generic;
using Aucy.Contracts.OnionClient;
using Aucy.Contracts.OnionServer;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        public const int User1Index = 0;
        public const int User2Index = 1;

        private const string TokenUser1 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMWMyZjJmNzEtYWEwZi00NmYxLTgzOWYtOTc2NGEzNGI0Zjc2IiwiZXhwIjoxNzgxMjQ1MjEzfQ.8JeaYba-Tphlhl-W0lPQWczjsSb21XZ7SE7DGzzMcrk";
        private const string TokenUser2 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMmMyZjJmNzEtYWEwZi00NmYxLTgzOWYtOTc2NGEzNGI0Zjc2IiwiZXhwIjoxNzgxMjQ1MjMxfQ.2uX07jRLRns9g3cvXrMO5_jtmU1zzaKakRbEyhTSOS8";
        private const string UserId1 = "1c2f2f71-aa0f-46f1-839f-9764a34b4f76";
        private const string UserId2 = "2c2f2f71-aa0f-46f1-839f-9764a34b4f76";

        private readonly UserProfile[] _users =
        {
            new("User 1", TokenUser1, Guid.Parse(UserId1)),
            new("User 2", TokenUser2, Guid.Parse(UserId2))
        };

        public IReadOnlyList<UserProfile> Users => _users;

        public event Action<string> StatusChanged;
        public event Action DataChanged;
        private bool _isInitialized;
        private bool _isInitializing;

        public AuctionViewModel()
        {
            InitAsync().Forget();
        }

        public async UniTask EnsureReady()
        {
            await InitAsync();
        }

        public async UniTask<InventoryDto> GetInventory(int userIndex)
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var itemSearchClient = MagicOnionClient.Create<IItemSearchService>(channel)
                .WithHeaders(Headers(userIndex));

            return await itemSearchClient.GetOwnInventory(new GetInventoryDto
            {
                Page = 1,
                PageSize = 1000
            });
        }

        public async UniTask<List<ItemConfigWithRichListing>> GetMarketItems()
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var searchClient = MagicOnionClient.Create<IItemSearchService>(channel)
                .WithHeaders(Headers(User1Index));

            var configs = await searchClient.GetItemConfigs(new GetItemConfigsDto(
                categoryPathFilter: null,
                nameFilter: null,
                rarity: null,
                minGrade: null,
                maxGrade: null,
                page: 1,
                pageSize: 100));

            var result = new List<ItemConfigWithRichListing>();
            if (configs?.ItemConfigs == null)
                return result;

            foreach (var config in configs.ItemConfigs)
            {
                try
                {
                    var richConfig = await searchClient.GetRichItemConfig(new GetItemConfigDto(config.Id));
                    if (richConfig != null)
                        result.Add(richConfig);
                }
                catch (Exception ex)
                {
                    FLogger.LogWarning($"Failed to fetch rich item config for {config.Name}: {ex.Message}");
                }
            }

            return result;
        }

        public async UniTask<OwnLotsDto> GetOwnLots(int userIndex)
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                .WithHeaders(Headers(userIndex));

            return await auctionClient.GetOwnLots();
        }

        public async UniTask CreateLot(int userIndex, Guid itemConfigId, long unitPrice, int quantity)
        {
            if (unitPrice <= 0 || quantity <= 0)
            {
                SetStatus("Unit price and quantity must be greater than zero.");
                return;
            }

            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                .WithHeaders(Headers(userIndex));

            var lot = await auctionClient.CreateLot(new CreateLotDto(itemConfigId, unitPrice, quantity));
            SetStatus($"{_users[userIndex].Name} listed {lot.RemainingQuantity} item(s) at {lot.UnitPrice}.");
            DataChanged?.Invoke();
        }

        public async UniTask BuyLot(int buyerUserIndex, Guid lotId, int quantity)
        {
            if (quantity <= 0)
            {
                SetStatus("Purchase quantity must be greater than zero.");
                return;
            }

            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                .WithHeaders(Headers(buyerUserIndex));

            await auctionClient.CollectLot(new CollectLotDto(lotId, quantity));
            SetStatus($"{_users[buyerUserIndex].Name} purchased {quantity} item(s).");
            DataChanged?.Invoke();
        }

        public async UniTask AddFunds(int userIndex, long amount)
        {
            if (amount <= 0)
            {
                SetStatus("Funds amount must be greater than zero.");
                return;
            }

            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                .WithHeaders(Headers(userIndex));

            await inventoryClient.AddMoney(amount);
            SetStatus($"Added {amount} funds to {_users[userIndex].Name}.");
            DataChanged?.Invoke();
        }

        public async UniTask WithdrawFunds(int userIndex, long amount)
        {
            if (amount <= 0)
            {
                SetStatus("Funds amount must be greater than zero.");
                return;
            }

            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                .WithHeaders(Headers(userIndex));

            await inventoryClient.RemoveMoney(amount);
            SetStatus($"Withdrew {amount} funds from {_users[userIndex].Name}.");
            DataChanged?.Invoke();
        }

        private async UniTask InitAsync()
        {
            if (_isInitialized)
                return;

            if (_isInitializing)
            {
                while (_isInitializing)
                    await UniTask.Yield();

                return;
            }

            _isInitializing = true;
            try
            {
                await UniTask.WhenAll(CreateInventory(User1Index), CreateInventory(User2Index));
                _isInitialized = true;
                SetStatus("Auction tester ready.");
                DataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                SetStatus($"Initialization failed: {ex.Message}");
                FLogger.LogError($"Auction tester initialization failed: {ex}");
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private async UniTask CreateInventory(int userIndex)
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var inventoryClient = MagicOnionClient.Create<IInventoryService>(channel)
                .WithHeaders(Headers(userIndex));

            var inventoryDto = await inventoryClient.CreateInventory(new CreateInventoryDto
            {
                Capacity = 1000,
                WeightLimit = 10000
            });

            await inventoryClient.AddItemStack(new AddItemStackDto
            {
                ItemConfig = new ItemConfigDto(
                id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
                name: "bread",
                isUnary: false,
                icon: "bread",
                grade: null,
                rarity: 0,
                weight: 0.1f,
                baseType: "consumable",
                extra: null,
                categoryPath: "food"),
                Quantity = Random.Range(500, 1001)
            });

            FLogger.LogGood($"{_users[userIndex].Name} inventory ready (Capacity: {inventoryDto.Capacity}, WeightLimit: {inventoryDto.WeightLimit}).");
        }

        private Metadata Headers(int userIndex)
        {
            return new Metadata { { "authorization", $"Bearer {_users[userIndex].Token}" } };
        }

        private void SetStatus(string message)
        {
            StatusChanged?.Invoke(message);
            FLogger.Log(message);
        }

        public readonly struct UserProfile
        {
            public string Name { get; }
            public string Token { get; }
            public Guid UserId { get; }

            public UserProfile(string name, string token, Guid userId)
            {
                Name = name;
                Token = token;
                UserId = userId;
            }
        }
    }
}
