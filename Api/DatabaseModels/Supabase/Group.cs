using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("Groups")]
    public class Group : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string Id { get; set; }

        [Column]
        public string DepartmentId { get; set; }

        [Column]
        public string Name { get; set; }
    }
}
