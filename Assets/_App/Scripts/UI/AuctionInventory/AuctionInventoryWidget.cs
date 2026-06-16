using Aucy.Contracts.OnionClient;
using DevKit.UI.MVVM;
using DevKit.Utils;
using R3;
using TradeArea.UI.Custom;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionInventory
{
    [UxmlElement]
    public partial class AuctionInventoryWidget : ToolkitWidget<AuctionInventoryViewModel>
    {
        [UxmlAttribute] private string _loadableListViewWidgetName = null;

        private LoadableListViewWidget _loadableList;

        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            _loadableList = Root.Q<LoadableListViewWidget>(_loadableListViewWidgetName);
            _loadableList.Init();
        }

        protected override void OnBind(AuctionInventoryViewModel viewModel)
        {
            _disposables = new();

            _loadableList.ListView.bindItem = (x, i) =>
            {
                var itemRow = x.Q<ItemConfigRow>(); // TODO: Replace with Element for ItemStack
                var itemStack = (ItemStackDto)_loadableList.ListView.itemsSource[i];
                
                itemRow.ItemName = $"{itemStack.ItemConfig.Name} {itemStack.Quantity}";
            };

            viewModel.ItemStacks.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}