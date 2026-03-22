using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("EventCategories")]
    public class EventCategory : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string Id { get; set; }

        [Column]
        public string DepartmentId { get; set; }

        [Column]
        public string Name { get; set; }
    }
}
