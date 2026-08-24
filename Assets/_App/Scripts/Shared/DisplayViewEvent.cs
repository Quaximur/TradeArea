using UnityEngine.UIElements;

namespace TradeArea.Shared
{
    public class DisplayViewEvent : EventBase<DisplayViewEvent>
    {
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        private void LocalInit()
        {
            bubbles = true;
            tricklesDown = true;
        }

        public DisplayViewEvent() => LocalInit();
    }
}