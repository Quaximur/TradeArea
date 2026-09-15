using MessagePack;
using System;

namespace Authy.Contracts.OnionClient
{
    [MessagePackObject]
    public class AuthResponseDto
    {
        [Key(0)] public string Token { get; set; }
        [Key(1)] public Guid UserId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string Email { get; set; }
    }
}