namespace Web.Services
{
    public interface IQualificationService
    {
        public Task<List<Models.Qualification>> GetAll(string departmentId);
        public Task<Models.Qualification> GetById(string departmentId, string qualificationId);
        public Task Delete(string departmentId, string qualificationId);
        public Task<string> UpdateOrCreate(string departmentId, string? roleId, string? qualificationId, string? newName);
        public Task UpdateQualificationMembers(string departmentId, string qualificationId, IEnumerable<string> newMembers, IEnumerable<string> formerMembers);
    }
}
