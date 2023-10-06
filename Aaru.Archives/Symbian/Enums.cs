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
public partial class Symbian
{
#region Nested type: LanguageCodes

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum LanguageCodes
    {
        Test,
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