using System;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using R3;
using TradeArea.Data.Items;
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

        public override void Show()
        {
            base.Show();
            _loadableList?.ListView.ScrollToItem(0);
        }

        protected override void OnInit()
        {
            _loadableList = Root.Q<LoadableListViewWidget>(_loadableListViewWidgetName);
            _loadableList.Init();
        }

        protected override void OnBind(AuctionLotsViewModel viewModel)
        {
            _disposables = new();

            _loadableList.ListView.makeItem = () =>
            {
                var row = new OwnLotRow();
                return row;
            };

            _loadableList.ListView.bindItem = (e, i) =>
            {
                var ownLotRow = (OwnLotRow)e;

                var sourceItem = _loadableList.ListView.itemsSource[i];
                if (sourceItem == null)
                {
                    FLogger.LogWarning<AuctionLotsWidget>($"OwnLot is null at index {i}");
                    ownLotRow.SetStub();

                    return;
                }

                var ownLot = (OwnLot)_loadableList.ListView.itemsSource[i];
                var soldQuantity = ownLot.SoldQuantity;

                if (ownLot.ItemConfig == null)
                {
                    FLogger.LogWarning<AuctionLotsWidget>($"ItemConfig is null for OwnLot at index {i}");
                    ownLotRow.SetStub();

                    ownLotRow.SetStats(
                        soldQuantity: soldQuantity.ToString(),
                        totalQuantity: ownLot.TotalQuantity.ToString(),
                        unitPrice: ownLot.UnitPrice.ToString(),
                        extraSoldoutMoney: ownLot.ExtraSoldoutMoney.ToString(),
                        revenue: (soldQuantity * ownLot.UnitPrice + ownLot.ExtraSoldoutMoney).ToString()
                    );

                    return;
                }

                ownLotRow.CollectButton.RegisterCallback<ClickEvent, Guid>(OnCollectClicked, ownLot.Id);
                ownLotRow.RemoveButton.RegisterCallback<ClickEvent, Guid>(OnRemoveClicked, ownLot.Id);

                ownLotRow.ItemName = ownLot.ItemConfig.Name;
                ownLotRow.ItemIcon = ownLot.ItemConfig.Icon;
                ownLotRow.Rarity = ownLot.ItemConfig.Rarity;
                ownLotRow.Grade = ownLot.ItemConfig.Grade;

                ownLotRow.SetStats(
                    soldQuantity: soldQuantity.ToString(),
                    totalQuantity: ownLot.TotalQuantity.ToString(),
                    unitPrice: ownLot.UnitPrice.ToString(),
                    extraSoldoutMoney: ownLot.ExtraSoldoutMoney.ToString(),
                    revenue: (soldQuantity * ownLot.UnitPrice + ownLot.ExtraSoldoutMoney).ToString()
                );

                ownLotRow.SetCollectButtonEnabled(soldQuantity > 0);
                ownLotRow.SetRemoveButtonEnabled(ownLot.RemainingQuantity > 0);
            };

            _loadableList.ListView.unbindItem = (e, i) =>
           {
               var ownLotRow = (OwnLotRow)e;
               ownLotRow.CollectButton?.UnregisterCallback<ClickEvent, Guid>(OnCollectClicked);
               ownLotRow.RemoveButton?.UnregisterCallback<ClickEvent, Guid>(OnRemoveClicked);
           };

            viewModel.OwnLots.Subscribe(_loadableList.OnLoadingStateChanged)
                .AddTo(_disposables);
        }

        private void OnRemoveClicked(ClickEvent _, Guid lotId)
        {
            ViewModel.RemoveLotAsync(lotId).Forget();
        }

        private void OnCollectClicked(ClickEvent _, Guid lotId)
        {
            ViewModel.CollectLotAsync(lotId).Forget();
        }
    }
}