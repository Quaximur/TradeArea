using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.Utils;
using R3;
using TradeArea.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace TradeArea.Infrastructure.Installers
{
    public class UserInstaller : ModuleInstaller
    {
        [SerializeField] private UserState _devUserState;
        [SerializeField] private UserState _devUserState2;

        public override void Install(IContainerBuilder builder)
        {
            // TODO: take user data from context
            builder.RegisterInstance(new UserModel(new UserState()
            {
                Name = _devUserState.Name,
                Token = _devUserState.Token,
                UserId = _devUserState.UserId,
            }));
        }

        protected override async UniTask OnInitialize(CancellationToken token)
        {
            var userModel = Container.Resolve<UserModel>();
            await UniTask.DelayFrame(1); // TODO: IAuthService.Login

#if UNITY_EDITOR
            // Dev user profiles switcher
            // TIP: Switch profiles before joining StreamingHub

            Observable.EveryUpdate().Where(_ => Keyboard.current.kKey.wasPressedThisFrame)
                .Subscribe(_ =>
                {
                    userModel.OnUserIdChanged.Value = _devUserState.UserId;
                    userModel.OnNameChanged.Value = _devUserState.Name;
                    userModel.OnTokenChanged.Value = _devUserState.Token;
                    FLogger.Log("USER 1 is used");
                }).AddTo(_disposables);

            Observable.EveryUpdate().Where(_ => Keyboard.current.lKey.wasPressedThisFrame)
                .Subscribe(_ =>
                {
                    userModel.OnUserIdChanged.Value = _devUserState2.UserId;
                    userModel.OnNameChanged.Value = _devUserState2.Name;
                    userModel.OnTokenChanged.Value = _devUserState2.Token;
                    FLogger.Log("USER 2 is used");
                }).AddTo(_disposables);
#endif
        }
    }
}