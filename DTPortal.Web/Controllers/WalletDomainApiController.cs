using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletDomainApiController : ControllerBase
    {
        private readonly IWalletDomainService _walletDomainService;
        public WalletDomainApiController(IWalletDomainService walletDomainService)
        { 
            _walletDomainService = walletDomainService;
        }
        [HttpGet]
        [Route("GetDomainsList")]
        public async Task<IActionResult> GetDomainsList()
        {
            var result = await _walletDomainService.GetWalletDomainsList();
            return Ok(new APIResponse()
            {
                Success=result.Success,
                Message=result.Message,
                Result=result.Resource
            });
        }
    }
}
