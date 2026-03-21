using Api.Converter;
using Api.Models;
using DTO;
using Supabase;

namespace Api.Manager
{
    public class QualificationManager : IQualificationManager
    {
        private Client _supabaseClient;

        public QualificationManager(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public Task Delete(string departmentId, string qualificationId)
        {
            return _supabaseClient
                .From<Qualification>()
                .Where(qual => qual.DepartmentId == departmentId && qual.Id == qualificationId)
                .Limit(1)
                .Delete();
        }

        public async IAsyncEnumerable<QualificationDTO> GetAll(string departmentId)
        {
            var res = await _supabaseClient.From<Qualification>().Where(qual => qual.DepartmentId == departmentId).Get();
            foreach (var qualification in res.Models)
            {
                var members = await GetQualificationMembers(departmentId, qualification.Id);
                yield return QualificationConverter.Convert(qualification, members);
            }
        }

        public Task UpdateOrCreate(string departmentId, string? roleId, string? qualificationId, string? newName)
        {
            if (string.IsNullOrEmpty(qualificationId))
            {
                if(newName == null || roleId == null)
                    throw new ArgumentNullException("Name or roleId were null when creating a new qualification");
                var newQualification = new Qualification
                {
                    DepartmentId = departmentId,
                    Name = newName,   
                    RoleId = roleId,
                };

                return _supabaseClient.From<Qualification>().Insert(newQualification);
            }
            else
            {
                var query = _supabaseClient
                    .From<Qualification>()
                    .Where(qual => qual.Id == roleId && qual.DepartmentId == departmentId)
                    .Limit(1);

                if (newName != null)
                {
                    query = query.Set(qual => qual.Name, newName);
                    return query.Update();
                }

                return Task.CompletedTask;
            }
        }

        public async Task UpdateRoleMembers(string departmentId, string roleId, string qualificationId, UpdateMembersListDTO updateMembersList)
        {
            await _supabaseClient
                .From<MemberQualificationJoin>()
                .Insert(updateMembersList.NewMembers
                    .Select(newMember => new MemberQualificationJoin
                    {
                        DepartmentId = departmentId,
                        RoleId = roleId,
                        QualificationId = qualificationId,
                        MemberId = newMember
                    }).ToList());

            await _supabaseClient
                .From<MemberQualificationJoin>()
                .Where(join => join.DepartmentId == departmentId && join.QualificationId == qualificationId && updateMembersList.FormerMembers.Contains(join.MemberId))
                .Delete();
        }

        public async Task<List<string>> GetQualificationMembers(string departmentId, string qualification)
        {
            var res = await _supabaseClient
                .From<MemberQualificationJoin>()
                .Select(nameof(MemberQualificationJoin.MemberId))
                .Where(join => join.DepartmentId == departmentId && join.QualificationId == qualification)
                .Get();

            return res.Models.Select(join => join.MemberId).ToList();
        }

        public async Task RemoveMembersFromQualifications(string departmentId, string roleId, IEnumerable<string>? members)
        {
            await _supabaseClient
                .From<MemberQualificationJoin>()
                .Where(join => join.DepartmentId == departmentId && join.RoleId == roleId && members.Contains(join.MemberId))
                .Delete();
        }
    }
}
