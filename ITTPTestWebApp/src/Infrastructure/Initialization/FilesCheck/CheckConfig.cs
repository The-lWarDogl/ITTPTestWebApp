using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Data;

namespace ITTPTestWebApp.Initialization.FilesCheck
{
    partial class FilesChecker
    {
        private static List<string> _ConfigKeys = new List<string>()
        {
            "DBConnectionString",
            "ServerConnectionUrl",
            "Jwt_Issuer",
            "Jwt_Audience",
            "Jwt_SecretKey"
        };

        private void CheckConfig()
        {
            var configData = Config.Instance.ReadAll();
            if (!configData.Keys.Any())
            { throw new Exception("config.json does not exist or could not be read"); }
            else if (!_ConfigKeys.All(configData.ContainsKey))
            { throw new Exception("config.json does not contain keys: " +
              string.Join(", ", _ConfigKeys.Where(key => !configData.ContainsKey(key)).ToList())); }
            else { Logger.Instance.Log($"config.json is correct", tag: "filescheck"); }
        }
    }
}
