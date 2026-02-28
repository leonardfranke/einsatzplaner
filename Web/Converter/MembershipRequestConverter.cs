using Api.Models;
using Web.Models;

namespace Web.Converter
{
    public static class MembershipRequestConverter
    {
        public static List<MembershipRequest> Convert(List<MembershipRequestDTO> requests)
        {
            return requests.Select(Convert).ToList();
        }

        public static MembershipRequest Convert(MembershipRequestDTO? request)
        {
            if (request == null)
                return null;

            return new MembershipRequest
            {
                UserId = request.UserId,
                UserName = request.UserName
            };
        }
    }
}
