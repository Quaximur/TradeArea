using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.Locator;
using TradeArea.Repositories;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    public class ItemConfigsInstaller : ModuleInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            var itemConfigRepository = new LocalItemConfigRepository();
            ServiceLocator.Current.Register<IItemConfigRepository>(itemConfigRepository);
        }

        protected override UniTask OnInitialize(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}