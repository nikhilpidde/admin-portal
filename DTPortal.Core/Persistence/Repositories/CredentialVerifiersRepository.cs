using DTPortal.Core.Constants;
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
    public class CredentialVerifiersRepository : GenericRepository<CredentialVerifier, idp_dtplatformContext>, ICredentialVerifiersRepository

    {
        private readonly ILogger _logger;
        public CredentialVerifiersRepository(idp_dtplatformContext context,
            ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsCredentialAlreadyExists(CredentialVerifierDTO credentialVerifierDTO)
        {
            try
            {
                return await Context.CredentialVerifiers.AsNoTracking().AnyAsync(u => u.OrganizationId == credentialVerifierDTO.organizationId && u.CredentialId == credentialVerifierDTO.credentialId);
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
                return await Context.CredentialVerifiers
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

        public async Task<IEnumerable<CredentialVerifier>> GetCredentialListDataByOrganizationIdAsync(string organizationId)
        {
            try
            {
                return await Context.CredentialVerifiers
                        .Where(u => u.OrganizationId == organizationId)
                        .ToListAsync();

            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialsListByOrganizationIdAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<CredentialVerifier>> GetActiveCredentialVerifierListAsync()
        {
            try
            {
                return await Context.CredentialVerifiers
                        .Where(u => u.Status == "SUBSCRIBED")
                        .Include(u => u.Credential)
                        .ToListAsync();

            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialsListByOrganizationIdAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<CredentialVerifier>> GetActiveCredentialListByOrganizationIdAsync(string orgId)
        {
            try
            {
                return await Context.CredentialVerifiers
                        .Where(u => u.Status == "SUBSCRIBED" && u.OrganizationId==orgId)
                        .ToListAsync();

            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialsListByOrganizationIdAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<CredentialVerifier>> GetCredentialVerifierListByIssuerIdAsync(string organizationId)
        {
            try
            {
                return await Context.CredentialVerifiers
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
