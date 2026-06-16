using Aucy.Contracts.OnionClient;
using DevKit.UI.MVVM;
using R3;
using TradeArea.UI.Custom;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionBids
{
    [UxmlElement]
    public partial class AuctionBidsWidget : ToolkitWidget<AuctionBidsViewModel>
    {
        [UxmlAttribute] private string _loadableListViewWidgetName = null;

        private LoadableListViewWidget _loadableList;

        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            _loadableList = Root.Q<LoadableListViewWidget>(_loadableListViewWidgetName);
            _loadableList.Init();
        }

        protected override void OnBind(AuctionBidsViewModel viewModel)
        {
            _disposables = new();

            _loadableList.ListView.bindItem = (x, i) =>
            {
                var itemRow = x.Q<ItemConfigRow>(); // TODO: Replace with Element for ItemStack
                itemRow.ItemName = ((OwnBidDto)_loadableList.ListView.itemsSource[i]).ItemConfig.Name;
            };

            viewModel.OwnBids.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }
    }
}