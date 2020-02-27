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
//     Multithread checksumming and hashing.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using Aaru.Checksums;
using Aaru.CommonTypes.Interfaces;
using Schemas;

namespace Aaru.Core
{
    [Flags]
    public enum EnableChecksum
    {
        Adler32 = 1, Crc16         = 2, Crc32         = 4,
        Crc64   = 8, Md5           = 16, Sha1         = 64,
        Sha256  = 128, Sha384      = 256, Sha512      = 512,
        SpamSum = 1024, Fletcher16 = 2048, Fletcher32 = 4096,
        All = Adler32 | Crc16 | Crc32 | Crc64 | Md5 | Sha1 | Sha256 | Sha384 | Sha512 | SpamSum | Fletcher16 |
              Fletcher32
    }

    /// <summary>Checksums and hashes data, with different algorithms multithreaded</summary>
    public class Checksum
    {
        readonly IChecksum      adler32Ctx;
        HashPacket              adlerPkt;
        Thread                  adlerThread;
        readonly IChecksum      crc16Ctx;
        HashPacket              crc16Pkt;
        Thread                  crc16Thread;
        readonly IChecksum      crc32Ctx;
        HashPacket              crc32Pkt;
        Thread                  crc32Thread;
        readonly IChecksum      crc64Ctx;
        HashPacket              crc64Pkt;
        Thread                  crc64Thread;
        readonly EnableChecksum enabled;
        readonly IChecksum      f16Ctx;
        HashPacket              f16Pkt;
        Thread                  f16Thread;
        readonly IChecksum      f32Ctx;
        HashPacket              f32Pkt;
        Thread                  f32Thread;
        readonly IChecksum      md5Ctx;
        HashPacket              md5Pkt;
        Thread                  md5Thread;
        readonly IChecksum      sha1Ctx;
        HashPacket              sha1Pkt;
        Thread                  sha1Thread;
        readonly IChecksum      sha256Ctx;
        HashPacket              sha256Pkt;
        Thread                  sha256Thread;
        readonly IChecksum      sha384Ctx;
        HashPacket              sha384Pkt;
        Thread                  sha384Thread;
        readonly IChecksum      sha512Ctx;
        HashPacket              sha512Pkt;
        Thread                  sha512Thread;
        HashPacket              spamsumPkt;
        Thread                  spamsumThread;
        readonly IChecksum      ssctx;

