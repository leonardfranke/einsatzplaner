using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("Members")]
    public class Member : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string Id { get; set; }

        [Column]
        public string DepartmentId { get; set; }

        [Column()]
        public string Name { get; set; }

        [Column()]
        public bool IsAdmin { get; set; }

        [Column()]
        public bool IsDummy { get; set; }

        [Column()]
        public bool EmailNotificationActive { get; set; }
    }
}
