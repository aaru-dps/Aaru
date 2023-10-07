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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#pragma warning disable CS0169 // Field is never used

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Aaru.Archives;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "InheritdocConsiderUsage")]
[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public sealed partial class Symbian
{
#region Nested type: AttributeConditionalExpression

    /// <summary>
    ///     Contains an attribute to be used as a parameter in a conditional expression
    /// </summary>
    class AttributeConditionalExpression : ConditionalExpression
    {
        public Attribute attribute;
        public uint      unused;
    }

#endregion

#region Nested type: BaseFileRecord

    /// <summary>
    ///     Common fields to simple file record and multiple file record
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BaseFileRecord
    {
        /// <summary>
        ///     File record type, in this case <see cref="FileRecordType.SimpleFile" /> or
        ///     <see cref="FileRecordType.MultipleLanguageFiles" />
        /// </summary>
        public FileRecordType recordType;
        /// <summary>
        ///     File type <see cref="FileType" />
        /// </summary>
        public FileType type;
        /// <summary>
        ///     File details <see cref="FileDetails" />
        /// </summary>
        public FileDetails details;
        /// <summary>
        ///     Length in bytes of the source name (filename on the machine that built the SIS)
        /// </summary>
        public uint sourceNameLen;
        /// <summary>
        ///     Pointer to the source name
        /// </summary>
        public uint sourceNamePtr;
        /// <summary>
        ///     Length in bytes of the destination name (filename+path it will be installed to. '!:' for drive means allow the user
        ///     to pick destination drive)
        /// </summary>
        public uint destinationNameLen;
        /// <summary>
        ///     Pointer to the destination name
        /// </summary>
        public uint destinationNamePtr;
    }

#endregion

#region Nested type: CapabilitiesRecord

    /// <summary>
    ///     TODO: Unclear, check on real files
    /// </summary>
    struct CapabilitiesRecord
    {
        public uint[] keys;
        public uint[] values;
    }

#endregion

#region Nested type: CertificatesRecord

    /// <summary>
    ///     Holds signature certifications, but the exact distribution of them is unclear
    /// </summary>
    struct CertificatesRecord
    {
        public ushort year;
        public ushort month;
        public ushort day;
        public ushort hour;
        public ushort minute;
        public ushort second;
        public uint   numberOfCertificates;
    }

#endregion

#region Nested type: ComponentRecord

    /// <summary>
    ///     Component record as pointed by the header
    /// </summary>
    struct ComponentRecord
    {
        /// <summary>
        ///     Lengths of the component names, array sorted as language records
        /// </summary>
        public uint[] namesLengths;
        /// <summary>
        ///     Pointers to the component names, array sorted as language records
        /// </summary>
        public uint[] namesPointers;
        /// <summary>
        ///     Decoded names, not on-disk
        /// </summary>
        public string[] names;
    }

#endregion

#region Nested type: ConditionalEndRecord

    /// <summary>
    ///     Contains an 'else' or 'endif' expression
    /// </summary>
    struct ConditionalEndRecord
    {
        /// <summary>
        ///     File record type in this case <see cref="FileRecordType.Else" /> or <see cref="FileRecordType.EndIf" />
        /// </summary>
        public FileRecordType recordType;
    }

#endregion

#region Nested type: ConditionalExpression

    /// <summary>
    ///     Conditional expression base
    /// </summary>
    class ConditionalExpression
    {
        /// <summary>
        ///     Conditional type <see cref="ConditionalType" />
        /// </summary>
        public ConditionalType type;
    }

#endregion

#region Nested type: ConditionalRecord

    /// <summary>
    ///     Contains an 'if' or 'else if' condition
    /// </summary>
    struct ConditionalRecord
    {
        /// <summary>
        ///     File record type in this case <see cref="FileRecordType.If" /> or <see cref="FileRecordType.ElseIf" />
        /// </summary>
        public FileRecordType recordType;
        /// <summary>
        ///     Length in bytes of the record and all contained expressions
        /// </summary>
        public uint length;
        /// <summary>
        ///     Conditional expression(s) (chain)
        /// </summary>
        public ConditionalExpression expression;
    }

#endregion

#region Nested type: DecodedFileRecord

    /// <summary>
    ///     On-memory structure
    /// </summary>
    struct DecodedFileRecord
    {
        /// <summary>
        ///     File type <see cref="FileType" />
        /// </summary>
        public FileType type;
        /// <summary>
        ///     File details <see cref="FileDetails" />
        /// </summary>
        public FileDetails details;
        /// <summary>
        ///     Source name (filename on the machine that built the SIS)
        /// </summary>
        public string sourceName;
        /// <summary>
        ///     Destination name (filename+path it will be installed to. '!:' for drive means allow the user
        ///     to pick destination drive)
        /// </summary>
        public string destinationName;
        /// <summary>
        ///     Length in bytes of the (compressed or uncompressed) file
        /// </summary>
        public uint length;
        /// <summary>
        ///     Pointer to the (compressed or uncompressed) file data
        /// </summary>
        public uint pointer;
        /// <summary>
        ///     EPOC Release >= 6, uncompressed file length
        /// </summary>
        public uint originalLength;
        /// <summary>
        ///     EPOC Release >= 6, MIME type string
        /// </summary>
        public string mime;
        /// <summary>
        ///     Language, or null for no language
        /// </summary>
        public string language;
    }

#endregion

#region Nested type: MultipleFileRecord

    /// <summary>
    ///     Multiple language file record, cannot be marshalled
    /// </summary>
    struct MultipleFileRecord
    {
        /// <summary>
        ///     Common fields to simple file record and multiple file record
        /// </summary>
        public BaseFileRecord record;
        /// <summary>
        ///     Lengths in bytes of the (compressed or uncompressed) files, array sorted as language records
        /// </summary>
        public uint[] lengths;
        /// <summary>
        ///     Pointers to the (compressed or uncompressed) files data, array sorted as language records
        /// </summary>
        public uint[] pointers;
        /// <summary>
        ///     EPOC Release >= 6, uncompressed files lengths, array sorted as language records
        /// </summary>
        public uint[] originalLengths;
        /// <summary>
        ///     EPOC Release >= 6, length in bytes of MIME type string
        /// </summary>
        public uint mimeLen;
        /// <summary>
        ///     EPOC Release >= 6, pointer to MIME type string
        /// </summary>
        public uint mimePtr;
    }

#endregion

#region Nested type: NumberConditionalExpression

    /// <summary>
    ///     Contains a number to be used as a parameter in a conditional expression
    /// </summary>
    class NumberConditionalExpression : ConditionalExpression
    {
        public uint number;
        public uint unused;
    }

#endregion

#region Nested type: OptionRecord

    struct OptionRecord
    {
        /// <summary>
        ///     Pointer to the option name lengths, array sorted as language records
        /// </summary>
        public uint[] lengths;
        /// <summary>
        ///     Pointer to the option names, array sorted as language records
        /// </summary>
        public uint[] strings;
    }

#endregion

#region Nested type: OptionsLineRecord

    struct OptionsLineRecord
    {
        /// <summary>
        ///     File record type in this case <see cref="FileRecordType.Options" />
        /// </summary>
        public FileRecordType recordType;
        /// <summary>
        ///     How many options follow
        /// </summary>
        public uint numberOfOptions;
        /// <summary>
        ///     Option records
        /// </summary>
        public OptionRecord[] options;
        /// <summary>
        ///     128-bit bitmap of selected options
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ulong[] selectedOptions;
    }

#endregion

#region Nested type: RequisiteRecord

    struct RequisiteRecord
    {
        /// <summary>
        ///     UID of the required component
        /// </summary>
        public uint uid;
        /// <summary>
        ///     Major version of the required component
        /// </summary>
        public ushort majorVersion;
        /// <summary>
        ///     Minor version of the required component
        /// </summary>
        public ushort minorVersion;
        /// <summary>
        ///     Variant of the required component, usually set to <c>0</c> and not checked by installer
        /// </summary>
        public uint variant;
        /// <summary>
        ///     Lengths of the requisite names, array sorted as language records
        /// </summary>
        public uint[] namesLengths;
        /// <summary>
        ///     Pointers to the requisite names, array sorted as language records
        /// </summary>
        public uint[] namesPointers;
    }

#endregion

#region Nested type: SimpleFileRecord

    /// <summary>
    ///     Simple file record, can be marshalled
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SimpleFileRecord
    {
        /// <summary>
        ///     Common fields to simple file record and multiple file record
        /// </summary>
        public BaseFileRecord record;
        /// <summary>
        ///     Length in bytes of the (compressed or uncompressed) file
        /// </summary>
        public uint length;
        /// <summary>
        ///     Pointer to the (compressed or uncompressed) file data
        /// </summary>
        public uint pointer;
        /// <summary>
        ///     EPOC Release >= 6, uncompressed file length
        /// </summary>
        public uint originalLength;
        /// <summary>
        ///     EPOC Release >= 6, length in bytes of MIME type string
        /// </summary>
        public uint mimeLen;
        /// <summary>
        ///     EPOC Release >= 6, pointer to MIME type string
        /// </summary>
        public uint mimePtr;
    }

#endregion

#region Nested type: StringConditionalExpression

    /// <summary>
    ///     Points to a string used as a parameter in a conditional expression
    /// </summary>
    class StringConditionalExpression : ConditionalExpression
    {
        public uint length;
        public uint pointer;
    }

#endregion

#region Nested type: SubConditionalExpression

    /// <summary>
    ///     Conditional expression that contains a single sub-expression
    /// </summary>
    class SubConditionalExpression : ConditionalExpression
    {
        /// <summary>
        ///     Sub-expression
        /// </summary>
        public ConditionalExpression subExpression;
    }

#endregion

#region Nested type: SymbianHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SymbianHeader
    {
        /// <summary>
        ///     Application UID before SymbianOS 9, magic after
        /// </summary>
        public uint uid1;
        /// <summary>
        ///     EPOC release magic before SOS 9, NULLs after
        /// </summary>
        public uint uid2;
        /// <summary>
        ///     Application UID after SOS 9, magic before
        /// </summary>
        public uint uid3;
        /// <summary>
        ///     Checksum of UIDs 1 to 3
        /// </summary>
        public uint uid4;
        /// <summary>
        ///     CRC16 of all header
        /// </summary>
        public ushort crc16;
        /// <summary>
        ///     Number of languages
        /// </summary>
        public ushort languages;
        /// <summary>
        ///     Number of files
        /// </summary>
        public ushort files;
        /// <summary>
        ///     Number of requisites
        /// </summary>
        public ushort requisites;
        /// <summary>
        ///     Installed language (only residual SIS)
        /// </summary>
        public ushort inst_lang;
        /// <summary>
        ///     Installed files (only residual SIS)
        /// </summary>
        public ushort inst_files;
        /// <summary>
        ///     Installed drive (only residual SIS), NULL or 0x0021
        /// </summary>
        public ushort inst_drive;
        /// <summary>
        ///     Number of capabilities
        /// </summary>
        public ushort capabilities;
        /// <summary>
        ///     Version of Symbian Installer required
        /// </summary>
        public uint inst_version;
        /// <summary>
        ///     Option flags
        /// </summary>
        public SymbianOptions options;
        /// <summary>
        ///     Type
        /// </summary>
        public SymbianType type;
        /// <summary>
        ///     Major version of application
        /// </summary>
        public ushort major;
        /// <summary>
        ///     Minor version of application
        /// </summary>
        public ushort minor;
        /// <summary>
        ///     Variant when SIS is a prerequisite for other SISs
        /// </summary>
        public uint variant;
        /// <summary>
        ///     Pointer to language records
        /// </summary>
        public uint lang_ptr;
        /// <summary>
        ///     Pointer to file records
        /// </summary>
        public uint files_ptr;
        /// <summary>
        ///     Pointer to requisite records
        /// </summary>
        public uint reqs_ptr;
        /// <summary>
        ///     Pointer to certificate records
        /// </summary>
        public uint certs_ptr;
        /// <summary>
        ///     Pointer to component name record
        /// </summary>
        public uint comp_ptr;
        // From EPOC Release 6
        /// <summary>
        ///     Pointer to signature record
        /// </summary>
        public uint sig_ptr;
        /// <summary>
        ///     Pointer to capability records
        /// </summary>
        public uint caps_ptr;
        /// <summary>
        ///     Installed space (only residual SIS)
        /// </summary>
        public uint instspace;
        /// <summary>
        ///     Space required
        /// </summary>
        public uint maxinsspc;
        /// <summary>
        ///     Reserved
        /// </summary>
        public ulong reserved1;
        /// <summary>
        ///     Reserved
        /// </summary>
        public ulong reserved2;
    }

#endregion

#region Nested type: TwoSubsConditionalExpression

    /// <summary>
    ///     Conditional expression that contains two sub-expressions
    /// </summary>
    class TwoSubsConditionalExpression : ConditionalExpression
    {
        /// <summary>
        ///     Left hand side sub-expression
        /// </summary>
        public ConditionalExpression leftOperand;
        /// <summary>
        ///     Right hand side sub-expression
        /// </summary>
        public ConditionalExpression rightOperand;
    }

#endregion
}