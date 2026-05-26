using DevKit.UI.MVVM.Bases;
using MagicOnion;
using MagicOnion.Client;
using Aucy.Contracts.OnionContracts;
using Cysharp.Threading.Tasks;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        public async UniTask CreateLotTest()
        {
            var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel);

            await serviceClient.CreateLot(new CreateLotDto
            (
                ownerUserId: "51808dcd-83bf-4348-8557-9a0f4bac2214",
                itemConfig: new ItemConfigDto(id: "b4ca4815-64fc-4370-8081-97adbcf0dab3", definition: "wheat", name: "wheat"),
                itemConfigPrice: 2500,
                itemConfigAmount: 10
            ));

            channel.Dispose();
        }

        public async UniTask RefreshListingTest()
        {
            var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel);

            // var lots = await serviceClient.GetOwnLots();
            // Do something with the retrieved lots

            channel.Dispose();
        }
    }
}