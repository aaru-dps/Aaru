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
        public double fillTime;
        public double fillSpeed;
        public double readTime;
        public double readSpeed;
        public double entropyTime;
        public double entropySpeed;
        public Dictionary<string, BenchmarkEntry> entries;
        public long minMemory;
        public long maxMemory;
        public double separateTime;
        public double separateSpeed;
        public double totalTime;
        public double totalSpeed;
    }

    public struct BenchmarkEntry
    {
        public double timeSpan;
        public double speed;
    }

    public static class Benchmark
    {
        public static event InitProgressHandler InitProgressEvent;
        public static event UpdateProgressHandler UpdateProgressEvent;
        public static event EndProgressHandler EndProgressEvent;

        public static void InitProgress()
        {
            if(InitProgressEvent != null)
                InitProgressEvent();
        }

        public static void UpdateProgress(string text, int current, int maximum)
        {
            if(UpdateProgressEvent != null)
                UpdateProgressEvent(string.Format(text, current, maximum), current, maximum);
        }

        public static void EndProgress()
        {
            if(EndProgressEvent != null)
                EndProgressEvent();
        }

        public static BenchmarkResults Do(int bufferSize, int blockSize)
        {
            BenchmarkResults results = new BenchmarkResults();
            results.entries = new Dictionary<string, BenchmarkEntry>();
            results.minMemory = long.MaxValue;
            results.maxMemory = 0;
            results.separateTime = 0;
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

            results.fillTime = (end - start).TotalSeconds;
            results.fillSpeed = (bufferSize / 1048576) / (end - start).TotalSeconds;

            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
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
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.readTime = (end - start).TotalSeconds;
            results.readSpeed = (bufferSize / 1048576) / (end - start).TotalSeconds;

            #region Adler32
            ctx = new Adler32Context();
            ((Adler32Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
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
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("Adler32", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion Adler32

            #region CRC16
            ctx = new CRC16Context();
            ((CRC16Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with CRC16.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((CRC16Context)ctx).Update(tmp);
            }
            EndProgress();
            ((CRC16Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("CRC16", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion CRC16

            #region CRC32
            ctx = new CRC32Context();
            ((CRC32Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with CRC32.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((CRC32Context)ctx).Update(tmp);
            }
            EndProgress();
            ((CRC32Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("CRC32", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion CRC32

            #region CRC64
            ctx = new CRC64Context();
            ((CRC64Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with CRC64.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((CRC64Context)ctx).Update(tmp);
            }
            EndProgress();
            ((CRC64Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("CRC64", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion CRC64

            #region MD5
            ctx = new MD5Context();
            ((MD5Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with MD5.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((MD5Context)ctx).Update(tmp);
            }
            EndProgress();
            ((MD5Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("MD5", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion MD5

            #region RIPEMD160
            ctx = new RIPEMD160Context();
            ((RIPEMD160Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with RIPEMD160.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((RIPEMD160Context)ctx).Update(tmp);
            }
            EndProgress();
            ((RIPEMD160Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("RIPEMD160", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion RIPEMD160

            #region SHA1
            ctx = new SHA1Context();
            ((SHA1Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SHA1.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((SHA1Context)ctx).Update(tmp);
            }
            EndProgress();
            ((SHA1Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("SHA1", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion SHA1

            #region SHA256
            ctx = new SHA256Context();
            ((SHA256Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SHA256.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((SHA256Context)ctx).Update(tmp);
            }
            EndProgress();
            ((SHA256Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("SHA256", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion SHA256

            #region SHA384
            ctx = new SHA384Context();
            ((SHA384Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SHA384.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((SHA384Context)ctx).Update(tmp);
            }
            EndProgress();
            ((SHA384Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("SHA384", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion SHA384

            #region SHA512
            ctx = new SHA512Context();
            ((SHA512Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with SHA512.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                ((SHA512Context)ctx).Update(tmp);
            }
            EndProgress();
            ((SHA512Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("SHA512", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion SHA512

            #region SpamSum
            ctx = new SpamSumContext();
            ((SpamSumContext)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
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
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entries.Add("SpamSum", new BenchmarkEntry() { timeSpan = (end - start).TotalSeconds, speed = (bufferSize / 1048576) / (end - start).TotalSeconds });
            results.separateTime += (end - start).TotalSeconds;
            #endregion SpamSum

            #region Entropy
            ulong[] entTable = new ulong[256];
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;
            start = DateTime.Now;
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Entropying block {0} of {1}.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);
                foreach(byte b in tmp)
                    entTable[b]++;
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
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.entropyTime = (end - start).TotalSeconds;
            results.entropySpeed = (bufferSize / 1048576) / (end - start).TotalSeconds;
            #endregion Entropy

            #region Multitasking
            start = DateTime.Now;
            Checksum allChecksums = new Checksum();
            InitProgress();
            for(int i = 0; i < bufferSize / blockSize; i++)
            {
                UpdateProgress("Checksumming block {0} of {1} with all algorithms at the same time.", i + 1, bufferSize / blockSize);
                byte[] tmp = new byte[blockSize];
                ms.Read(tmp, 0, blockSize);

                allChecksums.Update(tmp);
            }
            EndProgress();

            allChecksums.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > results.maxMemory)
                results.maxMemory = mem;
            if(mem < results.minMemory)
                results.minMemory = mem;

            results.totalTime = (end - start).TotalSeconds;
            results.totalSpeed = (bufferSize / 1048576) / results.totalTime;
            #endregion

            results.separateSpeed = (bufferSize / 1048576) / results.separateTime;

            return results;
        }
    }
}
