// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Checksum.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Multithreads checksumming and hashing.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using DiscImageChef.Checksums;
using Schemas;

namespace DiscImageChef.Core
{
    [Flags]
    public enum EnableChecksum
    {
        Adler32 = 1,
        Crc16 = 2,
        Crc32 = 4,
        Crc64 = 8,
        Md5 = 16,
        Ripemd160 = 32,
        Sha1 = 64,
        Sha256 = 128,
        Sha384 = 256,
        Sha512 = 512,
        SpamSum = 1024,
        All = Adler32 | Crc16 | Crc32 | Crc64 | Md5 | Ripemd160 | Sha1 | Sha256 | Sha384 | Sha512 | SpamSum
    }

    public class Checksum
    {
        Adler32Context adler32Ctx;
        Crc16Context crc16Ctx;
        Crc32Context crc32Ctx;
        Crc64Context crc64Ctx;
        Md5Context md5Ctx;
        Ripemd160Context ripemd160Ctx;
        Sha1Context sha1Ctx;
        Sha256Context sha256Ctx;
        Sha384Context sha384Ctx;
        Sha512Context sha512Ctx;
        SpamSumContext ssctx;

        Thread adlerThread;
        Thread crc16Thread;
        Thread crc32Thread;
        Thread crc64Thread;
        Thread md5Thread;
        Thread ripemd160Thread;
        Thread sha1Thread;
        Thread sha256Thread;
        Thread sha384Thread;
        Thread sha512Thread;
        Thread spamsumThread;

        AdlerPacket adlerPkt;
        Crc16Packet crc16Pkt;
        Crc32Packet crc32Pkt;
        Crc64Packet crc64Pkt;
        Md5Packet md5Pkt;
        Ripemd160Packet ripemd160Pkt;
        Sha1Packet sha1Pkt;
        Sha256Packet sha256Pkt;
        Sha384Packet sha384Pkt;
        Sha512Packet sha512Pkt;
        SpamsumPacket spamsumPkt;

        EnableChecksum enabled;

