using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using DTPortal.Web.Enums;
using DTPortal.Web.CustomValidations;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class BaseESealRegistrationViewModel
    {
        public BaseESealRegistrationViewModel()
        {
            DocumentListCheckbox = new List<DocumentListItem>()
            {
               
                 
                new DocumentListItem{Id =3, DisplayName = "Incorporation", IsSelected =false},
                new DocumentListItem{Id =4, DisplayName = "Tax", IsSelected =false},
                new DocumentListItem{Id =5, DisplayName = "Additional Legal Document", IsSelected =false}
            };

            //Countries = new List<SelectListItem>()
            //{
            //    new SelectListItem {Text = "India", Value = "1"},
            //    new SelectListItem {Text = "Uganda", Value = "2"}
            //};
            Countries = new List<string>() { "UAE" };
        }

        public List<DocumentListItem> DocumentListCheckbox { get; set; }

        public List<string> Countries { set; get; }
        //public List<SelectListItem> Countries { set; get; }
    }
}
