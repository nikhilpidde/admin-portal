using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Core.Domain.Lookups;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.ViewModel.UserManagement
{
    public class UserManagementEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "UUID")]
        public string Uuid { get; set; }

        //[Required]
        //[Display(Name = "Display Name*")]
        //public string UserId { get; set; }
        [Required]
        [Display(Name = "Name")]
        [MaxLength(50)]
        [MinLength(4)]
        public string FullName { get; set; }

        [Display(Name = "Gender")]
        public int gender { get; set; }

        [Required]
       // [EmailAddress]
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

        [Required]
        [Display(Name = "Role ")]
        public int RoleId { get; set; }

        public List<SelectListItem> Roles { get; set; }

        [Display(Name = "Status ")]
        public string Status { get; set; }

        [Required]
        [Display(Name = "Authentication scheme ")]
        public string AuthScheme { get; set; }

        public List<SelectListItem> AuthSchemeList { get; } = new List<SelectListItem>
        {
             new SelectListItem { Value = "DEFAULT", Text = "DEFAULT" },
            new SelectListItem { Value = "PASSWORD", Text = "PASSWORD"},
            new SelectListItem { Value = "FIDO2", Text = "FIDO2" },
        };
    }
}
