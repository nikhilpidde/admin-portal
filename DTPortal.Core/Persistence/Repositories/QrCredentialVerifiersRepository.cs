using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Persistence.Repositories
{
    public class QrCredentialVerifiersRepository : GenericRepository<QrCredentialVerifier, idp_dtplatformContext>, IQrCredentialVerifiersRepository

    {
        private readonly ILogger _logger;
        public QrCredentialVerifiersRepository(idp_dtplatformContext context,
            ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsCredentialAlreadyExists(QrCredentialVerifierDTO qrCredentialVerifierDTO)
        {
            try
            {
                return await Context.QrCredentialVerifiers.AsNoTracking().AnyAsync(u => u.OrganizationId == qrCredentialVerifierDTO.organizationId && u.CredentialId == qrCredentialVerifierDTO.credentialId);
            }
            catch (Exception error)
            {
                _logger.LogError("IsCredentialAlreadyExists::Database exception: {0}", error);
                return false;
            }
        }

        public async Task<List<string>> GetCredentialsListByOrganizationIdAsync(string organizationId)
        {
            try
            {
                return await Context.QrCredentialVerifiers
                        .OrderByDescending(u => u.Id)
                        .Where(u => u.OrganizationId == organizationId)
                        .Select(u => u.CredentialId)
                        .ToListAsync();

            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialsListByOrganizationIdAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<QrCredentialVerifier>> GetCredentialListDataByOrganizationIdAsync(string organizationId)
        {
            try
            {
                return await Context.QrCredentialVerifiers
                        .OrderByDescending(u => u.Id)
                        .Where(u => u.OrganizationId == organizationId)
                        .ToListAsync();

            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialsListByOrganizationIdAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<QrCredentialVerifier>> GetActiveCredentialVerifierListAsync()
        {
            try
            {
                return await Context.QrCredentialVerifiers
                        .Include(u => u.Credential)
                        .OrderByDescending(u => u.Id)
                        .Where(u => u.Status == "SUBSCRIBED")
                        .ToListAsync();

            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialsListByOrganizationIdAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<QrCredentialVerifier>> GetActiveCredentialListByOrganizationIdAsync(string orgId)
        {
            try
            {
                return await Context.QrCredentialVerifiers
                        .OrderByDescending(u => u.Id)
                        .Where(u => u.Status == "SUBSCRIBED" && u.OrganizationId==orgId)
                        .ToListAsync();

            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialsListByOrganizationIdAsync::Database exception: {0}", error);
                return null;
            }
        }
        public async Task<IEnumerable<QrCredentialVerifier>> GetCredentialVerifierListByIssuerIdAsync(string organizationId)
        {
            try
            {
                return await Context.QrCredentialVerifiers
                    .Include(cv => cv.Credential)
                    .Where(cv => cv.Credential.OrganizationId == organizationId)
                    .ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialsListByOrganizationIdAsync::Database exception: {0}", error);
                return null;
            }
        }
    }
}
