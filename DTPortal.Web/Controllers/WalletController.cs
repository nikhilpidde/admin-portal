using Microsoft.AspNetCore.Mvc;
using DTPortal.Core.Utilities;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
namespace DTPortal.Web.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletConfigurationService _walletConfigurationService;
        public WalletController(IWalletConfigurationService walletConfigurationService)
        {
            _walletConfigurationService = walletConfigurationService;
        }
        public async Task<IActionResult> GetWalletConfiguration()
        {
            var walletConfiguration= await _walletConfigurationService.GetWalletConfigurationDetails();
            return Ok(new APIResponse()
            {
                Success=walletConfiguration.Success,
                Message=walletConfiguration.Message,
                Result=walletConfiguration.Resource
            });
        }
    }
}
