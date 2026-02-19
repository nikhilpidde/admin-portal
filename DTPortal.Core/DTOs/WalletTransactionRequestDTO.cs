using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class WalletTransactionRequestDTO
    {
        public string status {  get; set; }
        public string statusMessage { get; set; }
        public string clientID { get; set; }
        public string suid { get; set; }
        public string presentationToken { get; set; }
        public List<CredentialDetail> profiles { get; set; } 
    }

    public class CallStackObject
    {
        public string presentationToken { get; set; }
        public List<CredentialDetail> profiles { get; set; }
    }
}
