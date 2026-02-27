using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("Departments")]
    public class Department : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string Id { get; set; }

        [Column()]
        public string Name { get; set; }

        [Column()]
        public string URL { get; set; }
    }
}
