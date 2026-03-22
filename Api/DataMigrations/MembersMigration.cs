using Api.Models;
using Google.Cloud.Firestore;
using Supabase;

namespace Api.DataMigrations
{
    public class Migration
    {
        private Client _supabaseClient;
        private FirestoreDb _firestoreDb;

        public Migration(FirestoreDb firestoreDb, Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
            _firestoreDb = firestoreDb;
        }

        public async Task MigrateRoles()
        {
            var departmentId = "v87avboSu7Dc74ZJpJFk";
            var res = await _supabaseClient.From<Member>().Where(member => member.DepartmentId == departmentId).Get();
            var memberIds = res.Models.Select(member => member.Id);

            var groupsSnap = await _firestoreDb.Collection("Department").Document(departmentId).Collection("Group").GetSnapshotAsync();
            foreach (var groupDoc in groupsSnap)
            {
                var group = groupDoc.ConvertTo<GroupOld>();
                if (group == null)
                    continue;

                var newGroup = new Group
                {
                    Id = group.Id,
                    Name = group.Name,
                    DepartmentId = departmentId
                };
                await _supabaseClient.From<Group>().Insert(newGroup);

                foreach(var member in group.MemberIds.Intersect(memberIds))
                {
                    var newMemberGroupJoin = new MemberGroupJoin
                    {
                        GroupId = group.Id,
                        MemberId = member,
                        DepartmentId = departmentId                        
                    };
                    await _supabaseClient.From<MemberGroupJoin>().Insert(newMemberGroupJoin);
                }
            }

            var categorySnap = await _firestoreDb.Collection("Department").Document(departmentId).Collection("EventCategory").GetSnapshotAsync();
            foreach (var categoryDoc in categorySnap)
            {
                var category = categoryDoc.ConvertTo<EventCategoryOld>();
                if (category == null)
                    continue;

                var newCategory = new EventCategory
                {
                    Id = category.Id,
                    Name = category.Name,
                    DepartmentId = departmentId
                };
                await _supabaseClient.From<EventCategory>().Insert(newCategory);
            }
        }
    }
}
