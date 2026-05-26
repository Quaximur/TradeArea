using System.Collections;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace TradeArea.Infrastructure
{
    public enum LoadingLogo
    {
        Show,
        Hide
    }

    public class LoadingScreen : MonoBehaviour
    {
        public Observable<Unit> OnShowStarted => _onShowStarted;
        public Observable<Unit> OnShowFinished => _onShowFinished;
        public Observable<Unit> OnHideStarted => _onHideStarted;
        public Observable<Unit> OnHideFinished => _onHideFinished;
        public Observable<float> OverlayFillProgress => _overlayFillProgress;

        [SerializeField] private GameObject _loadingRoot;
        [SerializeField] private Image _overlayImage;
        [SerializeField] private GameObject _loadingStub;
        [SerializeField] private GameObject _loadingText;
        [SerializeField] private string _overlayProgressProperty;
        [SerializeField, Min(0.01f)] private float _overlayFillSeconds = 2f;

        private readonly Subject<Unit> _onShowStarted = new();
        private readonly Subject<Unit> _onShowFinished = new();
        private readonly Subject<Unit> _onHideFinished = new();
        private readonly Subject<Unit> _onHideStarted = new();
        private readonly ReactiveProperty<float> _overlayFillProgress = new(1);

#region MonoBehaviour
        private void Awake()
        {
            Hide();
        }
#endregion

        public IEnumerator ShowCoroutine(bool isInstant)
        {
            if (isInstant)
                Show();
            else
                yield return ShowCoroutine();
        }
        
        public IEnumerator HideWithInstantCoroutine(bool isInstant)
        {
            if (isInstant)
                Hide();
            else
                yield return HideCoroutine();
        }
        
        public void Show()
        {
            _onShowStarted.OnNext(Unit.Default);

            SetActiveRootSafe(true);
            SetActiveTextSafe(true);
            SetActiveStubSafe(true);

            SetOverlayFillProgress(1);
            
            _onShowFinished.OnNext(Unit.Default);
        }

        public void Hide()
        {
             _onHideStarted.OnNext(Unit.Default);

            SetActiveRootSafe(false);
            SetActiveStubSafe(false);
            SetActiveTextSafe(false);
            
            SetOverlayFillProgress(0);
            
            _onHideFinished.OnNext(Unit.Default);
        }

        public IEnumerator ShowCoroutine(LoadingLogo logo = LoadingLogo.Show)
        {
            _onShowStarted.OnNext(Unit.Default);
            SetActiveRootSafe(true);

            while (_overlayFillProgress.Value < 1)
            {
                var currentProgress = _overlayFillProgress.Value + Time.unscaledDeltaTime / _overlayFillSeconds;
                
                if (currentProgress > 1)
                    currentProgress = 1;
                
                SetOverlayFillProgress(currentProgress);
                yield return null;
            }

            SetActiveStubSafe(logo == LoadingLogo.Show);
            SetActiveTextSafe(true);

            _onShowFinished.OnNext(Unit.Default);
        }

        public IEnumerator HideCoroutine()
        {
            _onHideStarted.OnNext(Unit.Default);

            SetActiveStubSafe(false);
            SetActiveTextSafe(false);

            while (_overlayFillProgress.Value > 0)
            {
                var currentProgress = _overlayFillProgress.Value - Time.unscaledDeltaTime / _overlayFillSeconds;
                if (currentProgress < 0)
                    currentProgress = 0;

                SetOverlayFillProgress(currentProgress);

                yield return null;
            }

            SetActiveRootSafe(false);

            _onHideFinished.OnNext(Unit.Default);
        }

        private void SetOverlayFillProgress(float progress)
        {
            _overlayFillProgress.Value = progress;

            if (_overlayImage == null || _overlayImage.material == null)
                return;

            _overlayImage.material.SetFloat(_overlayProgressProperty, _overlayFillProgress.Value);
        }

        private void SetActiveTextSafe(bool isActive)
        {
            if (_loadingText != null)
                _loadingText.SetActive(isActive);
        }

        public void SetActiveRootSafe(bool isActive)
        {
            if (_loadingRoot != null)
                _loadingRoot.SetActive(isActive);
        }
        
        private void SetActiveStubSafe(bool isActive)
        {
            if (_loadingStub != null)
                _loadingStub.SetActive(isActive);
        }
    }
}