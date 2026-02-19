using DTPortal.Core.Domain.Lookups;
using DTPortal.Web.ViewModel.Registration;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.UserDashboard
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "UUID ")]
        public string Uuid { get; set; }

        [Required]
        [MaxLength(50)]
        [MinLength(4)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Gender")]
        public int gender { get; set; }

        [Required]
        //[EmailAddress]
        [MaxLength(50)]
        [Display(Name = "Email ")]
        public string MailId { get; set; }

        public List<SelectListItem> EmailDomains { get; set; }
        public string EmailDomain { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number length should be 9 digit required")]
        [MinLength(9)]
        [MaxLength(9)]
        [Display(Name = "Mobile Number ")]
        public string MobileNo { get; set; }

        [DataType(DataType.Date)]
        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Of Birth ")]
        public DateTime? Dob { get; set; }

        public int RoleId { get; set; }
        public string Role { get; set; }

        [Display(Name = "Status ")]
        public string Status { get; set; }

        [Display(Name = "Fido2 Status ")]
        public string FidoStatus { get; set; }
        //[Required]
        //[Display(Name = "Authentication scheme ")]
        //public string AuthScheme { get; set; }

        //public List<SelectListItem> AuthSchemeList { get; } = new List<SelectListItem>
        //{
        //     new SelectListItem { Value = "DEFAULT", Text = "DEFAULT" ,Selected = true},
        //    new SelectListItem { Value = "PASSWORD", Text = "PASSWORD"},
        //    new SelectListItem { Value = "FIDO2", Text = "FIDO2" },
        //};

        public RegistrationViewModel fidoData { get; set; }

    }
}
