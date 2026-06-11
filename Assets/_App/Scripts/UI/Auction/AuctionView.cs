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
        [SerializeField] private string _refreshAllButtonName = "RefreshAllButton";

        private Label _statusLabel;
        private Button _refreshAllButton;
        private readonly UserPanel[] _userPanels = new UserPanel[2];
        private ScrollView _marketList;
        private AuctionViewModel _viewModel;
        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            base.OnInit();

            _statusLabel = Root.Q<Label>(_statusLabelName);
            _refreshAllButton = Root.Q<Button>(_refreshAllButtonName);
            _marketList = Root.Q<ScrollView>("MarketList");
            _userPanels[AuctionViewModel.User1Index] = FindUserPanel(1);
            _userPanels[AuctionViewModel.User2Index] = FindUserPanel(2);
        }

        protected override void OnBind(AuctionViewModel viewModel)
        {
            base.OnBind(viewModel);

            _viewModel = viewModel;
            _disposables = new CompositeDisposable();
            _viewModel.StatusChanged += SetStatus;
            _viewModel.DataChanged += Refresh;

            _refreshAllButton.SubscribeCallback<ClickEvent>(_ => Refresh()).AddTo(_disposables);

            BindFundsControls(AuctionViewModel.User1Index);
            BindFundsControls(AuctionViewModel.User2Index);
            SetUserLabels();
            Refresh();
        }

        public override void Dispose()
        {
            if (_viewModel != null)
            {
                _viewModel.StatusChanged -= SetStatus;
                _viewModel.DataChanged -= Refresh;
                _viewModel = null;
            }

            base.Dispose();
            Disposes.ClearDispose(ref _disposables);
        }

        private void BindFundsControls(int userIndex)
        {
            var panel = _userPanels[userIndex];
            panel.AddFundsButton.SubscribeCallback<ClickEvent>(_ =>
            {
                RunAction(() => _viewModel.AddFunds(userIndex, ParseLong(panel.FundsAmountField.value, 0)));
            }).AddTo(_disposables);

            panel.WithdrawFundsButton.SubscribeCallback<ClickEvent>(_ =>
            {
                RunAction(() => _viewModel.WithdrawFunds(userIndex, ParseLong(panel.FundsAmountField.value, 0)));
            }).AddTo(_disposables);
        }

        private void SetUserLabels()
        {
            for (var i = 0; i < _userPanels.Length; i++)
            {
                var user = _viewModel.Users[i];
                _userPanels[i].NameLabel.text = user.Name;
                _userPanels[i].MetaLabel.text = $"Inventory {ShortGuid(user.UserId)}";
            }
        }

        private void Refresh()
        {
            RefreshAsync().Forget();
        }

        private async UniTask RefreshAsync()
        {
            try
            {
                SetStatus("Refreshing auction tester...");
                await _viewModel.EnsureReady();

                var user1Inventory = await _viewModel.GetInventory(AuctionViewModel.User1Index);
                var user2Inventory = await _viewModel.GetInventory(AuctionViewModel.User2Index);
                var user1Lots = await _viewModel.GetOwnLots(AuctionViewModel.User1Index);
                var user2Lots = await _viewModel.GetOwnLots(AuctionViewModel.User2Index);
                var marketItems = await _viewModel.GetMarketItems();

                RenderInventory(AuctionViewModel.User1Index, user1Inventory);
                RenderInventory(AuctionViewModel.User2Index, user2Inventory);
                RenderOwnLots(AuctionViewModel.User1Index, user1Lots);
                RenderOwnLots(AuctionViewModel.User2Index, user2Lots);
                RenderMarket(marketItems);

                SetStatus("Display refreshed.");
            }
            catch (Exception ex)
            {
                SetStatus($"Refresh failed: {ex.Message}");
                Debug.LogException(ex);
            }
        }

        private void RenderInventory(int userIndex, InventoryDto inventory)
        {
            var list = _userPanels[userIndex].InventoryList;
            list.Clear();

            var stacks = inventory?.ItemStacks;
            if (stacks == null || stacks.Length == 0)
            {
                AddEmpty(list, "No inventory items.");
                return;
            }

            foreach (var stack in stacks)
            {
                list.Add(CreateInventoryRow(userIndex, stack));
            }
        }

        private VisualElement CreateInventoryRow(int userIndex, ItemStackDto stack)
        {
            var row = CreateRow();
            row.Add(CreateIconSlot());

            var main = CreateMainBlock(
                stack.ItemConfig?.Name ?? "Unknown item",
                $"Qty {stack.Quantity} | {stack.ItemConfig?.BaseType ?? "item"} | {stack.ItemConfig?.CategoryPath ?? "uncategorized"}");
            row.Add(main);

            if (stack.ItemConfig == null)
            {
                return row;
            }

            var quantityField = CreateSmallInput("1");
            var priceField = CreateSmallInput("100");
            var listButton = CreateButton("List");
            listButton.RegisterCallback<ClickEvent>(_ =>
            {
                var quantity = ParseInt(quantityField.value, 0);
                var price = ParseLong(priceField.value, 0);
                RunAction(() => _viewModel.CreateLot(userIndex, stack.ItemConfig.Id, price, quantity));
            });

            var actions = CreateActions();
            actions.Add(quantityField);
            actions.Add(priceField);
            actions.Add(listButton);
            row.Add(actions);

            return row;
        }

        private void RenderOwnLots(int userIndex, OwnLotsDto ownLots)
        {
            var list = _userPanels[userIndex].OwnLotsList;
            list.Clear();

            if (ownLots?.Lots == null || ownLots.Lots.Count == 0)
            {
                AddEmpty(list, "No own lots.");
                return;
            }

            foreach (var lot in ownLots.Lots)
            {
                var row = CreateRow();
                row.Add(CreateIconSlot());
                row.Add(CreateMainBlock(
                    lot.ItemConfig?.Name ?? "Unknown item",
                    $"Price {lot.UnitPrice} | Remaining {lot.RemainingQuantity}/{lot.TotalQuantity} | Sold {lot.SoldQuantity}"));
                list.Add(row);
            }
        }

        private void RenderMarket(System.Collections.Generic.List<ItemConfigWithRichListing> items)
        {
            _marketList.Clear();

            if (items == null || items.Count == 0)
            {
                AddEmpty(_marketList, "No ItemConfig entries found.");
                return;
            }

            foreach (var item in items)
            {
                var lots = item.Lots;
                if (lots == null || lots.Length == 0)
                {
                    _marketList.Add(CreateMarketConfigRow(item, null));
                    continue;
                }

                foreach (var lot in lots)
                {
                    _marketList.Add(CreateMarketConfigRow(item, lot));
                }
            }
        }

        private VisualElement CreateMarketConfigRow(ItemConfigDto item, LotDto lot)
        {
            var row = CreateRow();
            row.Add(CreateIconSlot());

            var detail = lot == null
                ? "No active lots"
                : $"Price {lot.UnitPrice} | Remaining {lot.RemainingQuantity} | Seller {ShortGuid(lot.OwnerUserId)}";
            row.Add(CreateMainBlock(item.Name, detail));

            if (lot != null)
            {
                var quantityField = CreateSmallInput("1");
                var buyUser1Button = CreateButton("Buy U1");
                var buyUser2Button = CreateButton("Buy U2");

                buyUser1Button.SetEnabled(lot.OwnerUserId != _viewModel.Users[AuctionViewModel.User1Index].UserId);
                buyUser2Button.SetEnabled(lot.OwnerUserId != _viewModel.Users[AuctionViewModel.User2Index].UserId);

                buyUser1Button.RegisterCallback<ClickEvent>(_ =>
                {
                    RunAction(() => _viewModel.BuyLot(AuctionViewModel.User1Index, lot.Id, ParseInt(quantityField.value, 0)));
                });
                buyUser2Button.RegisterCallback<ClickEvent>(_ =>
                {
                    RunAction(() => _viewModel.BuyLot(AuctionViewModel.User2Index, lot.Id, ParseInt(quantityField.value, 0)));
                });

                var actions = CreateActions();
                actions.Add(quantityField);
                actions.Add(buyUser1Button);
                actions.Add(buyUser2Button);
                row.Add(actions);
            }

            return row;
        }

        private void RunAction(Func<UniTask> action)
        {
            RunActionAsync(action).Forget();
        }

        private async UniTask RunActionAsync(Func<UniTask> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                SetStatus($"Action failed: {ex.Message}");
                Debug.LogException(ex);
            }
        }

        private UserPanel FindUserPanel(int userNumber)
        {
            return new UserPanel
            {
                NameLabel = Root.Q<Label>($"User{userNumber}NameLabel"),
                MetaLabel = Root.Q<Label>($"User{userNumber}MetaLabel"),
                FundsAmountField = Root.Q<TextField>($"User{userNumber}FundsAmount"),
                AddFundsButton = Root.Q<Button>($"User{userNumber}AddFundsButton"),
                WithdrawFundsButton = Root.Q<Button>($"User{userNumber}WithdrawFundsButton"),
                InventoryList = Root.Q<ScrollView>($"User{userNumber}InventoryList"),
                OwnLotsList = Root.Q<ScrollView>($"User{userNumber}OwnLotsList")
            };
        }

        private static VisualElement CreateRow()
        {
            var row = new VisualElement();
            row.AddToClassList("auction-row");
            return row;
        }

        private static VisualElement CreateIconSlot()
        {
            var iconSlot = new VisualElement();
            iconSlot.AddToClassList("auction-icon-slot");
            return iconSlot;
        }

        private static VisualElement CreateMainBlock(string title, string detail)
        {
            var main = new VisualElement();
            main.AddToClassList("auction-row-main");

            var titleLabel = new Label(title);
            titleLabel.AddToClassList("auction-row-title");
            main.Add(titleLabel);

            var detailLabel = new Label(detail);
            detailLabel.AddToClassList("auction-row-detail");
            main.Add(detailLabel);

            return main;
        }

        private static VisualElement CreateActions()
        {
            var actions = new VisualElement();
            actions.AddToClassList("auction-row-actions");
            return actions;
        }

        private static TextField CreateSmallInput(string value)
        {
            var input = new TextField { value = value };
            input.AddToClassList("auction-input");
            input.AddToClassList("auction-small-input");
            return input;
        }

        private static Button CreateButton(string text)
        {
            var button = new Button { text = text };
            button.AddToClassList("auction-button");
            return button;
        }

        private static void AddEmpty(VisualElement parent, string message)
        {
            var label = new Label(message);
            label.AddToClassList("auction-empty-label");
            parent.Add(label);
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;
        }

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : fallback;
        }

        private static long ParseLong(string value, long fallback)
        {
            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : fallback;
        }

        private static string ShortGuid(Guid guid)
        {
            return guid.ToString("N").Substring(0, 8);
        }

        private struct UserPanel
        {
            public Label NameLabel;
            public Label MetaLabel;
            public TextField FundsAmountField;
            public Button AddFundsButton;
            public Button WithdrawFundsButton;
            public ScrollView InventoryList;
            public ScrollView OwnLotsList;
        }
    }
}
