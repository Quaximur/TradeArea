using DevKit.UI.MVVM;
using UnityEngine.UIElements;

namespace TradeArea.UI.AuctionInventory
{
    [UxmlElement]
    public partial class AuctionInventoryWidget : ToolkitWidget<AuctionInventoryViewModel>//AttachableToolkitScreen<AuctionInventoryViewModel>
    {
        [UxmlAttribute] private string _itemStackContainerName = "ItemStackContainer";

        private VisualElement _itemStackContainer;

        protected override void OnInit()
        {
            _itemStackContainer = Root.Q(_itemStackContainerName);
        }

        protected override void OnBind(AuctionInventoryViewModel viewModel)
        {
            viewModel.RefreshInventory();
        }
    }
}