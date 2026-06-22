using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TradeArea.Infrastructure.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TradeArea.Infrastructure.Bootstraps
{
    public abstract class Bootstrap : LifetimeScope
    {
        protected CompositeDisposable _disposables = new();
        private ModuleInstaller[] _installers;
                    
        public async UniTask InitChildrenAsync(CancellationToken token)
        {
            if (_installers == null || _installers.Length == 0)
                return;

            foreach (var installer in _installers)
            {
                if (installer != null)
                    await installer.Initialize(Container, token);
                else
                    Debug.LogWarning("null Installer");
            }
        }

        protected sealed override void Configure(IContainerBuilder builder)
        {
            OnConfigure(builder);

            _installers = GetComponentsInChildren<ModuleInstaller>(); // includeInactive: false

            if (_installers != null && _installers.Length > 0)
                foreach (var installer in _installers)
                    installer.Install(builder);
        }

        protected abstract void OnConfigure(IContainerBuilder builder);
    }
}