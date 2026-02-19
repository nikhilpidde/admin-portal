using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }

        IRoleRepository Roles { get; }

        IActivityRespository Activities { get; }

        IEncDecKeyRepository EncDecKeys { get; }

        ISMTPRepository SMTP { get; }

        IClientRepository Client { get; }

        IUserAuthDatumRepository UserAuthData { get; }

        IUserSecurityQueRepository UserSecurityQue { get; }

        IUserLoginDetailRepository UserLoginDetail { get; }

        IConfigurationRepository Configuration { get; }

        IRoleActivityRepository RoleActivity { get; }

        ISubscriberRepository Subscriber { get; }

        IUserConsentRepository UserConsent { get; }

        IPasswordPolicyRepository PasswordPolicy { get; }

        ISubscriberStatusRepository SubscriberStatus { get; }

        IOrgnizationEmailRepository OrgnizationEmail { get; }

        IOperationsAuthSchemeRepository OperationsAuthScheme { get; }

        ITimeBasedAccessRepository TimeBasedAccess { get; }

        ICertificatesRepository Certificates { get; }

        IIpBasedAccessRepository IpBasedAccess { get; }

        IPortalSettingsRepository PortalSettings { get; }

        IMakerCheckerRepository MakerChecker { get; }

        IThresholdRepository Threshold { get; }

        IWalletConfigurationRepository WalletConfiguration { get; }

        ICredentialRepository Credential { get; }

        ICategoryRepository Category { get; }

        IProvisionStatusRepository ProvisionStatus { get; }

        IScopeRepository Scopes { get; }
        IUserClaimRepository UserClaims { get; }

        IPurposeRepository Purpose { get; }

        IWalletPurposeRepository WalletPurpose { get; }

        IWalletDomainRepository WalletDomain { get; }

        IClientsPurposeRepository ClientsPurpose { get; }

        ITransactionProfileStatusRepository TransactionProfileStatus { get; }

        ITransactionProfileRequestsRepository TransactionProfileRequests { get; }

        ITransactionProfileConsentRepository TransactionProfileConsent { get; }

        IDataPivotRepository Datapivots { get; }

        ISubscriberCardDetailRepository SubscriberCardDetail { get; }

        IEConsentClientRepository EConsentClient { get; }

        IUserProfilesConsentRepository UserProfilesConsent { get; }

        ICredentialVerifiersRepository CredentialVerifiers { get; }

        IAuthSchemeRepository AuthScheme { get; }
        INorAuthSchemeRepository NorAuthScheme { get; }
        IPrimaryAuthSchemeRepository PrimaryAuthScheme { get; }

        ISubscriberCompleteDetailsRepository SubscriberCompleteDetails { get; }
        IQrCredentialRepository QrCredential { get; }

        IQrCredentialVerifiersRepository QrCredentialVerifiers { get; }
        IWalletConsentRepository WalletConsent { get; }
        IKycMethodsRepository KycMethods { get; }
        IOrganizationKycMethodsRepository OrganizationKycMethods { get; }
        IKycDevicesRepository KycDevices { get; }
        IKycAttributesRepository KycAttributes { get; }
        IKycProfilesRepository KycProfiles { get; }
        IVerificationMethodRepository VerificationMethods { get; }

        Task<int> SaveAsync();

        void DisableDetectChanges();

        void EnableDetectChanges();

        int Save();
    }
}
