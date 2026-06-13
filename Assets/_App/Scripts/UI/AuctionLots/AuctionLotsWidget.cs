using DevKit.UI.MVVM;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionLots
{
    [UxmlElement]
    public partial class AuctionLotsWidget : ToolkitWidget<AuctionLotsViewModel>
    {
        protected override void OnBind(AuctionLotsViewModel viewModel)
        {
        }
    }
}