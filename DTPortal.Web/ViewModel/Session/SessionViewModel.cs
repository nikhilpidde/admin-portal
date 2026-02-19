using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Session
{
    public class SessionViewModel
    {
        [Required]
        [DisplayName("Search Type")]
        public string SearchType { get; set; }

        [Required]
        [DisplayName("Search Type")]
        public string SessionType { get; set; }

        public List<SelectListItem> UserSearchTypeList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Mobile Number" },
            new SelectListItem { Value = "2", Text = "Email ID"  },
        };

        public List<SelectListItem> RASearchTypeList { get; } = new List<SelectListItem>
        {
             new SelectListItem { Value = "3", Text = "Card Number"  },
            new SelectListItem { Value = "4", Text = "Passport"  },
            new SelectListItem { Value = "2", Text = "Email ID"  },
            new SelectListItem { Value = "1", Text = "Mobile Number" },

        };

        [Required]
        public string SearchValue { get; set; }

        public bool HasData { get; set; } = false;

        // public List<SessionListViewModel> SessionDetails { get; set; }
        //public IList<ViewSessionViewModel> SessionDetails { get; set; }
        public ViewSessionViewModel SessionDetails { get; set; }

    }
}
