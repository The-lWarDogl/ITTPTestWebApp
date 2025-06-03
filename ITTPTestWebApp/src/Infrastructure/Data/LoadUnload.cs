using ITTPTestWebApp.Logging;

namespace ITTPTestWebApp.Data
{
    partial class DataManager
    {
        public async Task<bool> Load()
        {
            try
            {
                await _ServiceTasks.Load(); 
                await _Users.Load(); 

                return true;
            }
            catch (Exception ex) { Logger.Instance.Log(ex); return false; }
        }

        public async Task Unload()
        {
            try
            {
                await _ServiceTasks.Sync();
                await _Users.Sync();
            }
            catch (Exception ex) { Logger.Instance.Log(ex); }
        }
    }
}
