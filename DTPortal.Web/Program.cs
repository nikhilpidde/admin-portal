using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using DinkToPdf;
using DinkToPdf.Contracts;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Persistence.Contexts;
using DTPortal.Core.Persistence.Repositories;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Extensions;
using DTPortal.Web.Hubs;
using DTPortal.Web.Middleware;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel.DataPivot;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using System.Threading.Tasks;
using NLog.Web;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("Init main");

try
{
    var builder = WebApplication.CreateBuilder(args);
 
    var securityConfig = builder.Configuration
                             .GetSection("SecurityConfig")
                             .Get<SecurityConfig>();

    // Call each setup function only if the feature is enabled
    if (securityConfig?.UseRateLimiting == true)
        WebHostExtensions.ConfigureRateLimiting(builder.Services, securityConfig,logger);

    if (securityConfig?.UseKestrelSettings == true)
        WebHostExtensions.ConfigureKestrel(builder.WebHost, securityConfig,logger);

    await ConfigureServices(builder);

    //builder.Configuration.AddIniFile($"settings.{builder.Environment.EnvironmentName}", true, false);

    builder.Host.UseNLog();

    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

    builder.Services.AddSignalR();
    var kestrelConfig = builder.Configuration.GetSection("Kestrel");

    var app = builder.Build();

    logger.Info("WebApplication build successful");

    // For Proxy Servers
    string basePath = builder.Configuration["BasePath"];
    if (!string.IsNullOrEmpty(basePath))
    {
        app.Use(async (context, next) =>
        {
            context.Request.PathBase = basePath;
            await next.Invoke();
        });
    }


    if (securityConfig?.UseSecurityHeaders == true)
        WebHostExtensions.ConfigureSecurityHeaders(app, securityConfig, logger);




    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseForwardedHeaders();
        // app.UseBrowserLink();
    }
    else
    {
        app.UseExceptionHandler("/Error");

        app.UseStatusCodePagesWithReExecute("/Error/{0}");
        app.UseForwardedHeaders();

        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        //app.UseHsts();
    }

    app.UseStaticFiles();

    app.UseCookiePolicy();

    app.UseRouting();

    app.UseMiddleware<ExceptionHandlingMiddleWare>();

    app.UseCors("AllowAll");

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseSession();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Login}/{action=Index}/{id?}");


    app.MapHub<DataHub>("/datahub");

    app.Run();
}
catch (Exception ex)
{
    // NLog: catch setup errors
    logger.Error(ex, ex.Message);
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

async Task ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // Only loopback proxies are allowed by default.
        // Clear that restriction because forwarders are enabled by explicit 
        // configuration.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    var environment = builder.Environment;
    // Load secrets from Vault only in Staging or Production
    if (environment.IsStaging() || environment.IsProduction())
    {
        var vaultAddress = builder.Configuration["Vault:Address"];
        var vaultToken = builder.Configuration["Vault:Token"];
        var secretPath = builder.Configuration["Vault:SecretPath"];

        // Initialize Vault client
        var authMethod = new TokenAuthMethodInfo(vaultToken);
        var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
        var vaultClient = new VaultClient(vaultClientSettings);

        // Fetch secret data from Vault
        Secret<SecretData> secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path: secretPath,
            mountPoint: "secret"
        );

        var data = secret.Data.Data;

        // Override configuration values
        var memoryConfig = new Dictionary<string, string>
        {
            ["ConnectionStrings:IDPConnString"] = data["ConnectionStrings:IDPConnString"]?.ToString(),
            ["ConnectionStrings:RAConnString"] = data["ConnectionStrings:RAConnString"]?.ToString(),
            ["ConnectionStrings:PKIConnString"] = data["ConnectionStrings:PKIConnString"]?.ToString(),
            ["RedisConnString"] = data["RedisConnString"]?.ToString()
        };

        // Inject Vault secrets into configuration
        builder.Configuration.AddInMemoryCollection(memoryConfig);
    }
    else
    {
        Console.WriteLine("Skipping Vault secrets loading (Development environment).");
    }
    var idpConnectionString = builder.Configuration.GetConnectionString("IDPConnString");
    var raConnectionString = builder.Configuration.GetConnectionString("RAConnString");
    var pkiConnectionString = builder.Configuration.GetConnectionString("PKIConnString");
    var redisConn = builder.Configuration["RedisConnString"];


    //logger.Info("IDP Database ConnString: " + idpConnectionString);
    //logger.Info("RA Database ConnString: " + raConnectionString);
    //logger.Info("PKI Database ConnString: " + pkiConnectionString);
    //logger.Info("Redis Database ConnString: " + redisConn);


    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            policy => policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
    });


    builder.Services.AddRouting(options => options.LowercaseUrls = true);
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
    builder.Services.AddAntiforgery(options => options.HeaderName = "XSRF-TOKEN");
   

    // Get JWT Token Configuration
    var jwtConfig = builder.Configuration.GetSection("JWTConfig").Get<JWTConfig>();
    builder.Services.AddSingleton(jwtConfig);

    builder.Services.AddScoped<IPushNotificationClient, PushNotificationClient>();
    builder.Services.AddSingleton<ILogClient, LogClient>();

    builder.Services.AddSession();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(Config =>
               {
                   Config.LoginPath = "/Login";
                   Config.Cookie.Name = "DTPlatform";
                   Config.LogoutPath = "/Logout";
                   Config.AccessDeniedPath = new PathString("/Error/401");
               });

    builder.Services.AddFido2(options =>
    {
        options.ServerDomain = builder.Configuration["fido2:serverDomain"];
        options.ServerName = builder.Configuration["fido2:serverName"];
        options.Origin = builder.Configuration["fido2:origin"];
        options.TimestampDriftTolerance = Convert.ToInt32(builder.Configuration["fido2:timestampDriftTolerance"]);
    });

    builder.Services.AddTransient<ExceptionHandlingMiddleWare>();
    builder.Services.AddScoped<Microsoft.Extensions.Logging.ILogger, Microsoft.Extensions.Logging.Logger<UnitOfWork>>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IPKIUnitOfWork, PKIUnitOfWork>();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();



    if (builder.Configuration.GetValue<bool>("EncryptionEnabled"))
    {
        idpConnectionString = PKIMethods.Instance.PKIDecryptSecureWireData(idpConnectionString);
        raConnectionString = PKIMethods.Instance.PKIDecryptSecureWireData(raConnectionString);
        pkiConnectionString = PKIMethods.Instance.PKIDecryptSecureWireData(pkiConnectionString);
    }

    builder.Services.AddDbContext<idp_dtplatformContext>(options =>
        options.UseNpgsql(idpConnectionString));

    builder.Services.AddDbContext<ra_0_2Context>(options =>
        options.UseNpgsql(raConnectionString));

    builder.Services.AddDbContext<DTPortal.Core.Persistence.Contexts.PKIDbContext>(options =>
        options.UseNpgsql(pkiConnectionString));

    builder.Services.AddHttpClient("ignoreSSL")
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
        });

    var context = new CustomAssemblyLoadContext();
    //#if Linux
    //            context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.so"));
    //#elif Windows
    //    context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.so"));
    //#endif
    if (builder.Environment.IsDevelopment())
    {
        context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
    }
    else
    {
        context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.so"));
    }

    builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
    builder.Services.AddScoped<IOrganizationService,
        DTPortal.Core.Services.OrganizationService>();
    builder.Services.AddSingleton<DTPortal.Core.Utilities.BackgroundService>();
    builder.Services.AddScoped<DataExportService>();
    builder.Services.AddScoped<ICacheClient, CacheClient>();
    builder.Services.AddScoped<IEmailSender, EmailSender>();
    builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    builder.Services.AddScoped<ISMTPService, SMTPService>();
    builder.Services.AddScoped<IClientService, ClientService>();
    builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
    builder.Services.AddScoped<DTPortal.Core.Utilities.IGlobalConfiguration, DTPortal.Core.Utilities.GlobalConfiguration>();
    builder.Services.AddSingleton<IKafkaConfigProvider, KafkaConfigProvider>();
    builder.Services.AddScoped<IUserConsoleService, UserConsoleService>();
    builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
    builder.Services.AddScoped<ISessionService, SessionService>();
    builder.Services.AddScoped<ILocalJWTManager, LocalJWTManager>();
    builder.Services.AddScoped<IOperationAuthenticationService, OperationAuthenticationService>();
    builder.Services.AddScoped<ITimeBasedAccessService, TimeBasedAccessService>();
    builder.Services.AddScoped<IIDPReportsService, IDPReportsService>();
    builder.Services.AddScoped<IAssertionValidationClient, AssertionValidationClient>();
    builder.Services.AddScoped<IMakerCheckerService, MakerCheckerService>();
    builder.Services.AddScoped<IMCValidationService, MCValidationService>();
    builder.Services.AddHttpClient<ISubscriberService, SubscriberService>();
    builder.Services.AddScoped<IPrivilegeRequestService, PrivilegeRequestService>();
    builder.Services.AddScoped<IUserDataService, UserDataService>();
    builder.Services.AddScoped<IWalletPurposeService, WalletPurposeService>();
    builder.Services.AddScoped<IWalletDomainService, WalletDomainService>();
    builder.Services.AddScoped<IOrganizationCategoriesService, OrganizationCategoriesService>();
    builder.Services.AddScoped<IBannerConfigService, BannerConfigService>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<ITokenManager, TokenManager>();
    builder.Services.AddScoped<IPKIServiceClient, PKIServiceClient>();
    builder.Services.AddScoped<IPKILibrary, PKILibrary>();
    builder.Services.AddScoped<ICertificateService, CertificateService>();

    builder.Services.AddScoped<Helper>();
    builder.Services.AddHttpClient<ISelfServiceConfigurationService, SelfServiceConfigurationService>()
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                };
            });
    builder.Services.AddHttpClient<IOnboardingTemplateService, OnboardingTemplateService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IRateCardService, RateCardService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IPackageService, PackageService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IOrganizationPaymentHistoryService, OrganizationPaymentHistoryService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IAccountBalanceService, AccountBalanceService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IConsentService, ConsentService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IGoogleMapService, GoogleMapService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IHealthCheckService, HealthCheckService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IDashboardService, DashboardService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddHttpClient<IMobileVersionConfigurationService, MobileVersionConfigurationService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
    });
    builder.Services.AddScoped<ILogoService, LogoService>();
    builder.Services.AddScoped<IPKIConfigurationService, PKIConfigurationService>();
    builder.Services.AddScoped<ILogReportService, LogReportService>();
    builder.Services.AddScoped<IServiceHealthStatusService, ServiceHealthStatusService>();
    builder.Services.AddScoped<IIpRestriction, IPRrestriction>();
    builder.Services.AddScoped<IIPBasedAccessService, IPBasedAccessService>();
    builder.Services.AddScoped<ICertificateService, CertificateService>();
    builder.Services.AddScoped<IAssertionValidationClient, AssertionValidationClient>();
    builder.Services.AddScoped<IResetPasswordService, ResetPasswordService>();
    builder.Services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
    builder.Services.AddScoped<SessionValidationAttribute>();
    builder.Services.AddScoped<IRazorRendererHelper, RazorRendererHelper>();
    builder.Services.AddScoped<ILicenseDetailsService, LicenseDetailsService>();
    builder.Services.AddScoped<IPriceSlabDefinitionService, PriceSlabDefinitionService>();
    builder.Services.AddScoped<IOrganizationPriceSlabDefinitionService, OrganizationPriceSlabDefinitionService>();
    builder.Services.AddScoped<IServiceDefinitionService, ServiceDefinitionService>();
    builder.Services.AddScoped<IServiceDefinitionService, ServiceDefinitionService>();
    builder.Services.AddScoped<IOrganizationBalanceSheetService, OrganizationBalanceSheetService>();
    builder.Services.AddScoped<IOrganizationUsageReportService, OrganizationUsageReportService>();
    builder.Services.AddScoped<ISubscriberBalanceSheetService, SubscriberBalanceSheetService>();
    builder.Services.AddScoped<ISubscriberPaymentHistoryService, SubscriberPaymentHistoryService>();
    builder.Services.AddScoped<IAllPaymentHistoryService, AllPaymentHistoryService>();
    builder.Services.AddScoped<IControlledOnboardingService, ControlledOnboardingService>();
    builder.Services.AddScoped<ICertificateReportService, CertificateReportService>();
    builder.Services.AddScoped<IRAServiceClient, RAServiceClient>();
    builder.Services.AddScoped<IHelper, Helper>();
    builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();
    builder.Services.AddScoped<ISigningCreditsService, SigningCreditsService>();
    builder.Services.AddScoped<IOfflinePaymentService, OfflinePaymentService>();
    builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
    builder.Services.AddScoped<ITrustedSpocService, TrustedSpocSrevice>();
    builder.Services.AddScoped<ILicenseService, LicenseService>();
    builder.Services.AddScoped<ISelfPortalService, SelfPortalService>();
    builder.Services.AddScoped<ISoftwareService, SoftwareService>();
    builder.Services.AddScoped<IWalletConfigurationService, WalletConfigurationService>();
    builder.Services.AddScoped<ICredentialService, CredentialService>();
    builder.Services.AddScoped<IProvisionStatusService, ProvisionStatusService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IClientsPurposeService, ClientsPurposeService>();
    builder.Services.AddScoped<IPurposeService, PurposeService>();
    builder.Services.AddScoped<ITransactionProfileRequestService, TransactionProfileRequestService>();
    builder.Services.AddScoped<ITransactionProfileConsentService, TransactionProfileConsentService>();
    builder.Services.AddScoped<ITransactionProfileStatusService, TransactionProfileStatusService>();
    builder.Services.AddScoped<IUserClaimService, UserClaimService>();
    builder.Services.AddScoped<IScopeService, ScopeService>();
    builder.Services.AddScoped<IAttributeServiceTransactionsService, AttributeServiceTransactionsService>();
    builder.Services.AddScoped<IDataPivotService, DataPivotService>();
    builder.Services.AddScoped<IEConsentService, EConsentService>();
    builder.Services.AddScoped<ICredentialVerifiersService, CredentialVerifiersService>();
    builder.Services.AddScoped<IQrCredentialService, QrCredentialService>();
    builder.Services.AddScoped<IQrCredentialVerifiersService, QrCredentialVerifiersService>();
    builder.Services.AddScoped<IAuthSchemeSevice, AuthSchemeService>();
    builder.Services.AddScoped<IPrimaryAuthSchemeService, PrimaryAuthSchemeService>();
    builder.Services.AddScoped<IAgentService,AgentService>();
    builder.Services.AddScoped<IWalletConsentService, WalletConsentService>();
    builder.Services.AddScoped<IKycApplicationService, KycApplicationService>();
    builder.Services.AddScoped<IKycMethodsService, KycMethodsService>();
    builder.Services.AddScoped<IOrganizationKycMethodsService, OrganizationKycMethodsService>();
    builder.Services.AddScoped<IIdValidationService, IdValidationService>();
    builder.Services.AddScoped<IKycDevicesService, KycDevicesService>();
    builder.Services.AddScoped<IKycAttributesService, KycAttributesService>();
    builder.Services.AddScoped<IKycProfilesService, KycProfilesService>();
    builder.Services.AddScoped<ICertificateIssuanceService, CertificateIssuanceService>();
    builder.Services.AddScoped<IUserInfoService, UserInfoService>();
}