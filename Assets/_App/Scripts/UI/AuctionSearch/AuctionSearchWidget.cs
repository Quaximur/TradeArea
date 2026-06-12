using DevKit.UI.MVVM;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionSearch
{
    [UxmlElement]
    public partial class AuctionSearchWidget : ToolkitWidget<AuctionSearchViewModel>
    {
        [UxmlAttribute] private string _refreshMarketButtonName = "RefreshMarket";
        [UxmlAttribute] private string _marketListName = "MarketList";

        private Button _refreshMarketButton;
        private ScrollView _marketList;

        protected override void OnInit()
        {
            _refreshMarketButton = Root.Q<Button>(_refreshMarketButtonName);
            _marketList = Root.Q<ScrollView>(_marketListName);
        }

        protected override void OnBind(AuctionSearchViewModel viewModel)
        {
        }
    }
}