using UnityEngine;

namespace TradeArea.Data
{
    [System.Serializable]
    public class UserState
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Token { get; set; }
        [field: SerializeField] public string UserId { get; set; }
    }
}