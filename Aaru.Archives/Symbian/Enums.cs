// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Symbian.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Symbian plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Symbian installer (.sis) packages and shows information.
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

using System;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.Archives;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public partial class Symbian
{
#region Nested type: Attribute

    enum Attribute : uint
    {
        /// <summary>
        ///     <see cref="ManufacturerCode" />
        /// </summary>
        Manufacturer = 0x00000000,
        /// <summary>
        ///     0x0100 = 1.00
        /// </summary>
        ManufacturerHardwareRev = 0x00000001,
        /// <summary>
        ///     0x0100 = 1.00
        /// </summary>
        ManufacturerSoftwareRev = 0x00000002,
        /// <summary>
        ///     Manufacturer specific
        /// </summary>
        ManufacturerSoftwareBuild = 0x00000003,
        Model = 0x00000004,
        /// <summary>
        ///     Device specific values for products as defined in epoc32\include\hal_data.h
        /// </summary>
        MachineUid = 0x00000005,
        /// <summary>
        ///     <see cref="DeviceFamilyCode" />
        /// </summary>
        DeviceFamily = 0x00000006,
        /// <summary>
        ///     0x0100 = 1.00
        /// </summary>
        DeviceFamilyRev = 0x00000007,
        /// <summary>
        ///     <see cref="CpuCode" />
        /// </summary>
        CPU = 0x00000008,
        /// <summary>
        ///     <see cref="CpuArchitecture" />
        /// </summary>
        CPUArch = 0x00000009,
        /// <summary>
        ///     <see cref="CPUABI" />
        /// </summary>
        CPUABI = 0x0000000a,
        /// <summary>
        ///     CPU clock speed / 1024, e.g. 36864=36MHz
        /// </summary>
        CPUSpeed = 0x0000000b,
        /// <summary>
        ///     Tick period in microseconds
        /// </summary>
        SystemTickPeriod = 0x0000000e,
        /// <summary>
        ///     Approximate speed relative to Psion Series 5 = 100
        /// </summary>
        SystemSpeed = 0x0000000e,
        /// <summary>
        ///     Total RAM size in bytes
        /// </summary>
        MemoryRAM = 0x0000000f,
        /// <summary>
        ///     Free RAM size in bytes
        /// </summary>
        MemoryRAMFree = 0x00000010,
        /// <summary>
        ///     Total ROM size
        /// </summary>
        MemoryROM = 0x00000011,
        /// <summary>
        ///     Size of memory management unit pages
        /// </summary>
        MemoryPageSize = 0x00000012,
        /// <summary>
        ///     0=none, 1=supported
        /// </summary>
        PowerBackup = 0x00000015,
        /// <summary>
        ///     0=none, 1=keypad, 2=full, 3=both
        /// </summary>
        Keyboard = 0x00000018,
        /// <summary>
        ///     Number of device specific keys
        /// </summary>
        KeyboardDeviceKeys = 0x00000019,
        /// <summary>
        ///     Number of application keys
        /// </summary>
        KeyboardAppKeys = 0x0000001a,
        /// <summary>
        ///     0=none, 1=supported
        /// </summary>
        KeyboardClick = 0x0000001b,
        KeyboardClickVolumeMax = 0x0000001e,
        /// <summary>
        ///     Screen width in pixels
        /// </summary>
        DisplayXPixels = 0x0000001f,
        /// <summary>
        ///     Screen height in pixels
        /// </summary>
        DisplayYPixels = 0x00000020,
        /// <summary>
        ///     Screen width in twips (1/1440 inch)
        /// </summary>
        DisplayXTwips = 0x00000021,
        /// <summary>
        ///     Screen height in twips (1/1440 inch)
        /// </summary>
        DisplayYTwips = 0x00000022,
        /// <summary>
        ///     2, 4, 16, 256, 65536, etc
        /// </summary>
        DisplayColors = 0x00000023,
        DisplayContrastMax = 0x00000026,
        /// <summary>
        ///     0=none, 1=supported
        /// </summary>
        Backlight = 0x00000027,
        /// <summary>
        ///     0=none, 1=supported
        /// </summary>
        Pen = 0x00000029,
        /// <summary>
        ///     Pen horizontal resolution
        /// </summary>
        PenX = 0x0000002a,
        /// <summary>
        ///     Pen vertical resolution
        /// </summary>
        PenY = 0x0000002b,
        /// <summary>
        ///     0=no 1=yes
        /// </summary>
        PenDisplayOn = 0x0000002c,
        /// <summary>
        ///     0=none, 1=supported
        /// </summary>
        PenClick = 0x0000002d,
        PenClickVolumeMax = 0x00000030,
        /// <summary>
        ///     0=none, 1=supported
        /// </summary>
        Mouse = 0x00000031,
        /// <summary>
        ///     Mouse horizontal resolution
        /// </summary>
        MouseX = 0x00000032,
        /// <summary>
        ///     Mouse vertical resolution
        /// </summary>
        MouseY = 0x00000033,
        /// <summary>
        ///     Number of mouse buttons
        /// </summary>
        MouseButtons = 0x00000037,
        /// <summary>
        ///     0=none, 1=supported
        /// </summary>
        CaseSwitch = 0x0000003a,
        /// <summary>
        ///     Number of LEDs
        /// </summary>
        LEDs = 0x0000003d,
        /// <summary>
        ///     0=none, 1=supported
        /// </summary>
        IntegratedPhone = 0x0000003f,
        DisplayBrightnessMax   = 0x00000041,
        KeyboardBacklightState = 0x00000042,
        AccessoryPower         = 0x00000043,
        /// <summary>
        ///     Number of supported HAL attributes
        /// </summary>
        NumHalAttributes = 0x00000059,
        /// <summary>
        ///     Machine language
        /// </summary>
        Language = 0x00001000,
        /// <summary>
        ///     0=Symbian OS based install, 1=installation via a PC
        /// </summary>
        RemoteInstall = 0x00001001
    }

#endregion

#region Nested type: ConditionalType

    enum ConditionalType : uint
    {
        /// <summary>
        ///     a == b
        /// </summary>
        Equals,
        /// <summary>
        ///     a != b
        /// </summary>
        Differs,
        /// <summary>
        ///     a &gt; b
        /// </summary>
        GreaterThan,
        /// <summary>
        ///     a &lt; b
        /// </summary>
        LessThan,
        /// <summary>
        ///     a &gt;= b
        /// </summary>
        GreaterOrEqualThan,
        /// <summary>
        ///     a &lt;= b
        /// </summary>
        LessOrEqualThan,
        /// <summary>
        ///     a AND b
        /// </summary>
        And,
        /// <summary>
        ///     a OR b
        /// </summary>
        Or,
        /// <summary>
        ///     exists(filename)
        /// </summary>
        Exists,
        /// <summary>
        ///     devcap(capability)
        /// </summary>
        DeviceCapability,
        /// <summary>
        ///     appcap(uid, capability)
        /// </summary>
        ApplicationCapability,
        /// <summary>
        ///     NOT a
        /// </summary>
        Not,
        /// <summary>
        ///     String
        /// </summary>
        String,
        /// <summary>
        ///     Attribute
        /// </summary>
        Attribute,
        /// <summary>
        ///     Number
        /// </summary>
        Number
    }

#endregion

#region Nested type: CpuAbiCode

    enum CpuAbiCode
    {
        ARM4  = 0,
        ARMI  = 1,
        Thumb = 2,
        MCORE = 3,
        MSVC  = 4
    }

#endregion

#region Nested type: CpuArchitecture

    enum CpuArchitecture
    {
        ARM4  = 0x400,
        ARM4T = 0x410,
        ARM5  = 0x500,
        M340  = 0x300
    }

#endregion

#region Nested type: CpuCode

    enum CpuCode
    {
        ARM   = 0,
        MCORE = 1,
        x86   = 2
    }

#endregion

#region Nested type: DeviceFamilyCode

    enum DeviceFamilyCode
    {
        Crystal = 0,
        Pearl   = 1,
        Quartz  = 2
    }

#endregion

#region Nested type: FileDetails

    /// <summary>
    ///     Gives some specific details about how to handle some special files
    /// </summary>
    enum FileDetails : uint
    {
        /// <summary>
        ///     Show the <c>continue</c> button and continue installing
        /// </summary>
        TextContinue = 0,
        /// <summary>
        ///     Show a <c>yes</c> and a <c>no</c> button and skip next file on <c>no</c>
        /// </summary>
        TextSkip = 1,
        /// <summary>
        ///     Show a <c>yes</c> and a <c>no</c> button and abort installation on <c>no</c>
        /// </summary>
        TextAbort = 2,
        /// <summary>
        ///     Show a <c>yes</c> and a <c>no</c> button and abort and undo installation on <c>no</c>
        /// </summary>
        TextExit = 3,
        /// <summary>
        ///     Run during installation
        /// </summary>
        RunInstall = 0,
        /// <summary>
        ///     Run during uninstallation
        /// </summary>
        RunRemove = 1,
        /// <summary>
        ///     Run during both installation and uninstallation
        /// </summary>
        RunBoth = 2,
        /// <summary>
        ///     Works as a flag. Close when installation is complete.
        /// </summary>
        RunsEnd = 0x100,
        /// <summary>
        ///     Works as a flag. Wait for it to close before continuing.
        /// </summary>
        RunWait = 0x200,
        /// <summary>
        ///     Works as a flag. Close when installation is complete.
        /// </summary>
        OpenClose = 0x100,
        /// <summary>
        ///     Works as a flag. Wait for it to close before continuing.
        /// </summary>
        OpenWait = 0x200
    }

#endregion

#region Nested type: FileRecordType

    /// <summary>
    ///     Define the file record type and therefore its structure
    /// </summary>
    enum FileRecordType : uint
    {
        /// <summary>
        ///     Points to a single file
        /// </summary>
        SimpleFile = 0,
        /// <summary>
        ///     Points to an array of files sorted by the language codes
        /// </summary>
        MultipleLanguageFiles = 1,
        /// <summary>
        ///     Points to an array of option strings
        /// </summary>
        Options = 2,
        If     = 3,
        ElseIf = 4,
        Else   = 5,
        EndIf  = 6
    }

#endregion

#region Nested type: FileType

    /// <summary>
    ///     Defines the file type
    /// </summary>
    enum FileType : uint
    {
        /// <summary>
        ///     Standard file
        /// </summary>
        File = 0,
        /// <summary>
        ///     Text file to show during installation
        /// </summary>
        FileText = 1,
        /// <summary>
        ///     SIS component
        /// </summary>
        Component = 2,
        /// <summary>
        ///     File to run during installation
        /// </summary>
        FileRun = 3,
        /// <summary>
        ///     File does not exist in SIS, will be created when application runs
        /// </summary>
        FileNull = 4,
        /// <summary>
        ///     Open file using whatever app is associated with its MIME type
        /// </summary>
        FileMime = 5
    }

#endregion

#region Nested type: LanguageCodes

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum LanguageCodes
    {
        Test = 0,
        EN,
        FR,
        GE,
        SP,
        IT,
        SW,
        DA,
        NO,
        FI,
        AM,
        SF,
        SG,
        PO,
        TU,
        IC,
        RU,
        HU,
        DU,
        BL,
        AU,
        BF,
        AS,
        NZ,
        IF,
        CS,
        SK,
        PL,
        SL,
        TC,
        HK,
        ZH,
        JA,
        TH,
        AF,
        SQ,
        AH,
        AR,
        HY,
        TL,
        BE,
        BN,
        BG,
        MY,
        CA,
        HR,
        CE,
        IE,
        ZA,
        ET,
        FA,
        CF,
        GD,
        KA,
        EL,
        CG,
        GU,
        HE,
        HI,
        IN,
        GA,
        SZ,
        KN,
        KK,
        KM,
        KO,
        LO,
        LV,
        LT,
        MK,
        MS,
        ML,
        MR,
        MO,
        MN,
        NN,
        BP,
        PA,
        RO,
        SR,
        SI,
        SO,
        OS,
        LS,
        SH,
        FS,
        TA,
        TE,
        BO,
        TI,
        CT,
        TK,
        UK,
        UR,
        VI,
        CY,
        ZU
    }

#endregion

#region Nested type: ManufacturerCode

    enum ManufacturerCode
    {
        Ericsson  = 0,
        Motorola  = 1,
        Nokia     = 2,
        Panasonic = 3,
        Psion     = 4,
        Intel     = 5,
        Cogent    = 6,
        Cirrus    = 7
    }

#endregion

#region Nested type: SymbianOptions

    /// <summary>
    ///     Options
    /// </summary>
    [Flags]
    enum SymbianOptions : ushort
    {
        IsUnicode       = 0x0001,
        IsDistributable = 0x0002,
        NoCompress      = 0x0008,
        ShutdownApps    = 0x0010
    }

#endregion

#region Nested type: SymbianType

    // Types
    enum SymbianType : ushort
    {
        /// <summary>
        ///     Application
        /// </summary>
        Application = 0x0000,
        /// <summary>
        ///     System component (library)
        /// </summary>
        SystemComponent = 0x0001,
        /// <summary>
        ///     Optional component
        /// </summary>
        OptionalComponent = 0x0002,
        /// <summary>
        ///     Configures an application
        /// </summary>
        Configurator = 0x0003,
        /// <summary>
        ///     Patch
        /// </summary>
        Patch = 0x0004,
        /// <summary>
        ///     Upgrade
        /// </summary>
        Upgrade = 0x0005
    }

#endregion
}