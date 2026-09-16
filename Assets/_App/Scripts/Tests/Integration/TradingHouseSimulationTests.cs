using Aucy.Contracts.OnionClient;
using Authy.Contracts.OnionClient;
using Cysharp.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeArea.Shared;

public sealed class TradingHouseSimulationTests
{
    private const string ServerAddress = "http://localhost:17500";
    private const string UsersResourceName = "trading-simulation-users";
    private const int MaxConcurrentUsers = 128;
    private const int OperationsPerUser = 20;

    private static readonly Guid[] ItemConfigIds =
    {
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Guid.Parse("00000000-0000-0000-0000-000000000002"),
        Guid.Parse("00000000-0000-0000-0000-000000000003"),
        Guid.Parse("00000000-0000-0000-0000-000000000004"),
        Guid.Parse("00000000-0000-0000-0000-000000000005"),
        Guid.Parse("00000000-0000-0000-0000-000000000006"),
        Guid.Parse("00000000-0000-0000-0000-000000000007"),
        Guid.Parse("00000000-0000-0000-0000-000000000008"),
        Guid.Parse("00000000-0000-0000-0000-000000000009")
    };

    [Test]
    public async Task SimulateTradingHouseAsync()
    {
        var users = LoadUsers();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        await RunSimulationAsync(users, cancellation.Token);
    }

