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
        [SerializeField] private string _createLotButtonName = "CreateLot";
        [SerializeField] private string _refreshListingButtonName = "RefreshListing";

        private Button _createLotButton;
        private Button _refreshListingButton;

        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            base.OnInit();

            _createLotButton = Root.Q<Button>(name: _createLotButtonName);
            _refreshListingButton = Root.Q<Button>(name: _refreshListingButtonName);
        }

        protected override void OnBind(AuctionViewModel viewModel)
        {
            base.OnBind(viewModel);

            _disposables = new();

            _createLotButton.SubscribeCallback<ClickEvent>(_ => viewModel.CreateLotTest().Forget())
                .AddTo(_disposables);
            _refreshListingButton.SubscribeCallback<ClickEvent>(_ => viewModel.RefreshListingTest().Forget())
                .AddTo(_disposables);   
        }

        public override void Dispose()
        {
            base.Dispose();
            Disposes.ClearDispose(ref _disposables);
        }
    }
}