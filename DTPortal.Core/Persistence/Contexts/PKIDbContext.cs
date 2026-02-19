using Microsoft.EntityFrameworkCore;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Persistence.Contexts
{
    public partial class PKIDbContext : DbContext
    {
        public PKIDbContext()
        {
        }

        public PKIDbContext(DbContextOptions<PKIDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PkiCaDatum> PkiCaData { get; set; }

        public virtual DbSet<PkiCaPlugin> PkiCaPlugins { get; set; }

        public virtual DbSet<PkiConfiguration> PkiConfigurations { get; set; }

        public virtual DbSet<PkiHashAlgorithm> PkiHashAlgorithms { get; set; }

        public virtual DbSet<PkiHsmDatum> PkiHsmData { get; set; }

        public virtual DbSet<PkiHsmPlugin> PkiHsmPlugins { get; set; }

        public virtual DbSet<PkiKeyAlgorithm> PkiKeyAlgorithms { get; set; }

        public virtual DbSet<PkiKeyDatum> PkiKeyData { get; set; }

        public virtual DbSet<PkiKeySize> PkiKeySizes { get; set; }

        public virtual DbSet<PkiPluginDatum> PkiPluginData { get; set; }

        public virtual DbSet<PkiProcedure> PkiProcedures { get; set; }

        public virtual DbSet<PkiServerConfigurationDatum> PkiServerConfigurationData { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PkiCaDatum>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_ca_data_pkey");

                entity.ToTable("pki_ca_data");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CaPluginId).HasColumnName("ca_plugin_id");
                entity.Property(e => e.CertificateAuthority)
                    .HasMaxLength(50)
                    .HasColumnName("certificate_authority");
                entity.Property(e => e.CertificateProfileName)
                    .HasMaxLength(50)
                    .HasColumnName("certificate_profile_name");
                entity.Property(e => e.CertificateValidity).HasColumnName("certificate_validity");
                entity.Property(e => e.ClientAuthCertificate)
                    .HasMaxLength(50)
                    .HasColumnName("client_auth_certificate");
                entity.Property(e => e.ClientAuthCertificatePassword)
                    .HasMaxLength(50)
                    .HasColumnName("client_auth_certificate_password");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.EndEntityProfileName)
                    .HasMaxLength(50)
                    .HasColumnName("end_entity_profile_name");
                entity.Property(e => e.IssuerDn)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("issuer_dn");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.OcspSignerCertificate)
                    .IsRequired()
                    .HasColumnName("ocsp_signer_certificate");
                entity.Property(e => e.ProcedureId).HasColumnName("procedure_id");
                entity.Property(e => e.SigningCertificateChain)
                    .IsRequired()
                    .HasColumnName("signing_certificate_chain");
                entity.Property(e => e.SigningCertificateIssuer)
                    .IsRequired()
                    .HasColumnName("signing_certificate_issuer");
                entity.Property(e => e.SigningCertificateRoot)
                    .IsRequired()
                    .HasColumnName("signing_certificate_root");
                entity.Property(e => e.StagingCertProcedureEc256)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("staging_cert_procedure_ec256");
                entity.Property(e => e.StagingCertProcedureEc512)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("staging_cert_procedure_ec512");
                entity.Property(e => e.StagingCertProcedureRsa256)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("staging_cert_procedure_rsa256");
                entity.Property(e => e.StagingCertProcedureRsa512)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("staging_cert_procedure_rsa512");
                entity.Property(e => e.TestCertProcedureEc256)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("test_cert_procedure_ec256");
                entity.Property(e => e.TestCertProcedureEc512)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("test_cert_procedure_ec512");
                entity.Property(e => e.TestCertProcedureRsa256)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("test_cert_procedure_rsa256");
                entity.Property(e => e.TestCertProcedureRsa512)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("test_cert_procedure_rsa512");
                entity.Property(e => e.TimestampingCertificate)
                    .IsRequired()
                    .HasColumnName("timestamping_certificate");
                entity.Property(e => e.TimestampingCertificateChain)
                    .IsRequired()
                    .HasColumnName("timestamping_certificate_chain");
                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("url");

                entity.HasOne(d => d.CaPlugin).WithMany(p => p.PkiCaData)
                    .HasForeignKey(d => d.CaPluginId)
                    .HasConstraintName("fk_ca_data_ca_plugin_id");

                entity.HasOne(d => d.Procedure).WithMany(p => p.PkiCaData)
                    .HasForeignKey(d => d.ProcedureId)
                    .HasConstraintName("fk_ca_data_procedure_id");
            });

            modelBuilder.Entity<PkiCaPlugin>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_ca_plugins_pkey");

                entity.ToTable("pki_ca_plugins");

                entity.HasIndex(e => e.Guid, "pki_ca_plugins_guid_key").IsUnique();

                entity.HasIndex(e => e.Name, "pki_ca_plugins_name_key").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CaPluginLibraryPath)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ca_plugin_library_path");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("guid");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<PkiConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_configuration_pkey");

                entity.ToTable("pki_configuration");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("created_by");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("updated_by");
                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<PkiHashAlgorithm>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_hash_algorithms_pkey");

                entity.ToTable("pki_hash_algorithms");

                entity.HasIndex(e => e.Name, "pki_hash_algorithms_name_key").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<PkiHsmDatum>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_hsm_data_pkey");

                entity.ToTable("pki_hsm_data");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ClientEnvPath)
                    .HasMaxLength(255)
                    .HasColumnName("client_env_path");
                entity.Property(e => e.ClientPath)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("client_path");
                entity.Property(e => e.CmAdminPwd)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("cm_admin_pwd");
                entity.Property(e => e.CmAdminUid)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("cm_admin_uid");
                entity.Property(e => e.CmapiUrl)
                    .HasMaxLength(255)
                    .HasColumnName("cmapi_url");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.HashAlgorithmId).HasColumnName("hash_algorithm_id");
                entity.Property(e => e.HsmPluginId).HasColumnName("hsm_plugin_id");
                entity.Property(e => e.KeyDataId).HasColumnName("key_data_id");
                entity.Property(e => e.KeyGenerationTimeout).HasColumnName("key_generation_timeout");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.SlotId).HasColumnName("slot_id");

                entity.HasOne(d => d.HashAlgorithm).WithMany(p => p.PkiHsmData)
                    .HasForeignKey(d => d.HashAlgorithmId)
                    .HasConstraintName("fk_hsm_data_hash_algorithm_id");

                entity.HasOne(d => d.HsmPlugin).WithMany(p => p.PkiHsmData)
                    .HasForeignKey(d => d.HsmPluginId)
                    .HasConstraintName("fk_hsm_data_hsm_plugin_id");

                entity.HasOne(d => d.KeyData).WithMany(p => p.PkiHsmData)
                    .HasForeignKey(d => d.KeyDataId)
                    .HasConstraintName("fk_hsm_data_key_data_id");
            });

            modelBuilder.Entity<PkiHsmPlugin>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_hsm_plugins_pkey");

                entity.ToTable("pki_hsm_plugins");

                entity.HasIndex(e => e.Guid, "pki_hsm_plugins_guid_key").IsUnique();

                entity.HasIndex(e => e.Name, "pki_hsm_plugins_name_key").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("guid");
                entity.Property(e => e.HsmPluginLibraryPath)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("hsm_plugin_library_path");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<PkiKeyAlgorithm>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_key_algorithms_pkey");

                entity.ToTable("pki_key_algorithms");

                entity.HasIndex(e => e.Name, "pki_key_algorithms_name_key").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<PkiKeyDatum>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_key_data_pkey");

                entity.ToTable("pki_key_data");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.KeyAlgorithmId).HasColumnName("key_algorithm_id");
                entity.Property(e => e.KeySizeId).HasColumnName("key_size_id");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");

                entity.HasOne(d => d.KeyAlgorithm).WithMany(p => p.PkiKeyData)
                    .HasForeignKey(d => d.KeyAlgorithmId)
                    .HasConstraintName("fk_key_data_key_algorithm");

                entity.HasOne(d => d.KeySize).WithMany(p => p.PkiKeyData)
                    .HasForeignKey(d => d.KeySizeId)
                    .HasConstraintName("fk_key_data_key_size");
            });

            modelBuilder.Entity<PkiKeySize>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_key_size_pkey");

                entity.ToTable("pki_key_size");

                entity.HasIndex(e => e.Size, "pki_key_size_size_key").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.KeyAlgorithmId).HasColumnName("key_algorithm_id");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.Size)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("size");

                entity.HasOne(d => d.KeyAlgorithm).WithMany(p => p.PkiKeySizes)
                    .HasForeignKey(d => d.KeyAlgorithmId)
                    .HasConstraintName("fk_key_size_key_algorithm");
            });

            modelBuilder.Entity<PkiPluginDatum>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_plugin_data_pkey");

                entity.ToTable("pki_plugin_data");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ApprovedBy)
                    .HasMaxLength(50)
                    .HasColumnName("approved_by");
                entity.Property(e => e.BlockedReason).HasColumnName("blocked_reason");
                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("created_by");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.PkiCaDataId).HasColumnName("pki_ca_data_id");
                entity.Property(e => e.PkiHsmDataId).HasColumnName("pki_hsm_data_id");
                entity.Property(e => e.PkiServerConfigurationDataId).HasColumnName("pki_server_configuration_data_id");
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("status");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("updated_by");

                entity.HasOne(d => d.PkiCaData).WithMany(p => p.PkiPluginData)
                    .HasForeignKey(d => d.PkiCaDataId)
                    .HasConstraintName("fk_plugin_data_ca");

                entity.HasOne(d => d.PkiHsmData).WithMany(p => p.PkiPluginData)
                    .HasForeignKey(d => d.PkiHsmDataId)
                    .HasConstraintName("fk_plugin_data_hsm");

                entity.HasOne(d => d.PkiServerConfigurationData).WithMany(p => p.PkiPluginData)
                    .HasForeignKey(d => d.PkiServerConfigurationDataId)
                    .HasConstraintName("fk_plugin_data_server_conf");
            });

            modelBuilder.Entity<PkiProcedure>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_procedure_pkey");

                entity.ToTable("pki_procedure");

                entity.HasIndex(e => e.Name, "pki_procedure_name_key").IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<PkiServerConfigurationDatum>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pki_server_configuration_data_pkey");

                entity.ToTable("pki_server_configuration_data");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CentralLogQueue)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("central_log_queue");
                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("client_id");
                entity.Property(e => e.ClientSecret)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("client_secret");
                entity.Property(e => e.ConfigDirectoryPath)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("config_directory_path");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.DssClient).HasColumnName("dss_client");
                entity.Property(e => e.EnableDss).HasColumnName("enable_dss");
                entity.Property(e => e.HandSignature).HasColumnName("hand_signature");
                entity.Property(e => e.IdpUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("idp_url");
                entity.Property(e => e.Introspect).HasColumnName("introspect");
                entity.Property(e => e.Jre64Directory)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("jre64_directory");
                entity.Property(e => e.LogCallstack).HasColumnName("log_callstack");
                entity.Property(e => e.LogLevel)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("log_level");
                entity.Property(e => e.LogPath)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("log_path");
                entity.Property(e => e.LogQueueIp)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("log_queue_ip");
                entity.Property(e => e.LogQueuePassword)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("log_queue_password");
                entity.Property(e => e.LogQueuePort).HasColumnName("log_queue_port");
                entity.Property(e => e.LogQueueUsername)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("log_queue_username");
                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");
                entity.Property(e => e.OcspUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("ocsp_url");
                entity.Property(e => e.PkiServiceUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("pki_service_url");
                entity.Property(e => e.RaLogQueue)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("ra_log_queue");
                entity.Property(e => e.SignLocally).HasColumnName("sign_locally");
                entity.Property(e => e.SignatureImage)
                    .IsRequired()
                    .HasColumnName("signature_image");
                entity.Property(e => e.SignatureServiceUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("signature_service_url");
                entity.Property(e => e.SigningLogQueue)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("signing_log_queue");
                entity.Property(e => e.StagingEnv).HasColumnName("staging_env");
                entity.Property(e => e.TsaUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("tsa_url");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
