using MagicOnion;

namespace Authy.Contracts.OnionClient
{
    public interface IAuthService : IService<IAuthService>
    {
        public UnaryResult<AuthResponseDto> Login(LoginDto loginDto);
        public UnaryResult<AuthResponseDto> Register(RegisterDto registerDto);
    }
}
