using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userService;
        public UsersController(IUserManagementService userManagementService)
        {
            _userService = userManagementService;
        }
        [Route("GetAdminUsersList")]
        [HttpGet]
        public async Task<IActionResult> GetAdminUsersList()
        {
            var users = await _userService.ListUsersAsync();
            if(users == null)
            {
                return Ok(new APIResponse()
                {
                    Success = false,
                    Message = "Internal Error"
                }); ;
            }
            List<string> AdminMails=new List<string>();
            foreach(var user in users)
            {
                if (user.RoleId == 36)
                {
                    AdminMails.Add(user.MailId);
                }
            }
            return Ok(new APIResponse()
            {
                Success = true,
                Message = "Get Admin List Success",
                Result = AdminMails
            });

        }
    }
}
