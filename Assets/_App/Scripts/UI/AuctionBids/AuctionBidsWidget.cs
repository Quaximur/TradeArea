using DevKit.UI.MVVM;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionBids
{
    [UxmlElement]
    public partial class AuctionBidsWidget : ToolkitWidget<AuctionBidsViewModel>
    {
        protected override void OnBind(AuctionBidsViewModel viewModel)
        {
        }
    }
}