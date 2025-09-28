namespace Web.Models
{
    public class Location
    {
        public string Id { get; set; }
        public string DepartmentId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }
    }
}
