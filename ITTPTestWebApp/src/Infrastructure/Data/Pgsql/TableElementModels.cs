namespace ITTPTestWebApp.Data.Pgsql.TableElements
{
    [Table("servicetasks")]
    public class ServiceTaskTableElement
    {
        [Column("id", isPrimaryKey: true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Column("tasktype")]
        public string TaskType { get; set; } = "None";
        [Column("parameters")]
        public string Parameters { get; set; } = "{}";
        [Column("scheduledtime")]
        public string ScheduledTime { get; set; } = $"{0}";

        public ServiceTaskTableElement(string id, string taskType, string parameters, string scheduledTime)
        { Id = id; TaskType = taskType; Parameters = parameters; ScheduledTime = scheduledTime; }
    }

    [Table("users")]
    public class UserTableElement
    {
        [Column("id", isPrimaryKey: true)]
        public string Id { get; set; }
        [Column("login")]
        public string Login { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("name")]
        public string Name { get; set; }
        /// <summary>
        /// 0 — женщина, 1 — мужчина, 2 — неизвестно
        /// </summary>
        [Column("gender")]
        public int Gender { get; set; }
        [Column("birthday")]
        public DateTime? Birthday { get; set; } = null;
        [Column("admin")]
        public bool Admin { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; }
        [Column("createdby")]
        public string CreatedBy { get; set; }
        [Column("modifiedon")]
        public DateTime ModifiedOn { get; set; }
        [Column("modifiedby")]
        public string ModifiedBy { get; set; }
        [Column("revokedon")]
        public DateTime? RevokedOn { get; set; } = null;
        [Column("revokedby")]
        public string? RevokedBy { get; set; } = null;

        public UserTableElement
        (
            string id, string login, string password, string name, int gender, DateTime? birthday, bool admin,
            DateTime createdOn, string createdBy, DateTime modifiedOn, string modifiedBy, DateTime? revokedOn, string? revokedBy
        )
        {
            Id = id; Login = login; Password = password; Name = name; Gender = gender; Birthday = birthday; Admin = admin;
            CreatedOn = createdOn; CreatedBy = createdBy; ModifiedOn = modifiedOn; ModifiedBy = modifiedBy; RevokedOn = revokedOn; RevokedBy = revokedBy;
        }
    }
}
