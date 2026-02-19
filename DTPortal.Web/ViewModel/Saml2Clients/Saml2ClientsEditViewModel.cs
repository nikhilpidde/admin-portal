using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Saml2Clients
{
    public class Saml2ClientsEditViewModel
    {
        public int id { get; set; }

        [Required]
        [Display(Name = "Application ID ")]
        public string ClientId { get; set; }

        [Required]
        [Display(Name = "Application Type ")]
        public string ApplicationType { get; set; }

        public List<SelectListItem> ApplcationTypeList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Native", Text = "Native" },
            new SelectListItem { Value = "Single Page Application", Text = "Single Page Application" },
            new SelectListItem { Value = "Regular Web Application", Text = "Regular Web Application" ,Selected = true },
            new SelectListItem { Value = "Machine to Machine Application", Text = "Machine to Machine Application"  },
        };

        [Required]
        [Display(Name = "Application Name ")]
        public string ApplicationName { get; set; }

        [Required]
        [Display(Name = "Application Url ")]
        // [Url]
        [RegularExpression(@"^(((http(s)?://)((((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))|localhost|((((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z]+)+[.])?((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z0-9]+)+([a-zA-Z]{0,30}[-_]?[a-zA-Z]{0,30})?([.][a-zA-Z]{2,30}[-_]?[a-zA-Z]{0,30}){0,10}?))(:([1-9]|[1-5]?[0-9]{2,4}|6[1-4][0-9]{3}|65[1-4][0-9]{2}|655[1-2][0-9]|6553[1-5]))?(([/][a-zA-Z0-9._-]+)*[/]?)?)|((?![-_0-9])([a-zA-Z0-9_-]+)(.[a-zA-Z0-9_-]+)+?)(://)(([a-zA-Z0-9-_/=]+)))$", ErrorMessage = "Invalid Url")]

        public string ApplicationUrl { get; set; }

        [Required]
        // [Url]
        [RegularExpression(@"^(((http(s)?://)((((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))|localhost|((((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z]+)+[.])?((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z0-9]+)+([a-zA-Z]{0,30}[-_]?[a-zA-Z]{0,30})?([.][a-zA-Z]{2,30}[-_]?[a-zA-Z]{0,30}){0,10}?))(:([1-9]|[1-5]?[0-9]{2,4}|6[1-4][0-9]{3}|65[1-4][0-9]{2}|655[1-2][0-9]|6553[1-5]))?(([/][a-zA-Z0-9._-]+)*[/]?)?)|((?![-_0-9])([a-zA-Z0-9_-]+)(.[a-zA-Z0-9_-]+)+?)(://)(([a-zA-Z0-9-_/=]+)))$", ErrorMessage = "Invalid Url")]
        [Display(Name = "Assertion Consumer Service URL ")]
        public string assertionConsumerServiceUrl { get; set; }

        [Required]
        //[Url]
        [RegularExpression(@"^(((http(s)?://)((((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))|localhost|((((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z]+)+[.])?((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z0-9]+)+([a-zA-Z]{0,30}[-_]?[a-zA-Z]{0,30})?([.][a-zA-Z]{2,30}[-_]?[a-zA-Z]{0,30}){0,10}?))(:([1-9]|[1-5]?[0-9]{2,4}|6[1-4][0-9]{3}|65[1-4][0-9]{2}|655[1-2][0-9]|6553[1-5]))?(([/][a-zA-Z0-9._-]+)*[/]?)?)|((?![-_0-9])([a-zA-Z0-9_-]+)(.[a-zA-Z0-9_-]+)+?)(://)(([a-zA-Z0-9-_/=]+)))$", ErrorMessage = "Invalid Url")]
        [Display(Name = "Audience URI (Entity ID) ")]
        public string entityID { get; set; }

        [Required]
        //[Url]
        [RegularExpression(@"^(((http(s)?://)((((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))|localhost|((((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z]+)+[.])?((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z0-9]+)+([a-zA-Z]{0,30}[-_]?[a-zA-Z]{0,30})?([.][a-zA-Z]{2,30}[-_]?[a-zA-Z]{0,30}){0,10}?))(:([1-9]|[1-5]?[0-9]{2,4}|6[1-4][0-9]{3}|65[1-4][0-9]{2}|655[1-2][0-9]|6553[1-5]))?(([/][a-zA-Z0-9._-]+)*[/]?)?)|((?![-_0-9])([a-zA-Z0-9_-]+)(.[a-zA-Z0-9_-]+)+?)(://)(([a-zA-Z0-9-_/=]+)))$", ErrorMessage = "Invalid Url")]
        [Display(Name = "Single Logout URL")]
        public string singleLogoutService { get; set; }

        [Required]
        [Display(Name = "Name ID format ")]
        public string nameIDFormat { get; set; }

        public List<SelectListItem> nameIDFormatList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent", Text = "persistent" },
            new SelectListItem { Value = "urn:oasis:names:tc:SAML:2.0:nameid-format:emailAddress", Text = "emailAddress" ,Selected = true },
            new SelectListItem { Value = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient", Text = "transient"  },
            new SelectListItem { Value = "urn:oasis:names:tc:SAML:2.0:nameid-format:unspecified", Text = "unspecified"  },
            new SelectListItem { Value = "urn:oasis:names:tc:SAML:2.0:nameid-format:X509SubjectName", Text = "X509SubjectName"  },
        };

       // [Required]
        [Display(Name = "Signature Algorithm")]
        public string requestSignatureAlgorithm { get; set; }

        public List<SelectListItem> requestSignatureAlgorithmList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "http://www.w3.org/2000/09/xmldsig#rsa-sha1", Text = "http://www.w3.org/2000/09/xmldsig#rsa-sha1" },
            new SelectListItem { Value = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", Text = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" ,Selected = true },
            new SelectListItem { Value = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512", Text = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512" },
            new SelectListItem { Value = "http://www.w3.org/2001/04/xmldsig#hmac-sha1", Text = "http://www.w3.org/2001/04/xmldsig#hmac-sha1" },
        };

       // [Required]
        [Display(Name = "Encryption Algorithm")]
        public string dataEncryptionAlgorithm { get; set; }

        public List<SelectListItem> dataEncryptionAlgorithmList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "http://www.w3.org/2001/04/xmlenc#aes128-cbc", Text = "http://www.w3.org/2001/04/xmlenc#aes128-cbc" },
            new SelectListItem { Value = "http://www.w3.org/2001/04/xmlenc#aes256-cbc", Text = "http://www.w3.org/2001/04/xmlenc#aes256-cbc" ,Selected = true  },
            new SelectListItem { Value = "http://www.w3.org/2009/xmlenc11#aes128-gcm", Text = "http://www.w3.org/2009/xmlenc11#aes128-gcm" },
            new SelectListItem { Value = "http://www.w3.org/2001/04/xmlenc#tripledes-cbc", Text = "http://www.w3.org/2001/04/xmlenc#tripledes-cbc" },
        };


       // [Required]
        [Display(Name = "Key Transport Algorithm")]
        public string keyEncryptionAlgorithm { get; set; }

        public List<SelectListItem> keyEncryptionAlgorithmList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "http://www.w3.org/2001/04/xmlenc#rsa-1_5", Text = "http://www.w3.org/2001/04/xmlenc#rsa-1_5" ,Selected = true  },
            new SelectListItem { Value = "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p", Text = "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p" },
        };

        [Required]
        [Display(Name = "Authentication Requests")]
        public string RequestSigned { get; set; }


        [Required]
        [Display(Name = "Assertion Signature")]
        public string assertionSignature { get; set; }

        //[Required]
        [Display(Name = " Assertion Encryption")]
        public string assertionEncryption { get; set; }

        //[Required]
        //[Display(Name = "Logout Requests")]
        //public string wantLogoutRequestSigned { get; set; }

        //[Required]
        //[Display(Name = "Logout Response")]
        //public string wantLogoutResponseSigned { get; set; }

        [Required]
        [Display(Name = "Authentication Responce")]
        public string ResponceSigned { get; set; }

        //[Required]
        //[Display(Name = "Assertion Signing Order")]
        //public string messageSigningOrder { get; set; }


        public List<SelectListItem> messageSigningOrderList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "sign-then-encrypt", Text = "sign-then-encrypt" ,Selected = true },
            new SelectListItem { Value = "encrypt-sign-then", Text = "encrypt-sign-then" },
        };

        public List<SelectListItem> list { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "true", Text = "Signed" },
            new SelectListItem { Value = "false", Text = "Unsigned" ,Selected = true},
        };


        public List<SelectListItem> elist { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "true", Text = "Encrypted"  },
            new SelectListItem { Value = "false", Text = "Unencrypted" ,Selected = true },
        };


        [Display(Name = "Signing Certificate (.crt,.cer only) *")]
        public IFormFile signingCert { get; set; }


        [Display(Name = "Encrypt Certificate (.crt,.cer only)*")]
        public IFormFile encryptCert { get; set; }


        [Display(Name = "EntityID")]
        public string entityIDIDP { get; set; }
        [Display(Name = "Signing Certificate")]
        public string signingCertIDP { get; set; }
        [Display(Name = "SingleSignOnService Url")]
        public string singleSignOnServiceIDP { get; set; }
        [Display(Name = "SingleLogoutService Url")]
        public string singleLogoutServiceIDP { get; set; }

        public string State { get; set; }

    }
}
