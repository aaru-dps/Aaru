// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using Aaru.Checksums;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Core;

/// <summary>Enabled checksums</summary>
[Flags]
public enum EnableChecksum
{
    /// <summary>Enables Adler-32</summary>
    Adler32 = 1,
    /// <summary>Enables CRC-16</summary>
    Crc16 = 2,
    /// <summary>Enables CRC-32</summary>
    Crc32 = 4,
    /// <summary>Enables CRC-64</summary>
    Crc64 = 8,
    /// <summary>Enables MD5</summary>
    Md5 = 16,
    /// <summary>Enables SHA1</summary>
    Sha1 = 64,
    /// <summary>Enables SHA2-256</summary>
    Sha256 = 128,
    /// <summary>Enables SHA2-384</summary>
    Sha384 = 256,
    /// <summary>Enables SHA2-512</summary>
    Sha512 = 512,
    /// <summary>Enables SpamSum</summary>
    SpamSum = 1024,
    /// <summary>Enables Fletcher-16</summary>
    Fletcher16 = 2048,
    /// <summary>Enables Fletcher-32</summary>
    Fletcher32 = 4096,
    /// <summary>Enables all known checksums</summary>
    All = Adler32 | Crc16 | Crc32 | Crc64 | Md5 | Sha1 | Sha256 | Sha384 | Sha512 | SpamSum | Fletcher16 | Fletcher32
}

/// <summary>Checksums and hashes data, with different algorithms, multithreaded</summary>
public sealed class Checksum
{
    readonly IChecksum      _adler32Ctx;
    readonly IChecksum      _crc16Ctx;
    readonly IChecksum      _crc32Ctx;
    readonly IChecksum      _crc64Ctx;
    readonly EnableChecksum _enabled;
    readonly IChecksum      _f16Ctx;
    readonly IChecksum      _f32Ctx;
    readonly IChecksum      _md5Ctx;
    readonly IChecksum      _sha1Ctx;
    readonly IChecksum      _sha256Ctx;
    readonly IChecksum      _sha384Ctx;
    readonly IChecksum      _sha512Ctx;
    readonly IChecksum      _ssCtx;
    HashPacket              _adlerPkt;
    Thread                  _adlerThread;
    HashPacket              _crc16Pkt;
    Thread                  _crc16Thread;
    HashPacket              _crc32Pkt;
    Thread                  _crc32Thread;
    HashPacket              _crc64Pkt;
    Thread                  _crc64Thread;
    HashPacket              _f16Pkt;
    Thread                  _f16Thread;
    HashPacket              _f32Pkt;
    Thread                  _f32Thread;
    HashPacket              _md5Pkt;
    Thread                  _md5Thread;
    HashPacket              _sha1Pkt;
    Thread                  _sha1Thread;
    HashPacket              _sha256Pkt;
    Thread                  _sha256Thread;
    HashPacket              _sha384Pkt;
    Thread                  _sha384Thread;
    HashPacket              _sha512Pkt;
    Thread                  _sha512Thread;
    HashPacket              _spamsumPkt;
    Thread                  _spamsumThread;

    /// <summary>Initializes an instance of the checksum operations</summary>
    /// <param name="enabled">Enabled checksums</param>
    public Checksum(EnableChecksum enabled = EnableChecksum.All)
    {
        _enabled = enabled;

        if(enabled.HasFlag(EnableChecksum.Adler32))
        {
            _adler32Ctx = new Adler32Context();

            _adlerPkt = new HashPacket
            {
                Context = _adler32Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Crc16))
        {
            _crc16Ctx = new CRC16IBMContext();

            _crc16Pkt = new HashPacket
            {
                Context = _crc16Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Crc32))
        {
            _crc32Ctx = new Crc32Context();

            _crc32Pkt = new HashPacket
            {
                Context = _crc32Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Crc64))
        {
            _crc64Ctx = new Crc64Context();

            _crc64Pkt = new HashPacket
            {
                Context = _crc64Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Md5))
        {
            _md5Ctx = new Md5Context();

            _md5Pkt = new HashPacket
            {
                Context = _md5Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Sha1))
        {
            _sha1Ctx = new Sha1Context();

            _sha1Pkt = new HashPacket
            {
                Context = _sha1Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Sha256))
        {
            _sha256Ctx = new Sha256Context();

            _sha256Pkt = new HashPacket
            {
                Context = _sha256Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Sha384))
        {
            _sha384Ctx = new Sha384Context();

            _sha384Pkt = new HashPacket
            {
                Context = _sha384Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Sha512))
        {
            _sha512Ctx = new Sha512Context();

            _sha512Pkt = new HashPacket
            {
                Context = _sha512Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.SpamSum))
        {
            _ssCtx = new SpamSumContext();

            _spamsumPkt = new HashPacket
            {
                Context = _ssCtx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Fletcher16))
        {
            _f16Ctx = new Fletcher16Context();

            _f16Pkt = new HashPacket
            {
                Context = _f16Ctx
            };
        }

        if(enabled.HasFlag(EnableChecksum.Fletcher32))
        {
            _f32Ctx = new Fletcher32Context();

            _f32Pkt = new HashPacket
            {
                Context = _f32Ctx
            };
        }

        _adlerThread   = new Thread(UpdateHash);
        _crc16Thread   = new Thread(UpdateHash);
        _crc32Thread   = new Thread(UpdateHash);
        _crc64Thread   = new Thread(UpdateHash);
        _md5Thread     = new Thread(UpdateHash);
        _sha1Thread    = new Thread(UpdateHash);
        _sha256Thread  = new Thread(UpdateHash);
        _sha384Thread  = new Thread(UpdateHash);
        _sha512Thread  = new Thread(UpdateHash);
        _spamsumThread = new Thread(UpdateHash);
        _f16Thread     = new Thread(UpdateHash);
        _f32Thread     = new Thread(UpdateHash);
    }

