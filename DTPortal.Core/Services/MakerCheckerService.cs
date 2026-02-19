using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class MakerCheckerService : IMakerCheckerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MakerCheckerService> _logger;
        private readonly IClientService _clientService;
        private readonly IUserManagementService _userManagementService;
        private readonly IRoleManagementService _roleManagementService;
        private readonly IPKIConfigurationService _pkiConfigurationService;
        private readonly IRateCardService _rateCardService;
        private readonly IPackageService _packageService;
        private readonly IOnboardingTemplateService _onboardingTemplateService;
        private readonly ISubscriberService _subscriberService;
        private readonly IConfigurationService _configurationService;
        private readonly IOrganizationService _organizationService;
        private readonly IPriceSlabDefinitionService _priceSlabDefinitionService;
        private readonly IOrganizationPriceSlabDefinitionService _organizationPriceSlabDefinitionService;
        private readonly IOrganizationPaymentHistoryService _organizationPaymentHistoryService;
        private readonly IControlledOnboardingService _controlledOnboardingService;
        private readonly IOfflinePaymentService _offlinePaymentService;
        private readonly ISigningCreditsService _signingCreditsService;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly ISelfPortalService _selfPortalService;
        public MakerCheckerService(IUnitOfWork unitOfWork, ILogger<MakerCheckerService> logger,
            IClientService clientService, IUserManagementService userManagementService,
            IRoleManagementService roleManagementService,
            IPKIConfigurationService pkiConfigurationService,
            IRateCardService rateCardService,
            IPackageService packageService,
            IOnboardingTemplateService onboardingTemplateService,
            ISubscriberService subscriberService,
            IConfigurationService configurationService,
            IOrganizationService organizationService,
            IPriceSlabDefinitionService priceSlabDefinitionService,
            IOrganizationPriceSlabDefinitionService organizationPriceSlabDefinitionService,
            IOrganizationPaymentHistoryService organizationPaymentHistoryService,
            IOfflinePaymentService OfflinePaymentService,
            IControlledOnboardingService controlledOnboardingService,
            ISigningCreditsService signingCreditsService,
            IBeneficiaryService beneficiaryService,
            ISelfPortalService selfPortalService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _clientService = clientService;
            _userManagementService = userManagementService;
            _roleManagementService = roleManagementService;
            _pkiConfigurationService = pkiConfigurationService;
            _rateCardService = rateCardService;
            _packageService = packageService;
            _onboardingTemplateService = onboardingTemplateService;
            _subscriberService = subscriberService;
            _configurationService = configurationService;
            _organizationService = organizationService;
            _priceSlabDefinitionService = priceSlabDefinitionService;
            _organizationPriceSlabDefinitionService = organizationPriceSlabDefinitionService;
            _organizationPaymentHistoryService = organizationPaymentHistoryService;
            _controlledOnboardingService = controlledOnboardingService;
            _offlinePaymentService = OfflinePaymentService;
            _signingCreditsService = signingCreditsService;
            _beneficiaryService = beneficiaryService;
            _selfPortalService = selfPortalService;
        }

        public async Task<bool> IsMCEnabled(int activityId)
        {
            // Get operation auth scheme
            var activity = await _unitOfWork.Activities.GetByIdAsync(activityId);
            if (null == activity)
            {
                _logger.LogError("Operation authenticationscheme not found");
                return false;
            }

            if (activity.McEnabled)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<BooleanResponse> IsCheckerApprovalRequired(
                int activityID, string operationType, string maker, string requestData)
        {


            //if (operationAuthScheme.AuthenticationRequired == false)
            //{
            //    _logger.LogInformation("Approval not required");
            //    return new BooleanResponse(false);
            //}
            var makerInDb = await _unitOfWork.Users.GetUserbyUuidAsync(maker);
            if (null == makerInDb)
            {
                _logger.LogError("User not found");
                return new BooleanResponse("User not found");
            }

            var makerChecker = new MakerChecker();
            makerChecker.CreatedDate = DateTime.Now;
            makerChecker.ModifiedDate = DateTime.Now;
            makerChecker.OperationPriority = "HIGH";
            makerChecker.OperationType = operationType;
            makerChecker.State = "PENDING";
            makerChecker.MakerRoleId = (int)makerInDb.RoleId;
            makerChecker.ActivityId = activityID;
            makerChecker.MakerId = makerInDb.Id;
            makerChecker.RequestData = requestData;

            try
            {
                await _unitOfWork.MakerChecker.AddAsync(makerChecker);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("CheckApprovalRequired Failed: {0}", ex.Message);
                return new BooleanResponse($"An error occurred" +
                    $"Please contact the admin.");
            }
            return new BooleanResponse(true);
        }

        public async Task<IEnumerable<Activity>> GetAllActivities()
        {
            return (await _unitOfWork.Activities.GetAllAsync()).Where(x => x.McSupported == true && x.Enabled == true);
        }

        public async Task<MakerCheckerResponse> UpdateMakerCheckerActivityConfiguration(IEnumerable<Activity> activities)
        {
            try
            {
                foreach (var activity in activities)
                {
                    _unitOfWork.Activities.Update(activity);
                }
                await _unitOfWork.SaveAsync();

                return new MakerCheckerResponse(null, "Updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return new MakerCheckerResponse("Failed to update");
        }

        public async Task<IEnumerable<MakerChecker>> GetAllRequestsByMakerId(int id)
        {
            return await _unitOfWork.MakerChecker.GetAllRequestsByMakerId(id);
        }

        public async Task<string> GetRequestDataById(int id)
        {
            var requestInDb = await _unitOfWork.MakerChecker.GetByIdAsync(id);
            return requestInDb.RequestData;
        }

        public async Task<IEnumerable<MakerChecker>> GetAllRequestsByCheckerRoleId(int id)
        {
            return await _unitOfWork.MakerChecker.GetAllRequestsByCheckerRoleId(id);
        }

        public async Task<MakerCheckerResponse> UpdateState(int id, bool isApproved,string token,
            string reason = null)
        {
            var makerCheckerInDB = await _unitOfWork.MakerChecker.GetByIdAsync(id);
            if (makerCheckerInDB == null)
            {
                _logger.LogError("Maker Checker Record not found");
                return new MakerCheckerResponse("Record not found");
            }

            if (isApproved)
            {
                try
                {
                    // Create request
                    var response = await CreateRequest(makerCheckerInDB,token);
                    if (!response.Success)
                    {
                        _logger.LogError("CreateRequest Failed");
                        return new MakerCheckerResponse(response.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("CreateRequest Failed: {0}", ex.Message);
                    return new MakerCheckerResponse($"Approval Failed." +
                        $"Please contact the admin.");
                }
                makerCheckerInDB.State = "APPROVED";
            }
            else
            {
                makerCheckerInDB.State = "REJECTED";
                makerCheckerInDB.Comment = reason;
            }

            try
            {
                _unitOfWork.MakerChecker.Update(makerCheckerInDB);
                await _unitOfWork.SaveAsync();

                return new MakerCheckerResponse(makerCheckerInDB);
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateState Failed: {0}", ex.Message);
                return new MakerCheckerResponse($"An error occurred" +
                    $"Please contact the admin.");
            }
        }

        public async Task<MakerCheckerResponse> UpdateStateWithRequetBodyChange(int id, string requetBody, bool isApproved, string reason = null)
        {
            var makerCheckerInDB = await _unitOfWork.MakerChecker.GetByIdAsync(id);
            if (makerCheckerInDB == null)
            {
                _logger.LogError("Maker Checker Record not found");
                return new MakerCheckerResponse("Record not found");
            }

            if (isApproved)
            {
                try
                {
                    // Create request
                    var response = await CreateRequest(makerCheckerInDB, requetBody);
                    if (!response.Success)
                    {
                        _logger.LogError("CreateRequest Failed");
                        return new MakerCheckerResponse(response.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("CreateRequest Failed: {0}", ex.Message);
                    return new MakerCheckerResponse($"Approval Failed." +
                        $"Please contact the admin.");
                }
                makerCheckerInDB.State = "APPROVED";
            }
            else
            {
                makerCheckerInDB.State = "REJECTED";
                makerCheckerInDB.Comment = reason;
            }

            try
            {
                _unitOfWork.MakerChecker.Update(makerCheckerInDB);
                await _unitOfWork.SaveAsync();

                return new MakerCheckerResponse(makerCheckerInDB);
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateState Failed: {0}", ex.Message);
                return new MakerCheckerResponse($"An error occurred" +
                    $"Please contact the admin.");
            }
        }

        private async Task<MakerCheckerResponse> CreateRequest(MakerChecker makerChecker,string token, string requetBody = null)
        {
            MakerCheckerResponse makerCheckerResponse = new MakerCheckerResponse(makerChecker);

            switch (makerChecker.ActivityId)
            {
                case ActivityIdConstants.ClientActivityId:
                    makerCheckerResponse = await CreateClientServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;

                case ActivityIdConstants.RoleActivityId:
                    makerCheckerResponse = await RoleManagementServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;

                case ActivityIdConstants.UserActivityId:
                    makerCheckerResponse = await UserManagementServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;

                case ActivityIdConstants.ConfigurationActivityId:
                    makerCheckerResponse = await ConfigurationServiceRequest(makerChecker.OperationType, makerChecker.RequestData, makerChecker.MakerId);
                    break;
                case ActivityIdConstants.ClientSaml2ActivityId:
                    makerCheckerResponse = await CreateClientServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;
                case ActivityIdConstants.OnboardingTemplateActivityId:
                    makerCheckerResponse = await CreateOnboardingTemplateServiceRequest(makerChecker.OperationType, makerChecker.RequestData,token);
                    break;

                case ActivityIdConstants.PKIConfigurationActivityId:
                    makerCheckerResponse = await CreatePKIConfigurationServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;

                case ActivityIdConstants.RateCardActivityId:
                    makerCheckerResponse = await CreateRateCardServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;

                case ActivityIdConstants.PackageActivityId:
                    makerCheckerResponse = await CreatePackageServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;

                case ActivityIdConstants.SubscriberActivityId:
                    makerCheckerResponse = await CreateSubscriberServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;

                case ActivityIdConstants.OrganizationActivityID:
                    makerCheckerResponse = await CreateOrganizationServiceRequest(makerChecker.OperationType, makerChecker.RequestData, requetBody);
                    break;

                case ActivityIdConstants.GenericPriceSlabActivityId:
                    makerCheckerResponse = await CreateGenericPriceSlabServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;

                case ActivityIdConstants.OrganizationPriceSlabActivityId:
                    makerCheckerResponse = await CreateOrganizationPriceSlabServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;
                case ActivityIdConstants.OrganizationPaymentHistoryActivityId:
                    makerCheckerResponse = await CreateOrganizationPaymentHistoryServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;
                case ActivityIdConstants.ControlledOnboardingActivityId:
                    makerCheckerResponse = await CreateControlledOnboardingServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;
                case ActivityIdConstants.OfflinePaymentHistoryActivityId:
                    makerCheckerResponse = await CeateOfflinePaymentHistoryServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;
                case ActivityIdConstants.ExclusiveAppsActivityId:
                    makerCheckerResponse = await CreateExclusiveAppServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;
                case ActivityIdConstants.BeneficiaryId:
                    makerCheckerResponse = await CreateBeneficiaryServiceRequest(makerChecker.OperationType, makerChecker.RequestData);
                    break;
                case ActivityIdConstants.OnboardingApprovalRequestActivityId:
                    makerCheckerResponse = await OnboardingApprovalRequest(makerChecker.OperationType, makerChecker.RequestData,requetBody);
                    break;
            }

            return makerCheckerResponse;
        }

        private async Task<MakerCheckerResponse> CreateClientServiceRequest(string operationType, string requestData)
        {
            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var requestObject = JsonConvert.DeserializeObject<Client>(requestData);

                        var response = await _clientService.CreateClientAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Client creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var requestObject = JsonConvert.DeserializeObject<ClientRequest>(requestData);
                        var client = requestObject.client;
                        var clientSaml2 = requestObject.ClientSaml2;

                        var response = await _clientService.UpdateClientAsync(client, clientSaml2,
                            true);
                        if (!response.Success)
                        {
                            _logger.LogError("Client update Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Delete:
                    {
                        var definition = new { Id = 0, UpdatedBy = "" };
                        var requestObject = JsonConvert.DeserializeAnonymousType(requestData, definition);

                        var response = await _clientService.DeleteClientAsync(requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Client deletion Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> RoleManagementServiceRequest(string operationType, string requestData)
        {
            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var requestObject = JsonConvert.DeserializeObject<roleRequest>(requestData);

                        var response = await _roleManagementService.AddRoleAsync(
                            requestObject.role, requestObject.selectedActivityIds, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Role creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var requestObject = JsonConvert.DeserializeObject<roleRequest>(requestData);

                        var response = await _roleManagementService.UpdateRoleAsync(
                            requestObject.role, requestObject.selectedActivityIds, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Role update Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Delete:
                    {
                        var definition = new { Id = 0, UpdatedBy = "" };
                        var requestObject = JsonConvert.DeserializeAnonymousType(requestData, definition);

                        var response = await _roleManagementService.DeleteRoleAsync(requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Role delete Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> UserManagementServiceRequest(string operationType, string requestData)
        {
            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var requestObject = JsonConvert.DeserializeObject<UserRequest>(requestData);

                        var response = await _userManagementService.AddUserAsync(
                                requestObject.user, requestObject.password, true);
                        if (!response.Success)
                        {
                            _logger.LogError("User creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var requestObject = JsonConvert.DeserializeObject<UserTable>(requestData);

                        var response = await _userManagementService.UpdateUserAsync(
                                requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("User update Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Delete:
                    {
                        var definition = new { Id = 0, UpdatedBy = "" };
                        var requestObject = JsonConvert.DeserializeAnonymousType(requestData, definition);

                        var response = await _userManagementService.DeleteUserAsync(
                                requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("User deletion Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> ConfigurationServiceRequest(string operationType, string requestData, int maker)
        {
            switch (operationType)
            {
                case OperationTypeConstants.Update:
                    {
                        var requestObject = JsonConvert.DeserializeObject<configurationMCRequest>(requestData);

                        var makerinDb = await _unitOfWork.Users.GetByIdAsync(maker);

                        var response = await _configurationService.SetConfigurationAsync(requestObject.configName,
                            requestObject.requestData, makerinDb.Uuid, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Configuration update Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreatePKIConfigurationServiceRequest(string operationType, string requestData)
        {
            var requestObject = JsonConvert.DeserializeObject<PkiPluginDatum>(requestData);

            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _pkiConfigurationService.AddPluginConfigurationAsync(
                            requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Plugin configuration creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var response = await _pkiConfigurationService.UpdatePluginConfigurationAsync(
                            requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Plugin configuration update Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Delete:
                    {
                        var response = await _pkiConfigurationService.DeletePluginConfigurationAsync(
                            requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Plugin configuration deletion Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Enable:
                    {
                        var response = await _pkiConfigurationService.EnablePluginConfigurationAsync(
                            requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Plugin configuration enable Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Disable:
                    {
                        var response = await _pkiConfigurationService.DisablePluginConfigurationAsync(
                            requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Plugin configuration disable Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateOnboardingTemplateServiceRequest(string operationType, string requestData, string token)
        {
            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var requestObject = JsonConvert.DeserializeObject<OnboardingTemplateDTO>(requestData);

                        var response = await _onboardingTemplateService.AddTemplateAsync(
                            requestObject,token, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Template creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var requestObject = JsonConvert.DeserializeObject<OnboardingTemplateDTO>(requestData);

                        var response = await _onboardingTemplateService.UpdateTemplateAsync(
                            requestObject, token, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Template update Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Delete:
                    {
                        var requestObject = JsonConvert.DeserializeObject<OnboardingTemplateDTO>(requestData);

                        var response = await _onboardingTemplateService.DeleteTemplateAsync(
                            requestObject.TemplateId, requestObject.UpdatedBy, token, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Template deletion Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Publish:
                    {
                        var requestObject = JsonConvert.DeserializeObject<OnboardingTemplateDTO>(requestData);

                        var response = await _onboardingTemplateService.PublishTemplateAsync(
                            requestObject.TemplateId, requestObject.UpdatedBy, token, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Template publish Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Unpublish:
                    {
                        var requestObject = JsonConvert.DeserializeObject<OnboardingTemplateDTO>(requestData);

                        var response = await _onboardingTemplateService.UnPublishTemplateAsync(
                            requestObject.TemplateId, requestObject.UpdatedBy, token, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Template unpublish Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateRateCardServiceRequest(string operationType, string requestData)
        {
            var requestObject = JsonConvert.DeserializeObject<RateCardDTO>(requestData);

            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _rateCardService.AddRateCardAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Rate Card creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var response = await _rateCardService.UpdateRateCardAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Rate Card creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Delete:
                    {
                        var response = await _rateCardService.DeleteRateCardAsync(
                            requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Rate Card deletion Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Enable:
                    {
                        var response = await _rateCardService.EnableRateCardAsync(
                            requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Rate Card enable Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Disable:
                    {
                        var response = await _rateCardService.DisableRateCardAsync(
                            requestObject.Id, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Rate Card disable Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreatePackageServiceRequest(string operationType, string requestData)
        {
            var requestObject = JsonConvert.DeserializeObject<PackageDTO>(requestData);

            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _packageService.AddPackageAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Package creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Delete:
                    {
                        var response = await _packageService.DeletePackageAsync(
                            requestObject.PackageId, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Package deletion Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Enable:
                    {
                        var response = await _packageService.EnablePackageAsync(
                            requestObject.PackageId, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Package enable Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Disable:
                    {
                        var response = await _packageService.DisablePackageAsync(
                            requestObject.PackageId, requestObject.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Package disable Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateSubscriberServiceRequest(string operationType, string requestData)
        {
            switch (operationType)
            {
                case OperationTypeConstants.RevokeCertificate:
                    {
                        var definition = new { SubscriberUniqueId = "", RevokeReasonId = 0, Remarks = "", Subscriber = "", RequestedBy = "" };
                        var requestObject = JsonConvert.DeserializeAnonymousType(requestData, definition);
                        var response = await _subscriberService.RevokeCertificateAsync(
                            requestObject.SubscriberUniqueId, requestObject.RevokeReasonId, requestObject.Remarks, requestObject.Subscriber, requestObject.RequestedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("RevokeCertificate Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.DeregisterDevice:
                    {
                        var definition = new { SubscriberUniqueId = "", Subscriber = "", RequestedBy = "" };
                        var requestObject = JsonConvert.DeserializeAnonymousType(requestData, definition);
                        var response = await _subscriberService.DeregisterDeviceAsync(
                            requestObject.SubscriberUniqueId, requestObject.Subscriber, requestObject.RequestedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("DeregisterDevice Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.ActivateAccount:
                    {
                        var definition = new { SubscriberUniqueId = "", RequestedBy = "" };
                        var requestObject = JsonConvert.DeserializeAnonymousType(requestData, definition);
                        var response = await _subscriberService.ActivateSubscriber(
                            requestObject.SubscriberUniqueId, requestObject.RequestedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Account Activation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateOrganizationServiceRequest(string operationType, string requestData, string requetBody = null)
        {
            var requestObject = JsonConvert.DeserializeObject<OrganizationDTO>(requestData);

            if (requetBody != null)
            {
                JObject jObject = JObject.Parse(requetBody);
                var documentType = new[] { new { Id = 0, DisplayName = "", IsSelected = false } };
                var documentList = JsonConvert.DeserializeAnonymousType(jObject["documentListCheckbox"].ToString(), documentType);
                requestObject.DocumentListCheckbox = documentList.Where(x => x.IsSelected == true).Select(x => x.DisplayName).ToList();
                //if (!String.IsNullOrEmpty(requestObject.Status) && (operationType == "GENERATE_E_SEAL" || (operationType == "UPDATE" && requestObject.Status.ToLower() == "active")))
                //{
                //    requestObject.SignedPdf = jObject["signedPdf"].ToString();
                //}
            }

            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _organizationService.AddOrganizationAsync(
                            requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Organization creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var response = await _organizationService.UpdateOrganizationAsync(
                            requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Organization update Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.GenerateESeal:
                    {
                        var requestDataObject = JsonConvert.DeserializeObject<OrganizationDTO>(requestData);

                        var response = await _organizationService.IssueCertificateAsync(
                            requestDataObject.OrganizationUid, requestDataObject.UpdatedBy, requestDataObject.TransactionReferenceId, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Organization E-seal generation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.RevokeCertificate:
                    {
                        var definition = new { Organization = new OrganizationDTO(), RevokeReasonId = 0, Remarks = "" };
                        var requestDataObject = JsonConvert.DeserializeAnonymousType(requestData, definition);

                        var response = await _organizationService.RevokeCertificateAsync(
                            requestDataObject.Organization.OrganizationUid, requestDataObject.RevokeReasonId,
                            requestDataObject.Remarks, requestDataObject.Organization.UpdatedBy, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Certificate revokation failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }

                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateGenericPriceSlabServiceRequest(string operationType, string requestData)
        {
            var requestObject = JsonConvert.DeserializeObject<IList<PriceSlabDefinitionDTO>>(requestData);

            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _priceSlabDefinitionService.AddPriceSlabDefinitionAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Price slab creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var response = await _priceSlabDefinitionService.UpdatePriceSlabDefinitionAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Price slab updation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateOrganizationPriceSlabServiceRequest(string operationType, string requestData)
        {
            var requestObject = JsonConvert.DeserializeObject<IList<OrganizationPriceSlabDefinitionDTO>>(requestData);

            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _organizationPriceSlabDefinitionService.AddPriceSlabDefinitionAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Organization price slab creation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var response = await _organizationPriceSlabDefinitionService.UpdatePriceSlabDefinitionAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Organization price slab updation Failed");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateOrganizationPaymentHistoryServiceRequest(string operationType, string requestData)
        {
            var requestObject = JsonConvert.DeserializeObject<OrganizationPaymentHistoryDTO>(requestData);

            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _organizationPaymentHistoryService.AddOrganizationPaymentHistoryAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Failed to add organization history");
                            return new MakerCheckerResponse(response.Message);
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateControlledOnboardingServiceRequest(string operationType, string requestData)
        {
            var requestObject = JsonConvert.DeserializeObject<ControlledOnboardingDTO>(requestData);

            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _controlledOnboardingService.AddTrustedUsersAsync(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Failed to add trusted users");

                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CeateOfflinePaymentHistoryServiceRequest(string operationType, string requestData)
        {
            var requestObject = JsonConvert.DeserializeObject<CreditAllocationListDTO>(requestData);
            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var response = await _offlinePaymentService.GetOfflineCredits(requestObject, true);
                        if (!response.Success)
                        {
                            _logger.LogError("Failed to add trusted users");

                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateExclusiveAppServiceRequest(string operationType, string requestData)
        {
            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var requestObject = JsonConvert.DeserializeObject<SaveBucketConfigDTO>(requestData);
                        var response = await _signingCreditsService.SaveBucket(requestObject, "", true);
                        if (!response.Success)
                        {
                            _logger.LogError("Failed to add trusted users");

                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var requestObject = JsonConvert.DeserializeObject<UpdateBucketConfigDTO>(requestData);
                        var response = await _signingCreditsService.UpdateBucket(requestObject, "", true);
                        if (!response.Success)
                        {
                            _logger.LogError("Failed to add trusted users");

                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> CreateBeneficiaryServiceRequest(string operationType, string requestData)
        {
            switch (operationType)
            {
                case OperationTypeConstants.Create:
                    {
                        var requestObject = JsonConvert.DeserializeObject<BeneficiaryAddDTO>(requestData);
                        var response = await _beneficiaryService.AddBeneficiaryAsync(requestObject, "", true);
                        if (!response.Success)
                        {


                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }
                case OperationTypeConstants.Update:
                    {
                        var requestObject = JsonConvert.DeserializeObject<BeneficiaryUpdateDTO>(requestData);
                        var response = await _beneficiaryService.EditBeneficiaryAsync(requestObject, "", true);
                        if (!response.Success)
                        {


                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }
                case OperationTypeConstants.CreateMany:
                    {
                        var requestList = JsonConvert.DeserializeObject<List<BeneficiaryAddDTO>>(requestData);
                        IList<BeneficiaryAddDTO> requestObject = requestList;
                        var response = await _beneficiaryService.AddMultipleBeneficiariesAsync(requestObject, "", true);
                        if (!response.Success)
                        {


                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }

                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }

        private async Task<MakerCheckerResponse> OnboardingApprovalRequest(string operationType, string requestData, string requestBody = null)
        {
            switch (operationType)
            {
                case OperationTypeConstants.Approve:
                    {
                        var requestObject = JsonConvert.DeserializeObject<SelfServiceOrganizationDTO>(requestData);

                        if (requestBody != null)
                        {
                            JObject jObject = JObject.Parse(requestBody);
                            requestObject.AdminUgpassEmail = jObject["AdminUgpassEmail"].ToString();
                        }

                            var response = await _selfPortalService.ApproveOrganizationAsync(requestObject,true);
                        if (!response.Success)
                        {
                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }
                case OperationTypeConstants.Reject:
                    {
                        var orgDetails = JsonConvert.DeserializeObject<SelfServiceOrganizationDTO>(requestData);

                        var response = await _selfPortalService.RejectOrganizationAsync(orgDetails,true);

                        if (!response.Success)
                        {
                            if (response.Resource != null)
                            {
                                return new MakerCheckerResponse(response.Message + "__" + response.Resource.ToString());
                            }
                            else
                            {
                                return new MakerCheckerResponse(response.Message);
                            }
                        }
                        break;
                    }
                default:
                    return new MakerCheckerResponse("Invalid Operation Name");
            }

            return new MakerCheckerResponse(new MakerChecker());
        }
    }
}
