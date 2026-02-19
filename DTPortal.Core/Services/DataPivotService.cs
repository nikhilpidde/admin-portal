using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class DataPivotService : IDataPivotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly idp_configuration idpConfiguration;
        private readonly OIDCConstants OIDCConstants;
        private readonly ICacheClient _cacheClient;
        private readonly IHelper _helper;

        public DataPivotService(
            IUnitOfWork unitOfWork,
            ILogger<DataPivotService> logger,
            ICacheClient cacheClient,
            IGlobalConfiguration globalConfiguration,
            IConfiguration Configuration,
            IHelper helper)

        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheClient = cacheClient;
            _globalConfiguration = globalConfiguration;
            _helper = helper;


            idpConfiguration = _globalConfiguration.GetIDPConfiguration();
            if (null == idpConfiguration)
            {
                _logger.LogError("Get IDP Configuration failed");
                throw new NullReferenceException();
            }

            var errorConfiguration = _globalConfiguration.GetErrorConfiguration();
            if (null == errorConfiguration)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }

            OIDCConstants = errorConfiguration.OIDCConstants;
            if (null == errorConfiguration)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }
        }
        public async Task<IEnumerable<DataPivot>> GetAllPivotDataAsync()
        {
            return await _unitOfWork.Datapivots.GetAllPivotDataAsync();
        }

        public async Task<DataPivotResponse> CreatePivotDataAsync(DataPivot dataPivot)
        {
            //var isExists = await _unitOfWork.Datapivots.IsCreatePivotExistsAsync(dataPivot);
            //if (true == isExists)
            //{
            //    _logger.LogError("Datapivot already exists with given id");
            //    return new DataPivotResponse("DataPivot already exists with given");
            //}
            dataPivot.CreatedDate = DateTime.Now;
            dataPivot.ModifiedDate = DateTime.Now;
            dataPivot.Status = "ACTIVE";
            try
            {
                dataPivot.PublicKeyCert = Convert.ToBase64String(Encoding.UTF8.GetBytes(dataPivot.PublicKeyCert));
                await _unitOfWork.Datapivots.AddAsync(dataPivot);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("<---CreatePivotDataAsync");
                return new DataPivotResponse(dataPivot, "DataPivot created successfully");
            }
            catch
            {
                _logger.LogError("DataPivot AddAsync failed");
                _logger.LogInformation("<---CreatePivotDataAsync");
                return new DataPivotResponse("An error occurred while creating the DataPivot.");
            }
        }

        public async Task<IEnumerable<DataPivot>> GetPivotUserAsync(string orgid)
        {

            return await _unitOfWork.Datapivots.GetPivotByIdAsync(orgid);

        }
        public async Task<DataPivot> GetPivotAsync(int id)
        {
            _logger.LogInformation("--->GetPivotAsync");
            var clientInDb = await _unitOfWork.Datapivots.GetByIdAsync(id);
            if (null == clientInDb)
            {
                return null;
            }


            return clientInDb;
        }

        public async Task<DataPivot> GetPivotByNameAsync(string name)
        {
            _logger.LogInformation("--->GetPivotByNameAsync");
            var clientInDb = await _unitOfWork.Datapivots.GetByNameAsync(name);
            if (null == clientInDb)
            {
                return null;
            }


            return clientInDb;
        }

        public async Task<DataPivotResponse> DeleteDatapivotAsync(int id, string UUID)
        {
            var DatapivotinDb = await _unitOfWork.Datapivots.GetByIdAsync(id);
            if (DatapivotinDb == null)
            {
                return new DataPivotResponse("Datapivot not found");
            }
            try
            {
                DatapivotinDb.Status = "DELETED";
                DatapivotinDb.ModifiedDate = DateTime.Now;
                DatapivotinDb.UpdatedBy = UUID;
                _unitOfWork.Datapivots.Update(DatapivotinDb);
                await _unitOfWork.SaveAsync();
                return new DataPivotResponse(DatapivotinDb, "Datapivot Deleted Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete the Datapivot : {0}", ex.Message);
                return new DataPivotResponse("Error Occured while deleting the Datapivot");
            }
        }

        public async Task<DataPivotResponse> UpdatePivotDataAsync(DataPivot dataPivot)
        {

            //var isExists = await _unitOfWork.Datapivots.IsUpdatePivotExistsAsync(dataPivot);
            // if (false == isExists)
            // {
            //     _logger.LogError("Datapivot not found");
            //     return new DataPivotResponse("Datapivot not found");
            // }
            // var allDataPivots = await _unitOfWork.Datapivots.GetAllAsync();
            // foreach(var item in allDataPivots)
            // {
            //     if(item.Id == dataPivot.Id)
            //     {
            //         _logger.LogError("Datapivot already exists");
            //         return new DataPivotResponse("Datapivot already exists");

            //     }
            // }
            //var DatapivotinDb = _unitOfWork.Datapivots.GetById(dataPivot.Id);
            //if (null == DatapivotinDb)
            //{
            //    _logger.LogError("DataPivot not found");
            //    return new DataPivotResponse("DataPivot not found");
            //}
            try
            {
                var DatapivotinDb = _unitOfWork.Datapivots.GetById(dataPivot.Id);
                if (null == DatapivotinDb)
                {
                    _logger.LogError("DataPivot not found");
                    return new DataPivotResponse("DataPivot not found");
                }

                DatapivotinDb.Name = dataPivot.Name;
                DatapivotinDb.Description = dataPivot.Description;
                DatapivotinDb.OrgnizationId = dataPivot.OrgnizationId;
                DatapivotinDb.AttributeConfiguration = dataPivot.AttributeConfiguration;
                DatapivotinDb.ServiceConfiguration = dataPivot.ServiceConfiguration;
                DatapivotinDb.AuthScheme = dataPivot.AuthScheme;
                DatapivotinDb.ScopeId= dataPivot.ScopeId;
                DatapivotinDb.DataPivotLogo= dataPivot.DataPivotLogo;
                DatapivotinDb.CategoryId= dataPivot.CategoryId;
                if (null != DatapivotinDb.PublicKeyCert)
                    DatapivotinDb.PublicKeyCert = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(dataPivot.PublicKeyCert));


                _unitOfWork.Datapivots.Update(DatapivotinDb);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("<---UpdatePivotDataAsync");
                return new DataPivotResponse(dataPivot, "DataPivot Updated successfully");
            }
            catch
            {
                _logger.LogError("DataPivot Update failed");
                _logger.LogInformation("<---UpdatePivotDataAsync");
                return new DataPivotResponse("An error occurred while updating the DataPivot.");
            }
        }

        public async Task<ServiceResult> GetDocumentsListAsync(string AccessToken)
        {
            try
            {
                _logger.LogInformation("--->GetDocumentsListAsync");


                Accesstoken accessToken = null;
                try
                {
                    // Get the access token record
                    accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                        AccessToken);
                    if (null == accessToken)
                    {
                        _logger.LogError("Access token not recieved from cache." +
                            "Expired or Invalid access token");
                        ErrorResponseDTO error = new ErrorResponseDTO();
                        error.error = OIDCConstants.InvalidToken;
                        error.error_description = OIDCConstants.InvalidTokenDesc;
                        return new ServiceResult(false,"Access token not recieved from cache." +
                            "Expired or Invalid access token",error);
                    }
                }
                catch (CacheException ex)
                {
                    _logger.LogError("Failed to get Access Token Record");
                    ErrorResponseDTO error = new ErrorResponseDTO();
                    error.error = OIDCConstants.InternalError;
                    error.error_description = _helper.GetRedisErrorMsg(
                        ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                    return new ServiceResult(false, "Failed to get Access Token Record", error);
                }

                //var scope = await _unitOfWork.Scopes.GetScopeByNameAsync(accessToken.Scopes);
                //if (scope == null)
                //{
                //    _logger.LogError("Scope cannot be null");
                //    return new ServiceResult(false, "Scope cannot be null");
                //}

                var list = await _unitOfWork.Datapivots.GetAllAsync();
                if (list == null || !list.Any())
                {
                    _logger.LogError("Document list is empty");
                    return new ServiceResult(false, "Document list is empty");
                }
                List<DataPivotDocDTO> docList = new();
                foreach (var item in list)
                {
                    docList.Add(new DataPivotDocDTO()
                    {
                        DataPivotUID = item.DataPivotUid,
                        Name = item.Name,
                        Status = item.Status,
                    });
                }

                return new ServiceResult(true, "Document list retrieved successfully", docList);

            }
            catch
            {
                _logger.LogError("Get Document list failed");
                _logger.LogInformation("<---GetDocumentsListAsync");
                return new ServiceResult(false, "An error occurred while getting the document list.");
            }
        }
        public async Task<DataPivot> GetPivotByUidAsync(string Uid)
        {
            _logger.LogInformation("--->GetPivotByNameAsync");
            var dataPivot = await _unitOfWork.Datapivots.GetByUIDAsync(Uid);
            if (null == dataPivot)
            {
                return null;
            }


            return dataPivot;
        }
        public async Task<IEnumerable<DataPivot>> GetAllPivotDataByOrgIdAsync(string orgId)
        {
            return await _unitOfWork.Datapivots.GetAllPivotDataByorgIdAsync(orgId);
        }
        public async Task<ServiceResult> GetDataPivotByCatIdAsync(string catId,string AccessToken)
        {
            try
            {
                _logger.LogInformation("--->GetDataPivotByCatIdAsync");


                Accesstoken accessToken = null;
                try
                {
                    // Get the access token record
                    accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                        AccessToken);
                    if (null == accessToken)
                    {
                        _logger.LogError("Access token not recieved from cache." +
                            "Expired or Invalid access token");
                        ErrorResponseDTO error = new ErrorResponseDTO();
                        error.error = OIDCConstants.InvalidToken;
                        error.error_description = OIDCConstants.InvalidTokenDesc;
                        return new ServiceResult(false, "Access token not recieved from cache." +
                            "Expired or Invalid access token", error);
                    }
                }
                catch (CacheException ex)
                {
                    _logger.LogError("Failed to get Access Token Record");
                    ErrorResponseDTO error = new ErrorResponseDTO();
                    error.error = OIDCConstants.InternalError;
                    error.error_description = _helper.GetRedisErrorMsg(
                        ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                    return new ServiceResult(false, "Failed to get Access Token Record", error);
                }

                var list = await _unitOfWork.Datapivots.GetDataPivotByCatIdAsync(catId);
                if (list == null)
                {
                    _logger.LogError("No Data Pivot Elements with {0} is found", catId);
                    return new ServiceResult(false, "Data Pivot list is empty");
                }
                var cat_name = await _unitOfWork.Category.GetCatNameByCatUIDAsync(catId);
                if (cat_name == null)
                {
                    _logger.LogError("No name associated with {0} is found", catId);
                    return new ServiceResult(false, "CatID Name not found");
                }
                IEnumerable<DataPivotByCatIdDTO> result = new List<DataPivotByCatIdDTO>();

                foreach (var cat in list)
                {
                    var profile = await _unitOfWork.Scopes.GetByIdAsync(cat.ScopeId);
                    DataPivotByCatIdDTO resultList = new DataPivotByCatIdDTO
                    {
                        CategorId = cat.CategoryId,
                        DataPivotName = cat.Name,
                        AuthSchema = cat.AuthScheme,
                        Id = cat.Id,
                        DataPivotUID = cat.DataPivotUid,
                        CategoryName = cat_name,
                        DataPivotLogo = cat.DataPivotLogo,
                        ProfileType= profile.Name,
                    };

                    ((List<DataPivotByCatIdDTO>)result).Add(resultList);
                }


                return new ServiceResult(true, "Document list retrieved successfully", result);

            }
            catch
            {
                _logger.LogError("Get Data Pivot list by Cat ID failed");
                _logger.LogInformation("<---GetDataPivotByCatIdAsync");
                return new ServiceResult(false, "An error occurred while getting the document list.");
            }
        }
        public async Task<ServiceResult> GetUserSpecificList(string suid, string AccessToken,string CategoryId)
        {
            try
            {
                _logger.LogInformation("--->GetUserSpecificDocumentsListAsync");

                string[] countryCodes = new string[]{"CAN","KEN","NZL","PAK","LKA","GBR"};
                Accesstoken accessToken = null;
                try
                {
                    // Get the access token record
                    accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                        AccessToken);
                    if (null == accessToken)
                    {
                        _logger.LogError("Access token not recieved from cache." +
                            "Expired or Invalid access token");
                        ErrorResponseDTO error = new ErrorResponseDTO();
                        error.error = OIDCConstants.InvalidToken;
                        error.error_description = OIDCConstants.InvalidTokenDesc;
                        return new ServiceResult(false, "Access token not recieved from cache." +
                            "Expired or Invalid access token", error);
                    }
                }
                catch (CacheException ex)
                {
                    _logger.LogError("Failed to get Access Token Record");
                    ErrorResponseDTO error = new ErrorResponseDTO();
                    error.error = OIDCConstants.InternalError;
                    error.error_description = _helper.GetRedisErrorMsg(
                        ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                    return new ServiceResult(false, "Failed to get Access Token Record", error);
                }

                var list = await _unitOfWork.Datapivots.GetAllAsync();
                if (list == null || !list.Any())
                {
                    _logger.LogError("Document list is empty");
                    return new ServiceResult(false, "Document list is empty");
                }

                List<UserSpecificDocumentListDTO> docList = new();

                foreach (var item in list)
                {
                    if (item.AllowedSubscriberTypes != null)
                    {
                        var profile = await _unitOfWork.Scopes.GetByIdAsync(item.ScopeId);
                        var allowedTypes= item.AllowedSubscriberTypes.Split(',');
                        if (CategoryId == null)
                        {
                            var catrgoryId = item.CategoryId;
                            var document = new UserSpecificDocumentListDTO()
                            {
                                DataPivotUID = item.DataPivotUid,
                                Name = item.Name,
                                DocumentStatus = item.Status,
                                Logo = item.DataPivotLogo,
                                ProfileType = profile.Name,
                            };
                            document.ApplicationStatus = "APPROVED";
                            document.ApplicationSubmitted = true;
                            if (catrgoryId != null)
                            {
                                var categoryName = await _unitOfWork.Category.GetCatNameByCatUIDAsync(catrgoryId);
                                document.CategoryName = categoryName;
                            }
                            docList.Add(document);
                        }
                        else
                        {
                            var profile1 = await _unitOfWork.Scopes.GetByIdAsync(item.ScopeId);
                            var catrgoryId = item.CategoryId;
                            var document = new UserSpecificDocumentListDTO()
                            {
                                DataPivotUID = item.DataPivotUid,
                                Name = item.Name,
                                DocumentStatus = item.Status,
                                Logo = item.DataPivotLogo,
                                ProfileType = profile1.Name,
                            };
                            document.ApplicationStatus = "APPROVED";
                            document.ApplicationSubmitted = true;
                            if (catrgoryId != null)
                            {
                                var categoryName = await _unitOfWork.Category.GetCatNameByCatUIDAsync(catrgoryId);
                                document.CategoryName = categoryName;
                            }
                            docList.Add(document);
                        }
                        
                    }   
                }
                return new ServiceResult(true, "Document list retrieved successfully", docList);

            }
            catch
            {
                _logger.LogError("Get Document list failed");
                _logger.LogInformation("<---GetDocumentsListAsync");
                return new ServiceResult(false, "An error occurred while getting the document list.");
            }
        }
        public async Task<ServiceResult> GetDataPivotById(string Id, string AccessToken,string suid)
        {
            try
            {
                _logger.LogInformation("--->GetUserSpecificDocumentsListAsync");


                Accesstoken accessToken = null;
                try
                {
                    // Get the access token record
                    accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                        AccessToken);
                    if (null == accessToken)
                    {
                        _logger.LogError("Access token not recieved from cache." +
                            "Expired or Invalid access token");
                        ErrorResponseDTO error = new ErrorResponseDTO();
                        error.error = OIDCConstants.InvalidToken;
                        error.error_description = OIDCConstants.InvalidTokenDesc;
                        return new ServiceResult(false, "Access token not recieved from cache." +
                            "Expired or Invalid access token", error);
                    }
                }
                catch (CacheException ex)
                {
                    _logger.LogError("Failed to get Access Token Record");
                    ErrorResponseDTO error = new ErrorResponseDTO();
                    error.error = OIDCConstants.InternalError;
                    error.error_description = _helper.GetRedisErrorMsg(
                        ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                    return new ServiceResult(false, "Failed to get Access Token Record", error);
                }
                var list = await _unitOfWork.Datapivots.GetAllAsync();
                if (list == null || !list.Any())
                {
                    _logger.LogError("Document list is empty");
                    return new ServiceResult(false, "Document list is empty");
                }

                List<UserSpecificDocumentListDTO> docList = new();
                var dataPivot = await _unitOfWork.Datapivots.GetByUIDAsync(Id);
                if (dataPivot.AllowedSubscriberTypes != null)
                {
                    var allowedTypes = dataPivot.AllowedSubscriberTypes.Split(',');
                    var profile = await _unitOfWork.Scopes.GetByIdAsync(dataPivot.ScopeId);
                    var catrgoryId = dataPivot.CategoryId;
                    var document = new UserSpecificDocumentListDTO()
                    {
                        DataPivotUID = dataPivot.DataPivotUid,
                        Name = dataPivot.Name,
                        DocumentStatus = dataPivot.Status,
                        ProfileType = profile.Name,
                    };

                    document.ApplicationStatus = "APPROVED";
                    document.ApplicationSubmitted = true;

                    if (catrgoryId != null)
                    {
                        var categoryName = await _unitOfWork.Category.GetCatNameByCatUIDAsync(catrgoryId);
                        document.CategoryName = categoryName;
                    }
                    return new ServiceResult(true, "Document list retrieved successfully", document);
                }
                return new ServiceResult(false, "Subscriber type not Matched" );

            }
            catch
            {
                _logger.LogError("Get Document list failed");
                _logger.LogInformation("<---GetDocumentsListAsync");
                return new ServiceResult(false, "An error occurred while getting the document list.");
            }
        }
    }
}
