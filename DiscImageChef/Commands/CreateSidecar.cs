// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CreateSidecar.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using Schemas;
using System.Collections.Generic;
using DiscImageChef.Plugins;
using DiscImageChef.ImagePlugins;
using DiscImageChef.Console;
using DiscImageChef.Checksums;
using System.IO;
using System.Threading;
using DiscImageChef.CommonTypes;
using DiscImageChef.PartPlugins;

namespace DiscImageChef.Commands
{
    public static class CreateSidecar
    {
        public static void doSidecar(CreateSidecarSubOptions options)
        {
            CICMMetadataType sidecar = new CICMMetadataType();
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();
            ImagePlugin _imageFormat;

            try
            {
                _imageFormat = ImageFormat.Detect(options.InputFile);

                if (_imageFormat == null)
                {
                    DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");
                    return;
                }
                else
                {
                    if (options.Verbose)
                        DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", _imageFormat.Name, _imageFormat.PluginUUID);
                    else
                        DicConsole.WriteLine("Image format identified by {0}.", _imageFormat.Name);
                }

                try
                {
                    if (!_imageFormat.OpenImage(options.InputFile))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");
                        return;
                    }

                    DicConsole.DebugWriteLine("Analyze command", "Correctly opened image file.");
                }
                catch (Exception ex)
                {
                    System.Console.Error.WriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    return;
                }

                FileInfo fi = new FileInfo(options.InputFile);
                FileStream fs = new FileStream(options.InputFile, FileMode.Open, FileAccess.Read);

                Adler32Context adler32ctx = new Adler32Context();
                CRC16Context crc16ctx = new CRC16Context();
                CRC32Context crc32ctx = new CRC32Context();
                CRC64Context crc64ctx = new CRC64Context();
                MD5Context md5ctx = new MD5Context();
                RIPEMD160Context ripemd160ctx = new RIPEMD160Context();
                SHA1Context sha1ctx = new SHA1Context();
                SHA256Context sha256ctx = new SHA256Context();
                SHA384Context sha384ctx = new SHA384Context();
                SHA512Context sha512ctx = new SHA512Context();
                SpamSumContext ssctx = new SpamSumContext();

                Thread adlerThread = new Thread(updateAdler);
                Thread crc16Thread = new Thread(updateCRC16);
                Thread crc32Thread = new Thread(updateCRC32);
                Thread crc64Thread = new Thread(updateCRC64);
                Thread md5Thread = new Thread(updateMD5);
                Thread ripemd160Thread = new Thread(updateRIPEMD160);
                Thread sha1Thread = new Thread(updateSHA1);
                Thread sha256Thread = new Thread(updateSHA256);
                Thread sha384Thread = new Thread(updateSHA384);
                Thread sha512Thread = new Thread(updateSHA512);
                Thread spamsumThread = new Thread(updateSpamSum);

                adlerPacket adlerPkt = new adlerPacket();
                crc16Packet crc16Pkt = new crc16Packet();
                crc32Packet crc32Pkt = new crc32Packet();
                crc64Packet crc64Pkt = new crc64Packet();
                md5Packet md5Pkt = new md5Packet();
                ripemd160Packet ripemd160Pkt = new ripemd160Packet();
                sha1Packet sha1Pkt = new sha1Packet();
                sha256Packet sha256Pkt = new sha256Packet();
                sha384Packet sha384Pkt = new sha384Packet();
                sha512Packet sha512Pkt = new sha512Packet();
                spamsumPacket spamsumPkt = new spamsumPacket();

                adler32ctx.Init();
                adlerPkt.context = adler32ctx;
                crc16ctx.Init();
                crc16Pkt.context = crc16ctx;
                crc32ctx.Init();
                crc32Pkt.context = crc32ctx;
                crc64ctx.Init();
                crc64Pkt.context = crc64ctx;
                md5ctx.Init();
                md5Pkt.context = md5ctx;
                ripemd160ctx.Init();
                ripemd160Pkt.context = ripemd160ctx;
                sha1ctx.Init();
                sha1Pkt.context = sha1ctx;
                sha256ctx.Init();
                sha256Pkt.context = sha256ctx;
                sha384ctx.Init();
                sha384Pkt.context = sha384ctx;
                sha512ctx.Init();
                sha512Pkt.context = sha512ctx;
                ssctx.Init();
                spamsumPkt.context = ssctx;

                byte[] data;
                long position = 0;
                while (position < (fi.Length - 1048576))
                {
                    data = new byte[1048576];
                    fs.Read(data, 0, 1048576);

                    DicConsole.Write("\rHashing image file byte {0} of {1}", position, fi.Length);

                    adlerPkt.data = data;
                    adlerThread.Start(adlerPkt);
                    crc16Pkt.data = data;
                    crc16Thread.Start(crc16Pkt);
                    crc32Pkt.data = data;
                    crc32Thread.Start(crc32Pkt);
                    crc64Pkt.data = data;
                    crc64Thread.Start(crc64Pkt);
                    md5Pkt.data = data;
                    md5Thread.Start(md5Pkt);
                    ripemd160Pkt.data = data;
                    ripemd160Thread.Start(ripemd160Pkt);
                    sha1Pkt.data = data;
                    sha1Thread.Start(sha1Pkt);
                    sha256Pkt.data = data;
                    sha256Thread.Start(sha256Pkt);
                    sha384Pkt.data = data;
                    sha384Thread.Start(sha384Pkt);
                    sha512Pkt.data = data;
                    sha512Thread.Start(sha512Pkt);
                    spamsumPkt.data = data;
                    spamsumThread.Start(spamsumPkt);

                    while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                           crc32Thread.IsAlive || crc64Thread.IsAlive ||
                           md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                           sha1Thread.IsAlive || sha256Thread.IsAlive ||
                           sha384Thread.IsAlive || sha512Thread.IsAlive ||
                           spamsumThread.IsAlive)
                    {
                    }

                    adlerThread = new Thread(updateAdler);
                    crc16Thread = new Thread(updateCRC16);
                    crc32Thread = new Thread(updateCRC32);
                    crc64Thread = new Thread(updateCRC64);
                    md5Thread = new Thread(updateMD5);
                    ripemd160Thread = new Thread(updateRIPEMD160);
                    sha1Thread = new Thread(updateSHA1);
                    sha256Thread = new Thread(updateSHA256);
                    sha384Thread = new Thread(updateSHA384);
                    sha512Thread = new Thread(updateSHA512);
                    spamsumThread = new Thread(updateSpamSum);

                    position += 1048576;
                }

                data = new byte[fi.Length - position];
                fs.Read(data, 0, (int)(fi.Length - position));

                DicConsole.Write("\rHashing image file byte {0} of {1}", position, fi.Length);

