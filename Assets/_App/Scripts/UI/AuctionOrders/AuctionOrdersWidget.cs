using DevKit.UI.MVVM;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionOrders
{
    [UxmlElement]
    public partial class AuctionOrdersWidget : ToolkitWidget<AuctionOrdersViewModel>
    {
        protected override void OnBind(AuctionOrdersViewModel viewModel)
        {
        }
    }
}