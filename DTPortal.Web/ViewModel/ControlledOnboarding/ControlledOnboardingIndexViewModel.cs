using DocumentFormat.OpenXml.Wordprocessing;
using DTPortal.Core.DTOs;
using DTPortal.Web.CustomValidations;
using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace DTPortal.Web.ViewModel.ControlledOnboarding
{
    public class ControlledOnboardingIndexViewModel
    {
        //[DataType(DataType.Upload)]
        //[MaxFileSize(100 * 1024)] // 100kb
        //[AllowedExtensions(new string[] { ".csv"})]
        [Required(ErrorMessage = "Please select a file")]
        public IFormFile TrustedUsersList { get; set; }

        public IList<TrustedUserDetails> UserList { get; set; }
    }
}
