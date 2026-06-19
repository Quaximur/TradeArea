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
        private CancellationTokenSource _cts = new();
        private GrpcChannelx _channel = null;

        public OrderHubService(AuctionModel auctionModel, UserModel userModel)
        {
            _auctionModel = auctionModel;
            _userModel = userModel;
        }

        public void StartMessaging()
        {
            StartMessagingAsync().Forget();

            async UniTaskVoid StartMessagingAsync()
            {
                try
                {
                    while (true)
                    {
                        _auctionModel.OnInventoryChanged.Value = LoadingState.Loading<List<ItemStackDto>>();
                        _auctionModel.OnOwnLotsChanged.Value = LoadingState.Loading<List<OwnLotDto>>();
                        _auctionModel.OnOwnBidsChanged.Value = LoadingState.Loading<List<OwnBidDto>>();

                        _channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                        var receiver = new OrderHubReceiver(_auctionModel);
                        OrderHub = await StreamingHubClient.ConnectAsync<IOrderHub, IOrderHubReceiver>(_channel, receiver,
                            cancellationToken: _cts.Token,
                            option: new CallOptions().WithHeaders(GetAuthHeader())).AsUniTask();

                        var disconnect = await OrderHub.WaitForDisconnectAsync()
                            .AsUniTask()
                            .AttachExternalCancellation(_cts.Token);

                        if (disconnect.Type == DisconnectionType.CompletedNormally)
                        {
                            FLogger.LogGood<OrderHubService>("DisconnectionType.CompletedNormally");
                            break;
                        }
                        else
                        {
                            FLogger.LogWarning<OrderHubService>(disconnect.Type.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    FLogger.LogError<OrderHubService>($"{nameof(StartMessagingAsync)}: {ex.Message}");

                    _auctionModel.OnInventoryChanged.Value = LoadingState.Fail<List<ItemStackDto>>();
                    _auctionModel.OnOwnLotsChanged.Value = LoadingState.Fail<List<OwnLotDto>>();
                    _auctionModel.OnOwnBidsChanged.Value = LoadingState.Fail<List<OwnBidDto>>();
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