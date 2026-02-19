using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.KycProfles
{
    public class KycProfileEditViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Display(Name = "Display Name")]
        [Required(ErrorMessage = "Dispaly Name is required")]
        public string DisplayName { get; set; }
        public List<AttributesListItem> AttributesList { get; set; }
        public string Attributes { get; set; }
    }
}
