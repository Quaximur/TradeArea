using System;
using System.Collections.Generic;
using System.Threading;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using R3;
using TradeArea.Data;
using TradeArea.Shared;
using TradeArea.UI.AuctionInventory;
using TradeArea.UI.AuctionSearch;
using TradeArea.UI.AuctionOrders;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        public AuctionSearchViewModel AuctionSearchViewModel { get; private set; }
        public AuctionInventoryViewModel AuctionInventoryViewModel { get; private set; }
        public AuctionOrdersViewModel AuctionOrdersViewModel { get; private set; }

        private CancellationTokenSource _cts = new();

        public AuctionViewModel(AuctionSearchViewModel auctionSearchViewModel,
            AuctionInventoryViewModel auctionInventoryViewModel,
            AuctionOrdersViewModel auctionOrdersViewModel)
        {
            AuctionOrdersViewModel = auctionOrdersViewModel;
            AuctionInventoryViewModel = auctionInventoryViewModel;
            AuctionSearchViewModel = auctionSearchViewModel;
        }

        public override void Dispose()
        {
            Disposes.ClearCts(ref _cts);

            AuctionSearchViewModel?.Dispose();
            AuctionInventoryViewModel?.Dispose();
            AuctionOrdersViewModel?.Dispose();
            
            base.Dispose();
        }
    }
}
