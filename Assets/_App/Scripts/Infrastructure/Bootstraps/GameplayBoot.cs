using Cysharp.Threading.Tasks;
using R3;
using SlopSquad.Infrastructure.Contexts;
using SlopSquad.Shared;
using VContainer;

namespace SlopSquad.Infrastructure.Bootstraps
{
    public class GameplayBoot : Bootstrap
    {
        private GameplayEnterContext _gameplayEnterContext;

        protected override void Configure(IContainerBuilder builder)
        {
            //// REGISTRATIONS
            builder.Register<Subject<Unit>>(Lifetime.Singleton)
                .Keyed(Constants.GAMEPLAY_EXIT_SIGNAL);

            builder.Register<Subject<Unit>>(Lifetime.Singleton)
                .Keyed(Constants.GAMEPLAY_RESTART_SIGNAL);

            ////
        }

        public async UniTask<Observable<GameplayExitContext>> BootAsync(GameplayEnterContext gameplayEnterContext)
        {
            _gameplayEnterContext = gameplayEnterContext;

            Build();

            //// INITIALIZATION

            ////

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

            return await UniTask.FromResult<Observable<GameplayExitContext>>(gameplayExitSignal);
        }
    }
}