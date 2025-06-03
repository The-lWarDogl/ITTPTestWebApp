namespace ITTPTestWebApp.Events
{
    [AttributeUsage(AttributeTargets.Method)]
    class ServerEventAttribute : Attribute
    {
        #region Fields 
        public Event EventName { get; }
        #endregion

        public ServerEventAttribute(Event eventName) { EventName = eventName; }
    }
}

