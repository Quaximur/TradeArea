using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.Utils;
using TradeArea.Data.Items;
using TradeArea.Repositories;
using UnityEngine;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    public class ItemConfigsInstaller : ModuleInstaller
    {
        [SerializeField] private ItemConfigCollectionSO _itemConfigCollection;

        public override void Install(IContainerBuilder builder)
        {
            var itemConfigRepository = new LocalItemConfigRepository();

            if (_itemConfigCollection == null)
            {
                FLogger.LogError<ItemConfigsInstaller>("ItemConfigCollectionSO is not assigned in the inspector.");
            }
            else
            {
                foreach (var itemConfig in _itemConfigCollection.AllData)
                {
                    if (itemConfig == null)
                    {
                        FLogger.LogWarning<ItemConfigsInstaller>("Encountered a null item config in the collection.");
                        continue;
                    }
                    itemConfigRepository.AddItemConfig(itemConfig.Id, itemConfig);
                }
            }

            builder.RegisterInstance<IItemConfigRepository>(itemConfigRepository);
        }

        protected override UniTask OnInitialize(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}