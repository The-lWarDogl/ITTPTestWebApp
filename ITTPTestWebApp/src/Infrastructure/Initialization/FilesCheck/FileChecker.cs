using ITTPTestWebApp.Logging;

namespace ITTPTestWebApp.Initialization.FilesCheck
{
    partial class FilesChecker
    {
        public static readonly FilesChecker Instance = new FilesChecker();
        private FilesChecker() { }

        public async Task<bool> FilesCheck()
        {
            Logger.Instance.Log($"Start", tag: "filescheck");
            try
            {
                CheckConfig();
                await CheckDB();
                CheckConnectionUrl();
                return true;
            }
            catch (Exception ex) { Logger.Instance.Log($"Fail : {ex.Message}", tag: "filescheck"); return false; }
            finally { Logger.Instance.Log($"End", tag: "filescheck"); }
        }
    }
}
