using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("Qualifications")]
    public class Qualification : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string Id { get; set; }

        [Column]
        public string DepartmentId { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string RoleId { get; set; }
    }
}
