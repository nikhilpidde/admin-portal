using System;

namespace DTPortal.Web.ViewModel.category
{
    public class CategoryAddViewModel
    {
       
        public string CategoryUid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
       
    }
}
