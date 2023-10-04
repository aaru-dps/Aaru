// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Entity framework database context.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Aaru.Database;

/// <inheritdoc />
/// <summary>Database context</summary>
public sealed class AaruContext : DbContext
{
    /// <inheritdoc />
    /// <summary>Creates a database context with the specified options</summary>
    /// <param name="options">Options</param>
    public AaruContext(DbContextOptions options) : base(options) {}

    /// <summary>List of known devices</summary>
    public DbSet<Device> Devices { get; set; }

    /// <summary>List of local device reports</summary>
    public DbSet<Report> Reports { get; set; }

    /// <summary>Command usage statistics</summary>
    public DbSet<Command> Commands { get; set; }

    /// <summary>Statistics for found filesystems</summary>
    public DbSet<Filesystem> Filesystems { get; set; }

    /// <summary>Statistics for used filters</summary>
    public DbSet<Filter> Filters { get; set; }

    /// <summary>Statistics for media image formats</summary>
    public DbSet<MediaFormat> MediaFormats { get; set; }

    /// <summary>Statistics for partitioning schemes</summary>
    public DbSet<Partition> Partitions { get; set; }

    /// <summary>Statistics for media types</summary>
    public DbSet<Media> Medias { get; set; }

    /// <summary>Statistics for devices seen using commands</summary>
    public DbSet<DeviceStat> SeenDevices { get; set; }

    /// <summary>Statistics for operating systems</summary>
    public DbSet<OperatingSystem> OperatingSystems { get; set; }

    /// <summary>Statistics for used Aaru versions</summary>
    public DbSet<Version> Versions { get; set; }

    /// <summary>List of known USB vendors</summary>
    public DbSet<UsbVendor> UsbVendors { get; set; }

    /// <summary>List of known USB products</summary>
    public DbSet<UsbProduct> UsbProducts { get; set; }

    /// <summary>List of CD reading offsets</summary>
    public DbSet<CdOffset> CdOffsets { get; set; }

    /// <summary>Statistics of remote applications</summary>
    public DbSet<RemoteApplication> RemoteApplications { get; set; }

    /// <summary>Statistics of remote architectures</summary>
    public DbSet<RemoteArchitecture> RemoteArchitectures { get; set; }

    /// <summary>Statistics of remote operating systems</summary>
    public DbSet<RemoteOperatingSystem> RemoteOperatingSystems { get; set; }

    /// <summary>Known iNES/NES 2.0 headers</summary>
    public DbSet<NesHeaderInfo> NesHeaders { get; set; }

    // Note: If table does not appear check that last migration has been REALLY added to the project

    /// <summary>Creates a database context with the database in the specified path</summary>
    /// <param name="dbPath">Path to database file</param>
    /// <param name="pooling">Enable database pooling</param>
    /// <returns>Database context</returns>
    public static AaruContext Create(string dbPath, bool pooling = true)
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        optionsBuilder.UseLazyLoadingProxies().
                       UseSqlite(!pooling ? $"Data Source={dbPath};Pooling=False" : $"Data Source={dbPath}");

