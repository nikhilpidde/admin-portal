using DocumentFormat.OpenXml.Wordprocessing;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.ESealRegistration;
using DTPortal.Web.ViewModel.WalletConfiguration;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{ 
    public class WalletConfigurationController : Controller
    {
        private readonly IWalletConfigurationService _walletConfigurationService;
        public WalletConfigurationController(IWalletConfigurationService walletConfigurationService)
        {
            _walletConfigurationService = walletConfigurationService;
        }
        public async Task<IActionResult> GetWalletConfiguartion()
        {
            var result = await _walletConfigurationService.GetWalletConfiguration();
            var apiResponse = new APIResponse()
            {
                Success = result.Success,
                Message = result.Message,
                Result = result.Resource
            };
            return Ok(apiResponse);
        }

        //public async Task<IActionResult> UpdateWalletConfiguartion([FromBody] WalletConfigurationDTO walletConfigurationDTO)
        //{
        //    var result = await _walletConfigurationService.UpdateWalletConfiguration(walletConfigurationDTO);
        //    var apiResponse = new APIResponse()
        //    {
        //        Success = result.Success,
        //        Message = result.Message,
        //        Result = result.Resource
        //    };
        //    return Ok(apiResponse);
        //}

        public async Task<IActionResult> Index()
        {
            var result = await _walletConfigurationService.GetConfiguration();
            if (result == null)
            {
                return NotFound();

            }
            var gy = (WalletConfigurationResponse)result.Resource;
            WalletConfigurationViewModel walletconfig = new WalletConfigurationViewModel
            {
                BindingMethods = gy.BindingMethods,
                CredentialFormats = gy.CredentialFormats,
            };
            return View(walletconfig);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm]WalletConfigurationViewModel model)
        {
            
            foreach (var credential in model.CredentialFormats)
            {
                // Process and update `isSelected` based on submitted data
                credential.isSelected = credential.isSelected;
            }

            foreach (var bindingMethod in model.BindingMethods)
            {
                if (bindingMethod.SupportedMethods!=null){
                    foreach (var method in bindingMethod.SupportedMethods)
                    {
                        // Update `isSelected` for each supported method
                        method.isSelected = method.isSelected;
                        //method.isSelected = method.DisplayName == bindingMethod.SelectedMethod;

                    }
                }
            }
            WalletConfigurationResponse walletconfig = new WalletConfigurationResponse()
            {
                BindingMethods = model.BindingMethods,
                CredentialFormats = model.CredentialFormats,
            };

            var response = await _walletConfigurationService.UpdateWalletConfiguration(walletconfig);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { IsSuccess = false, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }

            // Pass the updated model back to the view
            return RedirectToAction("Index"); // Ensure the view name matches your view
        }

    }
}
