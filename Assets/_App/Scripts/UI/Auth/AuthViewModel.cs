using System;
using System.Threading;
using Authy.Contracts.OnionClient;
using Cysharp.Threading.Tasks;
using DevKit.UI.MVVM.Bases;
using DevKit.Utils;
using MagicOnion;
using MagicOnion.Client;
using R3;
using TradeArea.Data;
using TradeArea.Shared;
using VContainer;

namespace TradeArea.UI.Auth
{
    public class AuthViewModel : ScreenViewModel
    {
        public Observable<bool> OnInputEnabled => _isInputEnabled;
        public Observable<string> OnErrorMessageChanged => _errorMessage;

        private readonly UserModel _userModel;
        private readonly Subject<Unit> _enterSignal;
        private readonly GrpcChannelx _channel;

        private readonly ReactiveProperty<bool> _isInputEnabled = new(true);
        private readonly ReactiveProperty<string> _errorMessage = new();

        private string _nameInput;
        private string _passwordInput;

        private CancellationTokenSource _cts = new();

        public AuthViewModel(UserModel userModel,
            [Key(Constants.GAMEPLAY_START_SIGNAL)] Subject<Unit> enterSignal,
            GrpcChannelx channel)
        {
            _userModel = userModel;
            _enterSignal = enterSignal;
            _channel = channel;
        }

        public void SetName(string name)
        {
            _nameInput = name;
        }

        public void SetPassword(string password)
        {
            _passwordInput = password;
        }

        public void Login()
        {
            // TODO: Validation

            if (!TryValidateFields())
                return;

            LoginAsync().Forget();
        }

        private bool TryValidateFields()
        {
            if (string.IsNullOrEmpty(_nameInput))
            {
                _errorMessage.Value = "invalid_name_error";
                return false;
            }

            if (string.IsNullOrEmpty(_passwordInput))
            {
                _errorMessage.Value = "invalid_password_error";
                return false;
            }

            _errorMessage.Value = null;
            return true;
        }

        private async UniTask LoginAsync()
        {
            _isInputEnabled.Value = false;
            _errorMessage.Value = null;

            try
            {
                var authClient = MagicOnionClient.Create<IAuthService>(_channel)
                    .WithCancellationToken(_cts.Token);

                var loginResponse = await authClient.Login(new LoginDto()
                {
                    Name = _nameInput,
                    Password = _passwordInput
                });

                _userModel.OnUserIdChanged.Value = loginResponse.UserId.ToString();
                _userModel.OnNameChanged.Value = loginResponse.Name;
                _userModel.OnTokenChanged.Value = loginResponse.Token;

                _enterSignal.OnNext(Unit.Default);
                return;
            }
            catch (OperationCanceledException)
            {
                FLogger.Log<AuthViewModel>($"{nameof(LoginAsync)}: cancelled");
                return;
            }
            catch (Exception ex)
            {
                FLogger.LogError<AuthViewModel>($"{nameof(LoginAsync)}: {ex.Message}");
                _errorMessage.Value = ex.Message;
            }

            _isInputEnabled.Value = true;
        }

        public override void Dispose()
        {
            Disposes.ClearCts(ref _cts);
            base.Dispose();
        }
    }
}