        return new AaruContext(optionsBuilder.Options);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.Ata",
                            b => b.HasOne("Aaru.CommonTypes.Metadata.TestedMedia", "ReadCapabilities").
                                   WithMany().
                                   HasForeignKey("ReadCapabilitiesId").
                                   OnDelete(DeleteBehavior.SetNull));

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.BlockDescriptor",
                            b => b.HasOne("Aaru.CommonTypes.Metadata.ScsiMode", null).
                                   WithMany("BlockDescriptors").
                                   HasForeignKey("ScsiModeId").
                                   OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.DensityCode",
                            b => b.HasOne("Aaru.CommonTypes.Metadata.SscSupportedMedia", null).
                                   WithMany("DensityCodes").
                                   HasForeignKey("SscSupportedMediaId").
                                   OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.Mmc",
                            b => b.HasOne("Aaru.CommonTypes.Metadata.MmcFeatures", "Features").
                                   WithMany().
                                   HasForeignKey("FeaturesId").
                                   OnDelete(DeleteBehavior.SetNull));

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.Scsi", b =>
        {
            b.HasOne("Aaru.CommonTypes.Metadata.ScsiMode", "ModeSense").
              WithMany().
              HasForeignKey("ModeSenseId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Mmc", "MultiMediaDevice").
              WithMany().
              HasForeignKey("MultiMediaDeviceId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.TestedMedia", "ReadCapabilities").
              WithMany().
              HasForeignKey("ReadCapabilitiesId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Ssc", "SequentialDevice").
              WithMany().
              HasForeignKey("SequentialDeviceId").
              OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.ScsiPage", b =>
        {
            b.HasOne("Aaru.CommonTypes.Metadata.Scsi", null).
              WithMany("EVPDPages").
              HasForeignKey("ScsiId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.ScsiMode", null).
              WithMany("ModePages").
              HasForeignKey("ScsiModeId").
              OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.SscSupportedMedia", b =>
        {
            b.HasOne("Aaru.CommonTypes.Metadata.Ssc", null).
              WithMany("SupportedMediaTypes").
              HasForeignKey("SscId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.TestedSequentialMedia", null).
              WithMany("SupportedMediaTypes").
              HasForeignKey("TestedSequentialMediaId").
              OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.SupportedDensity", b =>
        {
            b.HasOne("Aaru.CommonTypes.Metadata.Ssc", null).
              WithMany("SupportedDensities").
              HasForeignKey("SscId").
              OnDelete(DeleteBehavior.Cascade);

            b.HasOne("Aaru.CommonTypes.Metadata.TestedSequentialMedia", null).
              WithMany("SupportedDensities").
              HasForeignKey("TestedSequentialMediaId").
              OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.TestedMedia", b =>
        {
            b.HasOne("Aaru.CommonTypes.Metadata.Ata", null).
              WithMany("RemovableMedias").
              HasForeignKey("AtaId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Chs", "CHS").
              WithMany().
              HasForeignKey("CHSId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Chs", "CurrentCHS").
              WithMany().
              HasForeignKey("CurrentCHSId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Mmc", null).
              WithMany("TestedMedia").
              HasForeignKey("MmcId").
              OnDelete(DeleteBehavior.Cascade);

            b.HasOne("Aaru.CommonTypes.Metadata.Scsi", null).
              WithMany("RemovableMedias").
              HasForeignKey("ScsiId").
              OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity("Aaru.CommonTypes.Metadata.TestedSequentialMedia",
                            b => b.HasOne("Aaru.CommonTypes.Metadata.Ssc", null).
                                   WithMany("TestedMedia").
                                   HasForeignKey("SscId").
                                   OnDelete(DeleteBehavior.SetNull));

        modelBuilder.Entity("Aaru.Database.Models.Device", b =>
        {
            b.HasOne("Aaru.CommonTypes.Metadata.Ata", "ATA").
              WithMany().
              HasForeignKey("ATAId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Ata", "ATAPI").
              WithMany().
              HasForeignKey("ATAPIId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.FireWire", "FireWire").
              WithMany().
              HasForeignKey("FireWireId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.MmcSd", "MultiMediaCard").
              WithMany().
              HasForeignKey("MultiMediaCardId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Pcmcia", "PCMCIA").
              WithMany().
              HasForeignKey("PCMCIAId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Scsi", "SCSI").
              WithMany().
              HasForeignKey("SCSIId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.MmcSd", "SecureDigital").
              WithMany().
              HasForeignKey("SecureDigitalId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Usb", "USB").
              WithMany().
              HasForeignKey("USBId").
              OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity("Aaru.Database.Models.Report", b =>
        {
            b.HasOne("Aaru.CommonTypes.Metadata.Ata", "ATA").
              WithMany().
              HasForeignKey("ATAId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Ata", "ATAPI").
              WithMany().
              HasForeignKey("ATAPIId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.FireWire", "FireWire").
              WithMany().
              HasForeignKey("FireWireId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.MmcSd", "MultiMediaCard").
              WithMany().
              HasForeignKey("MultiMediaCardId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Pcmcia", "PCMCIA").
              WithMany().
              HasForeignKey("PCMCIAId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Scsi", "SCSI").
              WithMany().
              HasForeignKey("SCSIId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.MmcSd", "SecureDigital").
              WithMany().
              HasForeignKey("SecureDigitalId").
              OnDelete(DeleteBehavior.SetNull);

            b.HasOne("Aaru.CommonTypes.Metadata.Usb", "USB").
              WithMany().
              HasForeignKey("USBId").
              OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CdOffset>().HasIndex(b => b.ModifiedWhen);

        modelBuilder.Entity<UsbProduct>().HasIndex(b => b.ModifiedWhen);
        modelBuilder.Entity<UsbProduct>().HasIndex(b => b.ProductId);
        modelBuilder.Entity<UsbProduct>().HasIndex(b => b.VendorId);

        modelBuilder.Entity<UsbVendor>().HasIndex(b => b.ModifiedWhen);

        modelBuilder.Entity<NesHeaderInfo>().HasIndex(b => b.Sha256);
        modelBuilder.Entity<NesHeaderInfo>().HasIndex(b => b.ModifiedWhen);
    }
}