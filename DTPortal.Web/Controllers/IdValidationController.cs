using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DTPortal.Web.Controllers
{
    public class IdValidationController : Controller
    {
        private readonly IIdValidationService _idValidatonService;
        private readonly Core.Domain.Services.IClientService _clientService;
        private readonly IOrganizationService _organizationService;
        public IdValidationController(IIdValidationService idValidatonService,Core.Domain.Services.IClientService clientService,IOrganizationService organizationService)
        {
            _idValidatonService = idValidatonService;
            _clientService = clientService;
            _organizationService = organizationService;
        }


        public IActionResult Index()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> GetPaginatedIdValidations()
        {

            var pageNumber = Convert.ToInt32(Request.Form["pageNumber"].FirstOrDefault());
            var status = Request.Form["status"].FirstOrDefault();
            var searchValue = Request.Form["searchValue"].FirstOrDefault();

            var reports = await _idValidatonService.GetIdValidationLogReportAsync(searchValue,status,"",null,"","",pageNumber+1);
            //var reports = await _idValidatonService.GetAllIdValidationReportsAsync();

            if (reports == null || reports.Count == 0)
            {
                return Json(new
                {
                    totalCount = 0,
                    records = new List<IdValidationResponseDTO>()
                });
            }


            var orgDictionary = await _organizationService.GetOrganizationsDictionary();

            var clientDictionary = await _clientService.GetApplicationsDictionary();

            List<IdValidationResponseDTO> res = new List<IdValidationResponseDTO>();

            foreach (var report in reports)
            {
                if (!string.IsNullOrWhiteSpace(report.orgName) && orgDictionary != null && orgDictionary.ContainsKey(report.orgName))
                {
                    report.orgName = orgDictionary[report.orgName];
                }
                else
                {
                    report.orgName = "NA";
                }
                if (!string.IsNullOrWhiteSpace(report.applicationName) && clientDictionary != null && clientDictionary.ContainsKey(report.applicationName))
                {
                    report.applicationName = clientDictionary[report.applicationName];
                }
                else
                {
                    report.applicationName = "NA";
                }
                res.Add(report);
            }

            return Json(new
            {
                totalCount = reports.TotalCount,
                records = res               
            });
        }





        //public async Task<IActionResult> Index()
        //{
        //    var reports = await _idValidatonService.GetAllIdValidationReportsAsync();

        //    if (reports == null || reports.Count == 0)
        //    {
        //        return null;
        //    }

        //    var orgDictionary = await _organizationService.GetOrganizationsDictionary();

        //    var clientDictionary = await _clientService.GetApplicationsDictionary();

        //    List<IdValidationResponseDTO> res = new List<IdValidationResponseDTO>();

        //    foreach (var report in reports)
        //    {
        //        if (!string.IsNullOrWhiteSpace(report.orgName) && orgDictionary != null && orgDictionary.ContainsKey(report.orgName))
        //        {
        //            report.orgName = orgDictionary[report.orgName];
        //        }
        //        else
        //        {
        //            report.orgName = "NA";
        //        }
        //        if (!string.IsNullOrWhiteSpace(report.applicationName) && clientDictionary != null && clientDictionary.ContainsKey(report.applicationName))
        //        {
        //            report.applicationName = clientDictionary[report.applicationName];
        //        }
        //        else
        //        {
        //            report.applicationName = "NA";
        //        }
        //        res.Add(report);
        //    }

        //    return View(reports);
        //}




        [HttpPost]

        public IActionResult ValidateSignedData([FromBody] ValidateSignedDataRequestDTO signedDataRequest)

        {

            if (signedDataRequest == null ||

                string.IsNullOrEmpty(signedDataRequest.SignedData)

                || string.IsNullOrWhiteSpace(signedDataRequest.SignedData))

            {

                return Ok(new APIResponse()

                {

                    Success = false,

                    Message = "Signed data is required.",

                    Result = null

                });

            }

            var response = _idValidatonService.ValidateSignedDataAsync(signedDataRequest.SignedData, signedDataRequest.KycMethod);

            return Ok(new APIResponse()

            {

                Success = response.Success,

                Message = response.Message,

                Result = response.Resource

            });

        }


    }
}
