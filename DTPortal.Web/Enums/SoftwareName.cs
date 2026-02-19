using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DTPortal.Web.Enums
{
    public enum SoftwareName
    {
        //[Display(Name = "Enterprise Gateway Windows_x64")]
        //EnterpriseGateway_windows_x64 = 1,

        //[Display(Name = "Enterprise Gateway Lite Edition Windows_x64")]
        //EnterpriseGatewayLiteEdition_windows_x64,

        //[Display(Name = "Local Signing Agent Windows_x64")]
        //LocalSigningAgent_windows_x64

        //[Display(Name = "Agent Framework")]
        //AGENT_FRAMEWORK

        [Display(Name = "Enterprise Gateway")]
        ENTERPRISE_GATEWAY
    }
}
