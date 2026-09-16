using Aucy.Contracts.OnionClient;
using Authy.Contracts.OnionClient;
using Cysharp.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeArea.Shared;
using UnityEngine;

public sealed class TradingHouseSimulationClient : MonoBehaviour
{
    [SerializeField] private string serverAddress = "http://localhost:17537";
    [SerializeField] private TextAsset usersJson;
    [SerializeField] private int maxConcurrentUsers = 128;
    [SerializeField] private int operationsPerUser = 20;

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

    private CancellationTokenSource runCts;

    private async void Start()
    {
        runCts = new CancellationTokenSource();
        var users = LoadUsers();
        await RunSimulationAsync(users, runCts.Token);
    }

    private void OnDestroy()
    {
        runCts?.Cancel();
        runCts?.Dispose();
    }

    private async Task RunSimulationAsync(IReadOnlyList<SimulationUser> users, CancellationToken cancellationToken)
    {
        using var gate = new SemaphoreSlim(maxConcurrentUsers);
        using var authyChannel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
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
            catch (Exception ex)
            {
                Debug.LogWarning($"Simulation user {user.name} failed: {ex.Message}");
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

        for (var i = 0; i < operationsPerUser; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (receiver.Inventory?.ItemStacks?.Length > 0 && random.NextDouble() < 0.5)
                await CreateLotAndWaitAsync(hub, receiver, random, cancellationToken);
            else
                await CreateBidAndWaitAsync(hub, receiver, random, cancellationToken);
        }

        await hub.DisposeAsync();
    }

    private static async Task CreateLotAndWaitAsync(
        IOrderHub hub,
        SimulationReceiver receiver,
        System.Random random,
        CancellationToken cancellationToken)
    {
        var stack = receiver.Inventory.ItemStacks[random.Next(receiver.Inventory.ItemStacks.Length)];
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
        System.Random random,
        CancellationToken cancellationToken)
    {
        var itemConfigId = ItemConfigIds[random.Next(ItemConfigIds.Length)];
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
        return GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
        {
            HttpHandler = new SubdirectoryHandler(new YetAnotherHttpHandler()
            {
                Http2Only = true,
            }, "/aucy"),
            DisposeHttpClient = true,
        });
    }

    private SimulationUser[] LoadUsers()
    {
        if (usersJson == null)
            throw new InvalidOperationException("Assign trading-simulation-users.json as a TextAsset.");

        var wrappedJson = "{\"users\":" + usersJson.text + "}";
        return JsonUtility.FromJson<SimulationUserList>(wrappedJson).users;
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

        public InventoryDto Inventory { get; private set; }

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
            Inventory = inventory;
            joinWait.TrySetResult((ownLots, ownBids, inventory));
        }

        public void OnLotCreated(LotCreatedDto lotCreated) => lotCreatedWait.TrySetResult(lotCreated);
        public void OnLotCollected(LotCollectedDto lotCollected) { }
        public void OnLotRemoved(LotRemovedDto lotRemoved) => lotRemovedWait.TrySetResult(lotRemoved);
        public void OnBidCreated(BidCreatedDto bidCreated) => bidCreatedWait.TrySetResult(bidCreated);
        public void OnBidCollected(BidCollectedDto bidCollected) { }
        public void OnBidRemoved(BidRemovedDto bidRemoved) => bidRemovedWait.TrySetResult(bidRemoved);
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
    }
}
