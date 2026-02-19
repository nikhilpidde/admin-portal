using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Core.Domain.Lookups;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.ViewModel.UserManagement
{
    public class UserManagementNewViewModel
    {
        public int Id { get; set; }

        
        [Display(Name = "UUID")]
        public string Uuid { get; set; }

        [Required]
        [MaxLength(50)]
        [MinLength(4)]
        [Display(Name = "Name")]
        public string FullName { get; set; }

        [Required]
        //[EmailAddress]
        [MaxLength(50)]
        [Display(Name = "Email ")]
        public string MailId { get; set; }

        public List<SelectListItem> EmailDomains { get; set; }
        public string EmailDomain { get; set; }

        [Display(Name = "Gender")]
        public int gender { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber,ErrorMessage ="Mobile number length should be 9 digit required")]
        [MinLength(9)]
        [MaxLength(9)]
        
        [Display(Name = "Mobile Number ")]
        public string MobileNo { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Required]
        
        [Display(Name = "Date Of Birth ")]
        public DateTime? Dob { get; set; }

        [Required]
        [Display(Name = "Authentication scheme ")]
        public string AuthScheme { get; set; }

        public List<SelectListItem> AuthSchemeList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "DEFAULT", Text = "DEFAULT" },
            new SelectListItem { Value = "PASSWORD", Text = "PASSWORD"},
            new SelectListItem { Value = "FIDO2", Text = "FIDO2" },
        };
        //[Required]
        //[Display(Name = "Password ")]
        //[MaxLength(20)]
        //[MinLength(6)]
        //[DataType(DataType.Password,ErrorMessage ="password length should be min 6  or max 20 character")]
        //public string Password { get; set; }

        //[Required]
        //[MaxLength(20)]
        //[MinLength(6)]
        //[Display(Name = "Confirm Password ")]
        //[DataType(DataType.Password)]
        ///public string ConfPassword { get; set; }

        [Display(Name = "Role ")]
        [Required]
        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public List<SelectListItem> Roles { get; set; }
    }
}
