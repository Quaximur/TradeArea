using UnityEngine.UIElements;

namespace TradeArea.Shared
{
    public static class USSUtil
    {
        public static void Show(this VisualElement element)
        {
            element.style.display = DisplayStyle.Flex;
        }

        public static void Hide(this VisualElement element)
        {
            element.style.display = DisplayStyle.None;
        }
    }
}