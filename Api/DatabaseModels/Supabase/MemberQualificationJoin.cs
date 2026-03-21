using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("MemberQualification")]
    public class MemberQualificationJoin : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string MemberId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string QualificationId { get; set; }

        [Column]
        public string RoleId { get; set; }
    }
}