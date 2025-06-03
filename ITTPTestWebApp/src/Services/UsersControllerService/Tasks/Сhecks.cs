using ITTPTestWebApp.Common;
using ITTPTestWebApp.Services.ServicesTask;

namespace ITTPTestWebApp.Services.UsersController
{
    static class ParametersСheck
    {
        private static readonly Dictionary<ServicesTaskType, Dictionary<string, Type>> _ActionsNecessaryKeysAndValueTypes = new ()
        {
            {ServicesTaskType.UsersAdd, new Dictionary<string, Type>()
            {
                { "performedBy", typeof(string) },
                { "login", typeof(string) },
                { "password", typeof(string) },
                { "name", typeof(string) },
                { "gender", typeof(int) },
                { "admin", typeof(bool) }
            }},
            {ServicesTaskType.UsersRemove, new Dictionary<string, Type>()
            {
                { "login", typeof(string) }
            }},
            {ServicesTaskType.UsersUpdate, new Dictionary<string, Type>()
            {
                { "performedBy", typeof(string) },
                { "currentLogin", typeof(string) }
            }},
        };

        public static bool ParametersKeyСheck(Dictionary<string, object> parameters, ServicesTaskType taskType, out string exceptionText) =>
            ActionsParametersСheck.ParametersKeyСheck(_ActionsNecessaryKeysAndValueTypes, parameters, taskType, out exceptionText);

        public static bool ParameterСheck<T>(Dictionary<string, object> parameters, string key) =>
            ActionsParametersСheck.ParameterСheck<T>(parameters, key);
    }
}
