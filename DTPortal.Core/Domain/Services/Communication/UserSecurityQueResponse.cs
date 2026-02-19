using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class UserSecurityQueResponse : BaseResponse<UserSecurityQue>
    {
        public UserSecurityQueResponse(UserSecurityQue category) : base(category) { }

        public UserSecurityQueResponse(string message) : base(message) { }
    }
}
