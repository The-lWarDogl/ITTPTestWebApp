using Newtonsoft.Json;

using ITTPTestWebApp.Common;

namespace ITTPTestWebApp.Data
{
    class Config
    {
        public static readonly Config Instance = new Config();

        #region fields
        private readonly string _FilePath = "config.json";
        private readonly Dictionary<string, string> _Data;
        #endregion

        private Config() 
        {
            if (!File.Exists(_FilePath)) _Data = new();
            else _Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(_FilePath)) ?? new();
        }

        public Dictionary<string, string> ReadAll() =>
            Func.DeepCopy(_Data);

        public string Read(string key) =>
            _Data.ContainsKey(key)? _Data[key] : string.Empty;
    }
}
