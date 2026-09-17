using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using R3;
using UnityEngine.UIElements;

namespace TradeArea.UI.Auth
{
    public class AuthView : AttachableToolkitScreen<AuthViewModel>
    {
        private TextField _usernameField;
        private TextField _emailField;
        private TextField _passwordField;
        private TextField _confirmPasswordField;
        private Button _loginTab;
        private Button _registerTab;
        private Button _loginButton;
        private VisualElement _errorPanel;
        private Label _errorLabel;

        private DisposableBag _disposables;
        private bool _isRegisterMode;

        protected override void OnInit()
        {
            base.OnInit();

            _usernameField = Root.Q<TextField>("UsernameField");
            _emailField = Root.Q<TextField>("EmailField");
            _passwordField = Root.Q<TextField>("PasswordField");
            _confirmPasswordField = Root.Q<TextField>("ConfirmPasswordField");
            _loginTab = Root.Q<Button>("LoginTab");
            _registerTab = Root.Q<Button>("RegisterTab");
            _loginButton = Root.Q<Button>("SubmitButton");
            _errorPanel = Root.Q<VisualElement>("ErrorPanel");
            _errorLabel = Root.Q<Label>("ErrorLabel");

            _loginTab.RegisterCallback<ClickEvent>(_ => SetMode(false));
            _registerTab.RegisterCallback<ClickEvent>(_ => SetMode(true));
            _loginButton.RegisterCallback<ClickEvent>(_ => ViewModel.Login());

            _usernameField.RegisterValueChangedCallback<string>(x => ViewModel.SetName(x.newValue));
            _passwordField.RegisterValueChangedCallback<string>(x => ViewModel.SetPassword(x.newValue));
        }

        protected override void OnBind(AuthViewModel viewModel)
        {
            base.OnBind(viewModel);

            _disposables = new();

            viewModel.OnInputEnabled.Subscribe(enabled =>
            {
                _usernameField.SetEnabled(enabled);
                _emailField.SetEnabled(enabled);
                _passwordField.SetEnabled(enabled);
                _confirmPasswordField.SetEnabled(enabled);
                _loginTab.SetEnabled(enabled);
                _registerTab.SetEnabled(enabled);
                _loginButton.SetEnabled(enabled);
            }).AddTo(ref _disposables);

            viewModel.OnErrorMessageChanged.Subscribe(message =>
            {
                var hasError = !string.IsNullOrEmpty(message);
                _errorPanel.style.display = hasError ? DisplayStyle.Flex : DisplayStyle.None;
                _errorLabel.text = hasError ? message : string.Empty;
            }).AddTo(ref _disposables);
            
            CleanFields();
            SetMode(false);
        }

        private void SetMode(bool isRegisterMode)
        {
            if (_isRegisterMode == isRegisterMode)
                return;

            CleanFields();

            _isRegisterMode = isRegisterMode;

            _emailField.style.display = isRegisterMode ? DisplayStyle.Flex : DisplayStyle.None;
            _confirmPasswordField.style.display = isRegisterMode ? DisplayStyle.Flex : DisplayStyle.None;

            _loginTab.EnableInClassList("auth__tab--active", !isRegisterMode);
            _registerTab.EnableInClassList("auth__tab--active", isRegisterMode);
            _loginButton.style.display = isRegisterMode ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void CleanFields()
        {
            _usernameField.value = string.Empty;
            _passwordField.value = string.Empty;
            _confirmPasswordField.value = string.Empty;
            _emailField.value = string.Empty;

            ViewModel?.SetName(string.Empty);
            ViewModel?.SetPassword(string.Empty);
        }

        public override void Dispose()
        {
            Disposes.ClearDispose(ref _disposables);
            base.Dispose();
        }
    }
}