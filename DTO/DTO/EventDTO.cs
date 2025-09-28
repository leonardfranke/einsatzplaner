namespace DTO
{
    public class EventDTO
    {
        public string Id { get; set; }
        public string DepartmentId { get; set; }
        public string GroupId { get; set; }
        public string EventCategoryId { get; set; }
        public DateTime Date { get; set; }
        public string? LocationId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationText { get; set; }
    }
}
