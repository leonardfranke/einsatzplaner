using Api.Models;
using Google.Cloud.Firestore;
using Supabase;

namespace Api.DataMigrations
{
    public class MembersMigration
    {
        private Client _supabaseClient;
        private FirestoreDb _firestoreDb;

        public MembersMigration(FirestoreDb firestoreDb, Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
            _firestoreDb = firestoreDb;
        }

        public async Task Migrate()
        {
            var depSnap = await _firestoreDb.Collection("Department").GetSnapshotAsync();

            foreach (var depDoc in depSnap)
            {
                var department = depDoc.ConvertTo<DepartmentOld>();
                if (department == null)
                    continue;

                var newDepartment = new Department
                {
                    Id = department.Id,
                    Name = department.Name,
                    URL = department.URL
                };
                await _supabaseClient.From<Department>().Insert(newDepartment);

                var memSnap = await _firestoreDb.Collection("Department").Document(department.Id).Collection("Member").GetSnapshotAsync();

                foreach(var memDoc in memSnap)
                {
                    var member = memDoc.ConvertTo<MemberOld>();
                    if (member == null)
                        continue;

                    var newMember = new Member
                    {
                        DepartmentId = department.Id,
                        Id = member.Id,
                        Name = member.Name,
                        IsAdmin = member.IsAdmin,
                        IsDummy = member.IsDummy,
                        EmailNotificationActive = member.EmailNotificationActive
                    };
                    await _supabaseClient.From<Member>().Insert(newMember);
                }
            }

        }
    }
}
