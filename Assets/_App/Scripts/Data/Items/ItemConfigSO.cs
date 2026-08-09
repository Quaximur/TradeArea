using System.Collections.Generic;
using UnityEngine;

namespace TradeArea.Data.Items
{
    [CreateAssetMenu(fileName = nameof(ItemConfigSO),
        menuName = "Scriptable Objects/" + nameof(ItemConfigSO))]
    public class ItemConfigSO : ScriptableObject, IItemConfig
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public bool IsUnary { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int? Grade { get; private set; }
        [field: SerializeField] public int? Rarity { get; private set; }
        [field: SerializeField] public float Weight { get; private set; }
        [field: SerializeField] public string BaseType { get; private set; }
        public IList<string> Extra => _extra;
        [SerializeField] private string[] _extra;
        [field: SerializeField] public string CategoryPath { get; private set; }
        [field: SerializeField] public long MinPrice { get; private set; }
        [field: SerializeField] public long MaxPrice { get; private set; }
        [field: SerializeField] public int MaxSellQuantity { get; private set; }
        [field: SerializeField] public int MaxBuyQuantity { get; private set; }
    }
}