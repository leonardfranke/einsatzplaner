using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    public enum HelperStatus : int
    {
        Locked,
        Preselected,
        Available,
        NotAvailable,
        RequirementDeleted 
    }

    [Table("HelperNotification")]
    public class HelperNotification : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string EventId { get; set; }

        [Column]
        public string RoleId { get; set; }

        [Column]
        public Dictionary<string, HelperStatus> PreviousStatus { get; set; } = new();

        [Column]
        public Dictionary<string, HelperStatus> NewStatus { get; set; } = new();
    }

    [Table("EventNotification")]
    public class EventNotification : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string EventId { get; set; }

        [Column]
        public DateTimeOffset? PreviousDate { get; set; }

        [Column]
        public DateTimeOffset? NewDate { get; set; }

        [Column]
        public List<string> Members { get; set; } = new();
    }

    [Table("DeletionNotification")]
    public class DeletionNotification : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string EventId { get; set; }

        [Column]
        public string Group { get; set; }

        [Column]
        public string EventCategory { get; set; }

        [Column]
        public DateTime Date { get; set; }

        [Column]
        public List<string> Members { get; set; } = new();
    }
}
