using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class MessageConstants
    {
        public string InternalError { get; set; }
        public string ProcessingError { get; set; }
        public string InvalidArguments { get; set; }
        public string DBConnectionError { get; set; }
        public string UnAuthorizedError { get; set; }
        public string WrongCredentials { get; set; }
        public string SubscriberNotApproved { get; set; }
        public string RandomCodeNotMatched { get; set; }
        public string PinVerifyFailed { get; set; }
        public string FaceVerifyFailed { get; set; }
        public string UserDeniedAuthN { get; set; }
        public string WrongCode { get; set; }
        public string WrongPin { get; set; }
        public string WrongFace { get; set; }
        public string AuthNFailed { get; set; }
        public string SubAlreadyAuthenticated { get; set; }
        public string SubscriberNotFound { get; set; }
        public string TimeRestrictionApplied { get; set; }
        public string SubAccountSuspended { get; set; }
        public string NotificationSendFailed { get; set; }
        public string SubNotProvisioned { get; set; }
        public string SubNotActive { get; set; }
        public string SubAccountStatus { get; set; }
        public string TempSessionExpired { get; set; }
        public string AuthnTokenExpired { get; set; }
        public string AuthSchemeMisMatch { get; set; }
        public string AuthSchemeAlreadyAuthenticated { get; set; }
        public string UserSessionNotFound { get; set; }
        public string SessionMismatch { get; set; }
        public string SessionsNotFound { get; set; }
    }

    public class OIDCConstants
    {

        public string InternalError { get; set; }
        public string InvalidInput { get; set; }
        public string InvalidToken { get; set; }
        public string InvalidGrant { get; set; }
        public string CodeNotFound { get; set; }
        public string InvalidClient { get; set; }
        public string InvalidAuthZHeader { get; set; }
        public string UnsupportedAuthSchm { get; set; }
        public string VerifyTokenScopeMissing { get; set; }
        public string ClientSecretMismatch { get; set; }
        public string insufficientScope { get; set; }
        public string InsufficientScopeDesc { get; set; }
        public string ClientIdNotReceived { get; set; }
        public string ClientIdMisMatch { get; set; }
        public string ClientNotFound { get; set; }
        public string ClientNotActive { get; set; }
        public string InvalidCredentials { get; set; }
        public string InvalidTokenDesc { get; set; }
        public string CodeVerificationFailed { get; set; }
        public string AssertionValidationFailed { get; set; }
        public string ResponseTypeMisMatch { get; set; }
        public string ClientScopesNotMatched { get; set; }
        public string ScopesNotExists { get; set; }
        public string DeniedConsent { get; set; }
        public string RedirectUriMisMatch { get; set; }
        public string GrantTypeMismatch { get; set; }
        public string NonceNotReceived { get; set; }
    }

    public class WebConstants
    {
        public string RedirectUriMissing { get; set; }
        public string LogoutUriMissing { get; set; }
        public string InvalidLogoutUri { get; set; }
        public string InvalidIdToken { get; set; }
        public string NotAuthorized { get; set; }
        public string LogoutError { get; set; }
        public string InvalidClient { get; set; }
        public string InvalidPostLogout { get; set; }
        public string ClientIdNotFound { get; set; }
        public string ClientIdRedirectUriMissing { get; set; }
        public string InvalidJWT { get; set; }
        public string DeniedConsent { get; set; }
        public string ClientNotActive { get; set; }
        public string DeniedPermission { get; set; }
        public string ConsentRequired { get; set; }
        public string SessionNotFound { get; set; }
        public string InternalError { get; set; }
        public string InternalServerError { get; set; }
        public string ResponseTypeNotFound { get; set; }
        public string ScopeNotFound { get; set; }
        public string StateNotFound { get; set; }
        public string NonceNotFound { get; set; }
        public string SomethingWrong { get; set; }
        public string InvalidParams { get; set; }
        public string GetUserFailed { get; set; }
    }
    public class ErrorConfiguration
    {
        public MessageConstants Constants { get; set; }
        public OIDCConstants OIDCConstants { get; set; }
        public WebConstants WebConstants { get; set; }
    }
}
