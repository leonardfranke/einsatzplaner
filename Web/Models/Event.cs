namespace Web.Models
{
    public class Event
    {
        public string Id { get; set; }
        public string DepartmentId { get; set; }
        public string? GroupId { get; set; }
        public string? EventCategoryId { get; set; }
        public DateTime EventDate { get; set; }
    }
}
