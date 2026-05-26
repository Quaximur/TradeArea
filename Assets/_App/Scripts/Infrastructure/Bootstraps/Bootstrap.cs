using R3;
using VContainer.Unity;

namespace SlopSquad.Infrastructure.Bootstraps
{
    public class Bootstrap : LifetimeScope
    {
        protected CompositeDisposable _disposables = new();
    }
}