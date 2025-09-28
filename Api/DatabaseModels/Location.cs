using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Api.DatabaseModels
{
    [Table("locations")]
    public class Location : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }
        [Column("departmentId")]
        public string DepartmentId { get; set; }
        [Column("latitude", nullValueHandling: NullValueHandling.Ignore)]
        public double Latitude { get; set; }
        [Column("longitude", nullValueHandling: NullValueHandling.Ignore)]
        public double Longitude { get; set; }
        [Column("name", nullValueHandling: NullValueHandling.Ignore)]
        public string Name { get; set; }
        [Column("originalName", nullValueHandling: NullValueHandling.Ignore)]
        public string OriginalName { get; set; }
    }
}
