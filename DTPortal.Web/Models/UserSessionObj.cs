using System.Collections.Generic;

namespace DTPortal.Web.Models
{
    public class UserSessionObj
    {
        public string Uuid { get; set; }

        public string fullname { get; set; }
        public string dob { get; set; }
        public string mailId { get; set; }
        public int sub { get; set; }
        public string mobileNo { get; set; }

        public List<string> AccessibleModule { get; set; }

    }
}
