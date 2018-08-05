// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Mode2A.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SCSI MODE 2Ah.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models.SCSI.MMC
{
    public class Mode2A : BaseEntity
    {
        public bool                  AccurateCDDA                     { get; set; }
        public bool                  BCK                              { get; set; }
        public ushort                BufferSize                       { get; set; }
        public bool?                 BufferUnderRunProtection         { get; set; }
        public bool                  CanEject                         { get; set; }
        public bool                  CanLockMedia                     { get; set; }
        public bool                  CDDACommand                      { get; set; }
        public bool                  CompositeAudioVideo              { get; set; }
        public bool                  CSSandCPPMSupported              { get; set; }
        public ushort?               CurrentSpeed                     { get; set; }
        public ushort?               CurrentWriteSpeed                { get; set; }
        public ushort?               CurrentWriteSpeedSelected        { get; set; }
        public bool                  DeterministicSlotChanger         { get; set; }
        public bool                  DigitalPort1                     { get; set; }
        public bool                  DigitalPort2                     { get; set; }
        public bool                  LeadInPW                         { get; set; }
        public byte                  LoadingMechanismType             { get; set; }
        public bool                  LockStatus                       { get; set; }
        public bool                  LSBF                             { get; set; }
        public ushort?               MaximumSpeed                     { get; set; }
        public ushort?               MaximumWriteSpeed                { get; set; }
        public bool                  PlaysAudio                       { get; set; }
        public bool                  PreventJumperStatus              { get; set; }
        public bool                  RCK                              { get; set; }
        public bool                  ReadsBarcode                     { get; set; }
        public bool                  ReadsBothSides                   { get; set; }
        public bool                  ReadsCDR                         { get; set; }
        public bool                  ReadsCDRW                        { get; set; }
        public bool                  ReadsDeinterlavedSubchannel      { get; set; }
        public bool                  ReadsDVDR                        { get; set; }
        public bool                  ReadsDVDRAM                      { get; set; }
        public bool                  ReadsDVDROM                      { get; set; }
        public bool                  ReadsISRC                        { get; set; }
        public bool                  ReadsMode2Form2                  { get; set; }
        public bool                  ReadsMode2Form1                  { get; set; }
        public bool                  ReadsPacketCDR                   { get; set; }
        public bool                  ReadsSubchannel                  { get; set; }
        public bool                  ReadsUPC                         { get; set; }
        public bool                  ReturnsC2Pointers                { get; set; }
        public byte?                 RotationControlSelected          { get; set; }
        public bool                  SeparateChannelMute              { get; set; }
        public bool                  SeparateChannelVolume            { get; set; }
        public bool                  SSS                              { get; set; }
        public bool                  SupportsMultiSession             { get; set; }
        public ushort?               SupportedVolumeLevels            { get; set; }
        public bool                  TestWrite                        { get; set; }
        public bool                  WritesCDR                        { get; set; }
        public bool                  WritesCDRW                       { get; set; }
        public bool                  WritesDVDR                       { get; set; }
        public bool                  WritesDVDRAM                     { get; set; }
        public List<WriteDescriptor> WriteSpeedPerformanceDescriptors { get; set; }

        public static Mode2A MapMode2A(mmcModeType oldMode)
        {
            if(oldMode == null) return null;

            Mode2A newMode = new Mode2A
            {
                AccurateCDDA                = oldMode.AccurateCDDA,
                BCK                         = oldMode.BCK,
                BufferUnderRunProtection    = oldMode.BufferUnderRunProtection,
                CanEject                    = oldMode.CanEject,
                CanLockMedia                = oldMode.CanLockMedia,
                CDDACommand                 = oldMode.CDDACommand,
                CompositeAudioVideo         = oldMode.CompositeAudioVideo,
                CSSandCPPMSupported         = oldMode.CSSandCPPMSupported,
                DeterministicSlotChanger    = oldMode.DeterministicSlotChanger,
                DigitalPort1                = oldMode.DigitalPort1,
                DigitalPort2                = oldMode.DigitalPort2,
                LeadInPW                    = oldMode.LeadInPW,
                LoadingMechanismType        = oldMode.LoadingMechanismType,
                LockStatus                  = oldMode.LockStatus,
                LSBF                        = oldMode.LSBF,
                PlaysAudio                  = oldMode.PlaysAudio,
                PreventJumperStatus         = oldMode.PreventJumperStatus,
                RCK                         = oldMode.RCK,
                ReadsBarcode                = oldMode.ReadsBarcode,
                ReadsBothSides              = oldMode.ReadsBothSides,
                ReadsCDR                    = oldMode.ReadsCDR,
                ReadsCDRW                   = oldMode.ReadsCDRW,
                ReadsDeinterlavedSubchannel = oldMode.ReadsDeinterlavedSubchannel,
                ReadsDVDR                   = oldMode.ReadsDVDR,
                ReadsDVDRAM                 = oldMode.ReadsDVDRAM,
                ReadsDVDROM                 = oldMode.ReadsDVDROM,
                ReadsISRC                   = oldMode.ReadsISRC,
                ReadsMode2Form2             = oldMode.ReadsMode2Form2,
                ReadsMode2Form1             = oldMode.ReadsMode2Form1,
                ReadsPacketCDR              = oldMode.ReadsPacketCDR,
                ReadsSubchannel             = oldMode.ReadsSubchannel,
                ReadsUPC                    = oldMode.ReadsUPC,
                ReturnsC2Pointers           = oldMode.ReturnsC2Pointers,
                SeparateChannelMute         = oldMode.SeparateChannelMute,
                SeparateChannelVolume       = oldMode.SeparateChannelVolume,
                SSS                         = oldMode.SSS,
                SupportsMultiSession        = oldMode.SupportsMultiSession,
                TestWrite                   = oldMode.TestWrite,
                WritesCDR                   = oldMode.WritesCDR,
                WritesCDRW                  = oldMode.WritesCDRW,
                WritesDVDR                  = oldMode.WritesDVDR,
                WritesDVDRAM                = oldMode.WritesDVDRAM
            };

            if(oldMode.BufferSizeSpecified) newMode.BufferSize               = oldMode.BufferSize;
            if(oldMode.CurrentSpeedSpecified) newMode.CurrentSpeed           = oldMode.CurrentSpeed;
            if(oldMode.CurrentWriteSpeedSpecified) newMode.CurrentWriteSpeed = oldMode.CurrentWriteSpeed;
            if(oldMode.CurrentWriteSpeedSelectedSpecified)
                newMode.CurrentWriteSpeedSelected = oldMode.CurrentWriteSpeedSelected;
            if(oldMode.MaximumSpeedSpecified) newMode.MaximumSpeed           = oldMode.MaximumSpeed;
            if(oldMode.MaximumWriteSpeedSpecified) newMode.MaximumWriteSpeed = oldMode.MaximumWriteSpeed;
            if(oldMode.RotationControlSelectedSpecified)
                newMode.RotationControlSelected = oldMode.RotationControlSelected;
            if(oldMode.SupportedVolumeLevelsSpecified) newMode.SupportedVolumeLevels = oldMode.SupportedVolumeLevels;

            if(oldMode.WriteSpeedPerformanceDescriptors == null) return newMode;

            newMode.WriteSpeedPerformanceDescriptors =
                new List<WriteDescriptor>(oldMode.WriteSpeedPerformanceDescriptors.Select(t => new WriteDescriptor
                {
                    RotationControl = t.RotationControl,
                    WriteSpeed      = t.WriteSpeed
                }));

            return newMode;
        }
    }
}