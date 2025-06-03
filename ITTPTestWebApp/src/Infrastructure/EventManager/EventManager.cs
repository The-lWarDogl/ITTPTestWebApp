using ITTPTestWebApp.Initialization;

namespace ITTPTestWebApp.Events
{
    class EventManager
    {
        public static readonly EventManager Instance = new EventManager();

        #region Fields 
        private Dictionary<Event, List<Delegate>> _EventHandlers = new Dictionary<Event, List<Delegate>>();
        #endregion

        private EventManager() { }

        #region public methods
        public void AddHandler(Event eventName, Delegate handler)
        {
            if (!_EventHandlers.ContainsKey(eventName))
            { _EventHandlers[eventName] = new List<Delegate>(); }

            _EventHandlers[eventName].Add(handler);
        }

        public void RemoveHandler(Event eventName, Delegate handler)
        {
            if (_EventHandlers.ContainsKey(eventName))
            { _EventHandlers[eventName].Remove(handler); }
        }

        public void TriggerEvent(Event eventName, params object[] args)
        {
            if (eventName == Event.ResourceStart) { Initializing.InitAll(); } 

            if (_EventHandlers.ContainsKey(eventName))
            {
                foreach (var handler in _EventHandlers[eventName])
                { handler.DynamicInvoke(args); }
            }

            if (eventName == Event.ResourceStop) { Initializing.UninitAll(); }
        }
        #endregion
    }
}
