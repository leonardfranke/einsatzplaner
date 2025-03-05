using Api.Models;

namespace Api.Converter
{
    public class MembershipRequestConverter
    {
        public static List<MembershipRequestDTO?> Convert(List<MembershipRequest?> requests)
        {
            return requests.Select(Convert).ToList();
        }

        public static MembershipRequestDTO? Convert(MembershipRequest? request)
        {
            if (request == null)
                return null;
            return new MembershipRequestDTO
            {
                Id = request.Id,
                UserId = request.UserId,
                UserName = request.UserName
            };
        }
    }
}