        public Checksum(EnableChecksum enabled = EnableChecksum.All)
        {
            this.enabled = enabled;

            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                adler32Ctx = new Adler32Context();
                adlerPkt = new AdlerPacket();
                adler32Ctx.Init();
                adlerPkt.Context = adler32Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Crc16))
            {
                crc16Ctx = new Crc16Context();
                crc16Pkt = new Crc16Packet();
                crc16Ctx.Init();
                crc16Pkt.Context = crc16Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Crc32))
            {
                crc32Ctx = new Crc32Context();
                crc32Pkt = new Crc32Packet();
                crc32Ctx.Init();
                crc32Pkt.Context = crc32Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Crc64))
            {
                crc64Ctx = new Crc64Context();
                crc64Pkt = new Crc64Packet();
                crc64Ctx.Init();
                crc64Pkt.Context = crc64Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Md5))
            {
                md5Ctx = new Md5Context();
                md5Pkt = new Md5Packet();
                md5Ctx.Init();
                md5Pkt.Context = md5Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Ripemd160))
            {
                ripemd160Ctx = new Ripemd160Context();
                ripemd160Pkt = new Ripemd160Packet();
                ripemd160Ctx.Init();
                ripemd160Pkt.Context = ripemd160Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Sha1))
            {
                sha1Ctx = new Sha1Context();
                sha1Pkt = new Sha1Packet();
                sha1Ctx.Init();
                sha1Pkt.Context = sha1Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Sha256))
            {
                sha256Ctx = new Sha256Context();
                sha256Pkt = new Sha256Packet();
                sha256Ctx.Init();
                sha256Pkt.Context = sha256Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Sha384))
            {
                sha384Ctx = new Sha384Context();
                sha384Pkt = new Sha384Packet();
                sha384Ctx.Init();
                sha384Pkt.Context = sha384Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.Sha512))
            {
                sha512Ctx = new Sha512Context();
                sha512Pkt = new Sha512Packet();
                sha512Ctx.Init();
                sha512Pkt.Context = sha512Ctx;
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                ssctx = new SpamSumContext();
                spamsumPkt = new SpamsumPacket();
                ssctx.Init();
                spamsumPkt.Context = ssctx;
            }

            adlerThread = new Thread(UpdateAdler);
            crc16Thread = new Thread(UpdateCrc16);
            crc32Thread = new Thread(UpdateCrc32);
            crc64Thread = new Thread(UpdateCrc64);
            md5Thread = new Thread(UpdateMd5);
            ripemd160Thread = new Thread(UpdateRipemd160);
            sha1Thread = new Thread(UpdateSha1);
            sha256Thread = new Thread(UpdateSha256);
            sha384Thread = new Thread(UpdateSha384);
            sha512Thread = new Thread(UpdateSha512);
            spamsumThread = new Thread(UpdateSpamSum);
        }

        public void Update(byte[] data)
        {
            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                adlerPkt.Data = data;
                adlerThread.Start(adlerPkt);
            }

            if(enabled.HasFlag(EnableChecksum.Crc16))
            {
                crc16Pkt.Data = data;
                crc16Thread.Start(crc16Pkt);
            }
            if(enabled.HasFlag(EnableChecksum.Crc32))
            {
                crc32Pkt.Data = data;
                crc32Thread.Start(crc32Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.Crc64))
            {
                crc64Pkt.Data = data;
                crc64Thread.Start(crc64Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.Md5))
            {
                md5Pkt.Data = data;
                md5Thread.Start(md5Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.Ripemd160))
            {
                ripemd160Pkt.Data = data;
                ripemd160Thread.Start(ripemd160Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.Sha1))
            {
                sha1Pkt.Data = data;
                sha1Thread.Start(sha1Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.Sha256))
            {
                sha256Pkt.Data = data;
                sha256Thread.Start(sha256Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.Sha384))
            {
                sha384Pkt.Data = data;
                sha384Thread.Start(sha384Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.Sha512))
            {
                sha512Pkt.Data = data;
                sha512Thread.Start(sha512Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                spamsumPkt.Data = data;
                spamsumThread.Start(spamsumPkt);
            }

            while(adlerThread.IsAlive || crc16Thread.IsAlive || crc32Thread.IsAlive || crc64Thread.IsAlive ||
                  md5Thread.IsAlive || ripemd160Thread.IsAlive || sha1Thread.IsAlive || sha256Thread.IsAlive ||
                  sha384Thread.IsAlive || sha512Thread.IsAlive || spamsumThread.IsAlive) { }

            if(enabled.HasFlag(EnableChecksum.SpamSum)) adlerThread = new Thread(UpdateAdler);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) crc16Thread = new Thread(UpdateCrc16);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) crc32Thread = new Thread(UpdateCrc32);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) crc64Thread = new Thread(UpdateCrc64);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) md5Thread = new Thread(UpdateMd5);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) ripemd160Thread = new Thread(UpdateRipemd160);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) sha1Thread = new Thread(UpdateSha1);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) sha256Thread = new Thread(UpdateSha256);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) sha384Thread = new Thread(UpdateSha384);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) sha512Thread = new Thread(UpdateSha512);
            if(enabled.HasFlag(EnableChecksum.SpamSum)) spamsumThread = new Thread(UpdateSpamSum);
        }

        public List<ChecksumType> End()
        {
            List<ChecksumType> chks = new List<ChecksumType>();

            ChecksumType chk;

            if(enabled.HasFlag(EnableChecksum.All))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.adler32;
                chk.Value = adler32Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc16))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc16;
                chk.Value = crc16Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc32))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc32;
                chk.Value = crc32Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc64))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc64;
                chk.Value = crc64Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Md5))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.md5;
                chk.Value = md5Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Ripemd160))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.ripemd160;
                chk.Value = ripemd160Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha1))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha1;
                chk.Value = sha1Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha256))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha256;
                chk.Value = sha256Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha384))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha384;
                chk.Value = sha384Ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha512))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha512;
                chk.Value = sha512Ctx.End();
                chks.Add(chk);
            }

            if(!enabled.HasFlag(EnableChecksum.SpamSum)) return chks;

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.spamsum;
            chk.Value = ssctx.End();
            chks.Add(chk);

            return chks;
        }

        internal static List<ChecksumType> GetChecksums(byte[] data, EnableChecksum enabled = EnableChecksum.All)
        {
            Adler32Context adler32CtxData = null;
            Crc16Context crc16CtxData = null;
            Crc32Context crc32CtxData = null;
            Crc64Context crc64CtxData = null;
            Md5Context md5CtxData = null;
            Ripemd160Context ripemd160CtxData = null;
            Sha1Context sha1CtxData = null;
            Sha256Context sha256CtxData = null;
            Sha384Context sha384CtxData = null;
            Sha512Context sha512CtxData = null;
            SpamSumContext ssctxData = null;

            AdlerPacket adlerPktData;
            Crc16Packet crc16PktData;
            Crc32Packet crc32PktData;
            Crc64Packet crc64PktData;
            Md5Packet md5PktData;
            Ripemd160Packet ripemd160PktData;
            Sha1Packet sha1PktData;
            Sha256Packet sha256PktData;
            Sha384Packet sha384PktData;
            Sha512Packet sha512PktData;
            SpamsumPacket spamsumPktData;

            Thread adlerThreadData = new Thread(UpdateAdler);
            Thread crc16ThreadData = new Thread(UpdateCrc16);
            Thread crc32ThreadData = new Thread(UpdateCrc32);
            Thread crc64ThreadData = new Thread(UpdateCrc64);
            Thread md5ThreadData = new Thread(UpdateMd5);
            Thread ripemd160ThreadData = new Thread(UpdateRipemd160);
            Thread sha1ThreadData = new Thread(UpdateSha1);
            Thread sha256ThreadData = new Thread(UpdateSha256);
            Thread sha384ThreadData = new Thread(UpdateSha384);
            Thread sha512ThreadData = new Thread(UpdateSha512);
            Thread spamsumThreadData = new Thread(UpdateSpamSum);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                adler32CtxData = new Adler32Context();
                adlerPktData = new AdlerPacket();
                adler32CtxData.Init();
                adlerPktData.Context = adler32CtxData;
                adlerPktData.Data = data;
                adlerThreadData.Start(adlerPktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                crc16PktData = new Crc16Packet();
                crc16CtxData = new Crc16Context();
                crc16CtxData.Init();
                crc16PktData.Context = crc16CtxData;
                crc16PktData.Data = data;
                crc16ThreadData.Start(crc16PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                crc32PktData = new Crc32Packet();
                crc32CtxData = new Crc32Context();
                crc32CtxData.Init();
                crc32PktData.Context = crc32CtxData;
                crc32PktData.Data = data;
                crc32ThreadData.Start(crc32PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                crc64PktData = new Crc64Packet();
                crc64CtxData = new Crc64Context();
                crc64CtxData.Init();
                crc64PktData.Context = crc64CtxData;
                crc64PktData.Data = data;
                crc64ThreadData.Start(crc64PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                md5PktData = new Md5Packet();
                md5CtxData = new Md5Context();
                md5CtxData.Init();
                md5PktData.Context = md5CtxData;
                md5PktData.Data = data;
                md5ThreadData.Start(md5PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                ripemd160PktData = new Ripemd160Packet();
                ripemd160CtxData = new Ripemd160Context();
                ripemd160CtxData.Init();
                ripemd160PktData.Context = ripemd160CtxData;
                ripemd160PktData.Data = data;
                ripemd160ThreadData.Start(ripemd160PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                sha1PktData = new Sha1Packet();
                sha1CtxData = new Sha1Context();
                sha1CtxData.Init();
                sha1PktData.Context = sha1CtxData;
                sha1PktData.Data = data;
                sha1ThreadData.Start(sha1PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                sha256PktData = new Sha256Packet();
                sha256CtxData = new Sha256Context();
                sha256CtxData.Init();
                sha256PktData.Context = sha256CtxData;
                sha256PktData.Data = data;
                sha256ThreadData.Start(sha256PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                sha384PktData = new Sha384Packet();
                sha384CtxData = new Sha384Context();
                sha384CtxData.Init();
                sha384PktData.Context = sha384CtxData;
                sha384PktData.Data = data;
                sha384ThreadData.Start(sha384PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                sha512PktData = new Sha512Packet();
                sha512CtxData = new Sha512Context();
                sha512CtxData.Init();
                sha512PktData.Context = sha512CtxData;
                sha512PktData.Data = data;
                sha512ThreadData.Start(sha512PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                spamsumPktData = new SpamsumPacket();
                ssctxData = new SpamSumContext();
                ssctxData.Init();
                spamsumPktData.Context = ssctxData;
                spamsumPktData.Data = data;
                spamsumThreadData.Start(spamsumPktData);
            }

            while(adlerThreadData.IsAlive || crc16ThreadData.IsAlive || crc32ThreadData.IsAlive ||
                  crc64ThreadData.IsAlive || md5ThreadData.IsAlive || ripemd160ThreadData.IsAlive ||
                  sha1ThreadData.IsAlive || sha256ThreadData.IsAlive || sha384ThreadData.IsAlive ||
                  sha512ThreadData.IsAlive || spamsumThreadData.IsAlive) { }

            List<ChecksumType> dataChecksums = new List<ChecksumType>();
            ChecksumType chk;

            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.adler32;
                chk.Value = adler32CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc16))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc16;
                chk.Value = crc16CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc32))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc32;
                chk.Value = crc32CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc64))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc64;
                chk.Value = crc64CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Md5))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.md5;
                chk.Value = md5CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Ripemd160))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.ripemd160;
                chk.Value = ripemd160CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha1))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha1;
                chk.Value = sha1CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha256))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha256;
                chk.Value = sha256CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha384))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha384;
                chk.Value = sha384CtxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha512))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha512;
                chk.Value = sha512CtxData.End();
                dataChecksums.Add(chk);
            }

            if(!enabled.HasFlag(EnableChecksum.SpamSum)) return dataChecksums;

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.spamsum;
            chk.Value = ssctxData.End();
            dataChecksums.Add(chk);

            return dataChecksums;
        }

        #region Threading helpers
        struct AdlerPacket
        {
            public Adler32Context Context;
            public byte[] Data;
        }

        struct Crc16Packet
        {
            public Crc16Context Context;
            public byte[] Data;
        }

        struct Crc32Packet
        {
            public Crc32Context Context;
            public byte[] Data;
        }

        struct Crc64Packet
        {
            public Crc64Context Context;
            public byte[] Data;
        }

        struct Md5Packet
        {
            public Md5Context Context;
            public byte[] Data;
        }

        struct Ripemd160Packet
        {
            public Ripemd160Context Context;
            public byte[] Data;
        }

        struct Sha1Packet
        {
            public Sha1Context Context;
            public byte[] Data;
        }

        struct Sha256Packet
        {
            public Sha256Context Context;
            public byte[] Data;
        }

        struct Sha384Packet
        {
            public Sha384Context Context;
            public byte[] Data;
        }

        struct Sha512Packet
        {
            public Sha512Context Context;
            public byte[] Data;
        }

        struct SpamsumPacket
        {
            public SpamSumContext Context;
            public byte[] Data;
        }

        static void UpdateAdler(object packet)
        {
            ((AdlerPacket)packet).Context.Update(((AdlerPacket)packet).Data);
        }

        static void UpdateCrc16(object packet)
        {
            ((Crc16Packet)packet).Context.Update(((Crc16Packet)packet).Data);
        }

        static void UpdateCrc32(object packet)
        {
            ((Crc32Packet)packet).Context.Update(((Crc32Packet)packet).Data);
        }

        static void UpdateCrc64(object packet)
        {
            ((Crc64Packet)packet).Context.Update(((Crc64Packet)packet).Data);
        }

        static void UpdateMd5(object packet)
        {
            ((Md5Packet)packet).Context.Update(((Md5Packet)packet).Data);
        }

        static void UpdateRipemd160(object packet)
        {
            ((Ripemd160Packet)packet).Context.Update(((Ripemd160Packet)packet).Data);
        }

        static void UpdateSha1(object packet)
        {
            ((Sha1Packet)packet).Context.Update(((Sha1Packet)packet).Data);
        }

        static void UpdateSha256(object packet)
        {
            ((Sha256Packet)packet).Context.Update(((Sha256Packet)packet).Data);
        }

        static void UpdateSha384(object packet)
        {
            ((Sha384Packet)packet).Context.Update(((Sha384Packet)packet).Data);
        }

        static void UpdateSha512(object packet)
        {
            ((Sha512Packet)packet).Context.Update(((Sha512Packet)packet).Data);
        }

        static void UpdateSpamSum(object packet)
        {
            ((SpamsumPacket)packet).Context.Update(((SpamsumPacket)packet).Data);
        }
        #endregion Threading helpers
    }
}