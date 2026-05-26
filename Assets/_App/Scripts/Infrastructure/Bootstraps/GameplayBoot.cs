using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM;
using R3;
using TradeArea.Infrastructure.Contexts;
using TradeArea.Shared;
using TradeArea.UI.Auction;
using UnityEngine;
using VContainer;

namespace TradeArea.Infrastructure.Bootstraps
{
    public class GameplayBoot : Bootstrap
    {
        [SerializeField] private AuctionView _auctionViewPrefab;

        private GameplayEnterContext _gameplayEnterContext;

        protected override void Configure(IContainerBuilder builder)
        {
            //// REGISTRATIONS
            builder.Register<Subject<Unit>>(Lifetime.Singleton)
                .Keyed(Constants.GAMEPLAY_EXIT_SIGNAL);

            builder.Register<Subject<Unit>>(Lifetime.Singleton)
                .Keyed(Constants.GAMEPLAY_RESTART_SIGNAL);

            builder.RegisterMVVM<AuctionView, AuctionViewModel>(_auctionViewPrefab);
            ////
        }

        public async UniTask<Observable<GameplayExitContext>> BootAsync(GameplayEnterContext gameplayEnterContext)
        {
            _gameplayEnterContext = gameplayEnterContext;

            Build();

            //// INITIALIZATION
            Container.Resolve<IViewBinder<AuctionViewModel>>().Open();
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