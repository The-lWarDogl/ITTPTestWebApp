using System.Reflection;

using ITTPTestWebApp.Events;

namespace ITTPTestWebApp.Common
{
    static class Reflection
    {
        public static void UseReflection(dynamic Class)
        {
            Type subscriberType = Class.GetType();
            var array = subscriberType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in array)
            {
                var attributes = methodInfo.GetCustomAttributes(typeof(ServerEventAttribute), false);
                if (attributes.Length > 0)
                {
                    var eventName = ((ServerEventAttribute)attributes[0]).EventName;
                    var handler = Delegate.CreateDelegate(typeof(Action), Class, methodInfo);
                    EventManager.Instance.AddHandler(eventName, handler);
                }
            }
        }
        public static void UnUseReflection(dynamic Class)
        {
            Type subscriberType = Class.GetType();
            var array = subscriberType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in array)
            {
                var attributes = methodInfo.GetCustomAttributes(typeof(ServerEventAttribute), false);
                if (attributes.Length > 0)
                {
                    var eventName = ((ServerEventAttribute)attributes[0]).EventName;
                    var handler = Delegate.CreateDelegate(typeof(Action), Class, methodInfo);
                    EventManager.Instance.RemoveHandler(eventName, handler);
                }
            }
        }
    }
}
