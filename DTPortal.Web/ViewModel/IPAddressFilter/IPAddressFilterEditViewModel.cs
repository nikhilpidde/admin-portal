using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.IPAddressFilter
{
    public class IPAddressFilterEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "IpAddress")]
       
        public string IpAddress { get; set; }

        [Display(Name = "IP Address Type")]
        [Required]
        public string IpType { get; set; }

        [Display(Name = "Permission")]
        public string Permission { get; set; }
    }
}
