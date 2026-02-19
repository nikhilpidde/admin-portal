using System.ComponentModel.DataAnnotations;


namespace DTPortal.Web.ViewModel.SMTP
{
    public class SMTPViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "SMTP Host")]
        public string SMTPHost { get; set; }

        [Display(Name = "SMTP Port")]
        public int SMTPPort { get; set; }

        [Required]
        [Display(Name = "From Name")]
        public string FromName { get; set; }

        [Required]
        [Display(Name = "Mail Subject")]
        public string MailSubject { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "From Email Address")]
        public string FromEmailAddress { get; set; }

        [Required]
        public string Template { get; set; }

        [Required]
        [Display(Name = "SMTP User Name")]
        public string SMTPUserName { get; set; }

        [Required]
        //[DataType(DataType.Password)]
        [Display(Name = "SMTP Password")]
        public string SMTPPassword { get; set; }

        public bool RequireAuthentication { get; set; }

        public bool RequiresSSL { get; set; }
    }
}
