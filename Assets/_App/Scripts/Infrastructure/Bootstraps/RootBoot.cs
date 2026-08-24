using DevKit.UI.MVVM;
using MagicOnion;
using TradeArea.Shared;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TradeArea.Infrastructure.Bootstraps
{
    public class RootBoot : LifetimeScope
    {
        [SerializeField] private RootUIBinder _rootUIBinderPrefab;
        private RootUIBinder _rootUIBinder;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneLoader>(Lifetime.Singleton);

            // REGISTRATIONS
            
            _rootUIBinder = Instantiate(_rootUIBinderPrefab);
            DontDestroyOnLoad(_rootUIBinder);

            builder.RegisterInstance<IRootUIBinder>(_rootUIBinder);

            builder.Register<GrpcChannelx>(x => GrpcChannelx.ForAddress(Constants.AUCY_URL), 
                Lifetime.Singleton);

            //

            builder.RegisterBuildCallback(OnBuild);
        }

        private void OnBuild(IObjectResolver container)
        {
            if (Container.TryResolve<LoadingScreen>(out var loadingScreen))
                loadingScreen.transform.SetParent(_rootUIBinder.transform);
        }
    }
}