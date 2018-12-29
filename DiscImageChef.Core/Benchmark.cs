// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Benchmark.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Benchmarks DiscImageChef hashing and checksumming speeds.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.Core
{
    public struct BenchmarkResults
    {
        public double                             FillTime;
        public double                             FillSpeed;
        public double                             ReadTime;
        public double                             ReadSpeed;
        public double                             EntropyTime;
        public double                             EntropySpeed;
        public Dictionary<string, BenchmarkEntry> Entries;
        public long                               MinMemory;
        public long                               MaxMemory;
        public double                             SeparateTime;
        public double                             SeparateSpeed;
        public double                             TotalTime;
        public double                             TotalSpeed;
    }

    public struct BenchmarkEntry
    {
        public double TimeSpan;
        public double Speed;
    }

    /// <summary>
    ///     Benchmarks the speed at which we can do checksums
    /// </summary>
    public static class Benchmark
    {
        public static event InitProgressHandler   InitProgressEvent;
        public static event UpdateProgressHandler UpdateProgressEvent;
        public static event EndProgressHandler    EndProgressEvent;

        static void InitProgress()
        {
            InitProgressEvent?.Invoke();
        }

        static void UpdateProgress(string text, int current, int maximum)
        {
            UpdateProgressEvent?.Invoke(string.Format(text, current, maximum), current, maximum);
        }

        static void EndProgress()
        {
            EndProgressEvent?.Invoke();
        }

        public static BenchmarkResults Do(int bufferSize, int blockSize)
        {
            BenchmarkResults results = new BenchmarkResults
            {
                Entries      = new Dictionary<string, BenchmarkEntry>(),
                MinMemory    = long.MaxValue,
                MaxMemory    = 0,
                SeparateTime = 0
            };
            MemoryStream ms  = new MemoryStream(bufferSize);
            Random       rnd = new Random();
            DateTime     start;
            DateTime     end;

            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Writing block {0} of {1} with random data.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                rnd.NextBytes(tmp);
                ms.Write(tmp, 0, blockSize);
            }

            EndProgress();
            end = DateTime.Now;

            results.FillTime  = (end - start).TotalSeconds;
            results.FillSpeed = bufferSize / 1048576.0 / (end - start).TotalSeconds;

            ms.Seek(0, SeekOrigin.Begin);
            long mem                                      = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Reading block {0} of {1}.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
            }

            EndProgress();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.ReadTime  = (end - start).TotalSeconds;
            results.ReadSpeed = bufferSize / 1048576.0 / (end - start).TotalSeconds;

            #region Adler32
            IChecksum ctx = new Adler32Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with Adler32.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("Adler32",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion Adler32

            #region Fletcher16
            ctx = new Fletcher16Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with Fletcher-16.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("Fletcher16",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion Fletcher16

            #region Fletcher32
            ctx = new Fletcher32Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with Fletcher-32.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("Fletcher32",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion Fletcher32

            #region CRC16
            ctx = new Crc16Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with CRC16.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("CRC16",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion CRC16

            #region CRC32
            ctx = new Crc32Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with CRC32.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("CRC32",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion CRC32

            #region CRC64
            ctx = new Crc64Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with CRC64.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("CRC64",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion CRC64

            #region MD5
            ctx = new Md5Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with MD5.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("MD5",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion MD5

#if !NETSTANDARD2_0
            #region RIPEMD160
            ctx = new Ripemd160Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with RIPEMD160.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("RIPEMD160",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion RIPEMD160
#endif

            #region SHA1
            ctx = new Sha1Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SHA1.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SHA1",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SHA1

            #region SHA256
            ctx = new Sha256Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SHA256.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SHA256",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SHA256

            #region SHA384
            ctx = new Sha384Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SHA384.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SHA384",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SHA384

            #region SHA512
            ctx = new Sha512Context();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SHA512.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SHA512",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SHA512

            #region SpamSum
            ctx = new SpamSumContext();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SpamSum.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ctx.Update(tmp);
            }

            EndProgress();
            ctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SpamSum",
                                new BenchmarkEntry
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed    = bufferSize / 1048576.0 / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SpamSum

            #region Entropy
            ulong[] entTable = new ulong[256];
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Entropying block {0} of {1}.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                foreach(byte b in tmp) entTable[b]++;
            }

            EndProgress();

            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.EntropyTime  = (end - start).TotalSeconds;
            results.EntropySpeed = bufferSize / 1048576.0 / (end - start).TotalSeconds;
            #endregion Entropy

            /*
            #region Multitasking
            start                 = DateTime.Now;
            Checksum allChecksums = new Checksum();
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with all algorithms at the same time.", i + 1,
                               bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);

                allChecksums.Update(tmp);
            }

            EndProgress();

            allChecksums.End();
            end                                           = DateTime.Now;
            mem                                           = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.TotalTime  = (end - start).TotalSeconds;
            results.TotalSpeed = bufferSize / 1048576.0 / results.TotalTime;
            #endregion
            */
            results.SeparateSpeed = bufferSize / 1048576.0 / results.SeparateTime;

            return results;
        }
    }
}