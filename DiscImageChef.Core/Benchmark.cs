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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.Checksums;

namespace DiscImageChef.Core
{
    public struct BenchmarkResults
    {
        public double FillTime;
        public double FillSpeed;
        public double ReadTime;
        public double ReadSpeed;
        public double EntropyTime;
        public double EntropySpeed;
        public Dictionary<string, BenchmarkEntry> Entries;
        public long MinMemory;
        public long MaxMemory;
        public double SeparateTime;
        public double SeparateSpeed;
        public double TotalTime;
        public double TotalSpeed;
    }

    public struct BenchmarkEntry
    {
        public double TimeSpan;
        public double Speed;
    }

    public static class Benchmark
    {
        public static event InitProgressHandler InitProgressEvent;
        public static event UpdateProgressHandler UpdateProgressEvent;
        public static event EndProgressHandler EndProgressEvent;

        public static void InitProgress()
        {
            if(InitProgressEvent != null) InitProgressEvent();
        }

        public static void UpdateProgress(string text, int current, int maximum)
        {
            if(UpdateProgressEvent != null)
                UpdateProgressEvent(string.Format(text, current, maximum), current, maximum);
        }

        public static void EndProgress()
        {
            if(EndProgressEvent != null) EndProgressEvent();
        }

        public static BenchmarkResults Do(int bufferSize, int blockSize)
        {
            BenchmarkResults results = new BenchmarkResults();
            results.Entries = new Dictionary<string, BenchmarkEntry>();
            results.MinMemory = long.MaxValue;
            results.MaxMemory = 0;
            results.SeparateTime = 0;
            MemoryStream ms = new MemoryStream(bufferSize);
            Random rnd = new Random();
            DateTime start;
            DateTime end;
            long mem;
            object ctx;

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

            results.FillTime = (end - start).TotalSeconds;
            results.FillSpeed = (bufferSize / 1048576) / (end - start).TotalSeconds;

            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
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

            results.ReadTime = (end - start).TotalSeconds;
            results.ReadSpeed = (bufferSize / 1048576) / (end - start).TotalSeconds;

            #region Adler32
            ctx = new Adler32Context();
            ((Adler32Context)ctx).Init();
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
                ((Adler32Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Adler32Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("Adler32",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion Adler32

            #region CRC16
            ctx = new Crc16Context();
            ((Crc16Context)ctx).Init();
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
                ((Crc16Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Crc16Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("CRC16",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion CRC16

            #region CRC32
            ctx = new Crc32Context();
            ((Crc32Context)ctx).Init();
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
                ((Crc32Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Crc32Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("CRC32",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion CRC32

            #region CRC64
            ctx = new Crc64Context();
            ((Crc64Context)ctx).Init();
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
                ((Crc64Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Crc64Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("CRC64",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion CRC64

            #region MD5
            ctx = new Md5Context();
            ((Md5Context)ctx).Init();
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
                ((Md5Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Md5Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("MD5",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion MD5

            #region RIPEMD160
            ctx = new Ripemd160Context();
            ((Ripemd160Context)ctx).Init();
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
                ((Ripemd160Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Ripemd160Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("RIPEMD160",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion RIPEMD160

            #region SHA1
            ctx = new Sha1Context();
            ((Sha1Context)ctx).Init();
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
                ((Sha1Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Sha1Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SHA1",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SHA1

            #region SHA256
            ctx = new Sha256Context();
            ((Sha256Context)ctx).Init();
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
                ((Sha256Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Sha256Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SHA256",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SHA256

            #region SHA384
            ctx = new Sha384Context();
            ((Sha384Context)ctx).Init();
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
                ((Sha384Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Sha384Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SHA384",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SHA384

            #region SHA512
            ctx = new Sha512Context();
            ((Sha512Context)ctx).Init();
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
                ((Sha512Context)ctx).Update(tmp);
            }

            EndProgress();
            ((Sha512Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SHA512",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
                                });
            results.SeparateTime += (end - start).TotalSeconds;
            #endregion SHA512

            #region SpamSum
            ctx = new SpamSumContext();
            ((SpamSumContext)ctx).Init();
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
                ((SpamSumContext)ctx).Update(tmp);
            }

            EndProgress();
            ((SpamSumContext)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.Entries.Add("SpamSum",
                                new BenchmarkEntry()
                                {
                                    TimeSpan = (end - start).TotalSeconds,
                                    Speed = (bufferSize / 1048576) / (end - start).TotalSeconds
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
            double entropy = 0;
            foreach(ulong l in entTable)
            {
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                double frequency = (double)l / (double)bufferSize;
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
                entropy += -(frequency * Math.Log(frequency, 2));
            }

            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.EntropyTime = (end - start).TotalSeconds;
            results.EntropySpeed = (bufferSize / 1048576) / (end - start).TotalSeconds;
            #endregion Entropy

            #region Multitasking
            start = DateTime.Now;
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
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.MaxMemory) results.MaxMemory = mem;
            if(mem < results.MinMemory) results.MinMemory = mem;

            results.TotalTime = (end - start).TotalSeconds;
            results.TotalSpeed = (bufferSize / 1048576) / results.TotalTime;
            #endregion

            results.SeparateSpeed = (bufferSize / 1048576) / results.SeparateTime;

            return results;
        }
    }
}