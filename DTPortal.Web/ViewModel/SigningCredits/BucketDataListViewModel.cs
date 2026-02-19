using System;

namespace DTPortal.Web.ViewModel.SigningCredits
{
    public class BucketDataListViewModel
    {
        public int Id { get; set; }
        public string documentId { get; set; }
        public int totalDigital { get; set; }
        public int totalEseal { get; set; }
        public string createdOn { get; set; }
        public string status { get; set; }
    }
}
