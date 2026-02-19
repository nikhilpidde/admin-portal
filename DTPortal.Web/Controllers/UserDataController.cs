using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class UserDataController : Controller
    {
        private readonly ICredentialService _credentialService;
        public UserDataController(ICredentialService credentialService)
        {
            _credentialService = credentialService;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserProfile(string userId, string credentialId)
        {
            var response = await _credentialService.GetUserProfile(userId, credentialId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }

    }
}
