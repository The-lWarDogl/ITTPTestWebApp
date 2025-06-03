using System.Collections.Concurrent;
using Newtonsoft.Json;

using ITTPTestWebApp.Common;
using ITTPTestWebApp.Initialization;
using ITTPTestWebApp.Data;
using ITTPTestWebApp.Events;
using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Data.Pgsql.TableElements;

namespace ITTPTestWebApp.Services.ServicesTask
{
    abstract class ServiceTaskBase : ICloneable
    {
        #region fields 
        public readonly ServicesTaskType TaskType;
        public readonly ServicesType ServiceType;
        public readonly Func<Task> ExecutionFunc;

        [KeyProperty]
        public Guid Id { get; } = Guid.NewGuid();
        public readonly DateTime ScheduledTime = DateTime.UtcNow;
        public readonly Dictionary<string, object> Parameters = new Dictionary<string, object>();

        public readonly TaskCompletionSource<object?> TCS = new TaskCompletionSource<object?>();
        #endregion

        protected ServiceTaskBase
        (
            ServicesTaskType taskType,
            ServicesType serviceType,
            Func<Dictionary<string, object>, TaskCompletionSource<object?>, Task> executionFunc,
            DateTime? scheduledTime = null, Dictionary<string, object>? parameters = null
        )
        {
            TaskType = taskType;
            ServiceType = serviceType;
            if (scheduledTime != null) ScheduledTime = (DateTime)scheduledTime;
            if (parameters != null) Parameters = parameters;

            ExecutionFunc = () => executionFunc(Parameters, TCS);
        }

        public object Clone() =>
         ServiceTaskFactory.Create(TaskType, Parameters, ScheduledTime);

        public static explicit operator ServiceTaskBase(ServiceTaskTableElement te)
        {
            var taskType = Func.ConvertToEnumElement<ServicesTaskType>(te.TaskType);
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(te.Parameters,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            DateTime scheduledTimeUtcDateTime = DateTime.UtcNow;
            if (long.TryParse(te.ScheduledTime, out long unixMilliseconds))
                scheduledTimeUtcDateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;
            return ServiceTaskFactory.Create(taskType, parameters ?? new(), scheduledTimeUtcDateTime);
        }

        public static explicit operator ServiceTaskTableElement(ServiceTaskBase stb) =>
            new ServiceTaskTableElement(stb.Id.ToString(), stb.TaskType.ToString(),
                JsonConvert.SerializeObject(stb.Parameters, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }),
                (new DateTimeOffset(stb.ScheduledTime)).ToUnixTimeMilliseconds().ToString());
    }

    static class ServiceTaskFactory
    {
        public static ServiceTaskBase Create (ServicesTaskType taskType, Dictionary<string, object> parameters, DateTime scheduledTime)
        {
            switch (taskType)
            {
                case ServicesTaskType.Login: return new LoginServiceTask(parameters);
                case ServicesTaskType.UsersAdd: return new UsersAddServiceTask(parameters);
                case ServicesTaskType.UsersRemove: return new UsersRemoveServiceTask(parameters);
                case ServicesTaskType.UsersUpdate: return new UsersUpdateServiceTask(parameters);
                default: return new NoneServiceTask();
            }
        }
    }

    class ServicesTaskManager : IInitializable
    {
        public static readonly ServicesTaskManager Instance = new ServicesTaskManager();
        private ConcurrentDictionary<Guid, ServiceTaskBase> _Tasks = new ConcurrentDictionary<Guid, ServiceTaskBase>();

        public void Initialize() { Reflection.UseReflection(this); }
        public void Uninitialize() { Reflection.UnUseReflection(this); }

        private ServicesTaskManager() {  }
        ~ServicesTaskManager() { Uninitialize(); }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try { _Tasks = DataManager.Instance.GetServiceTasks(); }
            catch (Exception ex) { Logger.Instance.Log(ex); }
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop() { }

        #region public methods
        public bool TryAdd(ServiceTaskBase task) =>
            _Tasks.TryAdd(task.Id, task);

        public bool TryRemove(Guid id, out ServiceTaskBase? removedTask) =>
            _Tasks.TryRemove(id, out removedTask);

        public IEnumerable<ServicesType> GetAllServicesType() =>
            _Tasks.Values.Select(t => t.ServiceType).Distinct();

        public IEnumerable<ServiceTaskBase> GetTasksForServiceType(ServicesType serviceType) =>
            _Tasks.Values
                .Where(t =>
                {
                    var task = t;
                    return task != null && task.ServiceType == serviceType;
                });
        #endregion
    }

    class ServicesTaskExecutor
    {
        private static readonly Lazy<ServicesTaskExecutor> _LazyInstance = new Lazy<ServicesTaskExecutor>(() => new ServicesTaskExecutor());

        private static readonly CancellationToken _Ct = App.Ct;
        private readonly TimeSpan _PollInterval = TimeSpan.FromMilliseconds(200);

        private ServicesTaskExecutor() { }

        public static ServicesTaskExecutor Instance
        { get { return _LazyInstance.Value; } }

        private static readonly SemaphoreSlim _LockSemExe = new SemaphoreSlim(1, 1);
        public async Task ExecuteAsync()
        {
            //TODO there could be a task balancer here
            await _LockSemExe.WaitAsync();
            try
            {
                while (!_Ct.IsCancellationRequested)
                {
                    foreach (var serviceType in ServicesTaskManager.Instance.GetAllServicesType())
                        foreach (var task in ServicesTaskManager.Instance.GetTasksForServiceType(serviceType))
                            if (task.ScheduledTime <= DateTime.UtcNow)
                            { ServicesTaskManager.Instance.TryRemove(task.Id, out _); _ = task.ExecutionFunc(); }
                    try { await Task.Delay(_PollInterval, _Ct); } catch (TaskCanceledException) { break; }
                }
            }
            finally { _LockSemExe.Release(); }
        }
    }
}
