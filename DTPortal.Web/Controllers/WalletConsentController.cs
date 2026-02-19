using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletConsentController : ControllerBase
    {
        private readonly IWalletConsentService _walletConsentService;
        public WalletConsentController(IWalletConsentService walletConsentService)
        { 
            _walletConsentService = walletConsentService;
        }
        [HttpGet]
        [Route("GetConsentList")]
        public async Task<IActionResult> GetConsentList()
        {
            var result=await _walletConsentService.GetAllConsentAsync();

            return Ok(new APIResponse()
            {
                Success = result.Success,
                Message = result.Message,
                Result = result.Resource
            });
        }
        [HttpGet]
        [Route("GetActiveConsentList")]
        public async Task<IActionResult> GetActiveConsentList()
        {
            var result = await _walletConsentService.GetActiveConsentAsync();

            return Ok(new APIResponse()
            {
                Success = result.Success,
                Message = result.Message,
                Result = result.Resource
            });
        }
        [HttpGet]
        [Route("GetConsentsByUserId")]
        public async Task<IActionResult> GetConsentsByUserId(string Id)
        {
            var result = await _walletConsentService.GetConsentsByUserIdAsync(Id);

            return Ok(new APIResponse()
            {
                Success = result.Success,
                Message = result.Message,
                Result = result.Resource
            });
        }
        [HttpGet]
        [Route("GetActiveConsentsByUserId")]
        public async Task<IActionResult> GetActiveConsentsByUserId(string Id)
        {
            var result = await _walletConsentService.GetActiveConsentsByUserIdAsync(Id);

            return Ok(new APIResponse()
            {
                Success = result.Success,
                Message = result.Message,
                Result = result.Resource
            });
        }
        [HttpPost]
        [Route("AddConsent")]
        public async Task<IActionResult> AddConsent([FromBody] WalletConsentDTO walletConsentDTO)
        {
            var result = await _walletConsentService.AddConsent(walletConsentDTO);

            return Ok(new APIResponse()
            {
                Success = result.Success,
                Message = result.Message,
                Result = result.Resource
            });
        }
        [HttpGet]
        [Route("RevokeConsent")]
        public async Task<IActionResult> RevokeConsent(int id)
        {
            var result = await _walletConsentService.RevokeConsent(id);

            return Ok(new APIResponse()
            {
                Success = result.Success,
                Message = result.Message,
                Result = result.Resource
            });
        }
    }
}