        public Checksum(EnableChecksum enabled = EnableChecksum.All)
        {
            this.enabled = enabled;

            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                adler32Ctx = new Adler32Context();

                adlerPkt = new HashPacket
                {
                    Context = adler32Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Crc16))
            {
                crc16Ctx = new CRC16IBMContext();

                crc16Pkt = new HashPacket
                {
                    Context = crc16Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Crc32))
            {
                crc32Ctx = new Crc32Context();

                crc32Pkt = new HashPacket
                {
                    Context = crc32Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Crc64))
            {
                crc64Ctx = new Crc64Context();

                crc64Pkt = new HashPacket
                {
                    Context = crc64Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Md5))
            {
                md5Ctx = new Md5Context();

                md5Pkt = new HashPacket
                {
                    Context = md5Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Sha1))
            {
                sha1Ctx = new Sha1Context();

                sha1Pkt = new HashPacket
                {
                    Context = sha1Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Sha256))
            {
                sha256Ctx = new Sha256Context();

                sha256Pkt = new HashPacket
                {
                    Context = sha256Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Sha384))
            {
                sha384Ctx = new Sha384Context();

                sha384Pkt = new HashPacket
                {
                    Context = sha384Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Sha512))
            {
                sha512Ctx = new Sha512Context();

                sha512Pkt = new HashPacket
                {
                    Context = sha512Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                ssctx = new SpamSumContext();

                spamsumPkt = new HashPacket
                {
                    Context = ssctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Fletcher16))
            {
                f16Ctx = new Fletcher16Context();

                f16Pkt = new HashPacket
                {
                    Context = f16Ctx
                };
            }

            if(enabled.HasFlag(EnableChecksum.Fletcher32))
            {
                f32Ctx = new Fletcher32Context();

                f32Pkt = new HashPacket
                {
                    Context = f32Ctx
                };
            }

            adlerThread   = new Thread(UpdateHash);
            crc16Thread   = new Thread(UpdateHash);
            crc32Thread   = new Thread(UpdateHash);
            crc64Thread   = new Thread(UpdateHash);
            md5Thread     = new Thread(UpdateHash);
            sha1Thread    = new Thread(UpdateHash);
            sha256Thread  = new Thread(UpdateHash);
            sha384Thread  = new Thread(UpdateHash);
            sha512Thread  = new Thread(UpdateHash);
            spamsumThread = new Thread(UpdateHash);
            f16Thread     = new Thread(UpdateHash);
            f32Thread     = new Thread(UpdateHash);
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

            if(enabled.HasFlag(EnableChecksum.Fletcher16))
            {
                f16Pkt.Data = data;
                f16Thread.Start(f16Pkt);
            }

            if(enabled.HasFlag(EnableChecksum.Fletcher32))
            {
                f32Pkt.Data = data;
                f32Thread.Start(f32Pkt);
            }

            while(adlerThread.IsAlive   ||
                  crc16Thread.IsAlive   ||
                  crc32Thread.IsAlive   ||
                  crc64Thread.IsAlive   ||
                  md5Thread.IsAlive     ||
                  sha1Thread.IsAlive    ||
                  sha256Thread.IsAlive  ||
                  sha384Thread.IsAlive  ||
                  sha512Thread.IsAlive  ||
                  spamsumThread.IsAlive ||
                  f16Thread.IsAlive     ||
                  f32Thread.IsAlive) { }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                adlerThread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                crc16Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                crc32Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                crc64Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                md5Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                sha1Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                sha256Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                sha384Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                sha512Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                spamsumThread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                f16Thread = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.SpamSum))
                f32Thread = new Thread(UpdateHash);
        }

        public List<ChecksumType> End()
        {
            List<ChecksumType> chks = new List<ChecksumType>();

            ChecksumType chk;

            if(enabled.HasFlag(EnableChecksum.All))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.adler32, Value = adler32Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc16))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.crc16, Value = crc16Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc32))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.crc32, Value = crc32Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc64))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.crc64, Value = crc64Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Md5))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.md5, Value = md5Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha1))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.sha1, Value = sha1Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha256))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.sha256, Value = sha256Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha384))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.sha384, Value = sha384Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha512))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.sha512, Value = sha512Ctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.spamsum, Value = ssctx.End()
                };

                chks.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Fletcher16))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.fletcher16, Value = f16Ctx.End()
                };

                chks.Add(chk);
            }

            if(!enabled.HasFlag(EnableChecksum.Fletcher32))
                return chks;

            chk = new ChecksumType
            {
                type = ChecksumTypeType.fletcher32, Value = f32Ctx.End()
            };

            chks.Add(chk);

            return chks;
        }

        internal static List<ChecksumType> GetChecksums(byte[] data, EnableChecksum enabled = EnableChecksum.All)
        {
            IChecksum adler32CtxData = null;
            IChecksum crc16CtxData   = null;
            IChecksum crc32CtxData   = null;
            IChecksum crc64CtxData   = null;
            IChecksum md5CtxData     = null;
            IChecksum sha1CtxData    = null;
            IChecksum sha256CtxData  = null;
            IChecksum sha384CtxData  = null;
            IChecksum sha512CtxData  = null;
            IChecksum ssctxData      = null;
            IChecksum f16CtxData     = null;
            IChecksum f32CtxData     = null;

            var adlerThreadData   = new Thread(UpdateHash);
            var crc16ThreadData   = new Thread(UpdateHash);
            var crc32ThreadData   = new Thread(UpdateHash);
            var crc64ThreadData   = new Thread(UpdateHash);
            var md5ThreadData     = new Thread(UpdateHash);
            var sha1ThreadData    = new Thread(UpdateHash);
            var sha256ThreadData  = new Thread(UpdateHash);
            var sha384ThreadData  = new Thread(UpdateHash);
            var sha512ThreadData  = new Thread(UpdateHash);
            var spamsumThreadData = new Thread(UpdateHash);
            var f16ThreadData     = new Thread(UpdateHash);
            var f32ThreadData     = new Thread(UpdateHash);

            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                adler32CtxData = new Adler32Context();

                var adlerPktData = new HashPacket
                {
                    Context = adler32CtxData, Data = data
                };

                adlerThreadData.Start(adlerPktData);
            }

            if(enabled.HasFlag(EnableChecksum.Crc16))
            {
                crc16CtxData = new CRC16IBMContext();

                var crc16PktData = new HashPacket
                {
                    Context = crc16CtxData, Data = data
                };

                crc16ThreadData.Start(crc16PktData);
            }

            if(enabled.HasFlag(EnableChecksum.Crc32))
            {
                crc32CtxData = new Crc32Context();

                var crc32PktData = new HashPacket
                {
                    Context = crc32CtxData, Data = data
                };

                crc32ThreadData.Start(crc32PktData);
            }

            if(enabled.HasFlag(EnableChecksum.Crc64))
            {
                crc64CtxData = new Crc64Context();

                var crc64PktData = new HashPacket
                {
                    Context = crc64CtxData, Data = data
                };

                crc64ThreadData.Start(crc64PktData);
            }

            if(enabled.HasFlag(EnableChecksum.Md5))
            {
                md5CtxData = new Md5Context();

                var md5PktData = new HashPacket
                {
                    Context = md5CtxData, Data = data
                };

                md5ThreadData.Start(md5PktData);
            }

            if(enabled.HasFlag(EnableChecksum.Sha1))
            {
                sha1CtxData = new Sha1Context();

                var sha1PktData = new HashPacket
                {
                    Context = sha1CtxData, Data = data
                };

                sha1ThreadData.Start(sha1PktData);
            }

            if(enabled.HasFlag(EnableChecksum.Sha256))
            {
                sha256CtxData = new Sha256Context();

                var sha256PktData = new HashPacket
                {
                    Context = sha256CtxData, Data = data
                };

                sha256ThreadData.Start(sha256PktData);
            }

            if(enabled.HasFlag(EnableChecksum.Sha384))
            {
                sha384CtxData = new Sha384Context();

                var sha384PktData = new HashPacket
                {
                    Context = sha384CtxData, Data = data
                };

                sha384ThreadData.Start(sha384PktData);
            }

            if(enabled.HasFlag(EnableChecksum.Sha512))
            {
                sha512CtxData = new Sha512Context();

                var sha512PktData = new HashPacket
                {
                    Context = sha512CtxData, Data = data
                };

                sha512ThreadData.Start(sha512PktData);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                ssctxData = new SpamSumContext();

                var spamsumPktData = new HashPacket
                {
                    Context = ssctxData, Data = data
                };

                spamsumThreadData.Start(spamsumPktData);
            }

            if(enabled.HasFlag(EnableChecksum.Fletcher16))
            {
                f16CtxData = new Fletcher16Context();

                var f16PktData = new HashPacket
                {
                    Context = f16CtxData, Data = data
                };

                f16ThreadData.Start(f16PktData);
            }

            if(enabled.HasFlag(EnableChecksum.Fletcher32))
            {
                f32CtxData = new Fletcher32Context();

                var f32PktData = new HashPacket
                {
                    Context = f32CtxData, Data = data
                };

                f32ThreadData.Start(f32PktData);
            }

            while(adlerThreadData.IsAlive   ||
                  crc16ThreadData.IsAlive   ||
                  crc32ThreadData.IsAlive   ||
                  crc64ThreadData.IsAlive   ||
                  md5ThreadData.IsAlive     ||
                  sha1ThreadData.IsAlive    ||
                  sha256ThreadData.IsAlive  ||
                  sha384ThreadData.IsAlive  ||
                  sha512ThreadData.IsAlive  ||
                  spamsumThreadData.IsAlive ||
                  f16ThreadData.IsAlive     ||
                  f32ThreadData.IsAlive) { }

            List<ChecksumType> dataChecksums = new List<ChecksumType>();
            ChecksumType       chk;

            if(enabled.HasFlag(EnableChecksum.Adler32))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.adler32, Value = adler32CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc16))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.crc16, Value = crc16CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc32))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.crc32, Value = crc32CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Crc64))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.crc64, Value = crc64CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Md5))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.md5, Value = md5CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha1))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.sha1, Value = sha1CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha256))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.sha256, Value = sha256CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha384))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.sha384, Value = sha384CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Sha512))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.sha512, Value = sha512CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.SpamSum))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.spamsum, Value = ssctxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Fletcher16))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.fletcher16, Value = f16CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            if(enabled.HasFlag(EnableChecksum.Fletcher32))
            {
                chk = new ChecksumType
                {
                    type = ChecksumTypeType.fletcher32, Value = f32CtxData.End()
                };

                dataChecksums.Add(chk);
            }

            return dataChecksums;
        }

        #region Threading helpers
        struct HashPacket
        {
            public IChecksum Context;
            public byte[]    Data;
        }

        static void UpdateHash(object packet) => ((HashPacket)packet).Context.Update(((HashPacket)packet).Data);
        #endregion Threading helpers
    }
}