using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Web.Enums;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel.OperationAuthentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class OperationAuthenticationController : BaseController
    {

        private readonly IOperationAuthenticationService _operationAuthenticationService;
        //private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<OperationAuthenticationController> _logger;
        public IConfiguration Configuration { get; }
        public OperationAuthenticationController(ILogger<OperationAuthenticationController> logger ,ILogClient logClient, IConfiguration configuration, IOperationAuthenticationService operationAuthenticationService /*,IAuthenticationService authenticationService*/) : base(logClient)
        {
            _operationAuthenticationService = operationAuthenticationService;
            //_authenticationService = authenticationService;
            Configuration = configuration;
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> IsAuthenticationRequired(string id)
        {
            try
            {
                var args = new ValidateOperationAuthNRequest
                {
                    userName = UUID,
                    OperationName = id
                };

                var response = await _operationAuthenticationService.ValidateOperationAuthN(args);
                if (response == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Check Is Authentication Required", LogMessageType.FAILURE.ToString(), "Is Authentication Required in Additional Authentication Failed response value getting null" );
                    _logger.LogError("critical operation is authentication required fail getting null response");
                    return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
                }
                if (response.success)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    if (response.result != null)
                    {
                        TempData["TempSession"] = response.result.tempSession;
                        response.result.tempSession = string.Empty;
                        TempData["OperationName"] = id;
                        if (response.result.authenticationScheme.Equals("FIDO2"))
                        {
                            TempData["fido2.assertionOptions"] = response.result.Fido2Options;
                            response.result.tempSession = Configuration["fido2:origin"].ToString();
                        }
                    }
                    return new JsonResult(response);
                }
            }
            catch (Exception e)
            {

                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Check Is Authentication Required", LogMessageType.FAILURE.ToString(), "Is Authentication Required in Additional Authentication Failed : "+e.Message);
                _logger.LogError("critical operation is authentication required fail :{0}", e.Message);
                return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAuthenticationData([FromBody] OperationAuthenticationData model)
        {
            try
            {
                if (!TempData.ContainsKey("TempSession"))
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Verify User Authdata", LogMessageType.FAILURE.ToString(), "Verify Authentication Data in Additional Authentication Failed temporary Session not found");
                    return new JsonResult(new { Success = false, Message = "session Not Found" });
                }

                var tempsession = TempData["TempSession"] as string;
                TempData.Keep("TempSession");

                if (model.authSchemeName.Equals("FIDO2"))
                {
                    var jsonOptions = TempData["fido2.assertionOptions"].ToString();
                    TempData.Keep("fido2.assertionOptions");
                    model.credential = model.credential + "#" + jsonOptions;
                }



                var args = new VerifyOperationAuthDataRequest
                {
                    tempSession = tempsession,
                    authData = model.credential,
                    authNSchemeName = model.authSchemeName
                };

                var response = await _operationAuthenticationService.VerifyOperationAuthData(args);
                if (response == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Verify User Authdata", LogMessageType.FAILURE.ToString(), "Verify Authentication Data in Additional Authentication Failed Response value getting null");
                    _logger.LogError("Verify Authentication Data is  fail getting null response");
                    return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
                }
                if (response.Success)
                {
                    if (!TempData.ContainsKey("OperationName"))
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Verify User Authdata", LogMessageType.SUCCESS.ToString(), "Additional Authentication Success");
                    }
                    else
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Verify User Authdata", LogMessageType.SUCCESS.ToString(), "Additional Authentication Success for operation  " + TempData["OperationName"].ToString());
                    }
                    return new JsonResult(new { success = true });
                }
                else
                {

                    if (!TempData.ContainsKey("OperationName"))
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Verify User Authdata", LogMessageType.FAILURE.ToString(), "Additional Authentication Failed ");
                    }
                    else
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Verify User Authdata", LogMessageType.FAILURE.ToString(), "Additional Authentication Failed for operation = " + TempData["OperationName"].ToString());
                    }
                    TempData.Keep("OperationName");
                    return new JsonResult(response);
                }
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Verify User Authdata", LogMessageType.FAILURE.ToString(), "Verify Authentication Data in Additional Authentication Failed :"+e.Message);
                _logger.LogError("critical operation VerifyOperationAuthData fail"+ e);
                return StatusCode(500,"somethig went wrong! please contact to admin or try again later.");
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> IsUserVerifiedCode()
        //{
        //    try
        //    {
        //        if (!TempData.ContainsKey("TempSession"))
        //        {
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Check Is User VerifiedCode", LogMessageType.FAILURE.ToString(), "Is User Verified Code in Additional Authentication Failed session not found");
        //            return new JsonResult(new { Success = false, Message = "session Not Found" });
        //        }

        //        var session = TempData["TempSession"] as string;
        //        TempData.Keep("TempSession");

        //        var cookiesValue = "";
        //        var hasCookie = HttpContext.Request.Cookies.TryGetValue("verifiedCodeCount", out cookiesValue);
        //        var count = int.Parse(cookiesValue);
        //        count = count + 1;
        //        HttpContext.Response.Cookies.Delete("verifiedCodeCount");
        //        HttpContext.Response.Cookies.Append("verifiedCodeCount", count.ToString());

        //        var response = await _authenticationService.IsUserVerified(session);
        //        if (response == null)
        //        {
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Check Is User VerifiedCode", LogMessageType.FAILURE.ToString(), "Is User Verified Code in Additional Authentication Failed response value getting null");
        //            _logger.LogError("Is User Verified code fail getting null response");
        //            return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
        //        }
        //        if (response.Success)
        //        {
        //            if (response.Status == "success")
        //            {
        //                TempData.Remove("TempSession");
        //            }
        //            if (response.Status != "success")
        //            {
        //                if (count == 6)
        //                {
        //                    return Ok(new { Success = false, Status = response.Status, operation = "stop", Message = "Your are not verifing the code, Please Verify the Code" });
        //                }
        //            }
        //        }

        //        return Ok(response);
        //    }
        //    catch (Exception e)
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Check Is User VerifiedCode", LogMessageType.FAILURE.ToString(), "Is User Verified Code in Additional Authentication Failed : "+e.Message);
        //        _logger.LogError("critical operation IsUserVerified fail", e);
        //        return StatusCode(500,"somethig went wrong! please contact to admin or try again later.");
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> SendPushNotification()
        //{
        //    try
        //    {
        //        if (!TempData.ContainsKey("TempSession"))
        //        {
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Send PushNotification code", LogMessageType.FAILURE.ToString(), "Send PushNotification in Additional Authentication Failed session not found");
        //            return new JsonResult(new { Success = false, Message = "session Not Found" });
        //        }

        //        var session = TempData["TempSession"] as string;
        //        TempData.Keep("TempSession");


        //        var response = await _authenticationService.SendMobileNotification(session);
        //        if (response == null)
        //        {
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Send PushNotification code", LogMessageType.FAILURE.ToString(), "Send PushNotification in Additional Authentication Failed response value getting null");
        //            _logger.LogError("Send Mobile Notification fail getting null response");
        //            return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
        //        }
        //        return Ok(response);
        //    }
        //    catch (Exception e)
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AdditionalAuthentication, "Send PushNotification code", LogMessageType.FAILURE.ToString(), "Send PushNotification in Additional Authentication Failed : "+e.Message);
        //        _logger.LogError("critical operation SendMobileNotification fail", e);
        //        return StatusCode(500,"somethig went wrong! please contact to admin or try again later." );
        //    }
        //}
    }
}
