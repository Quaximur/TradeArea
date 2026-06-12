using System;
using System.Globalization;
using Aucy.Contracts.OnionClient;
using Aucy.Contracts.OnionServer;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace TradeArea.UI.Auction
{
    public class AuctionView : AttachableToolkitWindow<AuctionViewModel>
    {
        [SerializeField] private string _statusLabelName = "StatusLabel";
        [SerializeField] private string _refreshMarketName = "RefreshMarket";
        [SerializeField] private string _marketListName = "MarketList";

        private Label _statusLabel;
        private Button _refreshMarketButton;
        private ScrollView _marketList;
        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            base.OnInit();

            _statusLabel = Root.Q<Label>(_statusLabelName);
            _refreshMarketButton = Root.Q<Button>(_refreshMarketName);
            _marketList = Root.Q<ScrollView>(_marketListName);
        }

        protected override void OnBind(AuctionViewModel viewModel)
        {
            base.OnBind(viewModel);

            _disposables = new();
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}
