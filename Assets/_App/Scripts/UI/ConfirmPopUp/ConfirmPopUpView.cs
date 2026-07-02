using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TradeArea.UI.ConfirmPopUp
{
    public class ConfirmPopUpView : AttachableCanvasScreen<ConfirmPopUpViewModel>
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _messageLabel;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TMP_Text _confirmText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TMP_Text _cancelText;

        // [Header("SFX"), Space(4)]
        // [SerializeField] private SfxData _confirmClickSfx;
        // [SerializeField] private SfxData _cancelClickSfx;
        // [SerializeField] private SfxData _closedSfx;

        // private ILocalizer _localizer;

        private CompositeDisposable _disposables;

        // [Inject] private readonly AudioPlayer _audioPlayer;

        protected override void OnBind(ConfirmPopUpViewModel viewModel)
        {
            base.OnBind(viewModel);

            _disposables = new CompositeDisposable();

            viewModel.OnTitleChanged.Subscribe(x => SetOptionalLocalizedText(_titleLabel, x))
                .AddTo(_disposables);
            viewModel.OnMessageChanged.Subscribe(x => SetLocalizedText(_messageLabel, x))
                .AddTo(_disposables);
            viewModel.OnConfirmTextChanged.Subscribe(x => SetOptionalLocalizedText(_confirmText, x))
                .AddTo(_disposables);
            viewModel.OnCancelTextChanged.Subscribe(x => SetOptionalLocalizedText(_cancelText, x))
                .AddTo(_disposables);
            viewModel.OnConfirmEnabledChanged.Subscribe(_confirmButton.gameObject.SetActive)
                .AddTo(_disposables);

            _confirmButton.onClick.AddListener(OnConfirmClicked);

            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void SetOptionalLocalizedText(TMP_Text textElement, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                textElement.gameObject.SetActive(false);
            }
            else
            {
                textElement.gameObject.SetActive(true);
                SetLocalizedText(textElement, text);
            }
        }

        private void SetLocalizedText(TMP_Text textElement, string text)
        {
            textElement.text = text;
            // if (!string.IsNullOrEmpty(textElement.text))
            //     _localizer.Localize(textElement, Constants.SHARED_TABLE);
        }

        private void OnConfirmClicked()
        {
            // PlaySfx(_confirmClickSfx);
            ViewModel.Confirm();
        }

        private void OnCancelClicked()
        {
            // PlaySfx(_cancelClickSfx);
            ViewModel.Cancel();
        }

        // private void PlaySfx(SfxData sfxData)
        // {
        //     if (sfxData.IsValid)
        //         _audioPlayer.GetSfxBuilder().Play(sfxData);
        // }

        public override void Dispose()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveAllListeners();
                
            if (_cancelButton != null)
                _cancelButton.onClick.RemoveAllListeners();

            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}
