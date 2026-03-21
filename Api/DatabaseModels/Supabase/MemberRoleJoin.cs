using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("MemberRole")]
    public class MemberRoleJoin : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string MemberId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string RoleId { get; set; }
    }
}
