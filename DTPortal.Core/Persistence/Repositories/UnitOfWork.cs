using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using Google.Apis.Auth.OAuth2;

namespace DTPortal.Core.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly idp_dtplatformContext _idpContext;
        private readonly ra_0_2Context _raContext;
        private IUserRepository _users;
        private IRoleRepository _roles;
        private IActivityRespository _activities;
        private IEncDecKeyRepository _encDecKeys;
        private ISMTPRepository _smtp;
        private IClientRepository _client;
        private ILogger _logger;
        private IUserAuthDatumRepository _userAuthData;
        private IUserLoginDetailRepository _userLoginDetail;
        private IConfigurationRepository _configuration;
        private IRoleActivityRepository _roleActivity;
        private ISubscriberRepository _subscriber;
        private IOrgnizationEmailRepository _orgnizationEmail;
        private IUserConsentRepository _userConsent;
        private ISubscriberStatusRepository _subscriberStatus;
        private IOperationsAuthSchemeRepository _operationsAuthScheme;
        private ITimeBasedAccessRepository _timeBasedAccess;
        private ICertificatesRepository _certificates;
        private IIpBasedAccessRepository _ipBasedAccess;
        private IMakerCheckerRepository _makerChecker;
        private IPasswordPolicyRepository _passwordPolicy;
        private IUserSecurityQueRepository _userSecQue;        
        private IPortalSettingsRepository _portalSettings;
        private IThresholdRepository _threshold;
        private IWalletConfigurationRepository _walletConfiguration;
        private ICredentialRepository _credential;
        private ICategoryRepository _category;
        private IProvisionStatusRepository _provisionStatus;
        private IScopeRepository _scopes;
        private IUserClaimRepository _userClaims;
        private IPurposeRepository _purpose;
        private IClientsPurposeRepository _clientsPurpose;
        private ITransactionProfileConsentRepository _transactionProfileConsent;
        private ITransactionProfileRequestsRepository _transactionProfileRequests;
        private ITransactionProfileStatusRepository _transactionProfileStatus;
        private IDataPivotRepository _dataPivotRepository;
        private ISubscriberCardDetailRepository _subscriberCardDetail;
        private IEConsentClientRepository _eConsentClient;
        private IUserProfilesConsentRepository _userProfilesConsent;
        private ICredentialVerifiersRepository _credentialVerifiers;
        private IAuthSchemeRepository _authScheme;
        private INorAuthSchemeRepository _norAuthScheme;
        private IPrimaryAuthSchemeRepository _primaryAuthScheme;
        private ISubscriberCompleteDetailsRepository _subscriberCompleteDetails;
        private IQrCredentialRepository _qrCredential;
        private IQrCredentialVerifiersRepository _qrCredentialVerifiers;
        private IWalletPurposeRepository _walletPurpose;
        private IWalletDomainRepository _walletDomain;
        private IWalletConsentRepository _walletConsent;
        private IOrganizationKycMethodsRepository _organizationKycMethods;
        private IKycMethodsRepository _kycMethods;
        private IKycDevicesRepository _kycDevicesRepository;
        private IKycProfilesRepository _kycProfiles;
        private IKycAttributesRepository _kycAttributes;
        private IVerificationMethodRepository _verificationMethods;
        public UnitOfWork(idp_dtplatformContext idpContext,
            ra_0_2Context raContext,
            ILogger Logger)
        {
            _idpContext = idpContext;
            _raContext = raContext;
            _logger = Logger;
        }

        public IUserRepository Users
        {
            get { return _users = _users ?? new UserRepository(_idpContext, _logger); }
        }

        public IRoleRepository Roles
        {
            get { return _roles = _roles ?? new RoleRepository(_idpContext, _logger); }
        }

        public IActivityRespository Activities
        {
            get { return _activities = _activities ?? new ActivityRespository(_idpContext, _logger); }
        }

        public IEncDecKeyRepository EncDecKeys
        {
            get { return _encDecKeys = _encDecKeys ?? new EncDecKeyRepository(_idpContext, _logger); }
        }

        public ISMTPRepository SMTP
        {
            get { return _smtp = _smtp ?? new SMTPRepository(_idpContext, _logger); }
        }
        public IClientRepository Client
        {
            get { return _client = _client ?? new ClientRepository(_idpContext, _logger); }
        }

        public IUserAuthDatumRepository UserAuthData
        {
            get { return _userAuthData = _userAuthData ?? new UserAuthDatumRepository(_idpContext, _logger); }
        }

        public IUserLoginDetailRepository UserLoginDetail
        {
            get { return _userLoginDetail = _userLoginDetail ?? new UserLoginDetailRepository(_idpContext, _logger); }
        }
        public IPasswordPolicyRepository PasswordPolicy
        {
            get { return _passwordPolicy = _passwordPolicy ?? new PasswordPolicyRepository(_idpContext, _logger); }
        }

        public IConfigurationRepository Configuration
        {
            get { return _configuration = _configuration ?? new ConfigurationRepository(_idpContext, _logger); }
        }
        public IRoleActivityRepository RoleActivity
        {
            get { return _roleActivity = _roleActivity ?? new RoleActivityRepository(_idpContext, _logger); }
        }

        public ISubscriberRepository Subscriber
        {
            get { return _subscriber = _subscriber ?? new SubscriberRepository(_raContext, _logger); }
        }
        public IUserConsentRepository UserConsent
        {
            get { return _userConsent = _userConsent ?? new UserConsentRepository(_idpContext, _logger); }
        }
        public ISubscriberStatusRepository SubscriberStatus
        {
            get { return _subscriberStatus = _subscriberStatus ?? new SubscriberStatusRepository(_raContext, _logger); }
        }

        public IOrgnizationEmailRepository OrgnizationEmail
        {
            get { return _orgnizationEmail = _orgnizationEmail ?? new OrgnizationEmailRepository(_raContext, _logger); }
        }

        public IOperationsAuthSchemeRepository OperationsAuthScheme
        {
            get
            {
                return _operationsAuthScheme ??= new OperationsAuthSchemeRepository(_idpContext, _logger);
            }
        }

        public ITimeBasedAccessRepository TimeBasedAccess
        {
            get
            {
                return _timeBasedAccess ??= new TimeBasedAccessRepository(_idpContext, _logger);
            }
        }

        public ICertificatesRepository Certificates
        {
            get { return _certificates = _certificates ?? new CertificateRepository(_idpContext, _logger); }
        }

        public IMakerCheckerRepository MakerChecker
        {
            get { return _makerChecker = _makerChecker ?? new MakerCheckerRepository(_idpContext, _logger); }
        }

        public IIpBasedAccessRepository IpBasedAccess
        {
            get { return _ipBasedAccess = _ipBasedAccess ?? new IpBasedAccessRepository(_idpContext, _logger); }
        }

        public IUserSecurityQueRepository UserSecurityQue
        {
            get { return _userSecQue = _userSecQue ?? new UserSecurityQueRepository(_idpContext, _logger); }
        }

        public IThresholdRepository Threshold
        {
            get { return _threshold = _threshold ?? new ThresholdRepository(_raContext, _logger); }
        }
        public IPortalSettingsRepository PortalSettings
        {
            get
            {
                return _portalSettings ??= new PortalSettingsRepository(_idpContext, _logger);
            }
        }
        public IWalletConfigurationRepository WalletConfiguration
        {
            get
            {
                return _walletConfiguration ??= new WalletConfigurationRepository(_idpContext, _logger);
            }
        }

        public ICredentialRepository Credential
        {
            get
            {
                return _credential ??= new CredentialRepository(_idpContext, _logger);
            }
        }

        public ICategoryRepository Category
        {
            get { return _category = _category ?? new CategoryRepository(_idpContext, _logger); }
        }

        public IProvisionStatusRepository ProvisionStatus
        {
            get { return _provisionStatus = _provisionStatus ?? new ProvisionStatusRepository(_idpContext, _logger); }
        }
        public IScopeRepository Scopes
        {
            get { return _scopes = _scopes ?? new ScopeRepository(_idpContext, _logger); }
        }

        public IUserClaimRepository UserClaims
        {
            get { return _userClaims = _userClaims ?? new UserClaimRepository(_idpContext, _logger); }
        }
        public IPurposeRepository Purpose
        {
            get { return _purpose = _purpose ?? new PurposeRepository(_idpContext, _logger); }
        }

        public IWalletPurposeRepository WalletPurpose
        {
            get { return _walletPurpose = _walletPurpose ?? new WalletPurposeRepository(_idpContext, _logger); }
        }

        public IWalletDomainRepository WalletDomain
        {
            get { return _walletDomain = _walletDomain ?? new WalletDomainRepository(_idpContext, _logger); }
        }
        public IClientsPurposeRepository ClientsPurpose
        {
            get { return _clientsPurpose = _clientsPurpose ?? new ClientsPurposeRepository(_idpContext, _logger); }
        }

        public ITransactionProfileStatusRepository TransactionProfileStatus
        {
            get { return _transactionProfileStatus = _transactionProfileStatus ?? new TransactionProfileStatusRepository(_idpContext, _logger); }
        }

        public ITransactionProfileRequestsRepository TransactionProfileRequests
        {
            get { return _transactionProfileRequests = _transactionProfileRequests ?? new TransactionProfileRequestsRepository(_idpContext, _logger); }
        }

        public ITransactionProfileConsentRepository TransactionProfileConsent
        {
            get { return _transactionProfileConsent = _transactionProfileConsent ?? new TransactionProfileConsentRepository(_idpContext, _logger); }
        }

        public IDataPivotRepository Datapivots
        {
            get { return _dataPivotRepository = _dataPivotRepository ?? new DataPivotRepository(_idpContext, _logger); }
        }

        public ISubscriberCardDetailRepository SubscriberCardDetail
        {
            get { return _subscriberCardDetail = _subscriberCardDetail ?? new SubscriberCardDetailRepository(_raContext, _logger); }
        }

        public IEConsentClientRepository EConsentClient
        {
            get { return _eConsentClient = _eConsentClient ?? new EConsentClientRepository(_idpContext, _logger); }
        }

        public IUserProfilesConsentRepository UserProfilesConsent
        {
            get { return _userProfilesConsent = _userProfilesConsent ?? new UserProfilesConsentRepository(_idpContext, _logger); }
        }

        public ICredentialVerifiersRepository CredentialVerifiers
        {
            get { return _credentialVerifiers = _credentialVerifiers ?? new CredentialVerifiersRepository(_idpContext, _logger); }
        }

        public IAuthSchemeRepository AuthScheme
        {
            get { return _authScheme = _authScheme ?? new AuthSchemeRepository(_idpContext, _logger); }
        }

        public INorAuthSchemeRepository NorAuthScheme
        {
            get { return _norAuthScheme = _norAuthScheme ?? new NorAuthSchemeRepository(_idpContext, _logger); }
        }

        public IPrimaryAuthSchemeRepository PrimaryAuthScheme
        {
            get { return _primaryAuthScheme = _primaryAuthScheme ?? new PrimaryAuthSchemeRepository(_idpContext, _logger); }
        }

        public ISubscriberCompleteDetailsRepository SubscriberCompleteDetails
        {
            get { return _subscriberCompleteDetails = _subscriberCompleteDetails ?? new SubscriberCompleteDetailsRepository(_raContext, _logger); }
        }
        public IQrCredentialRepository QrCredential
        {
            get { return _qrCredential = _qrCredential ?? new QrCredentialRepository(_idpContext, _logger); }
        }

        public IQrCredentialVerifiersRepository QrCredentialVerifiers
        {
            get { return _qrCredentialVerifiers = _qrCredentialVerifiers ?? new QrCredentialVerifiersRepository(_idpContext, _logger); }
        }
        public IWalletConsentRepository WalletConsent
        {
            get { return _walletConsent = _walletConsent ?? new WalletConsentRepository(_idpContext, _logger); }
        }
        public IKycMethodsRepository KycMethods
        {
            get { return _kycMethods = _kycMethods ?? new KycMethodsRepository(_idpContext, _logger); }
        }
        public IOrganizationKycMethodsRepository OrganizationKycMethods
        {
            get { return _organizationKycMethods = _organizationKycMethods ?? new OrganizationKycMethodsRepository(_idpContext, _logger); }
        }

        public IKycDevicesRepository KycDevices
        {
            get { return _kycDevicesRepository = _kycDevicesRepository ?? new KycDevicesRepository(_idpContext, _logger); }
        }

        public IKycProfilesRepository KycProfiles
        {
            get { return _kycProfiles = _kycProfiles ?? new KycProfilesRepository(_idpContext, _logger); }
        }

        public IKycAttributesRepository KycAttributes
        {
            get { return _kycAttributes = _kycAttributes ?? new KycAttributesRepository(_idpContext, _logger); }
        }

        public IVerificationMethodRepository VerificationMethods
        {
            get { return _verificationMethods = _verificationMethods ?? new VerificationMethodRepository(_idpContext, _logger); }
        }

        public async Task<int> SaveAsync()
        {
            return await _idpContext.SaveChangesAsync();
        }

        public void DisableDetectChanges()
        {
            _idpContext.ChangeTracker.AutoDetectChangesEnabled = false;
            return;
        }

        public void EnableDetectChanges()
        {
            _idpContext.ChangeTracker.AutoDetectChangesEnabled = true;
            return;
        }
        public int Save()
        {
            return _idpContext.SaveChanges();
        }

    }
}
