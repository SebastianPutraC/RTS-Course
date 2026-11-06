using GameDevTV.RTS.Commands;
using GameDevTV.RTS.EventBus;

namespace GameDevTV.RTS.Events
{
    public struct ActionSelectedEvent : IEvent
    {
        public ActionBase Action { get; }

        public ActionSelectedEvent(ActionBase action)
        {
            Action = action;
        }
    }
}
