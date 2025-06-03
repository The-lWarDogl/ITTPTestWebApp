using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using ITTPTestWebApp.Data;
using ITTPTestWebApp.Network;
using ITTPTestWebApp.Events;
using ITTPTestWebApp.Initialization.FilesCheck;
using ITTPTestWebApp.Services.ServicesTask;

namespace ITTPTestWebApp
{
    static class App
    {
        #region fields 
        private static readonly CancellationTokenSource _Cts = new CancellationTokenSource();
        public static readonly CancellationToken Ct = _Cts.Token;
        #endregion

        [STAThread]
        public static async Task Main()
        {
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; _Cts.Cancel(); };

            if (!(await FilesChecker.Instance.FilesCheck())) { return; }
            if (!(await DataManager.Instance.Load())) { return; }
            EventManager.Instance.TriggerEvent(Event.ResourceStart);
            Server.Instance.Start();

            Task servicesTaskExecutorTask = ServicesTaskExecutor.Instance.ExecuteAsync();
            await Task.WhenAll(servicesTaskExecutorTask);

            Server.Instance.Stop();
            EventManager.Instance.TriggerEvent(Event.ResourceStop);
            await DataManager.Instance.Unload();

            _Cts.Dispose();
        }
    }
}