                adlerPkt.data = data;
                adlerThread.Start(adlerPkt);
                crc16Pkt.data = data;
                crc16Thread.Start(crc16Pkt);
                crc32Pkt.data = data;
                crc32Thread.Start(crc32Pkt);
                crc64Pkt.data = data;
                crc64Thread.Start(crc64Pkt);
                md5Pkt.data = data;
                md5Thread.Start(md5Pkt);
                ripemd160Pkt.data = data;
                ripemd160Thread.Start(ripemd160Pkt);
                sha1Pkt.data = data;
                sha1Thread.Start(sha1Pkt);
                sha256Pkt.data = data;
                sha256Thread.Start(sha256Pkt);
                sha384Pkt.data = data;
                sha384Thread.Start(sha384Pkt);
                sha512Pkt.data = data;
                sha512Thread.Start(sha512Pkt);
                spamsumPkt.data = data;
                spamsumThread.Start(spamsumPkt);

                while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                       crc32Thread.IsAlive || crc64Thread.IsAlive ||
                       md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                       sha1Thread.IsAlive || sha256Thread.IsAlive ||
                       sha384Thread.IsAlive || sha512Thread.IsAlive ||
                       spamsumThread.IsAlive)
                {
                }

                DicConsole.WriteLine();
                fs.Close();

                List<ChecksumType> imgChecksums = new List<ChecksumType>();

                ChecksumType chk = new ChecksumType();
                chk.type = ChecksumTypeType.adler32;
                chk.Value = adler32ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc16;
                chk.Value = crc16ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc32;
                chk.Value = crc32ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc64;
                chk.Value = crc64ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.md5;
                chk.Value = md5ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.ripemd160;
                chk.Value = ripemd160ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha1;
                chk.Value = sha1ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha256;
                chk.Value = sha256ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha384;
                chk.Value = sha384ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha512;
                chk.Value = sha512ctx.End();
                imgChecksums.Add(chk);

                chk = new ChecksumType();
                chk.type = ChecksumTypeType.spamsum;
                chk.Value = ssctx.End();
                imgChecksums.Add(chk);

