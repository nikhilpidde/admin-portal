namespace DTPortal.Web.ViewModel.PrivilegeRequest
{
    public class PrivilegeRequestItemViewModel
    {
        public int id { get; set; }
        public string organizationId { get; set; }
        public string organizationName { get; set; }
        public string privilege { get; set; }
        public string createdBy { get; set; }
        public string status { get; set; }
        public string modifiedBy { get; set; }
    }
}
