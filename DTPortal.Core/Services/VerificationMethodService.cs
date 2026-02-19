using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Google.Apis.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class VerificationMethodService : IVerificationMethodService
    {
        private readonly ILogger<VerificationMethodService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISelfServiceConfigurationService _selfServiceConfigurationService;
        private readonly IMCValidationService _mcValidationService;
        public VerificationMethodService(ILogger<VerificationMethodService> logger,
            ISelfServiceConfigurationService selfServiceConfigurationService,
            IMCValidationService mcValidationService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _selfServiceConfigurationService = selfServiceConfigurationService;
            _mcValidationService = mcValidationService;
        }
        public async Task<List<string>> GetAttributeDisplayNamesAsync(List<string> attributesNames)
        {
            var attributesList = await _unitOfWork.KycAttributes.GetAllAsync();
            var displayNames = new List<string>();
            foreach (var attributeName in attributesNames)
            {
                var attribute = attributesList.FirstOrDefault(a => a.Name == attributeName);
                if (attribute != null)
                {
                    displayNames.Add(attribute.DisplayName);
                }
            }
            return displayNames;
        }

        public async Task<List<string>> GetSegmentDisplayNamesAsync(List<string> segmentNames)
        {
            var segmentsListResponse = await _selfServiceConfigurationService.GetAllConfigCategories();

            if (segmentsListResponse == null || !segmentsListResponse.Success)
            {
                return new List<string>();
            }

            var segmentList = (List<SelfServiceCategoryDTO>)segmentsListResponse.Resource;
            //var segmentList = await _unitOfWork.KycSegment.GetAllAsync();
            var displayNames = new List<string>();
            foreach (var segmentName in segmentNames)
            {
                var attribute = segmentList.FirstOrDefault(a => a.categoryName == segmentName);
                if (attribute != null)
                {
                    displayNames.Add(attribute.labelName);
                }
            }
            return displayNames;
        }

        public async Task<ServiceResult> GetVerificationMethodsList()
        {
            var verificationMethodsList = await _unitOfWork.VerificationMethods.GetVerificationMethodsListAsync();
            if (verificationMethodsList == null)
            {
                return new ServiceResult(false, "Failed to get list");
            }
            return new ServiceResult(true, "Get List Success", verificationMethodsList);
        }

        public async Task<ServiceResult> GetVerificationMethodById(int Id)
        {
            var verificationMethod = await _unitOfWork.VerificationMethods.GetByIdAsync(Id);
            if (verificationMethod == null)
            {
                return new ServiceResult(false, "Failed to get list");
            }
            VerificationMethodResponse verificationMethodResponse = new VerificationMethodResponse()
            {
                Id = verificationMethod.Id,
                MethodUid = verificationMethod.MethodUid,
                MethodCode = verificationMethod.MethodCode,
                MethodType = verificationMethod.MethodType,
                MethodName = verificationMethod.MethodName,
                Pricing = verificationMethod.Pricing,
                ProcessingTime = verificationMethod.ProcessingTime,
                ConfidenceThreshold = verificationMethod.ConfidenceThreshold,
                TargetSegments = verificationMethod.TargetSegments,
                MandatoryAttributes = verificationMethod.MandatoryAttributes,
                OptionalAttributes = verificationMethod.OptionalAttributes,
                Description = verificationMethod.Description,
                Status = verificationMethod.Status
            };
            if (!string.IsNullOrEmpty(verificationMethod.PricingSlabDefinitions))
            {
                verificationMethodResponse.PriceSlabs =
                    JsonConvert.DeserializeObject<List<PricingSlabDefinition>>(verificationMethod.PricingSlabDefinitions);
            }
            return new ServiceResult(true, "Get List Success", verificationMethodResponse);
        }

        public async Task<ServiceResult> GetVerificationMethodDetailsById(int Id)
        {
            var verificationMethod = await _unitOfWork.VerificationMethods.GetByIdAsync(Id);
            if (verificationMethod == null)
            {
                return new ServiceResult(false, "Failed to get list");
            }

            return new ServiceResult(true, "Get List Success", verificationMethod);
        }

        public async Task<ServiceResult> CreateVerificationMethod(VerificationMethod verificationMethod, bool makerCheckerFlag = false)
        {
            try
            {
                // Check whether maker checker enabled for this module
                bool isEnabled = await _mcValidationService.IsMCEnabled(
                    ActivityIdConstants.VerificationMethodActivityId);

                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether maker checker enabled for this operation
                    // If enabled store request object in maker checker table
                    var response = await _mcValidationService.IsCheckerApprovalRequired
                        (ActivityIdConstants.VerificationMethodActivityId, "CREATE", verificationMethod.CreatedBy,
                        JsonConvert.SerializeObject(verificationMethod));
                    if (!response.Success)
                    {
                        _logger.LogError("CheckApprovalRequired Failed");
                        return new ServiceResult(false, "CheckApprovalRequired Failed : " + response.Message.ToString());
                    }
                    if (response.Result)
                    {
                        return new ServiceResult(true, "Your request sent for approval");
                    }
                }
    
                var isMethodExist = await _unitOfWork.VerificationMethods.
                    IsMethodCodeNameExist(verificationMethod.MethodCode, verificationMethod.MethodName);

                if (isMethodExist)
                {
                    return new ServiceResult(false, "Method Code or Method Name already exists");
                }
                await _unitOfWork.VerificationMethods.AddAsync(verificationMethod);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "Add Verification Method Success");
            }
            catch (Exception)
            {
                return new ServiceResult(false, "Add Verification Method Failed");
            }
        }

        public async Task<ServiceResult> UpdateVerificationMethod(VerificationMethod verificationMethod, bool makerCheckerFlag = false)
        {
            try
            {
                // Check whether maker checker enabled for this module
                bool isEnabled = await _mcValidationService.IsMCEnabled(
                    ActivityIdConstants.VerificationMethodActivityId);

                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether maker checker enabled for this operation
                    // If enabled store request object in maker checker table
                    var response = await _mcValidationService.IsCheckerApprovalRequired
                        (ActivityIdConstants.VerificationMethodActivityId, "UPDATE", verificationMethod.CreatedBy,
                        JsonConvert.SerializeObject(verificationMethod));
                    if (!response.Success)
                    {
                        _logger.LogError("CheckApprovalRequired Failed");
                        return new ServiceResult(false, "CheckApprovalRequired Failed : " + response.Message.ToString());
                    }
                    if (response.Result)
                    {
                        return new ServiceResult(true, "Your request sent for approval");
                    }
                }

                _unitOfWork.VerificationMethods.Update(verificationMethod);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "Update Verification Method Success");
            }
            catch (Exception)
            {
                return new ServiceResult(false, "Update Verification Method Failed");
            }
        }

        public async Task<ServiceResult> DeleteVerificationMethod(int Id)
        {
            try
            {
                var verificationMethod = await _unitOfWork.VerificationMethods.GetByIdAsync(Id);
                if (verificationMethod == null)
                {
                    return new ServiceResult(false, "Verification Method not found");
                }
                _unitOfWork.VerificationMethods.Remove(verificationMethod);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "Delete Verification Method Success");
            }
            catch (Exception)
            {
                return new ServiceResult(false, "Delete Verification Method Failed");
            }
        }

        public async Task<ServiceResult> GetVerificationMethodsStatistics()
        {
            try
            {
                var verificationMethodsList = await _unitOfWork.VerificationMethods.GetVerificationMethodsListAsync();

                if (verificationMethodsList == null || !verificationMethodsList.Any())
                {

                    var statistics = new VerificationMethodsStatsDTO()
                    {
                        TotalMethods = 0,
                        ActiveMethods = 0,
                        AveragePrice = 0,
                        TotalBiometricMethods = 0
                    };

                    return new ServiceResult(false, "No Verification Methods Stats Found", statistics);
                }

                var verificationMethodsListCount = verificationMethodsList.Count();

                var actievVerificationMethodsCount = verificationMethodsList.Where(vm => vm.Status == "ACTIVE").Count();

                var biometricMethodsCount = verificationMethodsList.Where(vm => vm.MethodType != null && vm.MethodType.ToUpper() == "BIOMETRIC").Count();

                var averagePrice = verificationMethodsList.Average(vm => vm.Pricing);

                var stats = new VerificationMethodsStatsDTO()
                {
                    TotalMethods = verificationMethodsListCount,
                    ActiveMethods = actievVerificationMethodsCount,
                    AveragePrice = averagePrice,
                    TotalBiometricMethods = biometricMethodsCount
                };

                return new ServiceResult(true, "Get Verification Method Stats Success", stats);
            }
            catch (Exception)
            {
                return new ServiceResult(false, "Get Verification Method Stats Failed");
            }
        }

        public async Task<ServiceResult> GetVerificationMethodsAnalytics()
        {
            try
            {
                var verificationMethodsList = await _unitOfWork.VerificationMethods.GetVerificationMethodsListAsync();
                if (verificationMethodsList == null || !verificationMethodsList.Any())
                {
                    var analytics = new VerificationMethodsAnalyticsDTO()
                    {
                        MethodsByType = new Dictionary<string, int>(),
                        MethodsBySegment = new Dictionary<string, int>()
                    };
                    return new ServiceResult(false, "No Verification Methods Analytics Found", analytics);
                }
                var methodsByType = verificationMethodsList
                    .GroupBy(vm => vm.MethodType)
                    .ToDictionary(g => g.Key, g => g.Count());

                var methodsBySegment = new Dictionary<string, int>();

                var segmentsListResponse = await _selfServiceConfigurationService.GetAllConfigCategories();
                if (segmentsListResponse == null || !segmentsListResponse.Success)
                {
                    return new ServiceResult(false, "Get Verification Method Analytics Failed");
                }

                var segmentsList = (List<SelfServiceCategoryDTO>)segmentsListResponse.Resource;

                if (segmentsList != null && segmentsList.Any())
                {
                    foreach (var segment in segmentsList)
                    {
                        methodsBySegment[segment.categoryName] = 0;
                    }
                }

                foreach (var vm in verificationMethodsList)
                {
                    if (!string.IsNullOrEmpty(vm.TargetSegments))
                    {
                        var segments = vm.TargetSegments.Split(',');
                        foreach (var segment in segments)
                        {
                            if (methodsBySegment.ContainsKey(segment))
                            {
                                methodsBySegment[segment]++;
                            }
                            else
                            {
                                methodsBySegment[segment] = 1;
                            }
                        }
                    }
                }
                var analyticsResult = new VerificationMethodsAnalyticsDTO()
                {
                    MethodsByType = methodsByType,
                    MethodsBySegment = methodsBySegment
                };
                return new ServiceResult(true, "Get Verification Method Analytics Success", analyticsResult);
            }
            catch (Exception)
            {
                return new ServiceResult(false, "Get Verification Method Analytics Failed");
            }
        }

        public async Task<ServiceResult> GetVerificationMethodsListByPage(int pageNumber, int pageSize)
        {
            var verificationMethodsList = await _unitOfWork.VerificationMethods.
                GetVerificationMethodsListByPageAsync(pageNumber, pageSize);

            if (verificationMethodsList == null)
            {
                return new ServiceResult(false, "Failed to get list");
            }
            return new ServiceResult(true, "Get List Success", verificationMethodsList);
        }

        public async Task<ServiceResult> GetVerificationMethodsByOrganizationId(string organizationId)
        {
            if (string.IsNullOrEmpty(organizationId))
            {
                return new ServiceResult(false, "Invalid Organization Id");
            }

            var organizationCategoryResult = await _selfServiceConfigurationService.
                GetCategoryByOrganizationId(organizationId);

            if (organizationCategoryResult == null || !organizationCategoryResult.Success)
            {
                _logger.LogError("Failed to get Category List for Organization Id: " +
                    "{organizationId}", organizationId);

                return new ServiceResult(false, "Failed to get Category List");
            }

            OrganizationCategoryDTO organizationCategoryDTO = (OrganizationCategoryDTO)
                organizationCategoryResult.Resource;

            var verificationMethod = await _unitOfWork.VerificationMethods.
                GetVerificationMethodsByOrganizationIdAsync();

            if (verificationMethod == null)
            {
                return new ServiceResult(false, "Failed to get Verification Method");
            }

            List<string> verificationMethodsList = new List<string>();

            foreach (var method in verificationMethod)
            {
                var segmentMethods = method.TargetSegments?.Split(',').ToList() ?? new List<string>();

                if (segmentMethods.Any(sm => organizationCategoryDTO.CategoryName.Contains(sm)))
                {
                    verificationMethodsList.Add(method.MethodCode);
                }
            }
            return new ServiceResult(true, "Get Verification Method Success", verificationMethodsList);
        }

        public async Task<ServiceResult> GetAttributesByMethodCodeAsync(string methodCode)
        {
            try
            {
                if (string.IsNullOrEmpty(methodCode))
                {
                    return new ServiceResult(false, "Invalid Method Code");
                }
                var verificationMethod = await _unitOfWork.VerificationMethods.
                    GetVerificationMethodDetailsByCodeAsync(methodCode);
                if (verificationMethod == null)
                {
                    return new ServiceResult(false, "Method Not Found");
                }
                List<string> attributes = verificationMethod.MandatoryAttributes?
                    .Split(',')
                    .ToList() ?? new List<string>();

                var personNo = attributes.Contains("personNo");

                var attributesList = GetAttributeListAsync(attributes);

                if (personNo)
                {
                    attributesList.Add("personNo");
                }

                return new ServiceResult(true, "Get Verification Method Attributes Success", attributesList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAttributesByMethodCodeAsync: {ex.Message}");
                return new ServiceResult(false, "Get Verification Method Attributes Failed");
            }
        }

        public List<string> GetAttributesList(List<string> attributeList)
        {
            var kycAttributes = new Dictionary<string, Dictionary<string, List<string>>>
            {
                ["currentNationality"] = new()
                {
                    ["code"] = new List<string>(),
                    ["descriptionAr"] = new List<string>(),
                    ["descriptionEn"] = new List<string>(),
                    ["abbreviation"] = new List<string>()
                },

                ["tribe"] = new()
                {
                    ["code"] = new List<string>(),
                    ["descriptionAr"] = new List<string>(),
                    ["descriptionEn"] = new List<string>()
                },

                ["personClass"] = new()
                {
                    ["code"] = new List<string>(),
                    ["descriptionEn"] = new List<string>(),
                    ["abbreviation"] = new List<string>()
                },

                ["gender"] = new()
                {
                    ["code"] = new List<string>(),
                    ["descriptionAr"] = new List<string>(),
                    ["descriptionEn"] = new List<string>()
                },

                ["maritalStatus"] = new()
                {
                    ["code"] = new List<string>(),
                    ["descriptionAr"] = new List<string>(),
                    ["descriptionEn"] = new List<string>()
                },

                ["binaryObjects"] = new()
                {
                    ["binaryDocType1"] = new List<string>(),
                    ["binaryDocType2"] = new List<string>(),
                    ["binaryDocType3"] = new List<string>()
                },

                ["activePassport"] = new()
                {
                    ["documentType"] = new List<string> { "code", "descriptionAr", "descriptionEn" },
                    ["documentNo"] = new List<string>(),
                    ["documentIssueCountry"] = new List<string> { "code", "descriptionAr", "descriptionEn", "abbreviation" },
                    ["issueDate"] = new List<string>(),
                    ["expiryDate"] = new List<string>()
                },

                ["emiratesIdDetail"] = new()
                {
                    ["documentType"] = new List<string> { "code", "descriptionAr", "descriptionEn", "abbreviation" },
                    ["documentNo"] = new List<string>(),
                    ["issueDate"] = new List<string>(),
                    ["expiryDate"] = new List<string>()
                },

                ["title"] = new()
                {
                    ["code"] = new List<string>(),
                    ["descriptionAr"] = new List<string>(),
                    ["descriptionEn"] = new List<string>()
                },

                ["occupation"] = new()
                {
                    ["code"] = new List<string>(),
                    ["descriptionAr"] = new List<string>(),
                    ["descriptionEn"] = new List<string>()
                },

                ["localAddress"] = new()
                {
                    ["emirate"] = new List<string>(),
                    ["city"] = new List<string>(),
                    ["area"] = new List<string>(),
                    ["street"] = new List<string>(),
                    ["poBox"] = new List<string>(),
                    ["mobileNo"] = new List<string>(),
                    ["homePhone"] = new List<string>(),
                    ["workPhone"] = new List<string>()
                },

                ["immigrationFile"] = new()
                {
                    ["fileType"] = new List<string>(),
                    ["fileNumber"] = new List<string>(),
                    ["issuePlace"] = new List<string>(),
                    ["issueDate"] = new List<string>(),
                    ["expiryDate"] = new List<string>()
                },

                ["immigrationStatus"] = new()
                {
                    ["code"] = new List<string>(),
                    ["descriptionAr"] = new List<string>(),
                    ["descriptionEn"] = new List<string>()
                },

                ["sponsor"] = new()
                {
                    ["nameAr"] = new List<string>(),
                    ["nameEn"] = new List<string>(),
                    ["department"] = new List<string>(),
                    ["number"] = new List<string>(),
                    ["localAddress"] = new List<string>(),
                    ["type"] = new List<string>(),
                    ["sponsorNationality"] = new List<string>()
                },

                ["travelDetail"] = new()
                {
                    ["isInside"] = new List<string>(),
                    ["personNo"] = new List<string>(),
                    ["eboCode"] = new List<string>(),
                    ["sdeCode"] = new List<string>(),
                    ["travelType"] = new List<string>(),
                    ["travelDate"] = new List<string>(),
                    ["travelDocumentNo"] = new List<string>(),
                    ["travelDocumentIssueDate"] = new List<string>(),
                    ["travelDocumentExpiryDate"] = new List<string>()
                }
            };
            List<string> duplicateAttributes = new List<string>();
            foreach (var parent in kycAttributes)
            {
                string parentKey = parent.Key;
                var childDict = parent.Value;
                if (!attributeList.Contains(parentKey))
                {
                    continue;
                }
                duplicateAttributes.Add(parentKey);
                foreach (var child in childDict)
                {
                    string childKey = child.Key;
                    var subList = child.Value;
                    if (!attributeList.Contains(childKey))
                    {
                        continue;
                    }
                    duplicateAttributes.Add(childKey);
                    if (subList.Count == 0)
                    {
                        attributeList.Add($"{parentKey}.{childKey}");
                    }
                    else
                    {
                        foreach (var subAttr in subList)
                        {
                            if (!attributeList.Contains(subAttr))
                            {
                                continue;
                            }
                            duplicateAttributes.Add(subAttr);
                            attributeList.Add($"{parentKey}.{childKey}.{subAttr}");
                        }
                    }
                }
            }
            foreach (var dupAttr in duplicateAttributes)
            {
                attributeList.Remove(dupAttr);
            }
            return attributeList;
        }

        public List<string> GetAttributeListAsync(List<string> attributesNames)
        {
            var kycAttributes = new List<AttributeNode>
            {
                new AttributeNode
                {
                    Name = "currentNationality",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "code" },
                        new AttributeNode { Name = "descriptionAr" },
                        new AttributeNode { Name = "descriptionEn" },
                        new AttributeNode { Name = "abbreviation" }
                    }
                },
                new AttributeNode
                    { Name = "tribe",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "code" },
                        new AttributeNode { Name = "descriptionAr" },
                        new AttributeNode { Name = "descriptionEn" }
                    }
                },
                new AttributeNode
                {
                    Name = "personClass",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "code" },
                        new AttributeNode { Name = "descriptionEn" },
                        new AttributeNode { Name = "abbreviation" }
                    }
                },
                new AttributeNode
                {
                    Name = "gender",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "code" },
                        new AttributeNode { Name = "descriptionAr" },
                        new AttributeNode { Name = "descriptionEn" }
                    }
                },
                new AttributeNode
                {
                    Name = "maritalStatus",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "code" },
                        new AttributeNode { Name = "descriptionAr" },
                        new AttributeNode { Name = "descriptionEn" }
                    }
                },
                new AttributeNode
                {
                    Name = "binaryObjects",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "binaryObjects[0]" },
                        new AttributeNode { Name = "binaryObjects[1]" },
                        new AttributeNode { Name = "binaryObjects[2]" }
                    }
                },
                new AttributeNode
                {
                    Name = "activePassport",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode
                        {
                            Name = "documentType",
                            SubAttributes = new List<AttributeNode>
                            {
                                new AttributeNode { Name = "code" },
                                new AttributeNode { Name = "descriptionAr" },
                                new AttributeNode { Name = "descriptionEn" }
                            }
                        },
                        new AttributeNode { Name = "documentNo" },
                        new AttributeNode
                        {
                            Name = "documentIssueCountry",
                            SubAttributes = new List<AttributeNode>
                            {
                                new AttributeNode { Name = "code" },
                                new AttributeNode { Name = "descriptionAr" },
                                new AttributeNode { Name = "descriptionEn" },
                                new AttributeNode { Name = "abbreviation" }
                            }
                        },
                        new AttributeNode { Name = "issueDate" },
                        new AttributeNode { Name = "expiryDate" }
                    }
                },
                new AttributeNode
                {
                    Name = "emiratesIdDetail",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode
                        {
                            Name = "documentType",
                            SubAttributes = new List<AttributeNode>
                            {
                                new AttributeNode { Name = "code" },
                                new AttributeNode { Name = "descriptionAr" },
                                new AttributeNode { Name = "descriptionEn" },
                                new AttributeNode { Name = "abbreviation" }
                            }
                        },
                        new AttributeNode { Name = "documentNo" },
                        new AttributeNode { Name = "issueDate" },
                        new AttributeNode { Name = "expiryDate" }
                    }
                },
                new AttributeNode
                    { Name = "title",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "code" },
                        new AttributeNode { Name = "descriptionAr" },
                        new AttributeNode { Name = "descriptionEn" }
                    }
                },
                new AttributeNode
                {
                    Name = "occupation",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "code" },
                        new AttributeNode { Name = "descriptionAr" },
                        new AttributeNode { Name = "descriptionEn" }
                    }
                },
                new AttributeNode
                {
                    Name = "localAddress",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "emirate" },
                        new AttributeNode { Name = "city" },
                        new AttributeNode { Name = "area" },
                        new AttributeNode { Name = "street" },
                        new AttributeNode { Name = "poBox" },
                        new AttributeNode { Name = "mobileNo" },
                        new AttributeNode { Name = "homePhone" },
                        new AttributeNode { Name = "workPhone" }
                    }
                },
                new AttributeNode
                {
                    Name = "immigrationFile",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "fileType" },
                        new AttributeNode { Name = "fileNumber" },
                        new AttributeNode { Name = "issuePlace" },
                        new AttributeNode { Name = "issueDate" },
                        new AttributeNode { Name = "expiryDate" }
                    }
                },
                new AttributeNode
                {
                    Name = "immigrationStatus",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "code" },
                        new AttributeNode { Name = "descriptionAr" },
                        new AttributeNode { Name = "descriptionEn" }
                    }
                },
                new AttributeNode
                {
                    Name = "sponsor",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "nameAr" },
                        new AttributeNode { Name = "nameEn" },

                        new AttributeNode
                        {
                            Name = "department",
                            SubAttributes = new List<AttributeNode>
                            {
                                new AttributeNode { Name = "code" },
                                new AttributeNode { Name = "descriptionAr" },
                                new AttributeNode { Name = "descriptionEn" }

                            }
                        },

                        new AttributeNode { Name = "number" },

                        new AttributeNode
                        {
                            Name = "localAddress",
                            SubAttributes = new List<AttributeNode>
                        {
                            new AttributeNode
                            {
                                Name = "emirate",
                                SubAttributes = new List<AttributeNode>
                                {
                                    new AttributeNode { Name = "code" },
                                    new AttributeNode { Name = "descriptionAr" },
                                    new AttributeNode { Name = "descriptionEn" }
                                }
                            },
                            new AttributeNode
                            {
                                Name = "city",
                                SubAttributes = new List<AttributeNode>
                                {
                                    new AttributeNode { Name = "code" },
                                    new AttributeNode { Name = "descriptionAr" },
                                    new AttributeNode { Name = "descriptionEn" }
                                }
                            },
                            new AttributeNode
                            {
                                Name = "area",
                                SubAttributes = new List<AttributeNode>
                                {
                                    new AttributeNode { Name = "code" },
                                    new AttributeNode { Name = "descriptionAr" },
                                    new AttributeNode { Name = "descriptionEn" }
                                }
                            },
                            new AttributeNode
                            { 
                                Name = "street" ,
                                SubAttributes = new List<AttributeNode>
                                {
                                    new AttributeNode { Name = "code" }
                                }
                            },
                            new AttributeNode { Name = "poBox" },
                            new AttributeNode { Name = "mobileNo" },
                            new AttributeNode { Name = "homePhone" }
                        }
                    },

                        new AttributeNode
                        {
                            Name = "type",
                            SubAttributes = new List<AttributeNode>
                            {
                                new AttributeNode { Name = "code" },
                                new AttributeNode { Name = "descriptionAr" },
                                new AttributeNode { Name = "descriptionEn" }
                            }
                        },

                        new AttributeNode
                        {
                            Name = "sponsorNationality",
                            SubAttributes = new List<AttributeNode>
                            {
                                new AttributeNode { Name = "code" }
                            }
                        }
                    }
                },
                new AttributeNode
                {
                    Name = "travelDetail",
                    SubAttributes = new List<AttributeNode>
                    {
                        new AttributeNode { Name = "isInside" },
                        new AttributeNode { Name = "personNo" },
                        new AttributeNode { Name = "eboCode" },
                        new AttributeNode { Name = "sdeCode" },
                        new AttributeNode { Name = "travelType" },
                        new AttributeNode { Name = "travelDate" },
                        new AttributeNode { Name = "travelDocumentNo" },
                        new AttributeNode { Name = "travelDocumentIssueDate" },
                        new AttributeNode { Name = "travelDocumentExpiryDate" }
                    }
                }
            };

            var attributesList = GetMatchedAttributePaths(kycAttributes, attributesNames);

            return attributesList;
        }

        public List<string> GetMatchedAttributePaths(List<AttributeNode> root, List<string> inputAttributes)
        {
            List<string> duplicates= new List<string>();

            foreach (var attribute in root)
            {
                void Traverse(AttributeNode node, string parentPath)
                {
                    if (node == null) return;

                    if(!inputAttributes.Contains(node.Name))
                    {
                        return;
                    }

                    if (node.SubAttributes.Count == 0)
                    {
                        duplicates.Add(node.Name);

                        inputAttributes.Add(parentPath + node.Name);

                        return;
                    }

                    if (node.SubAttributes.Count > 0)
                    {
                        duplicates.Add(node.Name);
                    }
                    foreach (var attribute in node.SubAttributes)
                    {
                        Traverse(attribute, parentPath + node.Name + ".");
                    }
                }

                Traverse(attribute, "");
            }
            foreach(var dup in duplicates)
            {
                inputAttributes.Remove(dup);
            }
            return inputAttributes;
        }
    }
}