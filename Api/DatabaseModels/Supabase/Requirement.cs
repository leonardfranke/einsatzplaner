using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("Requirements")]
    public class Requirement : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string EventId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string RoleId { get; set; }

        [Column]
        public int RequiredAmount { get; set; }

        [Column]
        public DateTimeOffset LockingTime { get; set; }

        [Column]
        public List<string> RecommendedGroups { get; set; }

    }
}
