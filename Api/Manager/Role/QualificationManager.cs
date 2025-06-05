using Google.Cloud.Firestore;
using Api.Converter;
using DTO;
using Api.FirestoreModels;

namespace Api.Manager
{
    public class QualificationManager : IQualificationManager
    {
        private FirestoreDb _firestoreDb;

        public QualificationManager(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        private CollectionReference GetQualificationCollectionReference(string departmentId)
        {
            return _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.QUALIFICATION);
        }

        public async Task Delete(string departmentId, string qualificationId)
        {
            var roleCollectionReference = GetQualificationCollectionReference(departmentId);
            var roleRef = roleCollectionReference.Document(qualificationId);
            await roleRef.DeleteAsync();
        }

        public async Task<List<QualificationDTO>> GetAll(string departmentId)
        {
            var roleReference = GetQualificationCollectionReference(departmentId);
            var snapshot = await roleReference.GetSnapshotAsync();
            var qualifications = snapshot.Select(doc => doc.ConvertTo<Qualification>()).ToList();
            return QualificationConverter.Convert(qualifications);
        }

        public async Task<string> UpdateOrCreate(string departmentId, string? roleId, string? qualificationId, string? newName)
        {
            var qualificationReference = GetQualificationCollectionReference(departmentId);
            if (string.IsNullOrEmpty(qualificationId))
            {
                if(newName == null || roleId == null)
                    throw new ArgumentNullException("Name or roleId were null when creating a new qualification");
                var newQualification = new Qualification
                {
                    Name = newName,   
                    RoleId = roleId,
                    MemberIds = new()
                };

                var newQualificationRef = await qualificationReference.AddAsync(newQualification);
                return newQualificationRef.Id;
            }
            else
            {
                var updates = new Dictionary<string, object>();
                if (newName != null)
                {
                    updates.Add(nameof(Qualification.Name), newName);
                    await qualificationReference.Document(qualificationId).UpdateAsync(updates, Precondition.MustExist);
                }
                return roleId;
            }
        }

        public async Task UpdateRoleMembers(string departmentId, string qualificationId, UpdateMembersListDTO updateMembersList)
        {
            var qualificationCollectionReference = GetQualificationCollectionReference(departmentId);
            var qualificationRef = qualificationCollectionReference.Document(qualificationId);
            await qualificationRef.UpdateAsync(nameof(Qualification.MemberIds), FieldValue.ArrayRemove(updateMembersList.FormerMembers.ToArray()));
            await qualificationRef.UpdateAsync(nameof(Qualification.MemberIds), FieldValue.ArrayUnion(updateMembersList.NewMembers.ToArray()));
        }

        public async Task RemoveMembersFromQualifications(string departmentId, string roleId, IEnumerable<string>? members)
        {
            var qualificationCollectionReference = GetQualificationCollectionReference(departmentId);
            var snapshot = await qualificationCollectionReference.WhereEqualTo(nameof(Qualification.RoleId), roleId).GetSnapshotAsync();
            Parallel.ForEach(snapshot.Documents, doc =>
            {
                if(members == null)
                    doc.Reference.UpdateAsync(nameof(Qualification.MemberIds), new List<string>());
                else
                    doc.Reference.UpdateAsync(nameof(Qualification.MemberIds), FieldValue.ArrayRemove(members.ToArray()));
            });
        }
    }
}
