using Api.Converter;
using Api.Models;
using DTO;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace Api.Manager
{
    public class GroupManager : IGroupManager
    {
        private Client _supabaseClient;

        public GroupManager(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public Task Delete(string departmentId, string groupId)
        {
            return _supabaseClient
                .From<Group>()
                .Where(group => group.DepartmentId == departmentId && group.Id == groupId)
                .Limit(1)
                .Delete();
        }

        public async IAsyncEnumerable<GroupDTO> GetAll(string departmentId)
        {
            var res = await _supabaseClient.From<Group>().Where(role => role.DepartmentId == departmentId).Get();
            foreach (var group in res.Models)
            {
                var members = await GetGroupMembers(departmentId, group.Id);
                yield return GroupConverter.Convert(group, members);
            }
        }

        private async Task<List<string>> GetGroupMembers(string departmentId, string groupId)
        {
            var res = await _supabaseClient
                .From<MemberGroupJoin>()
                .Select(nameof(MemberGroupJoin.MemberId))
                .Where(join => join.DepartmentId == departmentId && join.GroupId == groupId)
                .Get();

            return res.Models.Select(join => join.MemberId).ToList();
        }

        public Task UpdateOrCreate(string departmentId, string? groupId, string name)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                var newGroup = new Group
                {
                    Name = name,
                    DepartmentId = departmentId,
                };

                return _supabaseClient.From<Group>().Insert(newGroup);
            }
            else
            {
                return _supabaseClient
                    .From<Group>()
                    .Where(group => group.Id == groupId && group.DepartmentId == departmentId)
                    .Limit(1)                    
                    .Set(role => role.Name, name)
                    .Update();
            }
        }

        public async Task UpdateGroupMembers(string departmentId, string groupId, UpdateMembersListDTO updateMembersList)
        {
            if (updateMembersList.NewMembers.Any())
            {
                await _supabaseClient
                    .From<MemberGroupJoin>()
                    .Insert(updateMembersList.NewMembers
                        .Select(newMember => new MemberGroupJoin
                        {
                            DepartmentId = departmentId,
                            GroupId = groupId,
                            MemberId = newMember
                        }).ToList());
            }

            if (updateMembersList.FormerMembers.Any())
            {
                await _supabaseClient
                    .From<MemberGroupJoin>()
                    .Where(join => join.DepartmentId == departmentId && join.GroupId == groupId)
                    .Filter(nameof(MemberGroupJoin.MemberId), Operator.In, updateMembersList.FormerMembers)
                    .Delete();
            }
        }

        public Task<Group> GetById(string departmentId, string groupId)
        {
            return _supabaseClient.From<Group>().Where(group => group.Id == groupId && group.DepartmentId == departmentId).Single();
        }
    }
}
