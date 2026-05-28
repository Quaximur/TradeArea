using DevKit.UI.MVVM.Bases;
using MagicOnion;
using MagicOnion.Client;
using Aucy.Contracts.OnionContracts;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using System;
using Random = UnityEngine.Random;

namespace TradeArea.UI.Auction
{
    public class AuctionViewModel : ScreenViewModel
    {
        // 28.05.26
        private const string TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMmMyZjJmNzEtYWEwZi00NmYxLTgzOWYtOTc2NGEzNGI0Zjc2IiwiZXhwIjoxNzgwMDAxNjQzfQ.In6Aa41w8SV7EX4YPU2UrJBXr1u78kgNFc_iOh8vR-A";
        private const string USER_ID_1 = "2c2f2f71-aa0f-46f1-839f-9764a34b4f76";

        public async UniTask CreateLotTest()
        {
            var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

            await serviceClient.CreateLot(new CreateLotDto
            (
                ownerUserId: USER_ID_1,
                itemConfig: new ItemConfigDto(id: Guid.NewGuid().ToString(), definition: $"stats_{Guid.NewGuid()}", name: "TaintedWeapon"),
                unitPrice: Random.Range(1_000_000, 2_000_001),
                itemQuantity: 1
            ));

            channel.Dispose();
        }

        public async UniTask RefreshOwnLotsTest()
        {
            var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

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

        public async UniTask RefreshListingTest()
        {
            var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

            var itemConfigs = await serviceClient.GetItemConfigs(new GetItemConfigsDto(null, null, 1, 100));
            if (itemConfigs != null && itemConfigs.ItemConfigs != null && itemConfigs.ItemConfigs.Count > 0)
            {
                foreach (var item in itemConfigs.ItemConfigs)
                {
                    UnityEngine.Debug.Log($"ItemConfigId: {item.Id}, Definition: {item.Definition}, ItemName: {item.Name}");
                }
            }
            else
            {
                UnityEngine.Debug.Log("No items found.");
            }
            channel.Dispose();
        }
    }
}