using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Configuration
{
    public class IDPConfigureationViewModel
    {
        [Required]
        [Display(Name = "Issuer")]
        public string OAuth2_Issuer { get; set; }

        [Required]
        [Display(Name = "Authorization endpoint")]
        public string OAuth2_authorization_endpoint { get; set; }

        [Required]
        [Display(Name = "Token endpoint")]
        public string OAuth2_token_endpoint { get; set; }

        [Required]
        [Display(Name = "UserInfo endpoint")]
        public string OAuth2_userinfo_endpoint { get; set; }

        [Required]
        [Display(Name = "Introspection endpoint")]
        public string OAuth2_introspection_Endpoint { get; set; }

        [Required]
        [Display(Name = "JWKS endpoint")]
        public string OAuth2_jwks_uri { get; set; }

        [Display(Name = "Response Type Supported")]
        public string OAuth2_response_types_supported { get; set; }

        [Display(Name = "Scopes Supported")]
        public List<string> OAuth2_scopes_supported { get; set; }

        [Display(Name = "Grant Type Supported")]
        public List<string> OAuth2_grant_types_supported { get; set; }

        [Display(Name = "Token Endpoint Authentication Method Supported")]
        public List<string> OAuth2_token_endpoint_auth_methods_supported { get; set; }

        [Display(Name = "Claim Supported")]
        public List<string> OAuth2_claims_supported { get; set; }

        [Display(Name = "Request Parameter Supported")]
        public string OAuth2_request_parameter_supported { get; set; }

        [Display(Name = "Signing Algorithem Supported")]
        public string OAuth2_algorithem_supported { get; set; }

        [Display(Name = "EntityID")]
        public string SAML2_entityID { get; set; }

        [Display(Name = "SingleSignOn endpoint")]
        public string SAML2_singleSignOnService { get; set; }
        public string SAML2_singleSignOnService_hidden { get; set; }

        [Display(Name = "SingleLogout endpoint")]
        public string SAML2_singleLogoutService { get; set; }

        public string SAML2_singleLogoutService_hidden { get; set; }

        [Display(Name = "Method Binding Supported")]
        public List<string> SAML2_Method_binding_Supported { get; set; }

        public string signCert { get; set; }

        public string enctCert { get; set; }

        [Display(Name = "Signing Certificate")]
        public IFormFile signingCert { get; set; }


        [Display(Name = "Encrypt Certificate")]
        public IFormFile encryptCert { get; set; }
    }
}
