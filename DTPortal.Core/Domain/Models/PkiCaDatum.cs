using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiCaDatum
{
    public int Id { get; set; }

    public string Url { get; set; }

    public string CertificateAuthority { get; set; }

    public string EndEntityProfileName { get; set; }

    public string CertificateProfileName { get; set; }

    public string ClientAuthCertificate { get; set; }

    public string ClientAuthCertificatePassword { get; set; }

    public string IssuerDn { get; set; }

    public string StagingCertProcedureRsa256 { get; set; }

    public string StagingCertProcedureRsa512 { get; set; }

    public string StagingCertProcedureEc256 { get; set; }

    public string StagingCertProcedureEc512 { get; set; }

    public string TestCertProcedureRsa256 { get; set; }

    public string TestCertProcedureRsa512 { get; set; }

    public string TestCertProcedureEc256 { get; set; }

    public string TestCertProcedureEc512 { get; set; }

    public int CertificateValidity { get; set; }

    public string SigningCertificateIssuer { get; set; }

    public string SigningCertificateRoot { get; set; }

    public string OcspSignerCertificate { get; set; }

    public string SigningCertificateChain { get; set; }

    public string TimestampingCertificate { get; set; }

    public string TimestampingCertificateChain { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public int CaPluginId { get; set; }

    public int ProcedureId { get; set; }

    public virtual PkiCaPlugin CaPlugin { get; set; }

    public virtual ICollection<PkiPluginDatum> PkiPluginData { get; set; } = new List<PkiPluginDatum>();

    public virtual PkiProcedure Procedure { get; set; }
}
