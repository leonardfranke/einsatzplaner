using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("CalendarToken")]
    public class CalendarToken : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string MemberId { get; set; }

        [Column()]
        public string Token { get; set; }
    }
}