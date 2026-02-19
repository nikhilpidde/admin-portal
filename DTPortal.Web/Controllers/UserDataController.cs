using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
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
