namespace DTO
{
    public class EventDTO
    {
        public string Id { get; set; }
        public string DepartmentId { get; set; }
        public string GroupId { get; set; }
        public string EventCategoryId { get; set; }
        public DateTime Date { get; set; }
        public Geolocation? Place { get; set; }
    }
}
