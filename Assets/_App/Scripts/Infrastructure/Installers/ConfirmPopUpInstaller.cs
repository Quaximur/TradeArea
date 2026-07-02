using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using TradeArea.UI.ConfirmPopUp;
using UnityEngine;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    public class ConfirmPopUpInstaller : ModuleInstaller
    {
        [SerializeField] private ConfirmPopUpView _confirmPopUpViewPrefab;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterMVVM<ConfirmPopUpView, ConfirmPopUpViewModel>(_confirmPopUpViewPrefab,
                viewModelLifetime: Lifetime.Transient,
                binderLifetime: Lifetime.Transient);
        }

        protected override UniTask OnInitialize(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}