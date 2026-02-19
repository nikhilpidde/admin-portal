using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp.Core;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class AuthenticationTransactionRequest
    {
        public string suid { get; set; }
        public int pageNumber { get; set; }
    }
    public class AuthenticationTransactionResponse
    {
        public List<AuthenticationTransaction> authenticationTransactions { get; set; }

        public bool hasMoreResults { get; set; }

        public int totalResults { get; set; }
    }
    public class ApplicationInfo
    {
        public string ApplicationName { get; set; }
        public string OrganizationUid { get; set; }
    }

    public class AuthenticationTransaction
    {
        public string Id { get; set; }
        public string serviceProviderName { get; set; }
        public string serviceProviderAppName { get; set; }
        public string dateTime { get; set; }
        public string serviceName { get; set; }
        public string authenticationStatus { get; set; }
    }
}
