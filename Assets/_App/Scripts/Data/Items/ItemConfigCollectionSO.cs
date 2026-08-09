using System.Collections.Generic;
using UnityEngine;

namespace TradeArea.Data.Items
{
    [CreateAssetMenu(fileName = nameof(ItemConfigCollectionSO),
        menuName = "Scriptable Objects/" + nameof(ItemConfigCollectionSO))]
    public class ItemConfigCollectionSO : ScriptableObject, IItemConfigCollection
    {
        public IReadOnlyList<IItemConfig> AllData => _allData;
        private ItemConfigSO[] _allData;
    }
}