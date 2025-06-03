using System.Collections.Concurrent;
using System.Reflection;

using ITTPTestWebApp.Common;
using ITTPTestWebApp.Logging;

namespace ITTPTestWebApp.Data.Pgsql
{
    class PeriodicTableSynchronizedDictionary<TKey, TValue, TValueTableElement>
        where TKey : notnull
        where TValue : ICloneable
        where TValueTableElement : class
    {
        private static readonly CancellationToken _Ct = App.Ct;
        private readonly TablePgsql<TValueTableElement> _TablePgsql;
        private readonly TimeSpan _SyncInterval = TimeSpan.FromMinutes(5);
        
        public ConcurrentDictionary<TKey, TValue> ConcurrentDictionary { get; private set; } = new();

        public PeriodicTableSynchronizedDictionary(TablePgsql<TValueTableElement> tablePgsql, TimeSpan? syncInterval = null)
        { _TablePgsql = tablePgsql; if (syncInterval != null) { _SyncInterval = (TimeSpan)syncInterval; } }

        public async Task Load()
        {
            ConcurrentDictionary = new((await _TablePgsql.GetAllAsync())
                .Where(el => el != null)
                .Select(el => ConvertToTValue(el))
                .ToDictionary(el => GetKeyProperty(el), el => el));

            StartSynchronization();
        }

        public async Task Sync()
        { try { await _TablePgsql.ReplaceAllAsync(ConcurrentDictionary.Values.Select(el => ConvertToTValueTableElement(el))); } catch (Exception ex) { Logger.Instance.Log(ex); } }

        private void StartSynchronization()
        {
            _ = Task.Run(async () => 
            {
                while (!_Ct.IsCancellationRequested)
                { try { await Task.Delay(_SyncInterval, _Ct); } catch (TaskCanceledException) { break; } await Sync(); }
            });
        }

        private static TKey GetKeyProperty(TValue obj)
        {
            var keyProperty = typeof(TValue).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyPropertyAttribute)));
            if (keyProperty == null) { throw new InvalidOperationException($"Type {typeof(TValue)} must have a property with [KeyProperty] attribute."); }
            return (TKey)keyProperty.GetValue(obj)!;
        }

        private static MethodInfo MethodInfoConvertToTValue;
        private static MethodInfo MethodInfoConvertToTValueTableElement;
        static PeriodicTableSynchronizedDictionary()
        {
            var method1 = typeof(TValue).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Explicit"
                    && m.ReturnType == typeof(TValue)
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(TValueTableElement));
            if (method1 != null) { MethodInfoConvertToTValue = method1; }
            else { throw new InvalidCastException($"No explicit conversion found from {typeof(TValueTableElement)} to {typeof(TValue)}"); }

            var method2 = typeof(TValue).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Explicit"
                    && m.ReturnType == typeof(TValueTableElement)
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(TValue));
            if (method2 != null) { MethodInfoConvertToTValueTableElement = method2; }
            else { throw new InvalidCastException($"No explicit conversion found from {typeof(TValue)} to {typeof(TValueTableElement)}"); }
        }
        private static TValue ConvertToTValue(TValueTableElement element) =>
            (TValue)MethodInfoConvertToTValue.Invoke(null, new object[] { element })!;
        private static TValueTableElement ConvertToTValueTableElement(TValue element) =>
            (TValueTableElement)MethodInfoConvertToTValueTableElement.Invoke(null, new object[] { element })!;
    }
}
