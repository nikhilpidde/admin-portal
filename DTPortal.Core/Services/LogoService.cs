using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class LogoService : ILogoService
    {
        private readonly ILogger<LogoService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public LogoService(IUnitOfWork unitOfWork,
           ILogger<LogoService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PortalSetting> GetLogoPrimary()
        {
            return await _unitOfWork.PortalSettings.GetSettingByNameAsync("logo_primary");
        }

        public async Task<ServiceResult> UpdateLogoPrimary(string base64Image, string updatedBy)
        {
            try
            {
                var portalSetting = await _unitOfWork.PortalSettings.GetSettingByNameAsync("logo_primary");
                if (portalSetting == null)
                {
                    _logger.LogInformation("Primary Logo record not found. Creating new Logo record...");

                    portalSetting = new PortalSetting
                    {
                        Name = "logo_primary",
                        Value = Encoding.ASCII.GetBytes(base64Image),
                        CreatedBy = updatedBy,
                        UpdatedBy = string.Empty,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    await _unitOfWork.PortalSettings.AddAsync(portalSetting);
                    await _unitOfWork.SaveAsync();

                    return new ServiceResult(true, "Logo created successfully");
                }

                portalSetting.Value = Encoding.ASCII.GetBytes(base64Image);
                portalSetting.UpdatedBy = updatedBy;
                portalSetting.ModifiedDate = DateTime.Now;

                _unitOfWork.PortalSettings.Update(portalSetting);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Logo updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new ServiceResult(false, "An error occured while updating logo. Please try later.");
        }
    }
}
