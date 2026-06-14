using DevKit.UI.MVVM;
using UnityEngine.UIElements;
using R3;
using DevKit.Utils;

namespace TradeArea.UI.AuctionSearch
{
    [UxmlElement]
    public partial class AuctionSearchWidget : ToolkitWidget<AuctionSearchViewModel>
    {
        [UxmlAttribute] private string _refreshMarketButtonName = "RefreshMarket";
        [UxmlAttribute] private string _marketListName = "MarketList";

        private Button _refreshMarketButton;
        private ListView _marketList;
        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            _refreshMarketButton = Root.Q<Button>(_refreshMarketButtonName);
            _marketList = Root.Q<ListView>(_marketListName);
            _marketList.bindItem = (x, i) =>
            {
                // TODO: cast to custom Element
                var itemName = x.Q<Label>("Name");
                itemName.text = "Item name binding";
            };
        }

        protected override void OnBind(AuctionSearchViewModel viewModel)
        {
            _disposables = new();
            viewModel.OnMarketListChanged.Subscribe(x =>
            {

            }).AddTo(_disposables);
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}