using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using R3;
using Cysharp.Threading.Tasks;
using SlopSquad.Infrastructure.Contexts;
using SlopSquad.Infrastructure.Bootstraps;
using SlopSquad.Shared;

namespace SlopSquad.Infrastructure
{
    public class SceneLoader
    {
        private readonly LoadingScreen _loadingScreen;
#if UNITY_EDITOR
        private const float MIN_LOADING_TIME = 0.1f;
#else
        private const float MIN_LOADING_TIME = 1f;
#endif

        public SceneLoader(LoadingScreen loadingScreen)
        {
            _loadingScreen = loadingScreen;
        }

        public async UniTask LoadStartScene()
        {
            var currentScene = SceneManager.GetActiveScene().name;
#if UNITY_EDITOR
            switch (currentScene)
            {
                case Scenes.MAIN_MENU:
                    await LoadMainMenu(showLoadingImmediately: true);
                    break;
                case Scenes.GAMEPLAY:
                    await LoadGameplay(immediateLoading: true);
                    break;
                default:
                    _loadingScreen.Hide();
                    break;
            }
#else
            await LoadMainMenu(showLoadingImmediately: true);
#endif
        }

        private async UniTask LoadMainMenu(MainMenuEnterContext mainMenuEnterContext = null,
            bool showLoadingImmediately = false)
        {
            await _loadingScreen.ShowCoroutine(showLoadingImmediately);

            var startTime = Time.time;

            await LoadSceneAsync(Scenes.GAP);
            await LoadSceneAsync(Scenes.MAIN_MENU);

            Debug.Log("Main menu scene loaded");

            var mainMenuBootstrap = Object.FindAnyObjectByType<MainMenuBoot>();
            var exitMainMenuSignal = await mainMenuBootstrap.BootAsync(mainMenuEnterContext);

            exitMainMenuSignal.Take(1).Subscribe(mainMenuExitContext =>
            {
                LoadGameplay(mainMenuExitContext.GameplayEnterContext).Forget();
            });

            await GetRemainFakeLoadTime(startTime);
            await _loadingScreen.HideCoroutine();
        }

        private async UniTask LoadGameplay(GameplayEnterContext gameplayEnterContext = null, bool immediateLoading = false)
        {
            await _loadingScreen.ShowCoroutine(immediateLoading);

            var startTime = Time.time;

            await LoadSceneAsync(Scenes.GAP);
            await LoadSceneAsync(gameplayEnterContext == null ? Scenes.GAMEPLAY :
                gameplayEnterContext.ToSceneName);

            Debug.Log("Gameplay scene loaded");

            var gameplayBootstrap = Object.FindAnyObjectByType<GameplayBoot>();
            var gameplayExitSignal = await gameplayBootstrap.BootAsync(gameplayEnterContext);

            gameplayExitSignal.Take(1).Subscribe(gameplayExitContext =>
            {
                var enterContext = gameplayExitContext.EnterContext;
                switch (enterContext.ToSceneName)
                {
                    case Scenes.MAIN_MENU:
                        LoadMainMenu((MainMenuEnterContext)enterContext).Forget();
                        break;
                    case Scenes.GAMEPLAY:
                        LoadGameplay((GameplayEnterContext)enterContext).Forget();
                        break;
                    default:
                        Debug.LogWarning("Unhandled scene enter context");
                        LoadMainMenu().Forget();
                        break;
                }
            });

            await GetRemainFakeLoadTime(startTime);
            await _loadingScreen.HideCoroutine();
        }

        private async UniTask LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            await SceneManager.LoadSceneAsync(sceneName, loadMode).ToUniTask();
        }

        private IEnumerator GetRemainFakeLoadTime(float startTime)
        {
            var currentTime = Time.time;
            var remainTime = MIN_LOADING_TIME - (currentTime - startTime);
            if (remainTime > 0)
                yield return new WaitForSeconds(remainTime);
            else
                yield return null;
        }
    }
}