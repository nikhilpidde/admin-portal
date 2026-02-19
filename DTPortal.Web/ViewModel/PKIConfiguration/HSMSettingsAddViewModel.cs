using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Web.ViewModel.PKIConfiguration
{
    public class HSMSettingsAddViewModel
    {
        [Display(Name = "CMAPI Url")]
        public string CMAPIUrl { get; set; }

        [Required]
        [Display(Name = "Client Path")]
        public string ClientPath { get; set; }

        [Display(Name = "Client Environment Path")]
        public string ClientEnvironmentPath { get; set; }

        [Required]
        [Display(Name = "Admin User Id")]
        public string AdminUserId { get; set; }

        [Required]
        [Display(Name = "Admin Password")]
        public string AdminPassword { get; set; }

        [Required]
        [Display(Name = "Key Generation Timeout (in seconds)")]
        public int? KeyGenerationTimeout { get; set; }

        [Required]
        [Display(Name = "Slot Id")]
        public int? SlotId { get; set; }

        [Required]
        [Display(Name = "Key Algorithm")]
        public int? KeyAlgorithmId { get; set; }

        [Required]
        [Display(Name = "Key Size")]
        public int? KeySizeId { get; set; }

        [Required]
        [Display(Name = "Hash Algorithm")]
        public int? HashAlgorithmId { get; set; }

        [Required]
        [Display(Name = "HSM Plugin")]
        public int? HSMPluginId { get; set; }

        public IEnumerable<PkiKeyAlgorithm> KeyAlgorithms { get; set; }

        public IEnumerable<PkiHashAlgorithm> HashAlgorithms { get; set; }

        public IEnumerable<PkiHsmPlugin> HSMPlugins { get; set; }
    }
}
