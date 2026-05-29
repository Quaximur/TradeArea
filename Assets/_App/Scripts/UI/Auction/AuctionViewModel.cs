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
        // 29.05.26
        private const string TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMmMyZjJmNzEtYWEwZi00NmYxLTgzOWYtOTc2NGEzNGI0Zjc2IiwiZXhwIjoxNzgwMTE3NDU3fQ.e7WxDYTV4__D40Mml9w9Czl231xdfu-xqEE6TyWi5EA";
        // private const string USER_ID_1 = "2c2f2f71-aa0f-46f1-839f-9764a34b4f76";

        public async UniTask CreateLotTest()
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");

            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

            await serviceClient.CreateLot(new CreateLotDto
            (
                itemConfig: new ItemConfigDto(
                    id: Guid.NewGuid(),
                    name: "TaintedWeapon",
                    isUnary: true,
                    icon: "tainted_weapon",
                    grade: 0,
                    rarity: 2,
                    baseType: "weapon",
                    extra: null,
                    categoryPath: "artifact/weapon"
                ), 
                unitPrice: Random.Range(1_000_000, 2_000_001),
                totalQuantity: 1
            ));
        }

        public async UniTask RefreshOwnLotsTest()
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");

            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

            var lots = await serviceClient.GetOwnLots();
            if (lots != null && lots.Lots != null && lots.Lots.Count > 0)
            {
                foreach (var lot in lots.Lots)
                    UnityEngine.Debug.Log($"LotId: {lot.Id}, ItemName: {lot.ItemConfig.Name}, UnitPrice: {lot.UnitPrice}, Quantity: {lot.RemainingQuantity}/{lot.TotalQuantity}");
            }
            else
            {
                UnityEngine.Debug.Log("No lots found.");
            }
        }

        public async UniTask RefreshListingTest()
        {
            using var channel = GrpcChannelx.ForAddress("http://localhost:8765");
            var serviceClient = MagicOnionClient.Create<IAuctionService>(channel)
                .WithHeaders(new Metadata { { "authorization", $"Bearer {TOKEN}" } });

            var itemConfigs = await serviceClient.GetItemConfigs(new GetItemConfigsDto(
                categoryPathFilter: null,
                nameFilter: null,
                rarity: null,
                minGrade: null,
                maxGrade: null,
                page: 1,
                pageSize: 100
            ));

            if (itemConfigs != null && itemConfigs.ItemConfigs != null && itemConfigs.ItemConfigs.Count > 0)
            {
                foreach (var item in itemConfigs.ItemConfigs)
                    UnityEngine.Debug.Log($"ItemConfigId: {item.Id}, ItemName: {item.Name}");
            }
            else
            {
                UnityEngine.Debug.Log("No items found.");
            }
        }
    }
}