                switch (_imageFormat.ImageInfo.xmlMediaType)
                {
                    case XmlMediaType.OpticalDisc:
                        {
                            sidecar.OpticalDisc = new OpticalDiscType[1];
                            sidecar.OpticalDisc[0] = new OpticalDiscType();
                            sidecar.OpticalDisc[0].Checksums = imgChecksums.ToArray();
                            sidecar.OpticalDisc[0].Image = new ImageType();
                            sidecar.OpticalDisc[0].Image.format = _imageFormat.GetImageFormat();
                            sidecar.OpticalDisc[0].Image.offset = 0;
                            sidecar.OpticalDisc[0].Image.offsetSpecified = true;
                            sidecar.OpticalDisc[0].Image.Value = Path.GetFileName(options.InputFile);
                            sidecar.OpticalDisc[0].Size = fi.Length;
                            sidecar.OpticalDisc[0].Sequence = new SequenceType();
                            if (_imageFormat.GetDiskSequence() != 0 && _imageFormat.GetLastDiskSequence() != 0)
                            {
                                sidecar.OpticalDisc[0].Sequence.MediaSequence = _imageFormat.GetDiskSequence();
                                sidecar.OpticalDisc[0].Sequence.TotalMedia = _imageFormat.GetDiskSequence();
                            }
                            else
                            {
                                sidecar.OpticalDisc[0].Sequence.MediaSequence = 1;
                                sidecar.OpticalDisc[0].Sequence.TotalMedia = 1;
                            }
                            sidecar.OpticalDisc[0].Sequence.MediaTitle = _imageFormat.GetImageName();

                            DiskType dskType = _imageFormat.ImageInfo.diskType;

                            foreach (DiskTagType tagType in _imageFormat.ImageInfo.readableDiskTags)
                            {
                                switch (tagType)
                                {
                                    case DiskTagType.CD_ATIP:
                                        sidecar.OpticalDisc[0].ATIP = new DumpType();
                                        sidecar.OpticalDisc[0].ATIP.Checksums = GetChecksums(_imageFormat.ReadDiskTag(DiskTagType.CD_ATIP)).ToArray();
                                        sidecar.OpticalDisc[0].ATIP.Size = _imageFormat.ReadDiskTag(DiskTagType.CD_ATIP).Length;
                                        Decoders.CD.ATIP.CDATIP? atip = Decoders.CD.ATIP.Decode(_imageFormat.ReadDiskTag(DiskTagType.CD_ATIP));
                                        if (atip.HasValue)
                                        {
                                            if (atip.Value.DDCD)
                                                dskType = atip.Value.DiscType ? DiskType.DDCDRW : DiskType.DDCDR;
                                            else
                                                dskType = atip.Value.DiscType ? DiskType.CDRW : DiskType.CDR;
                                        }
                                        break;
                                    case DiskTagType.DVD_BCA:
                                        sidecar.OpticalDisc[0].BCA = new DumpType();
                                        sidecar.OpticalDisc[0].BCA.Checksums = GetChecksums(_imageFormat.ReadDiskTag(DiskTagType.DVD_BCA)).ToArray();
                                        sidecar.OpticalDisc[0].BCA.Size = _imageFormat.ReadDiskTag(DiskTagType.DVD_BCA).Length;
                                        break;
                                    case DiskTagType.BD_BCA:
                                        sidecar.OpticalDisc[0].BCA = new DumpType();
                                        sidecar.OpticalDisc[0].BCA.Checksums = GetChecksums(_imageFormat.ReadDiskTag(DiskTagType.BD_BCA)).ToArray();
                                        sidecar.OpticalDisc[0].BCA.Size = _imageFormat.ReadDiskTag(DiskTagType.BD_BCA).Length;
                                        break;
                                    case DiskTagType.DVD_CMI:
                                        sidecar.OpticalDisc[0].CMI = new DumpType();
                                        Decoders.DVD.CSS_CPRM.LeadInCopyright? cmi = Decoders.DVD.CSS_CPRM.DecodeLeadInCopyright(_imageFormat.ReadDiskTag(DiskTagType.DVD_CMI));
                                        if (cmi.HasValue)
                                        {
                                            switch (cmi.Value.CopyrightType)
                                            {
                                                case Decoders.DVD.CopyrightType.AACS:
                                                    sidecar.OpticalDisc[0].CopyProtection = "AACS";
                                                    break;
                                                case Decoders.DVD.CopyrightType.CSS:
                                                    sidecar.OpticalDisc[0].CopyProtection = "CSS";
                                                    break;
                                                case Decoders.DVD.CopyrightType.CPRM:
                                                    sidecar.OpticalDisc[0].CopyProtection = "CPRM";
                                                    break;
                                            }
                                        }
                                        sidecar.OpticalDisc[0].CMI.Checksums = GetChecksums(_imageFormat.ReadDiskTag(DiskTagType.DVD_CMI)).ToArray();
                                        sidecar.OpticalDisc[0].CMI.Size = _imageFormat.ReadDiskTag(DiskTagType.DVD_CMI).Length;
                                        break;
                                    case DiskTagType.DVD_DMI:
                                        sidecar.OpticalDisc[0].DMI = new DumpType();
                                        sidecar.OpticalDisc[0].DMI.Checksums = GetChecksums(_imageFormat.ReadDiskTag(DiskTagType.DVD_DMI)).ToArray();
                                        sidecar.OpticalDisc[0].DMI.Size = _imageFormat.ReadDiskTag(DiskTagType.DVD_DMI).Length;
                                        if (Decoders.Xbox.DMI.IsXbox(_imageFormat.ReadDiskTag(DiskTagType.DVD_DMI)))
                                        {
                                            dskType = DiskType.XGD;
                                            sidecar.OpticalDisc[0].Dimensions = new DimensionsType();
                                            sidecar.OpticalDisc[0].Dimensions.Diameter = 120;
                                        }
                                        else if (Decoders.Xbox.DMI.IsXbox360(_imageFormat.ReadDiskTag(DiskTagType.DVD_DMI)))
                                        {
                                            dskType = DiskType.XGD2;
                                            sidecar.OpticalDisc[0].Dimensions = new DimensionsType();
                                            sidecar.OpticalDisc[0].Dimensions.Diameter = 120;
                                        }
                                        break;
                                    case DiskTagType.DVD_PFI:
                                        sidecar.OpticalDisc[0].PFI = new DumpType();
                                        sidecar.OpticalDisc[0].PFI.Checksums = GetChecksums(_imageFormat.ReadDiskTag(DiskTagType.DVD_PFI)).ToArray();
                                        sidecar.OpticalDisc[0].PFI.Size = _imageFormat.ReadDiskTag(DiskTagType.DVD_PFI).Length;
                                        Decoders.DVD.PFI.PhysicalFormatInformation? pfi = Decoders.DVD.PFI.Decode(_imageFormat.ReadDiskTag(DiskTagType.DVD_PFI));
                                        if (pfi.HasValue)
                                        {
                                            if (dskType != DiskType.XGD &&
                                               dskType != DiskType.XGD2 &&
                                               dskType != DiskType.XGD3)
                                            {
                                                switch (pfi.Value.DiskCategory)
                                                {
                                                    case Decoders.DVD.DiskCategory.DVDPR:
                                                        dskType = DiskType.DVDPR;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.DVDPRDL:
                                                        dskType = DiskType.DVDPRDL;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.DVDPRW:
                                                        dskType = DiskType.DVDPRW;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.DVDPRWDL:
                                                        dskType = DiskType.DVDPRWDL;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.DVDR:
                                                        dskType = DiskType.DVDR;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.DVDRAM:
                                                        dskType = DiskType.DVDRAM;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.DVDROM:
                                                        dskType = DiskType.DVDROM;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.DVDRW:
                                                        dskType = DiskType.DVDRW;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.HDDVDR:
                                                        dskType = DiskType.HDDVDR;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.HDDVDRAM:
                                                        dskType = DiskType.HDDVDRAM;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.HDDVDROM:
                                                        dskType = DiskType.HDDVDROM;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.HDDVDRW:
                                                        dskType = DiskType.HDDVDRW;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.Nintendo:
                                                        dskType = DiskType.GOD;
                                                        break;
                                                    case Decoders.DVD.DiskCategory.UMD:
                                                        dskType = DiskType.UMD;
                                                        break;
                                                }

                                                if (dskType == DiskType.DVDR && pfi.Value.PartVersion == 6)
                                                    dskType = DiskType.DVDRDL;
                                                if (dskType == DiskType.DVDRW && pfi.Value.PartVersion == 3)
                                                    dskType = DiskType.DVDRWDL;
                                                if (dskType == DiskType.GOD && pfi.Value.DiscSize == DiscImageChef.Decoders.DVD.DVDSize.OneTwenty)
                                                    dskType = DiskType.WOD;

                                                sidecar.OpticalDisc[0].Dimensions = new DimensionsType();
                                                if (dskType == DiskType.UMD)
                                                    sidecar.OpticalDisc[0].Dimensions.Diameter = 60;
                                                else if (pfi.Value.DiscSize == DiscImageChef.Decoders.DVD.DVDSize.Eighty)
                                                    sidecar.OpticalDisc[0].Dimensions.Diameter = 80;
                                                else if (pfi.Value.DiscSize == DiscImageChef.Decoders.DVD.DVDSize.OneTwenty)
                                                    sidecar.OpticalDisc[0].Dimensions.Diameter = 120;
                                            }
                                        }
                                        break;
                                    case DiskTagType.CD_PMA:
                                        sidecar.OpticalDisc[0].PMA = new DumpType();
                                        sidecar.OpticalDisc[0].PMA.Checksums = GetChecksums(_imageFormat.ReadDiskTag(DiskTagType.CD_PMA)).ToArray();
                                        sidecar.OpticalDisc[0].PMA.Size = _imageFormat.ReadDiskTag(DiskTagType.CD_PMA).Length;
                                        break;
                                }
                            }

                            string dscType, dscSubType;
                            Metadata.DiskType.DiskTypeToString(dskType, out dscType, out dscSubType);
                            sidecar.OpticalDisc[0].DiscType = dscType;
                            sidecar.OpticalDisc[0].DiscSubType = dscSubType;

                            try
                            {
                                List<Session> sessions = _imageFormat.GetSessions();
                                sidecar.OpticalDisc[0].Sessions = sessions != null ? sessions.Count : 1;
                            }
                            catch
                            {
                                sidecar.OpticalDisc[0].Sessions = 1;
                            }

                            List<Track> tracks = _imageFormat.GetTracks();
                            List<Schemas.TrackType> trksLst = null;
                            if (tracks != null)
                            {
                                sidecar.OpticalDisc[0].Tracks = new int[1];
                                sidecar.OpticalDisc[0].Tracks[0] = tracks.Count;
                                trksLst = new List<Schemas.TrackType>();
                            }

                            foreach (Track trk in tracks)
                            {
                                Schemas.TrackType xmlTrk = new Schemas.TrackType();
                                switch (trk.TrackType)
                                {
                                    case DiscImageChef.ImagePlugins.TrackType.Audio:
                                        xmlTrk.TrackType1 = TrackTypeTrackType.audio;
                                        break;
                                    case DiscImageChef.ImagePlugins.TrackType.CDMode2Form2:
                                        xmlTrk.TrackType1 = TrackTypeTrackType.m2f2;
                                        break;
                                    case DiscImageChef.ImagePlugins.TrackType.CDMode2Formless:
                                        xmlTrk.TrackType1 = TrackTypeTrackType.mode2;
                                        break;
                                    case DiscImageChef.ImagePlugins.TrackType.CDMode2Form1:
                                        xmlTrk.TrackType1 = TrackTypeTrackType.m2f1;
                                        break;
                                    case DiscImageChef.ImagePlugins.TrackType.CDMode1:
                                        xmlTrk.TrackType1 = TrackTypeTrackType.mode1;
                                        break;
                                    case DiscImageChef.ImagePlugins.TrackType.Data:
                                        switch (sidecar.OpticalDisc[0].DiscType)
                                        {
                                            case "BD":
                                                xmlTrk.TrackType1 = TrackTypeTrackType.bluray;
                                                break;
                                            case "DDCD":
                                                xmlTrk.TrackType1 = TrackTypeTrackType.ddcd;
                                                break;
                                            case "DVD":
                                                xmlTrk.TrackType1 = TrackTypeTrackType.dvd;
                                                break;
                                            case "HD DVD":
                                                xmlTrk.TrackType1 = TrackTypeTrackType.hddvd;
                                                break;
                                            default:
                                                xmlTrk.TrackType1 = TrackTypeTrackType.mode1;
                                                break;
                                        }
                                        break;
                                }
                                xmlTrk.Sequence = new TrackSequenceType();
                                xmlTrk.Sequence.Session = trk.TrackSession;
                                xmlTrk.Sequence.TrackNumber = (int)trk.TrackSequence;
                                xmlTrk.StartSector = (long)trk.TrackStartSector;
                                xmlTrk.EndSector = (long)trk.TrackEndSector;

                                if (sidecar.OpticalDisc[0].DiscType == "CD" ||
                                   sidecar.OpticalDisc[0].DiscType == "GD")
                                {
                                    xmlTrk.StartMSF = LbaToMsf(xmlTrk.StartSector);
                                    xmlTrk.EndMSF = LbaToMsf(xmlTrk.EndSector);
                                }
                                else if (sidecar.OpticalDisc[0].DiscType == "DDCD")
                                {
                                    xmlTrk.StartMSF = DdcdLbaToMsf(xmlTrk.StartSector);
                                    xmlTrk.EndMSF = DdcdLbaToMsf(xmlTrk.EndSector);
                                }

                                xmlTrk.Image = new ImageType();
                                xmlTrk.Image.Value = Path.GetFileName(trk.TrackFile);
                                if (trk.TrackFileOffset > 0)
                                {
                                    xmlTrk.Image.offset = (long)trk.TrackFileOffset;
                                    xmlTrk.Image.offsetSpecified = true;
                                }

                                xmlTrk.Image.format = trk.TrackFileType;
                                xmlTrk.Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * trk.TrackRawBytesPerSector;
                                xmlTrk.BytesPerSector = trk.TrackBytesPerSector;

                                uint sectorsToRead = 512;

                                adler32ctx = new Adler32Context();
                                crc16ctx = new CRC16Context();
                                crc32ctx = new CRC32Context();
                                crc64ctx = new CRC64Context();
                                md5ctx = new MD5Context();
                                ripemd160ctx = new RIPEMD160Context();
                                sha1ctx = new SHA1Context();
                                sha256ctx = new SHA256Context();
                                sha384ctx = new SHA384Context();
                                sha512ctx = new SHA512Context();
                                ssctx = new SpamSumContext();

                                adlerThread = new Thread(updateAdler);
                                crc16Thread = new Thread(updateCRC16);
                                crc32Thread = new Thread(updateCRC32);
                                crc64Thread = new Thread(updateCRC64);
                                md5Thread = new Thread(updateMD5);
                                ripemd160Thread = new Thread(updateRIPEMD160);
                                sha1Thread = new Thread(updateSHA1);
                                sha256Thread = new Thread(updateSHA256);
                                sha384Thread = new Thread(updateSHA384);
                                sha512Thread = new Thread(updateSHA512);
                                spamsumThread = new Thread(updateSpamSum);

                                adlerPkt = new adlerPacket();
                                crc16Pkt = new crc16Packet();
                                crc32Pkt = new crc32Packet();
                                crc64Pkt = new crc64Packet();
                                md5Pkt = new md5Packet();
                                ripemd160Pkt = new ripemd160Packet();
                                sha1Pkt = new sha1Packet();
                                sha256Pkt = new sha256Packet();
                                sha384Pkt = new sha384Packet();
                                sha512Pkt = new sha512Packet();
                                spamsumPkt = new spamsumPacket();

                                adler32ctx.Init();
                                adlerPkt.context = adler32ctx;
                                crc16ctx.Init();
                                crc16Pkt.context = crc16ctx;
                                crc32ctx.Init();
                                crc32Pkt.context = crc32ctx;
                                crc64ctx.Init();
                                crc64Pkt.context = crc64ctx;
                                md5ctx.Init();
                                md5Pkt.context = md5ctx;
                                ripemd160ctx.Init();
                                ripemd160Pkt.context = ripemd160ctx;
                                sha1ctx.Init();
                                sha1Pkt.context = sha1ctx;
                                sha256ctx.Init();
                                sha256Pkt.context = sha256ctx;
                                sha384ctx.Init();
                                sha384Pkt.context = sha384ctx;
                                sha512ctx.Init();
                                sha512Pkt.context = sha512ctx;
                                ssctx.Init();
                                spamsumPkt.context = ssctx;

                                ulong sectors = (ulong)(xmlTrk.EndSector - xmlTrk.StartSector + 1);
                                ulong doneSectors = 0;

                                while (doneSectors < sectors)
                                {
                                    byte[] sector;

                                    if ((sectors - doneSectors) >= sectorsToRead)
                                    {
                                        sector = _imageFormat.ReadSectorsLong(doneSectors, sectorsToRead, (uint)xmlTrk.Sequence.TrackNumber);
                                        DicConsole.Write("\rHashings sectors {0} to {2} of track {1} ({3} sectors)", doneSectors, xmlTrk.Sequence.TrackNumber, doneSectors + sectorsToRead, sectors);
                                        doneSectors += sectorsToRead;
                                    }
                                    else
                                    {
                                        sector = _imageFormat.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors), (uint)xmlTrk.Sequence.TrackNumber);
                                        DicConsole.Write("\rHashings sectors {0} to {2} of track {1} ({3} sectors)", doneSectors, xmlTrk.Sequence.TrackNumber, doneSectors + (sectors - doneSectors), sectors);
                                        doneSectors += (sectors - doneSectors);
                                    }

                                    adlerPkt.data = sector;
                                    adlerThread.Start(adlerPkt);
                                    crc16Pkt.data = sector;
                                    crc16Thread.Start(crc16Pkt);
                                    crc32Pkt.data = sector;
                                    crc32Thread.Start(crc32Pkt);
                                    crc64Pkt.data = sector;
                                    crc64Thread.Start(crc64Pkt);
                                    md5Pkt.data = sector;
                                    md5Thread.Start(md5Pkt);
                                    ripemd160Pkt.data = sector;
                                    ripemd160Thread.Start(ripemd160Pkt);
                                    sha1Pkt.data = sector;
                                    sha1Thread.Start(sha1Pkt);
                                    sha256Pkt.data = sector;
                                    sha256Thread.Start(sha256Pkt);
                                    sha384Pkt.data = sector;
                                    sha384Thread.Start(sha384Pkt);
                                    sha512Pkt.data = sector;
                                    sha512Thread.Start(sha512Pkt);
                                    spamsumPkt.data = sector;
                                    spamsumThread.Start(spamsumPkt);

                                    while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                                               crc32Thread.IsAlive || crc64Thread.IsAlive ||
                                               md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                                               sha1Thread.IsAlive || sha256Thread.IsAlive ||
                                               sha384Thread.IsAlive || sha512Thread.IsAlive ||
                                               spamsumThread.IsAlive)
                                    {
                                    }

                                    adlerThread = new Thread(updateAdler);
                                    crc16Thread = new Thread(updateCRC16);
                                    crc32Thread = new Thread(updateCRC32);
                                    crc64Thread = new Thread(updateCRC64);
                                    md5Thread = new Thread(updateMD5);
                                    ripemd160Thread = new Thread(updateRIPEMD160);
                                    sha1Thread = new Thread(updateSHA1);
                                    sha256Thread = new Thread(updateSHA256);
                                    sha384Thread = new Thread(updateSHA384);
                                    sha512Thread = new Thread(updateSHA512);
                                    spamsumThread = new Thread(updateSpamSum);
                                }

