using DTPortal.Web.ViewModel.Scopes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.KycProfles
{
    public class KycProfileAddViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Display(Name = "Display Name")]
        [Required(ErrorMessage = "Display Name is required")]
        public string DisplayName { get; set; }
        public string Attributes { get; set; }
        public IEnumerable<AttributesListItem> AttributesList { get; set; }
    }
}
