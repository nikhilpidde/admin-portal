using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SigningCredits
{
    public class UpdateBucketViewModel
    {
        [Display(Name = "Label")]
        [Required]
        public string label { get; set; }
        public string closingMessage { get; set; }
        public int AdditionalDs { get; set; }
        public int AdditionalEDs { get; set; }
        [Display(Name = "Status")]
        public string status { get; set; }
        [Display(Name = "OrganizationName")]
        public string OrganizationName { get; set; }
        [Display(Name = "ApplicationName")]
        public string ApplicationName { get; set; }
        public int Id { get; set; }
        public string OrgId { get; set; }
        public string appId { get; set; }
    }
}
