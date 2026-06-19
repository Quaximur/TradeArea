using System;
using Aucy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
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

            _loadableList.ListView.bindItem = (e, i) =>
            {
                var itemRow = e.Q<ItemConfigRow>(); // TODO: Replace with Element for ItemStack
                var ownLot = (OwnLotDto)_loadableList.ListView.itemsSource[i];
                var soldQuantity = ownLot.SoldQuantity;

                e.RegisterCallback<ClickEvent, Guid>(OnItemClicked, ownLot.Id);

                itemRow.ItemName = $"{ownLot.ItemConfig.Name} | Remaining: {ownLot.TotalQuantity} | Sold: {soldQuantity} | Price: {ownLot.UnitPrice}$ | Collect: ({soldQuantity * ownLot.UnitPrice} + {ownLot.ExtraSoldoutMoney})$";
            };

             _loadableList.ListView.unbindItem = (e, i) =>
            {
                e.UnregisterCallback<ClickEvent, Guid>(OnItemClicked);
            };

            viewModel.OwnLots.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }

        private void OnItemClicked(ClickEvent _, Guid lotId)
        {
            // Получаем ViewModel через поле класса или другим способом
            ViewModel.TestRemoveLot(lotId).Forget();
        }
    }
}