using System.Net;

using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Data;

namespace ITTPTestWebApp.Initialization.FilesCheck
{
    partial class FilesChecker
    {
        private void CheckConnectionUrl()
        {
            using HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(Config.Instance.Read("ServerConnectionUrl"));
            try
            { httpListener.Start(); httpListener.Stop();
              Logger.Instance.Log($"ServerConnectionUrl is correct", tag: "filescheck"); }
            catch (Exception) { throw new Exception("ServerConnectionUrl is invalid"); }
        }
    }
}