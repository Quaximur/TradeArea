using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    public class AuthInstaller : ModuleInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
        }

        protected override UniTask OnInitialize(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}