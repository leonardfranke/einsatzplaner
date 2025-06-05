namespace DTO
{
    public class UpdateRoleDTO
    {
        public string? RoleId { get; set; }
        public string DepartmentId { get; set; }
        public string? NewName { get; set; }
        public int? NewLockingPeriod { get; set; }
        public bool? NewIsFree { get; set; }
    }
}
