using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IOnboardingTemplateService
    {
        Task<IEnumerable<OnboardingTemplateDTO>> GetAllTemplatesAsync(string token);

        Task<IEnumerable<OnboardingMethodDTO>> GetAllMethodsAsync(string token);

        Task<IEnumerable<OnboardingStepDTO>> GetAllStepsAsync(string token);

        Task<OnboardingTemplateDTO> GetTemplateAsync(int id, string token);

        Task<bool> IsTemplateExists(string templateName, string methodName, string token);

        Task<ServiceResult> AddTemplateAsync(OnboardingTemplateDTO onboardingTemplate, string token , bool makerCheckerFlag = false);

        Task<ServiceResult> UpdateTemplateAsync(OnboardingTemplateDTO onboardingTemplate, string token, bool makerCheckerFlag = false);

        Task<ServiceResult> PublishTemplateAsync(int id, string uuid, string token, bool makerCheckerFlag = false);

        Task<ServiceResult> UnPublishTemplateAsync(int id, string uuid, string token, bool makerCheckerFlag = false);

        Task<ServiceResult> DeleteTemplateAsync(int id, string uuid, string token, bool makerCheckerFlag = false);
    }
}