                                List<ChecksumType> trkChecksums = new List<ChecksumType>();

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.adler32;
                                chk.Value = adler32ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.crc16;
                                chk.Value = crc16ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.crc32;
                                chk.Value = crc32ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.crc64;
                                chk.Value = crc64ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.md5;
                                chk.Value = md5ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.ripemd160;
                                chk.Value = ripemd160ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.sha1;
                                chk.Value = sha1ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.sha256;
                                chk.Value = sha256ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.sha384;
                                chk.Value = sha384ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.sha512;
                                chk.Value = sha512ctx.End();
                                trkChecksums.Add(chk);

                                chk = new ChecksumType();
                                chk.type = ChecksumTypeType.spamsum;
                                chk.Value = ssctx.End();
                                trkChecksums.Add(chk);

                                xmlTrk.Checksums = trkChecksums.ToArray();

                                DicConsole.WriteLine();

                                if (trk.TrackSubchannelType != TrackSubchannelType.None)
                                {
                                    xmlTrk.SubChannel = new SubChannelType();
                                    xmlTrk.SubChannel.Image = new ImageType();
                                    switch (trk.TrackSubchannelType)
                                    {
                                        case TrackSubchannelType.Packed:
                                        case TrackSubchannelType.PackedInterleaved:
                                            xmlTrk.SubChannel.Image.format = "rw";
                                            break;
                                        case TrackSubchannelType.Raw:
                                        case TrackSubchannelType.RawInterleaved:
                                            xmlTrk.SubChannel.Image.format = "rw_raw";
                                            break;
                                    }

                                    if (trk.TrackFileOffset > 0)
                                    {
                                        xmlTrk.SubChannel.Image.offset = (long)trk.TrackSubchannelOffset;
                                        xmlTrk.SubChannel.Image.offsetSpecified = true;
                                    }
                                    xmlTrk.SubChannel.Image.Value = trk.TrackSubchannelFile;

                                    // TODO: Packed subchannel has different size?
                                    xmlTrk.SubChannel.Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * 96;

                                    adler32ctx = new Adler32Context();
                                    crc16ctx = new CRC16Context();
                                    crc32ctx = new CRC32Context();
                                    crc64ctx = new CRC64Context();
                                    md5ctx = new MD5Context();
                                    ripemd160ctx = new RIPEMD160Context();
                                    sha1ctx = new SHA1Context();
                                    sha256ctx = new SHA256Context();
                                    sha384ctx = new SHA384Context();
                                    sha512ctx = new SHA512Context();
                                    ssctx = new SpamSumContext();

                                    adlerThread = new Thread(updateAdler);
                                    crc16Thread = new Thread(updateCRC16);
                                    crc32Thread = new Thread(updateCRC32);
                                    crc64Thread = new Thread(updateCRC64);
                                    md5Thread = new Thread(updateMD5);
                                    ripemd160Thread = new Thread(updateRIPEMD160);
                                    sha1Thread = new Thread(updateSHA1);
                                    sha256Thread = new Thread(updateSHA256);
                                    sha384Thread = new Thread(updateSHA384);
                                    sha512Thread = new Thread(updateSHA512);
                                    spamsumThread = new Thread(updateSpamSum);

                                    adlerPkt = new adlerPacket();
                                    crc16Pkt = new crc16Packet();
                                    crc32Pkt = new crc32Packet();
                                    crc64Pkt = new crc64Packet();
                                    md5Pkt = new md5Packet();
                                    ripemd160Pkt = new ripemd160Packet();
                                    sha1Pkt = new sha1Packet();
                                    sha256Pkt = new sha256Packet();
                                    sha384Pkt = new sha384Packet();
                                    sha512Pkt = new sha512Packet();
                                    spamsumPkt = new spamsumPacket();

                                    adler32ctx.Init();
                                    adlerPkt.context = adler32ctx;
                                    crc16ctx.Init();
                                    crc16Pkt.context = crc16ctx;
                                    crc32ctx.Init();
                                    crc32Pkt.context = crc32ctx;
                                    crc64ctx.Init();
                                    crc64Pkt.context = crc64ctx;
                                    md5ctx.Init();
                                    md5Pkt.context = md5ctx;
                                    ripemd160ctx.Init();
                                    ripemd160Pkt.context = ripemd160ctx;
                                    sha1ctx.Init();
                                    sha1Pkt.context = sha1ctx;
                                    sha256ctx.Init();
                                    sha256Pkt.context = sha256ctx;
                                    sha384ctx.Init();
                                    sha384Pkt.context = sha384ctx;
                                    sha512ctx.Init();
                                    sha512Pkt.context = sha512ctx;
                                    ssctx.Init();
                                    spamsumPkt.context = ssctx;

                                    sectors = (ulong)(xmlTrk.EndSector - xmlTrk.StartSector + 1);
                                    doneSectors = 0;

                                    while (doneSectors < sectors)
                                    {
                                        byte[] sector;

                                        if ((sectors - doneSectors) >= sectorsToRead)
                                        {
                                            sector = _imageFormat.ReadSectorsTag(doneSectors, sectorsToRead, (uint)xmlTrk.Sequence.TrackNumber, SectorTagType.CDSectorSubchannel);
                                            DicConsole.Write("\rHashings subchannel sectors {0} to {2} of track {1} ({3} sectors)", doneSectors, xmlTrk.Sequence.TrackNumber, doneSectors + sectorsToRead, sectors);
                                            doneSectors += sectorsToRead;
                                        }
                                        else
                                        {
                                            sector = _imageFormat.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors), (uint)xmlTrk.Sequence.TrackNumber, SectorTagType.CDSectorSubchannel);
                                            DicConsole.Write("\rHashings subchannel sectors {0} to {2} of track {1} ({3} sectors)", doneSectors, xmlTrk.Sequence.TrackNumber, doneSectors + (sectors - doneSectors), sectors);
                                            doneSectors += (sectors - doneSectors);
                                        }

