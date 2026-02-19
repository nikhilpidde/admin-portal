using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Web.ViewModel.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using DTPortal.Web.ViewModel;
using Newtonsoft.Json;
using DTPortal.Core.Utilities;
using Newtonsoft.Json.Serialization;
using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using Microsoft.Extensions.Configuration;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "User Session,RA Session")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class SessionController : BaseController
    {

        private readonly ISessionService _sessionService;
        private readonly IConfiguration _configuration;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly string clientId;
        public SessionController(ILogClient logClient, ISessionService sessionService, IConfiguration configuration,IGlobalConfiguration globalConfiguration) : base(logClient)
        {
            _sessionService = sessionService;
            _configuration = configuration;
            _globalConfiguration = globalConfiguration;
            clientId = _globalConfiguration.AdminPortalClientId();
        }


        [HttpGet]
        public IActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var model = new SessionViewModel();
            model.SessionType = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SessionViewModel viewModel)
        {

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            GetAllUserSessionsResponse sessionindb;
            if (viewModel.SessionType == "user")
            {
                sessionindb = await _sessionService.GetAllIDPUserSessions(viewModel.SearchValue, int.Parse(viewModel.SearchType));
            }
            else
            {
                sessionindb = await _sessionService.GetAllRAUserSessions(viewModel.SearchValue, int.Parse(viewModel.SearchType));
            }

            if (sessionindb == null || !sessionindb.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "Get session", LogMessageType.FAILURE.ToString(), "Fail to search Session of type " + viewModel.RASearchTypeList.FirstOrDefault(x => x.Value == viewModel.SearchType).Text + "  by value " + viewModel.SearchValue + "");
                Alert alert = new Alert { Message = (sessionindb == null ? "Internal error please contact to admin" : sessionindb.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var ele in sessionindb.Result[0].ClientId)
            {
                list.Add(new SelectListItem() { Value = ele, Text = ele });
            }
            if (list.Any())
            {
                list[0].Selected = true;
            }

            var Model = new ViewSessionViewModel
            {
                Clientlist = list,
                UserId = sessionindb.Result[0].UserId,
                GlobalSessionId = sessionindb.Result[0].GlobalSessionId,
                UserAgentDetails = sessionindb.Result[0].UserAgentDetails,
                LastAccessTime = sessionindb.Result[0].LastAccessTime,
                LoggedInTime = sessionindb.Result[0].LoggedInTime,
                TypeOfDevice = sessionindb.Result[0].TypeOfDevice,
                AuthenticationScheme = sessionindb.Result[0].AuthenticationScheme,
                IpAddress = sessionindb.Result[0].IpAddress,
                MacAddress = sessionindb.Result[0].MacAddress,
                FullName = sessionindb.Result[0].FullName
            };

            if (viewModel.SessionType == "user")
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "Get session", LogMessageType.SUCCESS.ToString(), "Search Session of type " + viewModel.UserSearchTypeList.FirstOrDefault(x => x.Value == viewModel.SearchType).Text + " by value " + viewModel.SearchValue + " success");
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "Get session", LogMessageType.SUCCESS.ToString(), "Search Session of type " + viewModel.RASearchTypeList.FirstOrDefault(x => x.Value == viewModel.SearchType).Text + " by value " + viewModel.SearchValue + " success");
            }
            viewModel.HasData = true;
            viewModel.SessionDetails = Model;
            return View(viewModel);

        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            //var clientId = _configuration.GetValue<string>("DTIDP_Config:Client_id");
            //if (_configuration.GetValue<bool>("EncryptionEnabled"))
            //{
            //    clientId = PKIMethods.Instance.
            //            PKIDecryptSecureWireData(clientId);
            //};
            var sessionInDb = await _sessionService.GetAllClientSessions(clientId);
            if (sessionInDb == null || sessionInDb.Success == false)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "Get All Session", LogMessageType.FAILURE.ToString(), "Fail to get all session in user sessions");
                var model = new AdminSessionListViewModel();
                return View("SessionAll", model);
            }
            else
            {
                var model = new AdminSessionListViewModel
                {
                    session = sessionInDb.GlobalSessions
                };

                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "Get All Session", LogMessageType.SUCCESS.ToString(), "Get all session in user sessions success");
                return View("SessionAll", model);
            }

        }

        //[HttpGet]
        //public async Task<IActionResult> ViewSession(string id)
        //{
        //    var sessionInDb = await _sessionService.GetGlobalSession(id);
        //    if (sessionInDb == null)
        //    {
        //        return NotFound();
        //    }

        //    List<SelectListItem> list = new List<SelectListItem>();

        //    foreach(var ele in sessionInDb.Result.ClientId)
        //    {
        //        list.Add(new SelectListItem() { Value = ele, Text = ele });
        //    }

        //    var viewModel = new ViewSessionViewModel
        //    {
        //        Clientlist = list,
        //        UserId = sessionInDb.Result.UserId,
        //        GlobalSessionId = sessionInDb.Result.GlobalSessionId,
        //        UserAgentDetails = sessionInDb.Result.UserAgentDetails,
        //        LastAccessTime = sessionInDb.Result.LastAccessTime,
        //        LoggedInTime =sessionInDb.Result.LoggedInTime,
        //        TypeOfDevice = sessionInDb.Result.TypeOfDevice,
        //        AuthenticationScheme = sessionInDb.Result.AuthenticationScheme,
        //        IpAddress = sessionInDb.Result.IpAddress
        //    };

        //    return View(viewModel);
        //}

        [HttpGet]
        public async Task<ActionResult> GetJSON(JqueryDatatableParam param)
        {
            try
            {
                var dd = Request.Query["start"].ToString();
                var dd1 = Request.Query["length"].ToString();
                var start = (param.iDisplayStart != 0) ? param.iDisplayStart : int.Parse(dd);
                var length = (param.iDisplayLength != 0) ? param.iDisplayLength : int.Parse(dd1);
                var totalRecords = 0;

                var sessionInDb = await _sessionService.GetAllGlobalSession(start, length);
                if (!sessionInDb.Success)
                {
                    var displayResult = new List<SessionListViewModel>();
                    totalRecords = 0;
                    return Json(new { recordsFiltered = totalRecords, recordsTotal = totalRecords, data = displayResult });
                }
                else
                {
                    IList list = new List<SessionListViewModel>();

                    foreach (var ele in sessionInDb.GlobalSessions)
                    {
                        foreach (var clientId in ele.ClientId)
                        {
                            var newClient = new SessionListViewModel()
                            {
                                IpAddress = ele.IpAddress,
                                LastAccessTime = ele.LastAccessTime,
                                LoggedInTime = ele.LoggedInTime,
                                UserId = ele.UserId,
                                GlobalSessionId = ele.GlobalSessionId,
                                ClientId = clientId

                            };

                            list.Add(newClient);
                        }
                    }

                    var displayResult = list;
                    totalRecords = list.Count;
                    return Json(new { recordsFiltered = totalRecords, recordsTotal = totalRecords, data = displayResult });
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> Logout(string id, string name)
        {
            var data = new LogoutUserRequest
            {
                GlobalSession = id
            };

            var response = await _sessionService.LogoutUser(data);
            if (response == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "Logout Session", LogMessageType.FAILURE.ToString(), "Fail to logout Session of " + name + ", getting response value null");
                return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
            }
            if (response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "Logout Session", LogMessageType.SUCCESS.ToString(), "Logout Session of " + name + " is success");
                return new JsonResult(response);
            }

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "Logout Session", LogMessageType.FAILURE.ToString(), "Fail to logout Session of  " + name);
            return new JsonResult(response);


        }

        [HttpPost]
        public async Task<IActionResult> LogoutAll(List<string> id)
        {
            foreach (var session in id)
            {
                var data = new LogoutUserRequest
                {
                    GlobalSession = session
                };

                var response = await _sessionService.LogoutUser(data);
                if (response == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "LogoutAll Session", LogMessageType.FAILURE.ToString(), "Fail to logout all Session");
                    return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
                }
                if (!response.Success)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "LogoutAll Session", LogMessageType.FAILURE.ToString(), "Fail to logout all Session ");
                    return new JsonResult(response);
                }

            }

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Session, "LogoutAll Session", LogMessageType.SUCCESS.ToString(), "Logout all Session success ");
            return new JsonResult(new { success = true });
        }
    }
}
