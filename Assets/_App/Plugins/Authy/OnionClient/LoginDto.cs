using MessagePack;

namespace Authy.Contracts.OnionClient
{
    [MessagePackObject]
    public class LoginDto
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public string Password { get; set; }
    }
}
