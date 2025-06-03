namespace ITTPTestWebApp.Network
{
    static partial class ServerCommon
    {
        public static string GetSmartContentType(string baseType)
        {
            var final = baseType;
            if (new[] { "text", "application" }.Any(sub => baseType.Contains(sub, StringComparison.OrdinalIgnoreCase)))
            { final += "; charset=utf-8"; }
            return final;
        }
    }
}
