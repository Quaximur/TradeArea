using System;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
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
        [UxmlAttribute] private int _lotMarginTop = 4;
        [UxmlAttribute] private int _lotPaddingTop = 4;

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

            _loadableList.ListView.makeItem = () =>
            {
                var row = new ItemConfigRow();
                return row;
            };

            _loadableList.ListView.bindItem = (e, i) =>
            {
                var itemRow = (ItemConfigRow)e; // TODO: Replace with Element for ItemStack
                itemRow.style.marginTop = _lotMarginTop;
                itemRow.style.paddingTop = _lotPaddingTop;

                var itemStack = (ItemStackDto)_loadableList.ListView.itemsSource[i];

                e.RegisterCallback<ClickEvent, Guid>(OnItemClicked, itemStack.ItemConfig.Id);

                itemRow.ItemName = $"{itemStack.ItemConfig.Name} {itemStack.Quantity}";
            };

            _loadableList.ListView.unbindItem = (e, i) =>
            {
                e.UnregisterCallback<ClickEvent, Guid>(OnItemClicked);
            };

            viewModel.ItemStacks.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }

        private void OnItemClicked(ClickEvent _, Guid itemId)
        {
            // Получаем ViewModel через поле класса или другим способом
            ViewModel.TestSell(itemId).Forget();
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}