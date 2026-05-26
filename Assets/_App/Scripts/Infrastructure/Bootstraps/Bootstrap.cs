using R3;
using VContainer.Unity;

namespace TradeArea.Infrastructure.Bootstraps
{
    public class Bootstrap : LifetimeScope
    {
        protected CompositeDisposable _disposables = new();
    }
}