using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace ITTPTestWebApp.Logging
{
    class HostLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) =>
            new HostLogger(categoryName);

        public void Dispose() { }
    }

    class HostLogger : ILogger
    {
        private readonly string _Category;

        public HostLogger(string category)
        { _Category = category; }

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
        public IDisposable BeginScope<TState>(TState state) => default!;
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

        public bool IsEnabled(LogLevel logLevel) =>
        #if DEBUG
            logLevel >= LogLevel.Debug;
        #else
            logLevel >= LogLevel.Warning;
        #endif


        public void Log<TState>
        (
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) =>
            Logger.Instance.Log
            (
                formatter(state, exception), 
                tag: "host", 
                callerName: _Category
            );
    }
}