                                        adlerPkt.data = sector;
                                        adlerThread.Start(adlerPkt);
                                        crc16Pkt.data = sector;
                                        crc16Thread.Start(crc16Pkt);
                                        crc32Pkt.data = sector;
                                        crc32Thread.Start(crc32Pkt);
                                        crc64Pkt.data = sector;
                                        crc64Thread.Start(crc64Pkt);
                                        md5Pkt.data = sector;
                                        md5Thread.Start(md5Pkt);
                                        ripemd160Pkt.data = sector;
                                        ripemd160Thread.Start(ripemd160Pkt);
                                        sha1Pkt.data = sector;
                                        sha1Thread.Start(sha1Pkt);
                                        sha256Pkt.data = sector;
                                        sha256Thread.Start(sha256Pkt);
                                        sha384Pkt.data = sector;
                                        sha384Thread.Start(sha384Pkt);
                                        sha512Pkt.data = sector;
                                        sha512Thread.Start(sha512Pkt);
                                        spamsumPkt.data = sector;
                                        spamsumThread.Start(spamsumPkt);

                                        while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                                            crc32Thread.IsAlive || crc64Thread.IsAlive ||
                                            md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                                            sha1Thread.IsAlive || sha256Thread.IsAlive ||
                                            sha384Thread.IsAlive || sha512Thread.IsAlive ||
                                            spamsumThread.IsAlive)
                                        {
                                        }

                                        adlerThread = new Thread(updateAdler);
                                        crc16Thread = new Thread(updateCRC16);
                                        crc32Thread = new Thread(updateCRC32);
                                        crc64Thread = new Thread(updateCRC64);
                                        md5Thread = new Thread(updateMD5);
                                        ripemd160Thread = new Thread(updateRIPEMD160);
                                        sha1Thread = new Thread(updateSHA1);
                                        sha256Thread = new Thread(updateSHA256);
                                        sha384Thread = new Thread(updateSHA384);
                                        sha512Thread = new Thread(updateSHA512);
                                        spamsumThread = new Thread(updateSpamSum);
                                    }

                                    List<ChecksumType> subChecksums = new List<ChecksumType>();

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.adler32;
                                    chk.Value = adler32ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.crc16;
                                    chk.Value = crc16ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.crc32;
                                    chk.Value = crc32ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.crc64;
                                    chk.Value = crc64ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.md5;
                                    chk.Value = md5ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.ripemd160;
                                    chk.Value = ripemd160ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.sha1;
                                    chk.Value = sha1ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.sha256;
                                    chk.Value = sha256ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.sha384;
                                    chk.Value = sha384ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.sha512;
                                    chk.Value = sha512ctx.End();
                                    subChecksums.Add(chk);

                                    chk = new ChecksumType();
                                    chk.type = ChecksumTypeType.spamsum;
                                    chk.Value = ssctx.End();
                                    subChecksums.Add(chk);

                                    xmlTrk.SubChannel.Checksums = subChecksums.ToArray();

                                    DicConsole.WriteLine();
                                }

                                DicConsole.WriteLine("Checking filesystems on track {0} from sector {1} to {2}", xmlTrk.Sequence.TrackNumber, xmlTrk.StartSector, xmlTrk.EndSector);

                                List<Partition> partitions = new List<Partition>();

                                foreach (PartPlugin _partplugin in plugins.PartPluginsList.Values)
                                {
                                    List<Partition> _partitions;

                                    if (_partplugin.GetInformation(_imageFormat, out _partitions))
                                    {
                                        partitions = _partitions;
                                        break;
                                    }
                                }

                                xmlTrk.FileSystemInformation = new PartitionType[1];
                                if(partitions.Count > 0)
                                {
                                    xmlTrk.FileSystemInformation = new PartitionType[partitions.Count];
                                    for(int i = 0; i < partitions.Count; i++)
                                    {
                                        xmlTrk.FileSystemInformation[i] = new PartitionType();
                                        xmlTrk.FileSystemInformation[i].Description = partitions[i].PartitionDescription;
                                        xmlTrk.FileSystemInformation[i].EndSector = (int)(partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1);
                                        xmlTrk.FileSystemInformation[i].Name = partitions[i].PartitionName;
                                        xmlTrk.FileSystemInformation[i].Sequence = (int)partitions[i].PartitionSequence;
                                        xmlTrk.FileSystemInformation[i].StartSector = (int)partitions[i].PartitionStartSector;
                                        xmlTrk.FileSystemInformation[i].Type = partitions[i].PartitionType;

                                        List<FileSystemType> lstFs = new List<FileSystemType>();

                                        foreach (Plugin _plugin in plugins.PluginsList.Values)
                                        {
                                            try
                                            {
                                                if (_plugin.Identify(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector+partitions[i].PartitionSectors-1))
                                                {
                                                    string foo;
                                                    _plugin.GetInformation(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector+partitions[i].PartitionSectors-1, out foo);
                                                    lstFs.Add(_plugin.XmlFSType);
                                                }
                                            }
                                            catch
                                            {
                                                //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                                            }
                                        }

                                        if(lstFs.Count > 0)
                                            xmlTrk.FileSystemInformation[i].FileSystems = lstFs.ToArray();
                                    }
                                }
                                else
                                {
                                    xmlTrk.FileSystemInformation[0] = new PartitionType();
                                    xmlTrk.FileSystemInformation[0].EndSector = (int)xmlTrk.EndSector;
                                    xmlTrk.FileSystemInformation[0].StartSector = (int)xmlTrk.StartSector;

                                    List<FileSystemType> lstFs = new List<FileSystemType>();

                                    foreach (Plugin _plugin in plugins.PluginsList.Values)
                                    {
                                        try
                                        {
                                            if (_plugin.Identify(_imageFormat, (ulong)xmlTrk.StartSector, (ulong)xmlTrk.EndSector))
                                            {
                                                string foo;
                                                _plugin.GetInformation(_imageFormat, (ulong)xmlTrk.StartSector, (ulong)xmlTrk.EndSector, out foo);
                                                lstFs.Add(_plugin.XmlFSType);
                                            }
                                        }
                                        catch
                                        {
                                            //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                                        }
                                    }

                                    if(lstFs.Count > 0)
                                        xmlTrk.FileSystemInformation[0].FileSystems = lstFs.ToArray();
                                }

                                trksLst.Add(xmlTrk);
                            }

                            if (trksLst != null)
                                sidecar.OpticalDisc[0].Track = trksLst.ToArray();

                            break;
                        }
                    case XmlMediaType.BlockMedia:
                        {
                            sidecar.BlockMedia = new BlockMediaType[1];
                            sidecar.BlockMedia[0] = new BlockMediaType();
                            sidecar.BlockMedia[0].Checksums = imgChecksums.ToArray();
                            sidecar.BlockMedia[0].Image = new ImageType();
                            sidecar.BlockMedia[0].Image.format = _imageFormat.GetImageFormat();
                            sidecar.BlockMedia[0].Image.offset = 0;
                            sidecar.BlockMedia[0].Image.offsetSpecified = true;
                            sidecar.BlockMedia[0].Image.Value = Path.GetFileName(options.InputFile);
                            sidecar.BlockMedia[0].Size = fi.Length;
                            sidecar.BlockMedia[0].Sequence = new SequenceType();
                            if (_imageFormat.GetDiskSequence() != 0 && _imageFormat.GetLastDiskSequence() != 0)
                            {
                                sidecar.BlockMedia[0].Sequence.MediaSequence = _imageFormat.GetDiskSequence();
                                sidecar.BlockMedia[0].Sequence.TotalMedia = _imageFormat.GetDiskSequence();
                            }
                            else
                            {
                                sidecar.BlockMedia[0].Sequence.MediaSequence = 1;
                                sidecar.BlockMedia[0].Sequence.TotalMedia = 1;
                            }
                            sidecar.BlockMedia[0].Sequence.MediaTitle = _imageFormat.GetImageName();

                            //DiskType dskType = _imageFormat.ImageInfo.diskType;
                            // TODO: Complete it
                            break;
                        }
                    case XmlMediaType.LinearMedia:
                        {
                            sidecar.LinearMedia = new LinearMediaType[1];
                            sidecar.LinearMedia[0] = new LinearMediaType();
                            sidecar.LinearMedia[0].Checksums = imgChecksums.ToArray();
                            sidecar.LinearMedia[0].Image = new ImageType();
                            sidecar.LinearMedia[0].Image.format = _imageFormat.GetImageFormat();
                            sidecar.LinearMedia[0].Image.offset = 0;
                            sidecar.LinearMedia[0].Image.offsetSpecified = true;
                            sidecar.LinearMedia[0].Image.Value = Path.GetFileName(options.InputFile);
                            sidecar.LinearMedia[0].Size = fi.Length;

                            //DiskType dskType = _imageFormat.ImageInfo.diskType;
                            // TODO: Complete it
                            break;
                        }
                    case XmlMediaType.AudioMedia:
                        {
                            sidecar.AudioMedia = new AudioMediaType[1];
                            sidecar.AudioMedia[0] = new AudioMediaType();
                            sidecar.AudioMedia[0].Checksums = imgChecksums.ToArray();
                            sidecar.AudioMedia[0].Image = new ImageType();
                            sidecar.AudioMedia[0].Image.format = _imageFormat.GetImageFormat();
                            sidecar.AudioMedia[0].Image.offset = 0;
                            sidecar.AudioMedia[0].Image.offsetSpecified = true;
                            sidecar.AudioMedia[0].Image.Value = Path.GetFileName(options.InputFile);
                            sidecar.AudioMedia[0].Size = fi.Length;
                            sidecar.AudioMedia[0].Sequence = new SequenceType();
                            if (_imageFormat.GetDiskSequence() != 0 && _imageFormat.GetLastDiskSequence() != 0)
                            {
                                sidecar.AudioMedia[0].Sequence.MediaSequence = _imageFormat.GetDiskSequence();
                                sidecar.AudioMedia[0].Sequence.TotalMedia = _imageFormat.GetDiskSequence();
                            }
                            else
                            {
                                sidecar.AudioMedia[0].Sequence.MediaSequence = 1;
                                sidecar.AudioMedia[0].Sequence.TotalMedia = 1;
                            }
                            sidecar.AudioMedia[0].Sequence.MediaTitle = _imageFormat.GetImageName();

                            //DiskType dskType = _imageFormat.ImageInfo.diskType;
                            // TODO: Complete it
                            break;
                        }

                }

                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(Path.GetDirectoryName(options.InputFile) +
                    //Path.PathSeparator +
                                   Path.GetFileNameWithoutExtension(options.InputFile) + ".cicm.xml",
                                       FileMode.CreateNew);

                System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }
            catch (Exception ex)
            {
                DicConsole.ErrorWriteLine(String.Format("Error reading file: {0}", ex.Message));
                DicConsole.DebugWriteLine("Analyze command", ex.StackTrace);
            }

        }

        static string LbaToMsf(long lba)
        {
            long m, s, f;
            if (lba >= -150)
            {
                m = (lba + 150) / (75 * 60);
                lba -= m * (75 * 60);
                s = (lba + 150) / 75;
                lba -= s * 75;
                f = lba + 150;
            }
            else
            {
                m = (lba + 450150) / (75 * 60);
                lba -= m * (75 * 60);
                s = (lba + 450150) / 75;
                lba -= s * 75;
                f = lba + 450150;
            }

            return String.Format("{0}:{1:D2}:{2:D2}", m, s, f);
        }

        static string DdcdLbaToMsf(long lba)
        {
            long h, m, s, f;
            if (lba >= -150)
            {
                h = (lba + 150) / (75 * 60 * 60);
                lba -= h * (75 * 60 * 60);
                m = (lba + 150) / (75 * 60);
                lba -= m * (75 * 60);
                s = (lba + 150) / 75;
                lba -= s * 75;
                f = lba + 150;
            }
            else
            {
                h = (lba + 450150 * 2) / (75 * 60 * 60);
                lba -= h * (75 * 60 * 60);
                m = (lba + 450150 * 2) / (75 * 60);
                lba -= m * (75 * 60);
                s = (lba + 450150 * 2) / 75;
                lba -= s * 75;
                f = lba + 450150 * 2;
            }

            return String.Format("{3}:{0:D2}:{1:D2}:{2:D2}", m, s, f, h);
        }

        static List<ChecksumType> GetChecksums(byte[] data)
        {
            Adler32Context adler32ctx = new Adler32Context();
            CRC16Context crc16ctx = new CRC16Context();
            CRC32Context crc32ctx = new CRC32Context();
            CRC64Context crc64ctx = new CRC64Context();
            MD5Context md5ctx = new MD5Context();
            RIPEMD160Context ripemd160ctx = new RIPEMD160Context();
            SHA1Context sha1ctx = new SHA1Context();
            SHA256Context sha256ctx = new SHA256Context();
            SHA384Context sha384ctx = new SHA384Context();
            SHA512Context sha512ctx = new SHA512Context();
            SpamSumContext ssctx = new SpamSumContext();

            Thread adlerThread = new Thread(updateAdler);
            Thread crc16Thread = new Thread(updateCRC16);
            Thread crc32Thread = new Thread(updateCRC32);
            Thread crc64Thread = new Thread(updateCRC64);
            Thread md5Thread = new Thread(updateMD5);
            Thread ripemd160Thread = new Thread(updateRIPEMD160);
            Thread sha1Thread = new Thread(updateSHA1);
            Thread sha256Thread = new Thread(updateSHA256);
            Thread sha384Thread = new Thread(updateSHA384);
            Thread sha512Thread = new Thread(updateSHA512);
            Thread spamsumThread = new Thread(updateSpamSum);

            adlerPacket adlerPkt = new adlerPacket();
            crc16Packet crc16Pkt = new crc16Packet();
            crc32Packet crc32Pkt = new crc32Packet();
            crc64Packet crc64Pkt = new crc64Packet();
            md5Packet md5Pkt = new md5Packet();
            ripemd160Packet ripemd160Pkt = new ripemd160Packet();
            sha1Packet sha1Pkt = new sha1Packet();
            sha256Packet sha256Pkt = new sha256Packet();
            sha384Packet sha384Pkt = new sha384Packet();
            sha512Packet sha512Pkt = new sha512Packet();
            spamsumPacket spamsumPkt = new spamsumPacket();

            adler32ctx.Init();
            adlerPkt.context = adler32ctx;
            crc16ctx.Init();
            crc16Pkt.context = crc16ctx;
            crc32ctx.Init();
            crc32Pkt.context = crc32ctx;
            crc64ctx.Init();
            crc64Pkt.context = crc64ctx;
            md5ctx.Init();
            md5Pkt.context = md5ctx;
            ripemd160ctx.Init();
            ripemd160Pkt.context = ripemd160ctx;
            sha1ctx.Init();
            sha1Pkt.context = sha1ctx;
            sha256ctx.Init();
            sha256Pkt.context = sha256ctx;
            sha384ctx.Init();
            sha384Pkt.context = sha384ctx;
            sha512ctx.Init();
            sha512Pkt.context = sha512ctx;
            ssctx.Init();
            spamsumPkt.context = ssctx;

            adlerPkt.data = data;
            adlerThread.Start(adlerPkt);
            crc16Pkt.data = data;
            crc16Thread.Start(crc16Pkt);
            crc32Pkt.data = data;
            crc32Thread.Start(crc32Pkt);
            crc64Pkt.data = data;
            crc64Thread.Start(crc64Pkt);
            md5Pkt.data = data;
            md5Thread.Start(md5Pkt);
            ripemd160Pkt.data = data;
            ripemd160Thread.Start(ripemd160Pkt);
            sha1Pkt.data = data;
            sha1Thread.Start(sha1Pkt);
            sha256Pkt.data = data;
            sha256Thread.Start(sha256Pkt);
            sha384Pkt.data = data;
            sha384Thread.Start(sha384Pkt);
            sha512Pkt.data = data;
            sha512Thread.Start(sha512Pkt);
            spamsumPkt.data = data;
            spamsumThread.Start(spamsumPkt);

            while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                   crc32Thread.IsAlive || crc64Thread.IsAlive ||
                   md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                   sha1Thread.IsAlive || sha256Thread.IsAlive ||
                   sha384Thread.IsAlive || sha512Thread.IsAlive ||
                   spamsumThread.IsAlive)
            {
            }

            List<ChecksumType> imgChecksums = new List<ChecksumType>();
            ChecksumType chk = new ChecksumType();

            chk.type = ChecksumTypeType.adler32;
            chk.Value = adler32ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.crc16;
            chk.Value = crc16ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.crc32;
            chk.Value = crc32ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.crc64;
            chk.Value = crc64ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.md5;
            chk.Value = md5ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.ripemd160;
            chk.Value = ripemd160ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.sha1;
            chk.Value = sha1ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.sha256;
            chk.Value = sha256ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.sha384;
            chk.Value = sha384ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.sha512;
            chk.Value = sha512ctx.End();
            imgChecksums.Add(chk);

            chk.type = ChecksumTypeType.spamsum;
            chk.Value = ssctx.End();
            imgChecksums.Add(chk);

            return imgChecksums;
        }

        #region Threading helpers

        struct adlerPacket
        {
            public Adler32Context context;
            public byte[] data;
        }

        struct crc16Packet
        {
            public CRC16Context context;
            public byte[] data;
        }

        struct crc32Packet
        {
            public CRC32Context context;
            public byte[] data;
        }

        struct crc64Packet
        {
            public CRC64Context context;
            public byte[] data;
        }

        /*struct fletcher16Packet
        {
            public Fletcher16Context context;
            public byte[] data;
        }

        struct fletcher32Packet
        {
            public Fletcher32Context context;
            public byte[] data;
        }*/

        struct md5Packet
        {
            public MD5Context context;
            public byte[] data;
        }

        struct ripemd160Packet
        {
            public RIPEMD160Context context;
            public byte[] data;
        }

        struct sha1Packet
        {
            public SHA1Context context;
            public byte[] data;
        }

        struct sha256Packet
        {
            public SHA256Context context;
            public byte[] data;
        }

        struct sha384Packet
        {
            public SHA384Context context;
            public byte[] data;
        }

        struct sha512Packet
        {
            public SHA512Context context;
            public byte[] data;
        }

        struct spamsumPacket
        {
            public SpamSumContext context;
            public byte[] data;
        }

        static void updateAdler(object packet)
        {
            ((adlerPacket)packet).context.Update(((adlerPacket)packet).data);
        }

        static void updateCRC16(object packet)
        {
            ((crc16Packet)packet).context.Update(((crc16Packet)packet).data);
        }

        static void updateCRC32(object packet)
        {
            ((crc32Packet)packet).context.Update(((crc32Packet)packet).data);
        }

        static void updateCRC64(object packet)
        {
            ((crc64Packet)packet).context.Update(((crc64Packet)packet).data);
        }

        /*static void updateFletcher16(object packet)
        {
            ((fletcher16Packet)packet).context.Update(((fletcher16Packet)packet).data);
        }

        static void updateFletcher32(object packet)
        {
            ((fletcher32Packet)packet).context.Update(((fletcher32Packet)packet).data);
        }*/

        static void updateMD5(object packet)
        {
            ((md5Packet)packet).context.Update(((md5Packet)packet).data);
        }

        static void updateRIPEMD160(object packet)
        {
            ((ripemd160Packet)packet).context.Update(((ripemd160Packet)packet).data);
        }

        static void updateSHA1(object packet)
        {
            ((sha1Packet)packet).context.Update(((sha1Packet)packet).data);
        }

        static void updateSHA256(object packet)
        {
            ((sha256Packet)packet).context.Update(((sha256Packet)packet).data);
        }

        static void updateSHA384(object packet)
        {
            ((sha384Packet)packet).context.Update(((sha384Packet)packet).data);
        }

        static void updateSHA512(object packet)
        {
            ((sha512Packet)packet).context.Update(((sha512Packet)packet).data);
        }

        static void updateSpamSum(object packet)
        {
            ((spamsumPacket)packet).context.Update(((spamsumPacket)packet).data);
        }

        #endregion Threading helpers

    }
}

