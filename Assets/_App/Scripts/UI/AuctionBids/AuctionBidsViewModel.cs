using System;
using System.Collections.Generic;
using System.Threading;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using Grpc.Core;
using R3;
using TradeArea.Data;
using TradeArea.Shared;

namespace TradeArea.UI.AuctionBids
{
    public class AuctionBidsViewModel : BaseViewModel
    {
        public Observable<LoadingState<List<OwnBidDto>>> OwnBids => _auctionModel.OnOwnBidsChanged;

        private readonly AuctionModel _auctionModel;
        private readonly UserModel _userModel;

        private CancellationTokenSource _cts = new();

        public AuctionBidsViewModel(UserModel userModel, AuctionModel auctionModel)
        {
            _userModel = userModel;
            _auctionModel = auctionModel;
        }

        public async UniTask RemoveBid(Guid bidId)
        {
        }

        public async UniTask CollectBid(Guid bidId)
        {
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