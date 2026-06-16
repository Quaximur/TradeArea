using Aucy.Contracts.OnionClient;
using DevKit.UI.MVVM;
using R3;
using TradeArea.UI.Custom;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionLots
{
    [UxmlElement]
    public partial class AuctionLotsWidget : ToolkitWidget<AuctionLotsViewModel>
    {
        [UxmlAttribute] private string _loadableListViewWidgetName = null;
        
        private LoadableListViewWidget _loadableList;

        private CompositeDisposable _disposables;
        
        protected override void OnInit()
        {
            _loadableList = Root.Q<LoadableListViewWidget>(_loadableListViewWidgetName);
            _loadableList.Init();
        }

        protected override void OnBind(AuctionLotsViewModel viewModel)
        {
            _disposables = new();

            _loadableList.ListView.bindItem = (x, i) =>
            {
                var itemRow = x.Q<ItemConfigRow>(); // TODO: Replace with Element for ItemStack
                var ownLot = (OwnLotDto)_loadableList.ListView.itemsSource[i];
                itemRow.ItemName = $"{ownLot.ItemConfig.Name} ({ownLot.RemainingQuantity}/{ownLot.TotalQuantity}) | {ownLot.UnitPrice} $";
            };

            viewModel.OwnLots.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }
    }
}