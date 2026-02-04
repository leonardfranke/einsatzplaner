using Microsoft.AspNetCore.Components;
using Web.Models;

namespace Web.Extensions
{
    public static class MemberExtensions
    {
        public static MarkupString GetMemberName(this Member member)
        {
            if(member.IsDummy)
                return new MarkupString($"<i>{member.Name}</i>");
            return new MarkupString(member.Name);
        }
    }
}