    /// <summary>Updates the checksum with new data</summary>
    /// <param name="data">New data</param>
    public void Update(byte[] data)
    {
        if(_enabled.HasFlag(EnableChecksum.Adler32))
        {
            _adlerPkt.Data = data;
            _adlerThread.Start(_adlerPkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Crc16))
        {
            _crc16Pkt.Data = data;
            _crc16Thread.Start(_crc16Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Crc32))
        {
            _crc32Pkt.Data = data;
            _crc32Thread.Start(_crc32Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Crc64))
        {
            _crc64Pkt.Data = data;
            _crc64Thread.Start(_crc64Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Md5))
        {
            _md5Pkt.Data = data;
            _md5Thread.Start(_md5Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Sha1))
        {
            _sha1Pkt.Data = data;
            _sha1Thread.Start(_sha1Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Sha256))
        {
            _sha256Pkt.Data = data;
            _sha256Thread.Start(_sha256Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Sha384))
        {
            _sha384Pkt.Data = data;
            _sha384Thread.Start(_sha384Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Sha512))
        {
            _sha512Pkt.Data = data;
            _sha512Thread.Start(_sha512Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.SpamSum))
        {
            _spamsumPkt.Data = data;
            _spamsumThread.Start(_spamsumPkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Fletcher16))
        {
            _f16Pkt.Data = data;
            _f16Thread.Start(_f16Pkt);
        }

        if(_enabled.HasFlag(EnableChecksum.Fletcher32))
        {
            _f32Pkt.Data = data;
            _f32Thread.Start(_f32Pkt);
        }

        while(_adlerThread.IsAlive   ||
              _crc16Thread.IsAlive   ||
              _crc32Thread.IsAlive   ||
              _crc64Thread.IsAlive   ||
              _md5Thread.IsAlive     ||
              _sha1Thread.IsAlive    ||
              _sha256Thread.IsAlive  ||
              _sha384Thread.IsAlive  ||
              _sha512Thread.IsAlive  ||
              _spamsumThread.IsAlive ||
              _f16Thread.IsAlive     ||
              _f32Thread.IsAlive) {}

        if(_enabled.HasFlag(EnableChecksum.Adler32))
            _adlerThread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Crc16))
            _crc16Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Crc32))
            _crc32Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Crc16))
            _crc64Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Md5))
            _md5Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Sha1))
            _sha1Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Sha256))
            _sha256Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Sha384))
            _sha384Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Sha512))
            _sha512Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.SpamSum))
            _spamsumThread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Fletcher16))
            _f16Thread = new Thread(UpdateHash);

        if(_enabled.HasFlag(EnableChecksum.Fletcher32))
            _f32Thread = new Thread(UpdateHash);
    }

    /// <summary>Finishes the checksums</summary>
    /// <returns>Returns the checksum results</returns>
    public List<CommonTypes.AaruMetadata.Checksum> End()
    {
        List<CommonTypes.AaruMetadata.Checksum> chks = new();

        if(_enabled.HasFlag(EnableChecksum.Adler32))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Adler32,
                Value = _adler32Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Crc16))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.CRC16,
                Value = _crc16Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Crc32))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.CRC32,
                Value = _crc32Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Crc64))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.CRC64,
                Value = _crc64Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Md5))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Md5,
                Value = _md5Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Sha1))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Sha1,
                Value = _sha1Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Sha256))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Sha256,
                Value = _sha256Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Sha384))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Sha384,
                Value = _sha384Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Sha512))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Sha512,
                Value = _sha512Ctx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.SpamSum))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.SpamSum,
                Value = _ssCtx.End()
            });
        }

        if(_enabled.HasFlag(EnableChecksum.Fletcher16))
        {
            chks.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Fletcher16,
                Value = _f16Ctx.End()
            });
        }

        if(!_enabled.HasFlag(EnableChecksum.Fletcher32))
            return chks;

        chks.Add(new CommonTypes.AaruMetadata.Checksum
        {
            Type  = ChecksumType.Fletcher32,
            Value = _f32Ctx.End()
        });

        return chks;
    }

    internal static List<CommonTypes.AaruMetadata.Checksum> GetChecksums(
        byte[] data, EnableChecksum enabled = EnableChecksum.All)
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
                Context = adler32CtxData,
                Data    = data
            };

            adlerThreadData.Start(adlerPktData);
        }

        if(enabled.HasFlag(EnableChecksum.Crc16))
        {
            crc16CtxData = new CRC16IBMContext();

            var crc16PktData = new HashPacket
            {
                Context = crc16CtxData,
                Data    = data
            };

            crc16ThreadData.Start(crc16PktData);
        }

        if(enabled.HasFlag(EnableChecksum.Crc32))
        {
            crc32CtxData = new Crc32Context();

            var crc32PktData = new HashPacket
            {
                Context = crc32CtxData,
                Data    = data
            };

            crc32ThreadData.Start(crc32PktData);
        }

        if(enabled.HasFlag(EnableChecksum.Crc64))
        {
            crc64CtxData = new Crc64Context();

            var crc64PktData = new HashPacket
            {
                Context = crc64CtxData,
                Data    = data
            };

            crc64ThreadData.Start(crc64PktData);
        }

        if(enabled.HasFlag(EnableChecksum.Md5))
        {
            md5CtxData = new Md5Context();

            var md5PktData = new HashPacket
            {
                Context = md5CtxData,
                Data    = data
            };

            md5ThreadData.Start(md5PktData);
        }

        if(enabled.HasFlag(EnableChecksum.Sha1))
        {
            sha1CtxData = new Sha1Context();

            var sha1PktData = new HashPacket
            {
                Context = sha1CtxData,
                Data    = data
            };

            sha1ThreadData.Start(sha1PktData);
        }

        if(enabled.HasFlag(EnableChecksum.Sha256))
        {
            sha256CtxData = new Sha256Context();

            var sha256PktData = new HashPacket
            {
                Context = sha256CtxData,
                Data    = data
            };

            sha256ThreadData.Start(sha256PktData);
        }

        if(enabled.HasFlag(EnableChecksum.Sha384))
        {
            sha384CtxData = new Sha384Context();

            var sha384PktData = new HashPacket
            {
                Context = sha384CtxData,
                Data    = data
            };

            sha384ThreadData.Start(sha384PktData);
        }

        if(enabled.HasFlag(EnableChecksum.Sha512))
        {
            sha512CtxData = new Sha512Context();

            var sha512PktData = new HashPacket
            {
                Context = sha512CtxData,
                Data    = data
            };

            sha512ThreadData.Start(sha512PktData);
        }

        if(enabled.HasFlag(EnableChecksum.SpamSum))
        {
            ssctxData = new SpamSumContext();

            var spamsumPktData = new HashPacket
            {
                Context = ssctxData,
                Data    = data
            };

            spamsumThreadData.Start(spamsumPktData);
        }

        if(enabled.HasFlag(EnableChecksum.Fletcher16))
        {
            f16CtxData = new Fletcher16Context();

            var f16PktData = new HashPacket
            {
                Context = f16CtxData,
                Data    = data
            };

            f16ThreadData.Start(f16PktData);
        }

        if(enabled.HasFlag(EnableChecksum.Fletcher32))
        {
            f32CtxData = new Fletcher32Context();

            var f32PktData = new HashPacket
            {
                Context = f32CtxData,
                Data    = data
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
              f32ThreadData.IsAlive) {}

        List<CommonTypes.AaruMetadata.Checksum> dataChecksums = new();

        if(enabled.HasFlag(EnableChecksum.Adler32))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Adler32,
                Value = adler32CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Crc16))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.CRC16,
                Value = crc16CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Crc32))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.CRC32,
                Value = crc32CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Crc64))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.CRC64,
                Value = crc64CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Md5))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Md5,
                Value = md5CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Sha1))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Sha1,
                Value = sha1CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Sha256))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Sha256,
                Value = sha256CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Sha384))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Sha384,
                Value = sha384CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Sha512))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Sha512,
                Value = sha512CtxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.SpamSum))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.SpamSum,
                Value = ssctxData.End()
            });
        }

        if(enabled.HasFlag(EnableChecksum.Fletcher16))
        {
            dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
            {
                Type  = ChecksumType.Fletcher16,
                Value = f16CtxData.End()
            });
        }

        if(!enabled.HasFlag(EnableChecksum.Fletcher32))
            return dataChecksums;

        dataChecksums.Add(new CommonTypes.AaruMetadata.Checksum
        {
            Type  = ChecksumType.Fletcher32,
            Value = f32CtxData.End()
        });

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