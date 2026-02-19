using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DTPortal.Core.Constants;

namespace DTPortal.Core.Services
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMCValidationService _mcValidationService;

        public RoleManagementService(IUnitOfWork unitOfWork,
            ILogger<RoleManagementService> logger,
            IMCValidationService mcValidationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mcValidationService = mcValidationService;
        }
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<IEnumerable<RoleLookupItem>> GetRoleLookupItemsAsync()
        {
            _logger.LogInformation("-->GetRoleLookupItemsAsync");
            return await _unitOfWork.Roles.GetRoleLookupItemsAsync();
        }

        public async Task<IEnumerable<ActivityLookupItem>> GetActivityLookupItemsAsync()
        {
            return await _unitOfWork.Activities.GetActivityLookupItemsAsync();
        }

        public async Task<Role> GetRoleAsync(int id)
        {
            try
            {
                return await _unitOfWork.Roles.GetRoleByRoleIdWithActivities(id);
            }
            catch(Exception error)
            {
                _logger.LogError("GetRoleAsync Failed: {0}",
                    error.Message);
                return null;
            }
        }

        public async Task<RoleResponse> AddRoleAsync(Role role,
            IDictionary<int, bool> selectedActivityIds,
            bool makerCheckerFlag = false)
        {
            if(null == role)
            {
                _logger.LogError("Invalid input");
                return new RoleResponse("Invalid input");
            }

            var isExists = await _unitOfWork.Roles.IsRoleExistsByName(role);
            if(true == isExists)
            {
                _logger.LogError("IsRoleExistsByName: Role already exists: {0}", role.Name);
                return new RoleResponse("Role already exists, Please try with different name");
            }

            // Check isMCEnabled
            var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.RoleActivityId);

            if (false == makerCheckerFlag)
            {
                var activityList = await _unitOfWork.Activities.GetActivityLookupItemsAsync();

                foreach (var item in activityList)
                {
                    if (selectedActivityIds.ContainsKey(item.Id))
                    {
                        selectedActivityIds.TryGetValue(item.Id, out var val);

                        var roleActivity = new RoleActivity
                        {
                            ActivityId = item.Id,
                            IsChecker = val,
                            LocationOnlyAccess = false,
                            NativeAccess = true,
                            WebAccess = false,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            CreatedBy = role.CreatedBy,
                            UpdatedBy = role.UpdatedBy,
                            IsEnabled = true
                        };
                        role.RoleActivities.Add(roleActivity);
                    }
                    else
                    {
                        var roleActivity = new RoleActivity
                        {
                            ActivityId = item.Id,
                            IsChecker = false,
                            LocationOnlyAccess = false,
                            NativeAccess = true,
                            WebAccess = false,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            CreatedBy = role.CreatedBy,
                            UpdatedBy = role.UpdatedBy,
                            IsEnabled = false
                        };
                        role.RoleActivities.Add(roleActivity);
                    }
                }
            }

            //foreach (var id in selectedActivityIds)
            //{
            //    var roleActivity = new RoleActivity
            //    {
            //        ActivityId = id.Key,
            //        IsChecker = id.Value,
            //        LocationOnlyAccess = false,
            //        NativeAccess = true,
            //        WebAccess = false,
            //        CreatedDate = DateTime.Now,
            //        ModifiedDate = DateTime.Now,
            //        CreatedBy = role.CreatedBy,
            //        UpdatedBy = role.UpdatedBy
            //    };
            //    role.RoleActivities.Add(roleActivity);
            //}

            role.CreatedDate = DateTime.Now;
            role.ModifiedDate = DateTime.Now;
            role.Status = "ACTIVE";

            roleRequest request = new roleRequest()
            {
                role = role,
                selectedActivityIds = selectedActivityIds
            };

            if (false == makerCheckerFlag && true == isEnabled)
            {
                // Validation in makerchecker table
                var response = await _mcValidationService.IsCheckerApprovalRequired(
                    ActivityIdConstants.RoleActivityId,
                    "CREATE", role.CreatedBy,
                    JsonConvert.SerializeObject(request));
                if (!response.Success)
                {
                    _logger.LogError("IsCheckerApprovalRequired Failed");
                    return new RoleResponse(response.Message);
                }
                if (response.Result)
                {
                    return new RoleResponse(role, "Your request sent for approval");
                }

            }
            try
            {
                await _unitOfWork.Roles.AddAsync(role);
                await _unitOfWork.SaveAsync();

                return new RoleResponse(role,"Role created successfully");
            }
            catch
            {
                // Log the exception 
                return new RoleResponse("An error occurred while creating the role." +
                    " Please contact the admin.");
            }
        }

        private async Task<RoleResponse> UpdateMCOffRoleAsync(Role role,
            IDictionary<int, bool> selectedActivityIds)
        {

            var rolesinDb = await _unitOfWork.Roles.GetRoleByRoleIdWithActivities(role.Id);
            if (null == rolesinDb)
            {
                _logger.LogError("No role activites found");
                return new RoleResponse("No role activites found");
            }

            //var roleActivity = new List<RoleActivity>();
            //foreach (var item in rolesinDb.RoleActivities)
            //{
            //    rolesinDb.RoleActivities.Remove(item);
            //    _unitOfWork.RoleActivity.Remove(item);
            //    await _unitOfWork.SaveAsync();
            //}

            foreach(var item in rolesinDb.RoleActivities)
            {
                if (selectedActivityIds.ContainsKey(item.ActivityId))

                {
                    selectedActivityIds.TryGetValue(item.ActivityId, out var val);

                    item.IsChecker = val;
                    item.IsEnabled = true;
                }
                else
                {
                    item.IsChecker = false;
                    item.IsEnabled = false;
                }
            }

            //foreach (var id in selectedActivityIds)
            //{
            //    var RoleActivity = new RoleActivity
            //    {
            //        ActivityId = id.Key,
            //        IsChecker = id.Value,
            //        LocationOnlyAccess = false,
            //        NativeAccess = true,
            //        WebAccess = false,
            //        CreatedDate = DateTime.Now,
            //        ModifiedDate = DateTime.Now,
            //        CreatedBy = role.CreatedBy,
            //        UpdatedBy = role.UpdatedBy,
            //        RoleId = role.Id
            //    };
            //    roleActivity.Add(RoleActivity);
            //}

            rolesinDb.Description = role.Description;
            rolesinDb.ModifiedDate = DateTime.Now;
            rolesinDb.UpdatedBy = role.UpdatedBy;
            //rolesinDb.RoleActivities = roleActivity;

            try
            {
                _unitOfWork.Roles.Update(rolesinDb);
                await _unitOfWork.SaveAsync();


                return new RoleResponse(role, "Role updated successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("UpdateMCOffRoleAsync Failed: {0}",
                        error.Message);
                return new RoleResponse("An error occurred while updating the role. Please contact the admin.");
            }
        }

        public async Task<RoleResponse> UpdateRoleAsync(Role role,
            IDictionary<int, bool> selectedActivityIds, bool makerCheckerFlag = false)
        {
            //role.RoleActivities.Clear();
            var isEnabled = false;
            //_unitOfWork.Roles.Update(role);
            //await _unitOfWork.SaveAsync();

            isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.RoleActivityId);

            if (false == makerCheckerFlag && false == isEnabled)
            {
                var rolresp = await UpdateMCOffRoleAsync(role, selectedActivityIds);

                return rolresp;
            }


            var roleinDb = await _unitOfWork.Roles.GetRoleByRoleIdWithActivities(role.Id);
            if(null == roleinDb)
            {
                _logger.LogError("No role activites found");
                return new RoleResponse("No role activites found");
            }

            if(roleinDb.Status == "DELETED")
            {
                _logger.LogError("Role already deleted");
                return new RoleResponse("Role already deleted");
            }

            if (false == makerCheckerFlag)
            {
                 _unitOfWork.DisableDetectChanges();
                // Check isMCEnabled

                foreach (var item in role.RoleActivities)
                {
                    item.Role = null;
                    if(null != item.Activity)
                    {
                        item.Activity = null;
                    }
                }

                role.CreatedDate = DateTime.Now;
                role.ModifiedDate = DateTime.Now;
                role.Status = "ACTIVE";
            }

            roleRequest request = new roleRequest()
            {
                role = role,
                selectedActivityIds = selectedActivityIds
            };

            if (false == makerCheckerFlag && true == isEnabled)
            {
                _unitOfWork.DisableDetectChanges();

                // Validation in makerchecker table
                var response = await _mcValidationService.IsCheckerApprovalRequired(
                    ActivityIdConstants.RoleActivityId,
                    "UPDATE", role.UpdatedBy,
                    JsonConvert.SerializeObject(request));
                if (!response.Success)
                {
                    _logger.LogError("IsCheckerApprovalRequired Failed");
                    return new RoleResponse(response.Message);
                }
                if (response.Result)
                {
                    return new RoleResponse(role, "Your request sent for approval");
                }
            }

            if(false == makerCheckerFlag)
                _unitOfWork.DisableDetectChanges();

            var resp = await UpdateMCOffRoleAsync(role, selectedActivityIds);

            return resp;
            //var rolesinDb = await _unitOfWork.Roles.GetRoleByRoleIdWithActivities(role.Id);
            //if (null == rolesinDb)
            //{
            //    _logger.LogError("No role activites found");
            //    return new RoleResponse("No role activites found");
            //}

            //var roleActivity = new List<RoleActivity>();
            //foreach (var item in rolesinDb.RoleActivities)
            //{
            //    rolesinDb.RoleActivities.Remove(item);
            //    if(true == makerCheckerFlag)
            //        _unitOfWork.RoleActivity.Remove(item);
            //    await _unitOfWork.SaveAsync();
            //}

            //foreach (var id in selectedActivityIds)
            //{
            //    var RoleActivity = new RoleActivity
            //    {
            //        ActivityId = id.Key,
            //        IsChecker = id.Value,
            //        LocationOnlyAccess = false,
            //        NativeAccess = true,
            //        WebAccess = false,
            //        CreatedDate = DateTime.Now,
            //        ModifiedDate = DateTime.Now,
            //        CreatedBy = role.CreatedBy,
            //        UpdatedBy = role.UpdatedBy,
            //        RoleId = role.Id
            //    };
            //    roleActivity.Add(RoleActivity);
            //}

            //rolesinDb.Description = role.Description;
            //rolesinDb.ModifiedDate = DateTime.Now;
            //rolesinDb.UpdatedBy = role.UpdatedBy;
            //rolesinDb.RoleActivities = roleActivity;

            //try
            //{
            //    _unitOfWork.Roles.Update(rolesinDb);
            //    await _unitOfWork.SaveAsync();


            //    return new RoleResponse(role, "Role updated successfully");
            //}
            //catch(Exception error)
            //{
            //    // Log the exception 
            //    return new RoleResponse("An error occurred while updating the role. Please contact the admin.");
            //}

        }

        public async Task<RoleResponse> DeleteRoleAsync(int id, string updatedBy,
            bool makerCheckerFlag = false)
        {
            var roleInDB = new Role();

            roleInDB = await _unitOfWork.Roles.GetByIdAsync(id);
            if (roleInDB == null)
            {
                return new RoleResponse("Role not found");
            }

            // Check isMCEnabled
            var isEnabled = await _mcValidationService.IsMCEnabled(
                ActivityIdConstants.RoleActivityId);

            if (false == makerCheckerFlag && true == isEnabled)
            {
                // Validation in makerchecker table
                var response = await _mcValidationService.IsCheckerApprovalRequired(ActivityIdConstants.RoleActivityId,
                    "DELETE",
                    roleInDB.UpdatedBy,
                    JsonConvert.SerializeObject(new { Id = id, UpdatedBy = updatedBy }));
                if (!response.Success)
                {
                    _logger.LogError("IsCheckerApprovalRequired Failed");
                    return new RoleResponse(response.Message);
                }
                if (response.Result)
                {
                    return new RoleResponse(roleInDB, "Your request sent for approval");
                }


            }
            try
            {
                roleInDB.Status = "DELETED";
                roleInDB.UpdatedBy = updatedBy;
                roleInDB.ModifiedDate = DateTime.Now;

                _unitOfWork.Roles.Update(roleInDB);
                await _unitOfWork.SaveAsync();

                return new RoleResponse(roleInDB, "Role deleted successfully");
            }
            catch (Exception)
            {
                // Do some logging stuff
                return new RoleResponse($"An error occurred while deleting the role. Please contact the admin.");
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        public async Task<RoleResponse> UpdateRoleState(int id,
            bool isApproved, string reason = null)
        {
            var roleInDB = await _unitOfWork.Roles.GetByIdAsync(id);
            if (roleInDB == null)
            {
                return new RoleResponse("Role not found");
            }

            if (isApproved)
            {
                roleInDB.Status = "ACTIVE";
            }
            else
            {
                roleInDB.Status = "BLOCKED";
            }
            try
            {
                _unitOfWork.Roles.Update(roleInDB);
                await _unitOfWork.SaveAsync();

                return new RoleResponse(roleInDB);
            }
            catch (Exception)
            {
                // Do some logging stuff
                return new RoleResponse($"An error occurred while changing state of the role. Please contact the admin.");
            }
        }

        public async Task<RoleResponse> ActivateRoleAsync(int id)
        {

            var roleInDB = await _unitOfWork.Roles.GetByIdAsync(id);
            if (roleInDB == null)
            {
                return new RoleResponse("Role not found");
            }

            roleInDB.Status = "ACTIVE";

            try
            {
                _unitOfWork.Roles.Update(roleInDB);
                await _unitOfWork.SaveAsync();

                return new RoleResponse(roleInDB);
            }
            catch (Exception)
            {
                // Do some logging stuff
                return new RoleResponse($"An error occurred while changing state of the role. Please contact the admin.");
            }
        }

        public async Task<RoleResponse> DeActivateRoleAsync(int id)
        {

            var roleInDB = await _unitOfWork.Roles.GetByIdAsync(id);
            if (roleInDB == null)
            {
                return new RoleResponse("Role not found");
            }

            roleInDB.Status = "DEACTIVATED";

            try
            {
                _unitOfWork.Roles.Update(roleInDB);
                await _unitOfWork.SaveAsync();

                return new RoleResponse(roleInDB);
            }
            catch (Exception)
            {
                // Do some logging stuff
                return new RoleResponse($"An error occurred while changing state of the role. Please contact the admin.");
            }
        }
    }
}
