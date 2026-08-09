using System.Threading;
using Cysharp.Threading.Tasks;
using TradeArea.Repositories;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    public class ItemConfigsInstaller : ModuleInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            var itemConfigRepository = new LocalItemConfigRepository();
            builder.RegisterInstance<IItemConfigRepository>(itemConfigRepository);
        }

        protected override UniTask OnInitialize(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}