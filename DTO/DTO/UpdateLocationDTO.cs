namespace DTO
{
    public class UpdateLocationDTO
    {
        public string DepartmentId { get; set; }
        public string? LocationId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Name { get; set; }
        public string? OriginalName { get; set; }
    }
}
