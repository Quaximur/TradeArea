using Cysharp.Threading.Tasks;
using R3;
using SlopSquad.Infrastructure.Contexts;
using SlopSquad.Shared;
using VContainer;

namespace SlopSquad.Infrastructure.Bootstraps
{
    public class MainMenuBoot : Bootstrap
    {
        private MainMenuEnterContext _mainMenuEnterContext;

        protected override void Configure(IContainerBuilder builder)
        {
            //// REGISTRATIONS
            builder.Register<Subject<Unit>>(Lifetime.Scoped)
                .Keyed(Constants.GAMEPLAY_START_SIGNAL);
 
            ////
        }


        public UniTask<Observable<MainMenuExitContext>> BootAsync(MainMenuEnterContext mainMenuEnterContext = null)
        {
            _mainMenuEnterContext = mainMenuEnterContext;
            
            Build();

            //// INITIALIZATION

            ////

            var startSessionSignal = Container.Resolve<Subject<Unit>>(Constants.GAMEPLAY_START_SIGNAL);
            var mainMenuExitSignal = new Subject<MainMenuExitContext>();
            var gameplayEnterContext = new GameplayEnterContext(); // TODO: take context from state
            var mainMenuExitContext = new MainMenuExitContext(gameplayEnterContext);

            startSessionSignal.Take(1).Subscribe(_ =>
            {
                mainMenuExitSignal.OnNext(mainMenuExitContext);
            }).AddTo(_disposables);

            return UniTask.FromResult<Observable<MainMenuExitContext>>(mainMenuExitSignal);
        }

        protected override void OnDestroy()
        {
            if (_disposables != null)
            {
                _disposables.Dispose();
                _disposables = null;
            }

            base.OnDestroy();
        }
    }
}