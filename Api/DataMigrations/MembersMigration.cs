using Api.FirestoreModels;
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

            var rolesSnap = await _firestoreDb.Collection("Department").Document(departmentId).Collection("Role").GetSnapshotAsync();
            foreach (var roleDoc in rolesSnap)
            {
                var role = roleDoc.ConvertTo<RoleOld>();
                if (role == null)
                    continue;

                var newRole = new Role
                {
                    Id = role.Id,
                    Name = role.Name,
                    DepartmentId = departmentId,
                    IsFree = role.IsFree,
                    LockingPeriod = role.LockingPeriod,
                };
                await _supabaseClient.From<Role>().Insert(newRole);

                foreach(var member in role.MemberIds.Intersect(memberIds))
                {
                    var newMemberRoleJoin = new MemberRoleJoin
                    {
                        RoleId = role.Id,
                        MemberId = member,
                        DepartmentId = departmentId                        
                    };
                    await _supabaseClient.From<MemberRoleJoin>().Insert(newMemberRoleJoin);
                }
            }

            var qualificationSnap = await _firestoreDb.Collection("Department").Document(departmentId).Collection("Qualification").GetSnapshotAsync();
            foreach (var qualDoc in qualificationSnap)
            {
                var qual = qualDoc.ConvertTo<QualificationOld>();
                if (qual == null)
                    continue;

                var newQual = new Qualification
                {
                    DepartmentId = departmentId,
                    Id = qual.Id,
                    Name = qual.Name,
                    RoleId = qual.RoleId
                };
                await _supabaseClient.From<Qualification>().Insert(newQual);

                foreach (var member in qual.MemberIds.Intersect(memberIds))
                {
                    var newMemberQualJoin = new MemberQualificationJoin
                    {
                        RoleId = qual.RoleId,
                        DepartmentId = departmentId,
                        MemberId = member,
                        QualificationId = qual.Id
                    };
                    await _supabaseClient.From<MemberQualificationJoin>().Insert(newMemberQualJoin);
                }
            }
        }
    }
}
