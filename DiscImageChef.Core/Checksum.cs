// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Checksum.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core methods.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to checksum data.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.Checksums;
using Schemas;
using System.Threading;
using System;

namespace DiscImageChef.Core
{
    [Flags]
    public enum EnableChecksum
    {
        Adler32 = 1,
        CRC16 = 2,
        CRC32 = 4,
        CRC64 = 8,
        MD5 = 16,
        RIPEMD160 = 32,
        SHA1 = 64,
        SHA256 = 128,
        SHA384 = 256,
        SHA512 = 512,
        SpamSum = 1024,
        All = Adler32 | CRC16 | CRC32 | CRC64 | MD5 | RIPEMD160 | SHA1 | SHA256 | SHA384 | SHA512 | SpamSum
    }

    public class Checksum
    {
        Adler32Context adler32ctx;
        CRC16Context crc16ctx;
        CRC32Context crc32ctx;
        CRC64Context crc64ctx;
        MD5Context md5ctx;
        RIPEMD160Context ripemd160ctx;
        SHA1Context sha1ctx;
        SHA256Context sha256ctx;
        SHA384Context sha384ctx;
        SHA512Context sha512ctx;
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

        adlerPacket adlerPkt;
        crc16Packet crc16Pkt;
        crc32Packet crc32Pkt;
        crc64Packet crc64Pkt;
        md5Packet md5Pkt;
        ripemd160Packet ripemd160Pkt;
        sha1Packet sha1Pkt;
        sha256Packet sha256Pkt;
        sha384Packet sha384Pkt;
        sha512Packet sha512Pkt;
        spamsumPacket spamsumPkt;

        EnableChecksum enabled;

        public Checksum(EnableChecksum enabled = EnableChecksum.All)
        {
            this.enabled = enabled;

            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                adler32ctx = new Adler32Context();
                adlerPkt = new adlerPacket();
                adler32ctx.Init();
                adlerPkt.context = adler32ctx;
            }

            if(enabled.HasFlag(EnableChecksum.CRC16))
            {
                crc16ctx = new CRC16Context();
                crc16Pkt = new crc16Packet();
                crc16ctx.Init();
                crc16Pkt.context = crc16ctx;
            }

            if(enabled.HasFlag(EnableChecksum.CRC32))
            {
                crc32ctx = new CRC32Context();
                crc32Pkt = new crc32Packet();
                crc32ctx.Init();
                crc32Pkt.context = crc32ctx;
            }

            if(enabled.HasFlag(EnableChecksum.CRC64))
            {
                crc64ctx = new CRC64Context();
                crc64Pkt = new crc64Packet();
                crc64ctx.Init();
                crc64Pkt.context = crc64ctx;
            }

            if(enabled.HasFlag(EnableChecksum.MD5))
            {
                md5ctx = new MD5Context();
                md5Pkt = new md5Packet();
                md5ctx.Init();
                md5Pkt.context = md5ctx;
            }

            if(enabled.HasFlag(EnableChecksum.RIPEMD160))
            {
                ripemd160ctx = new RIPEMD160Context();
                ripemd160Pkt = new ripemd160Packet();
                ripemd160ctx.Init();
                ripemd160Pkt.context = ripemd160ctx;
            }

            if(enabled.HasFlag(EnableChecksum.SHA1))
            {
                sha1ctx = new SHA1Context();
                sha1Pkt = new sha1Packet();
                sha1ctx.Init();
                sha1Pkt.context = sha1ctx;
            }

            if(enabled.HasFlag(EnableChecksum.SHA256))
            {
                sha256ctx = new SHA256Context();
                sha256Pkt = new sha256Packet();
                sha256ctx.Init();
                sha256Pkt.context = sha256ctx;
            }

            if(enabled.HasFlag(EnableChecksum.SHA384))
            {
                sha384ctx = new SHA384Context();
                sha384Pkt = new sha384Packet();
                sha384ctx.Init();
                sha384Pkt.context = sha384ctx;
            }

