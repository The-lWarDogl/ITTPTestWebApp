using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using ITTPTestWebApp.Common;

namespace ITTPTestWebApp.Logging
{
    /// <summary>
    /// Singleton класс для логирования данных и их распределения по нужным папкам.
    /// Поддерживает дополнительные обработчики логов
    /// </summary>
    class Logger
    {
        public static readonly Logger Instance = new Logger();

        #region fields 
        private readonly SemaphoreSlim _FileSemaphore = new SemaphoreSlim(1, 1);

        public List<Func<string, Task>> AdditionalLogList = new();
        #endregion

        private Logger()
        {
            if (!Directory.Exists(@"Logs")) { Directory.CreateDirectory(@"Logs"); }
        } 

        #region public methods
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Log(string message, string tag = "message", [CallerMemberName] string callerName = "", StackTrace? stackTrace = null, bool writeToFile = true)
        {
            _Log
            (
                logEntry:
                tag != ""
                ? $"{DateTime.Now} - [{tag.ToUpper()}] {callerName} : {message}"
                : $"{DateTime.Now} - {callerName} : {message}",
                writeToFile: writeToFile,
                logFilePath: _GetLogFilePath(tag, (stackTrace ?? new StackTrace()).GetFrame(1)?.GetMethod())
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Log(Exception ex, [CallerMemberName] string callerName = "")
        {
            _Log
            (
                logEntry: $"{DateTime.Now} - [EXCEPTION] {callerName} : {ex.ToString()}",
                writeToFile: true,
                logFilePath: _GetLogFilePath("exception", (new StackTrace()).GetFrame(1)?.GetMethod())
            );
        }
        #endregion

        #region private methods
        private string _GetLogFilePath(string tag, System.Reflection.MethodBase? method)
        {

            string logFilePath = "";
            if (method != null && method.DeclaringType != null && method.DeclaringType.FullName != null)
            {
                string fullName = method.DeclaringType.FullName;
                //Убираем сгенерированное компилятором имя для асинхронных методов
                fullName = fullName.Contains("+") ? fullName.Substring(0, fullName.IndexOf('+')) : fullName;
                List<string> filePathParts = fullName.Split(".").ToList(); filePathParts.RemoveAt(0);
                string relativePath = Path.Combine(filePathParts.ToArray());
                string tagedRelativePath = Path.Combine(Func.ConvertToTitleCase(tag), relativePath);
                logFilePath = Path.Combine("Logs", $"{tagedRelativePath}_{DateTime.Now:dd_MM_yy}.txt");
                return logFilePath;
            }
            else return logFilePath;
        }

        private void _Log(string logEntry, bool writeToFile = true, string logFilePath = "")
        {
            Console.WriteLine(logEntry);
            if (writeToFile && logFilePath != "") _ = _WriteToFile(logFilePath, logEntry);

            if (AdditionalLogList.Any())
            foreach (var additionalLog in AdditionalLogList) { additionalLog(logEntry); }
        }

        private async Task _WriteToFile(string logFilePath, string logEntry)
        {
            try
            {
                await _FileSemaphore.WaitAsync();

                string? directoryPath = Path.GetDirectoryName(logFilePath);
                if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                if (!File.Exists(logFilePath)) using (File.Create(logFilePath)) { }

                using (StreamWriter sw = File.AppendText(logFilePath)) { sw.WriteLine(logEntry); }
            }
            catch (Exception ex) { Console.WriteLine($"[EXCEPTION] WriteToFile: {ex.ToString()}"); }
            finally { _FileSemaphore.Release(); }
        }
        #endregion
    }
}