using System;
using System.Threading;
using Aucy.Contracts.OnionClient;
using Aucy.Contracts.OnionServer;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using ObservableCollections;
using R3;
using TradeArea.Data;
using TradeArea.Shared;

namespace TradeArea.UI.AuctionLots
{
    public class AuctionLotsViewModel : BaseViewModel
    {
        private readonly UserModel _userModel;

        private CancellationTokenSource _cts = new();

        public AuctionLotsViewModel(UserModel userModel)
        {
            _userModel = userModel;
        }

        public async UniTask<OwnLotsDto> GetOwnLotsAsync()
        {
            using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
            var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                .WithHeaders(GetAuthHeader())
                .WithCancellationToken(_cts.Token);

            return await auctionClient.GetOwnLots();
        }

        public async UniTask CreateLotAsync(Guid itemConfigId, long unitPrice, int quantity)
        {
            if (unitPrice <= 0 || quantity <= 0)
            {
                FLogger.Log<AuctionLotsViewModel>("Unit price and quantity must be greater than zero.");
                return;
            }

            try
            {
                using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                    .WithHeaders(GetAuthHeader())
                    .WithCancellationToken(_cts.Token);

                var lot = await auctionClient.CreateLot(new CreateLotDto(itemConfigId, unitPrice, quantity));
                FLogger.Log<AuctionLotsViewModel>($"{_userModel.OnNameChanged.Value} listed {lot.RemainingQuantity} item(s) at {lot.UnitPrice}.");
            }
            catch (Exception ex)
            {
                FLogger.LogError<AuctionLotsViewModel>($"{nameof(CreateLotAsync)}: {ex.Message}");
            }
        }

        public async UniTask CollectLotAsync(Guid lotId, int quantity)
        {
            if (quantity <= 0)
            {
                FLogger.Log<AuctionLotsViewModel>("Purchase quantity must be greater than zero.");
                return;
            }

            try
            {
                using var channel = GrpcChannelx.ForAddress(Constants.AUCY_URL);
                var auctionClient = MagicOnionClient.Create<IOrderBookService>(channel)
                    .WithHeaders(GetAuthHeader())
                    .WithCancellationToken(_cts.Token);

                await auctionClient.CollectLot(new CollectLotDto(lotId, quantity));
                FLogger.Log<AuctionLotsViewModel>($"{_userModel.OnNameChanged.Value} purchased {quantity} item(s).");
            }
            catch (Exception ex)
            {
                FLogger.LogError<AuctionLotsViewModel>($"{nameof(CollectLotAsync)}: {ex.Message}");
            }
        }

        private Metadata GetAuthHeader()
        {
            return new Metadata { { "authorization", $"Bearer {_userModel.OnTokenChanged.Value}" } };
        }

        public override void Dispose()
        {
            Disposes.ClearCts(ref _cts);
            base.Dispose();
        }
    }
}