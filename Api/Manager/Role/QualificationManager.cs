using Api.Converter;
using Api.Models;
using DTO;
using Supabase;
using static Supabase.Postgrest.Constants;

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
            var qualificationResults = await _supabaseClient.From<Qualification>().Where(qual => qual.DepartmentId == departmentId).Get();
            var qualificationIds = qualificationResults.Models.Select(quali => quali.Id).ToList();
            var memberQualificationJoinResult = await _supabaseClient
                .From<MemberQualificationJoin>()
                .Filter(nameof(MemberQualificationJoin.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(MemberQualificationJoin.QualificationId), Operator.In, qualificationIds)
                .Get();
            foreach (var quali in qualificationResults.Models)
            {
                var qualiMembers = memberQualificationJoinResult.Models.Where(join => join.QualificationId == quali.Id).Select(join => join.MemberId).ToList();
                yield return QualificationConverter.Convert(quali, qualiMembers);
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
            if (updateMembersList.NewMembers.Any())
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
            }

            if (updateMembersList.FormerMembers.Any())
            {
                await _supabaseClient
                .From<MemberQualificationJoin>()
                .Where(join => join.DepartmentId == departmentId && join.QualificationId == qualificationId)
                .Filter(nameof(MemberQualificationJoin.MemberId), Operator.In, updateMembersList.FormerMembers)
                .Delete();
            }
        }
    }
}
