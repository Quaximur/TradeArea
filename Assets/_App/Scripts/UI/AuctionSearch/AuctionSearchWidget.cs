using DevKit.UI.MVVM;
using UnityEngine.UIElements;
using R3;
using DevKit.Utils;
using TradeArea.UI.Custom;
using Aucy.Contracts.OnionClient;
using TradeArea.Shared;

namespace TradeArea.UI.AuctionSearch
{
    [UxmlElement]
    public partial class AuctionSearchWidget : ToolkitWidget<AuctionSearchViewModel>
    {
        [UxmlAttribute] private string _refreshMarketButtonName = "RefreshMarket";
        [UxmlAttribute] private string _marketListName = "MarketList";
        [UxmlAttribute] private string _noItemsContainerName = "NoItemsContainer";
        [UxmlAttribute] private string _errorContainerName = "ErrorContainer";
        [UxmlAttribute] private string _loadingContainerName = "LoadingContainer";

        private Button _refreshMarketButton;
        private ListView _marketList;
        private VisualElement _noItemsContainer;
        private VisualElement _errorContainer;
        private VisualElement _loadingContainer;

        private CompositeDisposable _disposables;

        protected override void OnInit()
        {
            _errorContainer = Root.Q<VisualElement>(_errorContainerName);
            _loadingContainer = Root.Q<VisualElement>(_loadingContainerName);
            _noItemsContainer = Root.Q<VisualElement>(_noItemsContainerName);

            _refreshMarketButton = Root.Q<Button>(_refreshMarketButtonName);
            _marketList = Root.Q<ListView>(_marketListName);
            _marketList.bindItem = (x, i) =>
            {
                var itemRow = x.Q<ItemConfigRow>();
                itemRow.ItemName = ((ItemConfigDto)_marketList.itemsSource[i]).Name;
            };
        }

        protected override void OnBind(AuctionSearchViewModel viewModel)
        {
            _disposables = new();

            viewModel.UpdateMarketItems();
            viewModel.OnMarketListChanged.Subscribe(x =>
            {
                switch (x.LoadingFlag)
                {
                    case LoadingFlag.Null:
                    case LoadingFlag.Loading:
                        USSUtil.Show(_loadingContainer);
                        USSUtil.Hide(_errorContainer);
                        break;

                    case LoadingFlag.Success:
                        _marketList.itemsSource = x.Value;

                        if (x.Value == null || x.Value.Count == 0)
                            USSUtil.Show(_noItemsContainer);
                        else
                            USSUtil.Hide(_noItemsContainer);

                        USSUtil.Hide(_loadingContainer);
                        USSUtil.Hide(_errorContainer);
                        break;

                    case LoadingFlag.Fail:
                        _marketList.itemsSource = null;
                        USSUtil.Hide(_loadingContainer);
                        USSUtil.Hide(_noItemsContainer);
                        USSUtil.Show(_errorContainer);
                        break;
                }
            }).AddTo(_disposables);
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}