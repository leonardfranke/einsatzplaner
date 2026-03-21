using Api.Converter;
using Api.Models;
using DTO;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace Api.Manager
{
    public class RoleManager : IRoleManager
    {
        private Client _supabaseClient;
        private IQualificationManager _qualificationManager;

        public RoleManager(Client supabaseClient, IQualificationManager qualificationManager)
        {
            _supabaseClient = supabaseClient;
            _qualificationManager = qualificationManager;
        }

        public Task Delete(string departmentId, string roleId)
        {
            return _supabaseClient
                .From<Role>()
                .Where(role => role.DepartmentId == departmentId && role.Id == roleId)
                .Limit(1)
                .Delete();
        }

        public async IAsyncEnumerable<RoleDTO> GetAll(string departmentId)
        {
            var res = await _supabaseClient.From<Role>().Where(role => role.DepartmentId == departmentId).Get();
            foreach (var role in res.Models)
            {
                var members = await GetRoleMembers(departmentId, role.Id);
                yield return RoleConverter.Convert(role, members);
            }
        }

        public Task UpdateOrCreate(string departmentId, string? roleId, string? newName, int? newLockingPeriod, bool? newIsFree)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                if(newName == null || newLockingPeriod == null || newIsFree == null)
                    throw new ArgumentNullException("Name, LockingPeriod or IsFree were null when creating a new role");
                var newRole = new Role
                {
                    DepartmentId = departmentId,
                    Name = newName,
                    LockingPeriod = newLockingPeriod.Value,
                    IsFree = newIsFree.Value
                };

                return _supabaseClient.From<Role>().Insert(newRole);
            }
            else
            {
                var query = _supabaseClient
                    .From<Role>()
                    .Where(role => role.Id == roleId && role.DepartmentId == departmentId)
                    .Limit(1);

                var updates = new Dictionary<string, object>();
                if (newName != null)
                    query = query.Set(role => role.Name, newName);
                if (newLockingPeriod != null)
                    query = query.Set(role => role.LockingPeriod, newLockingPeriod);
                if (newIsFree != null)
                    query = query.Set(role => role.IsFree, newIsFree);

                return query.Update();                
            }
        }

        public async Task UpdateRoleMembers(string departmentId, string roleId, UpdateMembersListDTO updateMembersList)
        {
            if(updateMembersList.NewMembers.Any())
            {
                await _supabaseClient
                    .From<MemberRoleJoin>()
                    .Insert(updateMembersList.NewMembers
                        .Select(newMember => new MemberRoleJoin
                        {
                            DepartmentId = departmentId,
                            RoleId = roleId,
                            MemberId = newMember
                        }).ToList());
            }

            if(updateMembersList.FormerMembers.Any())
            {
                await _supabaseClient
                    .From<MemberRoleJoin>()
                    .Where(join => join.DepartmentId == departmentId && join.RoleId == roleId)
                    .Filter(nameof(MemberRoleJoin.MemberId), Operator.In, updateMembersList.FormerMembers)
                    .Delete();
            }
        }

        public async Task<List<string>> GetRoleMembers(string departmentId, string roleId)
        {
            var res = await _supabaseClient
                .From<MemberRoleJoin>()
                .Select(nameof(MemberRoleJoin.MemberId))
                .Where(join => join.DepartmentId == departmentId && join.RoleId == roleId)
                .Get();
            
            return res.Models.Select(join => join.MemberId).ToList();
        }

        public Task<Role?> GetRole(string departmentId, string roleId)
        {
            return _supabaseClient.From<Role>().Where(role => role.Id == roleId && role.DepartmentId == departmentId).Single();
        }
    }
}
