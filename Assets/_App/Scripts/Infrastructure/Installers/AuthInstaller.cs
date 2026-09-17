using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using TradeArea.UI.Auth;
using UnityEngine;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    public class AuthInstaller : ModuleInstaller
    {
        [SerializeField] private AuthView _authViewPrefab;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterMVVM<AuthView, AuthViewModel>(_authViewPrefab);
        }

        protected override UniTask OnInitialize(CancellationToken token)
        {
            var authBinder = Container.Resolve<IViewBinder<AuthViewModel>>();
            authBinder.Open();

            return UniTask.CompletedTask;
        }
    }
}