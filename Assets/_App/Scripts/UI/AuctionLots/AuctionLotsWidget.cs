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
                var ownLotRow = e.Q<OwnLotRow>();
                var ownLot = (OwnLotDto)_loadableList.ListView.itemsSource[i];
                var soldQuantity = ownLot?.SoldQuantity ?? 0;

                ownLotRow.CollectButton.RegisterCallback<ClickEvent, Guid>(OnCollectClicked, ownLot.Id);
                ownLotRow.RemoveButton.RegisterCallback<ClickEvent, Guid>(OnRemoveClicked, ownLot.Id);

                ownLotRow.ItemName = ownLot.ItemConfig.Name;
                // itemRow.ItemIcon = ownLot.ItemConfig.Icon;

                ownLotRow.SetStats(
                    soldQuantity: soldQuantity.ToString(),
                    totalQuantity: ownLot.TotalQuantity.ToString(),
                    unitPrice: ownLot.UnitPrice.ToString(),
                    extraSoldoutMoney: ownLot.ExtraSoldoutMoney.ToString(),
                    revenue: (soldQuantity * ownLot.UnitPrice + ownLot.ExtraSoldoutMoney).ToString()
                );

                ownLotRow.SetCollectButtonEnabled(soldQuantity > 0);
                ownLotRow.SetRemoveButtonEnabled(soldQuantity < ownLot.TotalQuantity);
            };

            _loadableList.ListView.unbindItem = (e, i) =>
           {
               var ownLotRow = e?.Q<OwnLotRow>();
               ownLotRow?.CollectButton?.UnregisterCallback<ClickEvent, Guid>(OnCollectClicked);
               ownLotRow?.RemoveButton?.UnregisterCallback<ClickEvent, Guid>(OnRemoveClicked);
           };

            viewModel.OwnLots.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }

        private void OnRemoveClicked(ClickEvent _, Guid lotId)
        {
            // Получаем ViewModel через поле класса или другим способом
            ViewModel.TestRemoveLot(lotId).Forget();
        }

        private void OnCollectClicked(ClickEvent _, Guid lotId)
        {
            // Получаем ViewModel через поле класса или другим способом
            ViewModel.CollectLot(lotId).Forget();
        }
    }
}