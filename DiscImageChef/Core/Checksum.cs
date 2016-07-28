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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.Checksums;
using Schemas;
using System.Threading;
using System.IO;

namespace DiscImageChef.Core
{
    class Checksum
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

        internal Checksum()
        {
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
        }

        internal void Update(byte[] data)
        {
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

            while(adlerThread.IsAlive || crc16Thread.IsAlive ||
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

        internal List<ChecksumType> End()
        {
            List<ChecksumType> Checksums = new List<ChecksumType>();

            ChecksumType chk = new ChecksumType();
            chk.type = ChecksumTypeType.adler32;
            chk.Value = adler32ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.crc16;
            chk.Value = crc16ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.crc32;
            chk.Value = crc32ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.crc64;
            chk.Value = crc64ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.md5;
            chk.Value = md5ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.ripemd160;
            chk.Value = ripemd160ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.sha1;
            chk.Value = sha1ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.sha256;
            chk.Value = sha256ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.sha384;
            chk.Value = sha384ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.sha512;
            chk.Value = sha512ctx.End();
            Checksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.spamsum;
            chk.Value = ssctx.End();
            Checksums.Add(chk);

            return Checksums;
        }

        internal static List<ChecksumType> GetChecksums(byte[] data)
        {
            Adler32Context adler32ctxData = new Adler32Context();
            CRC16Context crc16ctxData = new CRC16Context();
            CRC32Context crc32ctxData = new CRC32Context();
            CRC64Context crc64ctxData = new CRC64Context();
            MD5Context md5ctxData = new MD5Context();
            RIPEMD160Context ripemd160ctxData = new RIPEMD160Context();
            SHA1Context sha1ctxData = new SHA1Context();
            SHA256Context sha256ctxData = new SHA256Context();
            SHA384Context sha384ctxData = new SHA384Context();
            SHA512Context sha512ctxData = new SHA512Context();
            SpamSumContext ssctxData = new SpamSumContext();

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

            adlerPacket adlerPktData = new adlerPacket();
            crc16Packet crc16PktData = new crc16Packet();
            crc32Packet crc32PktData = new crc32Packet();
            crc64Packet crc64PktData = new crc64Packet();
            md5Packet md5PktData = new md5Packet();
            ripemd160Packet ripemd160PktData = new ripemd160Packet();
            sha1Packet sha1PktData = new sha1Packet();
            sha256Packet sha256PktData = new sha256Packet();
            sha384Packet sha384PktData = new sha384Packet();
            sha512Packet sha512PktData = new sha512Packet();
            spamsumPacket spamsumPktData = new spamsumPacket();

            adler32ctxData.Init();
            adlerPktData.context = adler32ctxData;
            crc16ctxData.Init();
            crc16PktData.context = crc16ctxData;
            crc32ctxData.Init();
            crc32PktData.context = crc32ctxData;
            crc64ctxData.Init();
            crc64PktData.context = crc64ctxData;
            md5ctxData.Init();
            md5PktData.context = md5ctxData;
            ripemd160ctxData.Init();
            ripemd160PktData.context = ripemd160ctxData;
            sha1ctxData.Init();
            sha1PktData.context = sha1ctxData;
            sha256ctxData.Init();
            sha256PktData.context = sha256ctxData;
            sha384ctxData.Init();
            sha384PktData.context = sha384ctxData;
            sha512ctxData.Init();
            sha512PktData.context = sha512ctxData;
            ssctxData.Init();
            spamsumPktData.context = ssctxData;

            adlerPktData.data = data;
            adlerThreadData.Start(adlerPktData);
            crc16PktData.data = data;
            crc16ThreadData.Start(crc16PktData);
            crc32PktData.data = data;
            crc32ThreadData.Start(crc32PktData);
            crc64PktData.data = data;
            crc64ThreadData.Start(crc64PktData);
            md5PktData.data = data;
            md5ThreadData.Start(md5PktData);
            ripemd160PktData.data = data;
            ripemd160ThreadData.Start(ripemd160PktData);
            sha1PktData.data = data;
            sha1ThreadData.Start(sha1PktData);
            sha256PktData.data = data;
            sha256ThreadData.Start(sha256PktData);
            sha384PktData.data = data;
            sha384ThreadData.Start(sha384PktData);
            sha512PktData.data = data;
            sha512ThreadData.Start(sha512PktData);
            spamsumPktData.data = data;
            spamsumThreadData.Start(spamsumPktData);

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

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.adler32;
            chk.Value = adler32ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.crc16;
            chk.Value = crc16ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.crc32;
            chk.Value = crc32ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.crc64;
            chk.Value = crc64ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.md5;
            chk.Value = md5ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.ripemd160;
            chk.Value = ripemd160ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.sha1;
            chk.Value = sha1ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.sha256;
            chk.Value = sha256ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.sha384;
            chk.Value = sha384ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.sha512;
            chk.Value = sha512ctxData.End();
            dataChecksums.Add(chk);

            chk = new ChecksumType();
            chk.type = ChecksumTypeType.spamsum;
            chk.Value = ssctxData.End();
            dataChecksums.Add(chk);

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