    private async Task RunSimulationAsync(IReadOnlyList<SimulationUser> users, CancellationToken cancellationToken)
    {
        using var gate = new SemaphoreSlim(MaxConcurrentUsers);
        using var authyChannel = GrpcChannel.ForAddress(ServerAddress, new GrpcChannelOptions
        {
            HttpHandler = new SubdirectoryHandler(new YetAnotherHttpHandler()
            {
                Http2Only = true,
            }, "/authy"),
            DisposeHttpClient = true,
        });

        var authService = MagicOnionClient.Create<IAuthService>(authyChannel);

        var tasks = users.Select(async user =>
        {
            await gate.WaitAsync(cancellationToken);

            try
            {
                var loginRes = await authService.Login(new LoginDto() { Name = user.name, Password = user.password });
                await RunUserAsync(user, cancellationToken, loginRes.Token);
            }
            finally
            {
                gate.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task RunUserAsync(SimulationUser user, CancellationToken cancellationToken, string token)
    {
        using var channel = CreateAucyChannel();

        var receiver = new SimulationReceiver();
        var hub = await StreamingHubClient.ConnectAsync<IOrderHub, IOrderHubReceiver>(
            channel,
            receiver,
            cancellationToken: cancellationToken,
            option: new CallOptions().WithHeaders(new Metadata { { "authorization", $"Bearer {token}" } })
        );

        await receiver.WaitForJoinAsync(TimeSpan.FromSeconds(15), cancellationToken);

        var random = new System.Random(user.userId.GetHashCode());

        for (var i = 0; i < OperationsPerUser; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var buyableItemConfigIds = receiver.GetBuyableItemConfigIds();
            if (!receiver.HasSellableItems && buyableItemConfigIds.Count == 0)
                break;

            if (receiver.HasSellableItems && (buyableItemConfigIds.Count == 0 || random.NextDouble() < 0.5))
                await CreateLotAndWaitAsync(hub, receiver, random, cancellationToken);
            else
                await CreateBidAndWaitAsync(hub, receiver, buyableItemConfigIds, random, cancellationToken);
        }

        await hub.DisposeAsync();
    }

    private static async Task CreateLotAndWaitAsync(
        IOrderHub hub,
        SimulationReceiver receiver,
        System.Random random,
        CancellationToken cancellationToken)
    {
        var sellableItems = receiver.GetSellableItems();
        var stack = sellableItems[random.Next(sellableItems.Count)];
        var quantity = random.Next(1, Math.Min(5000, stack.Quantity) + 1);
        var price = RandomTierPrice(random);
        var wait = receiver.NextLotCreatedAsync(cancellationToken);

        await hub.CreateLot(new CreateLotDto(stack.ItemConfigId, price, quantity));
        var created = await wait;

        if (random.NextDouble() < 0.25)
        {
            var removeWait = receiver.NextLotRemovedAsync(cancellationToken);
            await hub.RemoveLot(new RemoveLotDto(created.LotId));
            await removeWait;
        }
    }

    private static async Task CreateBidAndWaitAsync(
        IOrderHub hub,
        SimulationReceiver receiver,
        IReadOnlyList<Guid> buyableItemConfigIds,
        System.Random random,
        CancellationToken cancellationToken)
    {
        var itemConfigId = buyableItemConfigIds[random.Next(buyableItemConfigIds.Count)];
        var price = RandomTierPrice(random);
        var quantity = random.Next(1, 5001);
        var wait = receiver.NextBidCreatedAsync(cancellationToken);

        await hub.CreateBid(new CreateBidDto(itemConfigId, price, quantity));
        var created = await wait;

        if (random.NextDouble() < 0.25)
        {
            var removeWait = receiver.NextBidRemovedAsync(cancellationToken);
            await hub.RemoveBid(new RemoveBidDto(created.BidId));
            await removeWait;
        }
    }

    private GrpcChannel CreateAucyChannel()
    {
        return GrpcChannel.ForAddress(ServerAddress, new GrpcChannelOptions
        {
            HttpHandler = new SubdirectoryHandler(new YetAnotherHttpHandler()
            {
                Http2Only = true,
            }, "/aucy"),
            DisposeHttpClient = true,
        });
    }

    private static SimulationUser[] LoadUsers()
    {
        var usersJson = UnityEngine.Resources.Load<UnityEngine.TextAsset>(UsersResourceName);
        if (usersJson == null)
            throw new InvalidOperationException($"Add {UsersResourceName}.json to a Resources folder.");

        var wrappedJson = "{\"users\":" + usersJson.text + "}";
        return UnityEngine.JsonUtility.FromJson<SimulationUserList>(wrappedJson).users;
    }

    private static long RandomTierPrice(System.Random random)
    {
        return 50 + random.Next(20) * 50;
    }

    [Serializable]
    private sealed class SimulationUserList
    {
        public SimulationUser[] users;
    }

    [Serializable]
    private sealed class SimulationUser
    {
        public string userId;
        public string name;
        public string password;
    }

    private sealed class SimulationReceiver : IOrderHubReceiver
    {
        private TaskCompletionSource<(OwnLotsDto Lots, OwnBidsDto Bids, InventoryDto Inventory)> joinWait =
            NewWait<(OwnLotsDto, OwnBidsDto, InventoryDto)>();

        private TaskCompletionSource<LotCreatedDto> lotCreatedWait = NewWait<LotCreatedDto>();
        private TaskCompletionSource<LotRemovedDto> lotRemovedWait = NewWait<LotRemovedDto>();
        private TaskCompletionSource<BidCreatedDto> bidCreatedWait = NewWait<BidCreatedDto>();
        private TaskCompletionSource<BidRemovedDto> bidRemovedWait = NewWait<BidRemovedDto>();

        private readonly Dictionary<Guid, int> inventory = new Dictionary<Guid, int>();
        private readonly Dictionary<Guid, int> ownLotItemCounts = new Dictionary<Guid, int>();
        private readonly Dictionary<Guid, int> ownBidItemCounts = new Dictionary<Guid, int>();
        private readonly Dictionary<Guid, Guid> bidItemConfigIds = new Dictionary<Guid, Guid>();

        public bool HasSellableItems => GetSellableItems().Count > 0;

        public async Task WaitForJoinAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var completed = await Task.WhenAny(joinWait.Task, Task.Delay(timeout, cancellationToken));
            if (completed != joinWait.Task)
                throw new TimeoutException("Timed out waiting for OrderHub OnJoin.");
        }

        public Task<LotCreatedDto> NextLotCreatedAsync(CancellationToken _) => ResetAndReturn(ref lotCreatedWait);
        public Task<LotRemovedDto> NextLotRemovedAsync(CancellationToken _) => ResetAndReturn(ref lotRemovedWait);
        public Task<BidCreatedDto> NextBidCreatedAsync(CancellationToken _) => ResetAndReturn(ref bidCreatedWait);
        public Task<BidRemovedDto> NextBidRemovedAsync(CancellationToken _) => ResetAndReturn(ref bidRemovedWait);

        public void OnJoin(OwnLotsDto ownLots, OwnBidsDto ownBids, InventoryDto inventory)
        {
            if (inventory?.ItemStacks != null)
            {
                foreach (var stack in inventory.ItemStacks)
                    AddQuantity(this.inventory, stack.ItemConfigId, stack.Quantity);
            }

            if (ownLots?.Lots != null)
            {
                foreach (var lot in ownLots.Lots)
                    AddQuantity(ownLotItemCounts, lot.ItemConfigId, 1);
            }

            if (ownBids?.OwnBids != null)
            {
                foreach (var bid in ownBids.OwnBids)
                {
                    AddQuantity(ownBidItemCounts, bid.ItemConfigId, 1);
                    bidItemConfigIds[bid.Id] = bid.ItemConfigId;
                }
            }

            joinWait.TrySetResult((ownLots, ownBids, inventory));
        }

        public void OnLotCreated(LotCreatedDto lotCreated)
        {
            AddQuantity(ownLotItemCounts, lotCreated.ItemConfigId, 1);
            AddQuantity(inventory, lotCreated.ItemConfigId, -lotCreated.TotalQuantity);
            lotCreatedWait.TrySetResult(lotCreated);
        }
        public void OnLotCollected(LotCollectedDto lotCollected) { }
        public void OnLotRemoved(LotRemovedDto lotRemoved)
        {
            AddQuantity(ownLotItemCounts, lotRemoved.ItemConfigId, -1);
            AddQuantity(inventory, lotRemoved.ItemConfigId, lotRemoved.RemovedQuantity);
            lotRemovedWait.TrySetResult(lotRemoved);
        }
        public void OnBidCreated(BidCreatedDto bidCreated)
        {
            AddQuantity(ownBidItemCounts, bidCreated.ItemConfigId, 1);
            bidItemConfigIds[bidCreated.BidId] = bidCreated.ItemConfigId;
            bidCreatedWait.TrySetResult(bidCreated);
        }
        public void OnBidCollected(BidCollectedDto bidCollected) { }
        public void OnBidRemoved(BidRemovedDto bidRemoved)
        {
            if (bidItemConfigIds.TryGetValue(bidRemoved.BidId, out var itemConfigId))
            {
                AddQuantity(ownBidItemCounts, itemConfigId, -1);
                bidItemConfigIds.Remove(bidRemoved.BidId);
            }
            bidRemovedWait.TrySetResult(bidRemoved);
        }
        public void OnSold(Guid lotId, int soldQuantity, long extraMoney) { }
        public void OnBought(Guid bidId, int boughtQuantity, long extraMoney) { }

        private static TaskCompletionSource<T> NewWait<T>()
        {
            return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private static Task<T> ResetAndReturn<T>(ref TaskCompletionSource<T> wait)
        {
            wait = NewWait<T>();
            return wait.Task;
        }

        public List<ItemStackDto> GetSellableItems()
        {
            return inventory
                .Where(pair => pair.Value > 0 && !ownLotItemCounts.ContainsKey(pair.Key))
                .Select(pair => new ItemStackDto(pair.Key, null, pair.Value))
                .ToList();
        }

        public List<Guid> GetBuyableItemConfigIds()
        {
            return ItemConfigIds
                .Where(itemConfigId => !ownLotItemCounts.ContainsKey(itemConfigId)
                    && !ownBidItemCounts.ContainsKey(itemConfigId))
                .ToList();
        }

        private static void AddQuantity(Dictionary<Guid, int> quantities, Guid itemConfigId, int amount)
        {
            quantities.TryGetValue(itemConfigId, out var quantity);
            quantity += amount;

            if (quantity > 0)
                quantities[itemConfigId] = quantity;
            else
                quantities.Remove(itemConfigId);
        }
    }
}
