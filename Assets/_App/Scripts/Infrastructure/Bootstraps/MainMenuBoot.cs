using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using TradeArea.Infrastructure.Contexts;
using TradeArea.Shared;
using VContainer;

namespace TradeArea.Infrastructure.Bootstraps
{
    public class MainMenuBoot : Bootstrap
    {
        private MainMenuEnterContext _mainMenuEnterContext;

        protected override void OnConfigure(IContainerBuilder builder)
        {
            // REGISTRATIONS
            
            builder.Register<Subject<Unit>>(Lifetime.Scoped)
                .Keyed(Constants.GAMEPLAY_START_SIGNAL);
 
            //
        }

        public async UniTask<Observable<MainMenuExitContext>> BootAsync(MainMenuEnterContext mainMenuEnterContext = null)
        {
            _mainMenuEnterContext = mainMenuEnterContext;
            
            Build();

            // INITIALIZATION
            
            await InitChildrenAsync(CancellationToken.None);

            //

            var startSessionSignal = Container.Resolve<Subject<Unit>>(Constants.GAMEPLAY_START_SIGNAL);
            var mainMenuExitSignal = new Subject<MainMenuExitContext>();
            var gameplayEnterContext = new GameplayEnterContext(); // TODO: take context from state
            var mainMenuExitContext = new MainMenuExitContext(gameplayEnterContext);

            startSessionSignal.Take(1).Subscribe(_ =>
            {
                mainMenuExitSignal.OnNext(mainMenuExitContext);
            }).AddTo(_disposables);

            return mainMenuExitSignal;
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