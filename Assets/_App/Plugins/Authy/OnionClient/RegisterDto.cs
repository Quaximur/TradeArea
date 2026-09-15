// Contracts/OnionClient/RegisterDto.cs
using MessagePack;

namespace Authy.Contracts.OnionClient
{
    [MessagePackObject]
    public class RegisterDto
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public string Email { get; set; }
        [Key(2)] public string Password { get; set; }
    }
}