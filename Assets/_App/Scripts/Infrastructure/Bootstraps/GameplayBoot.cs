using System.Threading;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using DevKit.Utils;
using R3;
using TradeArea.Infrastructure.Contexts;
using TradeArea.Shared;
using UnityEngine;
using VContainer;

namespace TradeArea.Infrastructure.Bootstraps
{
    public class GameplayBoot : Bootstrap
    {
        private GameplayEnterContext _gameplayEnterContext;

        private CancellationTokenSource _cts = new();

        protected override void OnConfigure(IContainerBuilder builder)
        {
            // REGISTRATIONS

            builder.Register<Subject<Unit>>(Lifetime.Singleton)
                .Keyed(Constants.GAMEPLAY_EXIT_SIGNAL);

            builder.Register<Subject<Unit>>(Lifetime.Singleton)
                .Keyed(Constants.GAMEPLAY_RESTART_SIGNAL);

            //
        }

        public async UniTask<Observable<GameplayExitContext>> BootAsync(GameplayEnterContext gameplayEnterContext)
        {
            _gameplayEnterContext = gameplayEnterContext;

            Build();

            // INITIALIZATION
            
            var rootUIBinder = Container.Resolve<IRootUIBinder>();
            rootUIBinder.ClearViews();

            await InitChildrenAsync(CancellationToken.None);

            //

            var exitSignal = Container.Resolve<Subject<Unit>>(Constants.GAMEPLAY_EXIT_SIGNAL);
            var gameplayExitSignal = new Subject<GameplayExitContext>();
            var mainMenuEnterContext = new MainMenuEnterContext();

            var gameplayExitContext = new GameplayExitContext(mainMenuEnterContext);

            exitSignal.Take(1)
                .Subscribe(_ =>
                {
                    gameplayExitSignal.OnNext(gameplayExitContext);
                })
                .AddTo(_disposables);

            return gameplayExitSignal;
        }

        protected override void OnDestroy()
        {
            Disposes.ClearCts(ref _cts);
            base.OnDestroy();
        }
    }
}