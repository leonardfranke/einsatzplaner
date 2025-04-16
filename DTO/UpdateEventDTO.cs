namespace DTO
{
    public class UpdateEventDTO
    {
        public string DepartmentId { get; set; }
        public string? EventId { get; set; }
        public string? GroupId { get; set; }
        public string? EventCategoryId { get; set; }
        public DateTime? Date { get; set; }
        public List<UpdateHelperDTO>? Helpers { get; set; }
        public bool RemoveMembers { get; set; }
    }
}
