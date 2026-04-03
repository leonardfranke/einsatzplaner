using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.Models
{
    [Table("Events")]
    public class Event : BaseModel
    {
        [PrimaryKey(shouldInsert: true)]
        public string DepartmentId { get; set; }

        [PrimaryKey(shouldInsert: true)]
        public string Id { get; set; }

        [Column]
        public string? GroupId { get; set; }

        [Column]
        public string? EventCategoryId { get; set; }

        [Column]
        public DateTimeOffset Date { get; set; }

        [Column]
        public string? LocationId { get; set; }

        [Column]
        public double? LocationLatitude { get; set; }

        [Column]
        public double? LocationLongitude { get; set; }

        [Column]
        public string? LocationText { get; set; }
    }
}
