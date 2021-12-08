using System;
using System.ComponentModel.DataAnnotations;
using Aaru.CommonTypes.Enums;

namespace Aaru.Database.Models;

/// <summary>Information needed to rebuild an iNES/NES 2.0 header from a ROM hash</summary>
public class NesHeaderInfo : BaseModel
{
    /// <summary>ROM hash</summary>
    [StringLength(64), Required]
    public string Sha256 { get; set; }
    /// <summary>If <c>true</c> vertical mirroring is hard-wired, horizontal or mapper defined otherwise</summary>
    public bool NametableMirroring { get; set; }
    /// <summary>If <c>true</c> a battery is present</summary>
    public bool BatteryPresent { get; set; }
    /// <summary>If <c>true</c> the four player screen mode is hardwired</summary>
    public bool FourScreenMode { get; set; }
    /// <summary>Mapper number (NES 2.0 when in conflict)</summary>
    public ushort Mapper { get; set; }
    /// <summary>Console type</summary>
    public NesConsoleType ConsoleType { get; set; }
    /// <summary>Submapper number</summary>
    public byte Submapper { get; set; }
    /// <summary>Timing mode</summary>
    public NesTimingMode TimingMode { get; set; }
    /// <summary>Vs. PPU type</summary>
    public NesVsPpuType VsPpuType { get; set; }
    /// <summary>Vs. hardware type</summary>
    public NesVsHardwareType VsHardwareType { get; set; }
    /// <summary>Extended console type</summary>
    public NesExtendedConsoleType ExtendedConsoleType { get; set; }
    /// <summary>Default expansion device</summary>
    public NesDefaultExpansionDevice DefaultExpansionDevice { get; set; }
    /// <summary>Date when model has been added to the database</summary>
    public DateTime AddedWhen { get; set; }
    /// <summary>Date when model was last modified</summary>
    public DateTime ModifiedWhen { get; set; }
}