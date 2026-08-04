using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion.Unity;
using TradeArea.Shared;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TradeArea.Infrastructure
{
    public class EntryPoint
    {
        private static EntryPoint _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Enter()
        {
            _instance = new();
            _instance.Run();
        }

        private void Run()
        {
            var loadingScreenPrefab = Resources.Load<LoadingScreen>("loading_screen");
            var loadingScreen = Object.Instantiate(loadingScreenPrefab);
            Object.DontDestroyOnLoad(loadingScreen);

            loadingScreen.Show();

            RunAsync().Forget();

            return;

            async UniTaskVoid RunAsync()
            {
                GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(() => new GrpcChannelOptions()
                {
                    HttpHandler = new SubdirectoryHandler(new YetAnotherHttpHandler()
                    {
                        Http2Only = true,
                    }, "/aucy"),
                    DisposeHttpClient = true,
                }));

                var vContainerSettings = Resources.Load<VContainerSettings>("vcontainer_settings");
                var instanceProperty = typeof(VContainerSettings).GetProperty("Instance");
                instanceProperty.SetValue(null, vContainerSettings);

                var rootScope = vContainerSettings.GetOrCreateRootLifetimeScopeInstance();
                using (LifetimeScope.Enqueue(builder =>
                   {
                       builder.RegisterInstance(loadingScreen);
                   }))
                {
                    rootScope.Build();
                }

                var rootContainer = rootScope.Container;

                var sceneLoader = rootContainer.Resolve<SceneLoader>();

                await sceneLoader.LoadStartScene();
            }
        }
    }
}