            if(enabled.HasFlag(EnableChecksum.SHA512))
            {
                sha512ctx = new SHA512Context();
                sha512Pkt = new sha512Packet();
                sha512ctx.Init();
                sha512Pkt.context = sha512ctx;
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                ssctx = new SpamSumContext();
                spamsumPkt = new spamsumPacket();
                ssctx.Init();
                spamsumPkt.context = ssctx;
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

        public void Update(byte[] data)
        {
            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                adlerPkt.data = data;
                adlerThread.Start(adlerPkt);
            }

            if(enabled.HasFlag(EnableChecksum.CRC16))
            {
                crc16Pkt.data = data;
                crc16Thread.Start(crc16Pkt);
            }
            if(enabled.HasFlag(EnableChecksum.CRC32))
            {
                crc32Pkt.data = data;
                crc32Thread.Start(crc32Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.CRC64))
            {
                crc64Pkt.data = data;
                crc64Thread.Start(crc64Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.MD5))
            {
                md5Pkt.data = data;
                md5Thread.Start(md5Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.RIPEMD160))
            {
                ripemd160Pkt.data = data;
                ripemd160Thread.Start(ripemd160Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.SHA1))
            {
                sha1Pkt.data = data;
                sha1Thread.Start(sha1Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.SHA256))
            {
                sha256Pkt.data = data;
                sha256Thread.Start(sha256Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.SHA384))
            {
                sha384Pkt.data = data;
                sha384Thread.Start(sha384Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.SHA512))
            {
                sha512Pkt.data = data;
                sha512Thread.Start(sha512Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                spamsumPkt.data = data;
                spamsumThread.Start(spamsumPkt);
            }

            while(adlerThread.IsAlive || crc16Thread.IsAlive ||
                   crc32Thread.IsAlive || crc64Thread.IsAlive ||
                   md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                   sha1Thread.IsAlive || sha256Thread.IsAlive ||
                   sha384Thread.IsAlive || sha512Thread.IsAlive ||
                   spamsumThread.IsAlive)
            {
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                adlerThread = new Thread(updateAdler);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                crc16Thread = new Thread(updateCRC16);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                crc32Thread = new Thread(updateCRC32);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                crc64Thread = new Thread(updateCRC64);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                md5Thread = new Thread(updateMD5);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                ripemd160Thread = new Thread(updateRIPEMD160);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                sha1Thread = new Thread(updateSHA1);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                sha256Thread = new Thread(updateSHA256);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                sha384Thread = new Thread(updateSHA384);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                sha512Thread = new Thread(updateSHA512);
            if(enabled.HasFlag(EnableChecksum.SpamSum))
                spamsumThread = new Thread(updateSpamSum);
        }

        public List<ChecksumType> End()
        {
            List<ChecksumType> chks = new List<ChecksumType>();

            ChecksumType chk;

            if(enabled.HasFlag(EnableChecksum.All))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.adler32;
                chk.Value = adler32ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.CRC16))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc16;
                chk.Value = crc16ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.CRC32))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc32;
                chk.Value = crc32ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.CRC64))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc64;
                chk.Value = crc64ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.MD5))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.md5;
                chk.Value = md5ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.RIPEMD160))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.ripemd160;
                chk.Value = ripemd160ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SHA1))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha1;
                chk.Value = sha1ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SHA256))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha256;
                chk.Value = sha256ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SHA384))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha384;
                chk.Value = sha384ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SHA512))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha512;
                chk.Value = sha512ctx.End();
                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.spamsum;
                chk.Value = ssctx.End();
                chks.Add(chk);
            }

            return chks;
        }

        public static List<ChecksumType> GetChecksums(byte[] data, EnableChecksum enabled = EnableChecksum.All)
        {
            Adler32Context adler32ctxData = null;
            CRC16Context crc16ctxData = null;
            CRC32Context crc32ctxData = null;
            CRC64Context crc64ctxData = null;
            MD5Context md5ctxData = null;
            RIPEMD160Context ripemd160ctxData = null;
            SHA1Context sha1ctxData = null;
            SHA256Context sha256ctxData = null;
            SHA384Context sha384ctxData = null;
            SHA512Context sha512ctxData = null;
            SpamSumContext ssctxData = null;

            adlerPacket adlerPktData;
            crc16Packet crc16PktData;
            crc32Packet crc32PktData;
            crc64Packet crc64PktData;
            md5Packet md5PktData;
            ripemd160Packet ripemd160PktData;
            sha1Packet sha1PktData;
            sha256Packet sha256PktData;
            sha384Packet sha384PktData;
            sha512Packet sha512PktData;
            spamsumPacket spamsumPktData;

            Thread adlerThreadData = new Thread(updateAdler);
            Thread crc16ThreadData = new Thread(updateCRC16);
            Thread crc32ThreadData = new Thread(updateCRC32);
            Thread crc64ThreadData = new Thread(updateCRC64);
            Thread md5ThreadData = new Thread(updateMD5);
            Thread ripemd160ThreadData = new Thread(updateRIPEMD160);
            Thread sha1ThreadData = new Thread(updateSHA1);
            Thread sha256ThreadData = new Thread(updateSHA256);
            Thread sha384ThreadData = new Thread(updateSHA384);
            Thread sha512ThreadData = new Thread(updateSHA512);
            Thread spamsumThreadData = new Thread(updateSpamSum);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                adler32ctxData = new Adler32Context();
                adlerPktData = new adlerPacket();
                adler32ctxData.Init();
                adlerPktData.context = adler32ctxData;
                adlerPktData.data = data;
                adlerThreadData.Start(adlerPktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                crc16PktData = new crc16Packet();
                crc16ctxData = new CRC16Context();
                crc16ctxData.Init();
                crc16PktData.context = crc16ctxData;
                crc16PktData.data = data;
                crc16ThreadData.Start(crc16PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                crc32PktData = new crc32Packet();
                crc32ctxData = new CRC32Context();
                crc32ctxData.Init();
                crc32PktData.context = crc32ctxData;
                crc32PktData.data = data;
                crc32ThreadData.Start(crc32PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                crc64PktData = new crc64Packet();
                crc64ctxData = new CRC64Context();
                crc64ctxData.Init();
                crc64PktData.context = crc64ctxData;
                crc64PktData.data = data;
                crc64ThreadData.Start(crc64PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                md5PktData = new md5Packet();
                md5ctxData = new MD5Context();
                md5ctxData.Init();
                md5PktData.context = md5ctxData;
                md5PktData.data = data;
                md5ThreadData.Start(md5PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                ripemd160PktData = new ripemd160Packet();
                ripemd160ctxData = new RIPEMD160Context();
                ripemd160ctxData.Init();
                ripemd160PktData.context = ripemd160ctxData;
                ripemd160PktData.data = data;
                ripemd160ThreadData.Start(ripemd160PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                sha1PktData = new sha1Packet();
                sha1ctxData = new SHA1Context();
                sha1ctxData.Init();
                sha1PktData.context = sha1ctxData;
                sha1PktData.data = data;
                sha1ThreadData.Start(sha1PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                sha256PktData = new sha256Packet();
                sha256ctxData = new SHA256Context();
                sha256ctxData.Init();
                sha256PktData.context = sha256ctxData;
                sha256PktData.data = data;
                sha256ThreadData.Start(sha256PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                sha384PktData = new sha384Packet();
                sha384ctxData = new SHA384Context();
                sha384ctxData.Init();
                sha384PktData.context = sha384ctxData;
                sha384PktData.data = data;
                sha384ThreadData.Start(sha384PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                sha512PktData = new sha512Packet();
                sha512ctxData = new SHA512Context();
                sha512ctxData.Init();
                sha512PktData.context = sha512ctxData;
                sha512PktData.data = data;
                sha512ThreadData.Start(sha512PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                spamsumPktData = new spamsumPacket();
                ssctxData = new SpamSumContext();
                ssctxData.Init();
                spamsumPktData.context = ssctxData;
                spamsumPktData.data = data;
                spamsumThreadData.Start(spamsumPktData);
            }


            while(adlerThreadData.IsAlive || crc16ThreadData.IsAlive ||
                crc32ThreadData.IsAlive || crc64ThreadData.IsAlive ||
                md5ThreadData.IsAlive || ripemd160ThreadData.IsAlive ||
                sha1ThreadData.IsAlive || sha256ThreadData.IsAlive ||
                sha384ThreadData.IsAlive || sha512ThreadData.IsAlive ||
                spamsumThreadData.IsAlive)
            {
            }

            List<ChecksumType> dataChecksums = new List<ChecksumType>();
            ChecksumType chk;

            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.adler32;
                chk.Value = adler32ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.CRC16))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc16;
                chk.Value = crc16ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.CRC32))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc32;
                chk.Value = crc32ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.CRC64))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.crc64;
                chk.Value = crc64ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.MD5))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.md5;
                chk.Value = md5ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.RIPEMD160))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.ripemd160;
                chk.Value = ripemd160ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SHA1))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha1;
                chk.Value = sha1ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SHA256))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha256;
                chk.Value = sha256ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SHA384))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha384;
                chk.Value = sha384ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SHA512))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.sha512;
                chk.Value = sha512ctxData.End();
                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                chk = new ChecksumType();
                chk.type = ChecksumTypeType.spamsum;
                chk.Value = ssctxData.End();
                dataChecksums.Add(chk);
            }

            return dataChecksums;
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