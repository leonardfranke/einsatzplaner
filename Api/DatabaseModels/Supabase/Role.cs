using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("Roles")]
    public class Role : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string Id { get; set; }

        [Column]
        public string DepartmentId { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public int LockingPeriod { get; set; }

        [Column]
        public bool IsFree { get; set; }
    }
}
