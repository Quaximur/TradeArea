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

namespace TradeArea
{
    public class OrderHubService : IDisposable
    {
        public IOrderHub OrderHub { get; private set; }

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;
        private CancellationTokenSource _cts;
        private GrpcChannelx _channel = null;

        public OrderHubService(AuctionModel auctionModel, UserModel userModel)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
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
                        _auctionModel.OnInventoryChanged.Value = LoadingState.Loading<List<ItemStackDto>>();
                        _auctionModel.OnOwnLotsChanged.Value = LoadingState.Loading<List<OwnLotDto>>();
                        _auctionModel.OnOwnBidsChanged.Value = LoadingState.Loading<List<OwnBidDto>>();

                        Disposes.ClearDispose(ref _channel);
                        _channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                        
                        var receiver = new OrderHubReceiver(_auctionModel);
                        OrderHub = await StreamingHubClient.ConnectAsync<IOrderHub, IOrderHubReceiver>(
                            _channel, receiver,
                            cancellationToken: _cts.Token,
                            option: new CallOptions().WithHeaders(GetAuthHeader())).AsUniTask();

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

                        _auctionModel.OnInventoryChanged.Value = LoadingState.Fail<List<ItemStackDto>>();
                        _auctionModel.OnOwnLotsChanged.Value = LoadingState.Fail<List<OwnLotDto>>();
                        _auctionModel.OnOwnBidsChanged.Value = LoadingState.Fail<List<OwnBidDto>>();
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
            Disposes.ClearDispose(ref _channel);
            Disposes.ClearDispose(ref _cts);
        }
    }
}