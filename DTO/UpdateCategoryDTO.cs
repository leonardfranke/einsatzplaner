namespace DTO
{
    public class UpdateRoleDTO
    {
        public string? RoleId { get; set; }
        public string DepartmentId { get; set; }
        public string Name { get; set; }
        public int LockingPeriod { get; set; }
    }
}
