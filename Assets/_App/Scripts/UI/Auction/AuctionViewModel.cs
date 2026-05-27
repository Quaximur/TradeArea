using DevKit.UI.MVVM.Bases;
using MagicOnion;
using MagicOnion.Client;
using Aucy.Contracts.OnionContracts;
using Cysharp.Threading.Tasks;
using Grpc.Core;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        private const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMmMyZjJmNzEtYWEwZi00NmYxLTgzOWYtOTc2NGEzNGI0Zjc2In0.6jLzRRfzdWSd4oSCqzA-OiSrEUH95VsZFoTtB60BR9Q";
        
        public async UniTask CreateLotTest()
        {
            var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {token}" } });

            await serviceClient.CreateLot(new CreateLotDto
            (
                ownerUserId: "2c2f2f71-aa0f-46f1-839f-9764a34b4f76",
                itemConfig: new ItemConfigDto(id: "eeea4815-64fc-4370-8081-97adbcf0dab3", definition: "egg", name: "egg"),
                unitPrice: 700,
                itemQuantity: 2
            ));

            channel.Dispose();
        }

        public async UniTask RefreshListingTest()
        {
            var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {token}" } });

            var lots = await serviceClient.GetOwnLots();
            if (lots != null && lots.Lots != null && lots.Lots.Count > 0)
            {
                foreach (var lot in lots.Lots)
                {
                    UnityEngine.Debug.Log($"LotId: {lot.Id}, ItemName: {lot.ItemConfig.Name}, UnitPrice: {lot.UnitPrice}, Quantity: {lot.ItemQuantity}");
                }
            }
            else
            {
                UnityEngine.Debug.Log("No lots found.");
            }
            channel.Dispose();
        }
    }
}