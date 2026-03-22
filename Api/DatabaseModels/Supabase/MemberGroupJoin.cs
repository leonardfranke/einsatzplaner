using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("MemberGroup")]
    public class MemberGroupJoin : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string MemberId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string GroupId { get; set; }
    }
}
