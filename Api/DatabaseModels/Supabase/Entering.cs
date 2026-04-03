using DTO;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("Enterings")]
    public class Entering : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string EventId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string RoleId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string MemberId { get; set; }

        [Column]
        public EnteringType EnteringType { get; set; }
    }
}
