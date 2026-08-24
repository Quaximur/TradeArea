using TradeArea.Data;
using Cysharp.Threading.Tasks;
using TradeArea.Shared;
using Aucy.Contracts.OnionClient;
using System.Collections.Generic;
using MagicOnion;
using MagicOnion.Client;
using Grpc.Core;
using DevKit.Utils;
using System.Threading;
using System;
using TradeArea.Repositories;
using TradeArea.Data.Items;

namespace TradeArea
{
    public class OrderHubService : IDisposable
    {
        public IOrderHub OrderHub { get; private set; }

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;
        private readonly IItemConfigRepository _itemConfigRepository;
        private readonly GrpcChannelx _channel;

        private CancellationTokenSource _cts;

        public OrderHubService(AuctionModel auctionModel,
            UserModel userModel,
            IItemConfigRepository itemConfigRepository,
            GrpcChannelx channel)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
            _itemConfigRepository = itemConfigRepository;
            _channel = channel;
        }

        public void StartMessaging()
        {
            Disposes.ClearCts(ref _cts);
            _cts = new();

            StartMessagingAsync().Forget();

            async UniTaskVoid StartMessagingAsync()
            {
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        _auctionModel.OnMoneyChanged.Value = LoadingState.Loading<long>();
                        _auctionModel.OnUsedWeightChanged.Value = LoadingState.Loading<float>();
                        _auctionModel.OnWeightLimitChanged.Value = LoadingState.Loading<float>();
                        _auctionModel.OnUsedCapacityChanged.Value = LoadingState.Loading<int>();
                        _auctionModel.OnCapacityLimitChanged.Value = LoadingState.Loading<int>();
                        _auctionModel.OnInventoryChanged.Value = LoadingState.Loading<List<ItemStack>>();
                        _auctionModel.OnOwnLotsChanged.Value = LoadingState.Loading<List<OwnLot>>();
                        _auctionModel.OnOwnBidsChanged.Value = LoadingState.Loading<List<OwnBid>>();

                        var receiver = new OrderHubReceiver(_auctionModel, _itemConfigRepository);
                        OrderHub = await StreamingHubClient.ConnectAsync<IOrderHub, IOrderHubReceiver>(
                            _channel, 
                            receiver,
                            cancellationToken: _cts.Token,
                            option: new CallOptions().WithHeaders(GetAuthHeader())
                        ).AsUniTask();

                        var disconnect = await OrderHub.WaitForDisconnectAsync()
                            .AsUniTask()
                            .AttachExternalCancellation(_cts.Token);

                        if (disconnect.Type == DisconnectionType.CompletedNormally)
                        {
                            FLogger.LogGood<OrderHubService>("DisconnectionType.CompletedNormally");
                            return; // intentional disconnect, stop retrying
                        }

                        FLogger.LogWarning<OrderHubService>($"Disconnected: {disconnect.Type}");
                    }
                    catch (OperationCanceledException)
                    {
                        return; // _cts was cancelled, exit cleanly
                    }
                    catch (Exception ex)
                    {
                        FLogger.LogError<OrderHubService>($"{nameof(StartMessagingAsync)}: {ex.Message}");

                        _auctionModel.OnMoneyChanged.Value = LoadingState.Fail<long>();
                        _auctionModel.OnInventoryChanged.Value = LoadingState.Fail<List<ItemStack>>();
                        _auctionModel.OnOwnLotsChanged.Value = LoadingState.Fail<List<OwnLot>>();
                        _auctionModel.OnOwnBidsChanged.Value = LoadingState.Fail<List<OwnBid>>();
                        _auctionModel.OnUsedWeightChanged.Value = LoadingState.Fail<float>();
                        _auctionModel.OnWeightLimitChanged.Value = LoadingState.Fail<float>();
                        _auctionModel.OnUsedCapacityChanged.Value = LoadingState.Fail<int>();
                        _auctionModel.OnCapacityLimitChanged.Value = LoadingState.Fail<int>();
                    }

                    // Wait before retrying (won't delay if token is cancelled)
                    if (_cts != null)
                        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: _cts.Token)
                            .SuppressCancellationThrow();
                }
            }
        }

        private Metadata GetAuthHeader()
        {
            return new Metadata { { "authorization", $"Bearer {_userModel.OnTokenChanged.Value}" } };
        }

        public void Dispose()
        {
            Disposes.ClearDispose(ref _cts);
        }
    }
}