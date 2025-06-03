namespace ITTPTestWebApp.Common
{
    static class ActionsParametersСheck
    {
        public static bool ParametersKeyСheck<TEnum>
        (
            Dictionary<TEnum, Dictionary<string, Type>> actionsNecessaryKeysAndValueTypes,
            Dictionary<string, object> parameters,
            TEnum taskType,
            out string exceptionText
        )
            where TEnum : struct, Enum
        {
            var necessaryKeysAndvalueTypes = actionsNecessaryKeysAndValueTypes.ContainsKey(taskType) ? actionsNecessaryKeysAndValueTypes[taskType] : new ();

            List<string> noKeys = new List<string>();
            List<string> incorrectValuesFormat = new List<string>();
            foreach (var kvp in necessaryKeysAndvalueTypes)
            {
                if (!parameters.ContainsKey(kvp.Key))
                { noKeys.Add(kvp.Key); continue; }
                if (!necessaryKeysAndvalueTypes[kvp.Key].IsAssignableFrom(parameters[kvp.Key].GetType()))
                { incorrectValuesFormat.Add(kvp.Key); }
            }

            exceptionText = $"Parameters does not contain keys: [{string.Join(", ", noKeys)}] " +
                            $"Parameters keys incorrect values format: [{string.Join(", ", incorrectValuesFormat)}]";

            return !noKeys.Any() && !incorrectValuesFormat.Any();
        }

        public static bool ParameterСheck<T>(Dictionary<string, object> parameters, string key) =>
            parameters.ContainsKey(key) && typeof(T).IsAssignableFrom(parameters[key].GetType());
    }
}
