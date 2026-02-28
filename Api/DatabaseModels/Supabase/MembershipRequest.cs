using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("MembershipRequests")]
    public class MembershipRequest : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string UserId { get; set; }

        [Column] 
        public string UserName { get; set; }
    }
}
