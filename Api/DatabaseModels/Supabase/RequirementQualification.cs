using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("QualificationRequirements")]
    public class QualificationRequirement : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string EventId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string RoleId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string QualificationId { get; set; }

        [Column]
        public int RequiredAmount { get; set; }

    }
}
