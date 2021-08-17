// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ErrorLog.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.Decoders.ATA;
using Aaru.Decoders.SCSI;

namespace Aaru.Core.Logging
{
    /// <summary>
    /// Logs errors
    /// </summary>
    public sealed class ErrorLog
    {
        readonly StreamWriter _logSw;

        /// <summary>Initializes the error log</summary>
        /// <param name="outputFile">Output log file</param>
        public ErrorLog(string outputFile)
        {
            if(string.IsNullOrEmpty(outputFile))
                return;

            _logSw = new StreamWriter(outputFile, true);

            _logSw.WriteLine("Start error logging at {0}", DateTime.Now);
            _logSw.WriteLine("######################################################");
            _logSw.Flush();
        }

        /// <summary>Finishes and closes the error log</summary>
        public void Close()
        {
            _logSw.WriteLine("######################################################");
            _logSw.WriteLine("End logging at {0}", DateTime.Now);
            _logSw.Close();
        }

        /// <summary>Register an ATA error after sending a CHS command</summary>
        /// <param name="command">Command</param>
        /// <param name="osError"><c>true</c> if operating system returned an error status instead of the device</param>
        /// <param name="errno">Operating system error number</param>
        /// <param name="registers">Error registers</param>
        public void WriteLine(string command, bool osError, int errno, AtaErrorRegistersChs registers)
        {
            if(osError)
            {
                _logSw.WriteLine("ATA command {0} operating system error: {1}.", command, errno);
                _logSw.Flush();
            }
            else
            {
                List<string> error  = new List<string>();
                List<string> status = new List<string>();

                if((registers.Status & 0x01) == 0x01)
                    status.Add("ERR");

                if((registers.Status & 0x02) == 0x02)
                    status.Add("IDX");

                if((registers.Status & 0x04) == 0x04)
                    status.Add("CORR");

                if((registers.Status & 0x08) == 0x08)
                    status.Add("DRQ");

                if((registers.Status & 0x10) == 0x10)
                    status.Add("SRV");

                if((registers.Status & 0x20) == 0x20)
                    status.Add("DF");

                if((registers.Status & 0x40) == 0x40)
                    status.Add("RDY");

                if((registers.Status & 0x80) == 0x80)
                    status.Add("BSY");

                if((registers.Error & 0x01) == 0x01)
                    error.Add("AMNF");

                if((registers.Error & 0x02) == 0x02)
                    error.Add("T0NF");

                if((registers.Error & 0x04) == 0x04)
                    error.Add("ABRT");

                if((registers.Error & 0x08) == 0x08)
                    error.Add("MCR");

                if((registers.Error & 0x10) == 0x10)
                    error.Add("IDNF");

                if((registers.Error & 0x20) == 0x20)
                    error.Add("MC");

                if((registers.Error & 0x40) == 0x40)
                    error.Add("UNC");

                if((registers.Error & 0x80) == 0x80)
                    error.Add("BBK");

                _logSw.WriteLine("ATA command {0} error: status = {1}, error = {2}.", command, string.Join(' ', status),
                                 string.Join(' ', error));

                _logSw.Flush();
            }
        }

        /// <summary>Register an ATA error after trying to read using CHS commands</summary>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="head">Head</param>
        /// <param name="sector">Sector</param>
        /// <param name="osError"><c>true</c> if operating system returned an error status instead of the device</param>
        /// <param name="errno">Operating system error number</param>
        /// <param name="registers">Error registers</param>
        public void WriteLine(ushort cylinder, byte head, byte sector, bool osError, int errno,
                              AtaErrorRegistersChs registers)
        {
            if(osError)
            {
                _logSw.WriteLine("ATA reading C/H/S {0}/{1}/{2} operating system error: {3}.", cylinder, head, sector,
                                 errno);

                _logSw.Flush();
            }
            else
            {
                List<string> error  = new List<string>();
                List<string> status = new List<string>();

                if((registers.Status & 0x01) == 0x01)
                    status.Add("ERR");

                if((registers.Status & 0x02) == 0x02)
                    status.Add("IDX");

                if((registers.Status & 0x04) == 0x04)
                    status.Add("CORR");

                if((registers.Status & 0x08) == 0x08)
                    status.Add("DRQ");

                if((registers.Status & 0x10) == 0x10)
                    status.Add("SRV");

                if((registers.Status & 0x20) == 0x20)
                    status.Add("DF");

                if((registers.Status & 0x40) == 0x40)
                    status.Add("RDY");

                if((registers.Status & 0x80) == 0x80)
                    status.Add("BSY");

                if((registers.Error & 0x01) == 0x01)
                    error.Add("AMNF");

                if((registers.Error & 0x02) == 0x02)
                    error.Add("T0NF");

                if((registers.Error & 0x04) == 0x04)
                    error.Add("ABRT");

                if((registers.Error & 0x08) == 0x08)
                    error.Add("MCR");

                if((registers.Error & 0x10) == 0x10)
                    error.Add("IDNF");

                if((registers.Error & 0x20) == 0x20)
                    error.Add("MC");

                if((registers.Error & 0x40) == 0x40)
                    error.Add("UNC");

                if((registers.Error & 0x80) == 0x80)
                    error.Add("BBK");

                _logSw.WriteLine("ATA reading C/H/S {0}/{1}/{2} error: status = {3}, error = {4}.", cylinder, head,
                                 sector, string.Join(' ', status), string.Join(' ', error));

                _logSw.Flush();
            }
        }

        /// <summary>Register an ATA error after trying to read using 28-bit LBA commands</summary>
        /// <param name="block">Starting block</param>
        /// <param name="osError"><c>true</c> if operating system returned an error status instead of the device</param>
        /// <param name="errno">Operating system error number</param>
        /// <param name="registers">Error registers</param>
        public void WriteLine(ulong block, bool osError, int errno, AtaErrorRegistersLba28 registers)
        {
            if(osError)
            {
                _logSw.WriteLine("ATA reading LBA {0} operating system error: {1}.", block, errno);
                _logSw.Flush();
            }
            else
            {
                List<string> error  = new List<string>();
                List<string> status = new List<string>();

                if((registers.Status & 0x01) == 0x01)
                    status.Add("ERR");

                if((registers.Status & 0x02) == 0x02)
                    status.Add("IDX");

                if((registers.Status & 0x04) == 0x04)
                    status.Add("CORR");

                if((registers.Status & 0x08) == 0x08)
                    status.Add("DRQ");

                if((registers.Status & 0x10) == 0x10)
                    status.Add("SRV");

                if((registers.Status & 0x20) == 0x20)
                    status.Add("DF");

                if((registers.Status & 0x40) == 0x40)
                    status.Add("RDY");

                if((registers.Status & 0x80) == 0x80)
                    status.Add("BSY");

                if((registers.Error & 0x01) == 0x01)
                    error.Add("AMNF");

                if((registers.Error & 0x02) == 0x02)
                    error.Add("T0NF");

                if((registers.Error & 0x04) == 0x04)
                    error.Add("ABRT");

                if((registers.Error & 0x08) == 0x08)
                    error.Add("MCR");

                if((registers.Error & 0x10) == 0x10)
                    error.Add("IDNF");

                if((registers.Error & 0x20) == 0x20)
                    error.Add("MC");

                if((registers.Error & 0x40) == 0x40)
                    error.Add("UNC");

                if((registers.Error & 0x80) == 0x80)
                    error.Add("BBK");

                _logSw.WriteLine("ATA reading LBA {0} error: status = {1}, error = {2}.", block,
                                 string.Join(' ', status), string.Join(' ', error));

                _logSw.Flush();
            }
        }

        /// <summary>Register an ATA error after trying to read using 48-bit LBA commands</summary>
        /// <param name="block">Starting block</param>
        /// <param name="osError"><c>true</c> if operating system returned an error status instead of the device</param>
        /// <param name="errno">Operating system error number</param>
        /// <param name="registers">Error registers</param>
        public void WriteLine(ulong block, bool osError, int errno, AtaErrorRegistersLba48 registers)
        {
            if(osError)
            {
                _logSw.WriteLine("ATA reading LBA {0} operating system error: {1}.", block, errno);
                _logSw.Flush();
            }
            else
            {
                List<string> error  = new List<string>();
                List<string> status = new List<string>();

                if((registers.Status & 0x01) == 0x01)
                    status.Add("ERR");

                if((registers.Status & 0x02) == 0x02)
                    status.Add("IDX");

                if((registers.Status & 0x04) == 0x04)
                    status.Add("CORR");

                if((registers.Status & 0x08) == 0x08)
                    status.Add("DRQ");

                if((registers.Status & 0x10) == 0x10)
                    status.Add("SRV");

                if((registers.Status & 0x20) == 0x20)
                    status.Add("DF");

                if((registers.Status & 0x40) == 0x40)
                    status.Add("RDY");

                if((registers.Status & 0x80) == 0x80)
                    status.Add("BSY");

                if((registers.Error & 0x01) == 0x01)
                    error.Add("AMNF");

                if((registers.Error & 0x02) == 0x02)
                    error.Add("T0NF");

                if((registers.Error & 0x04) == 0x04)
                    error.Add("ABRT");

                if((registers.Error & 0x08) == 0x08)
                    error.Add("MCR");

                if((registers.Error & 0x10) == 0x10)
                    error.Add("IDNF");

                if((registers.Error & 0x20) == 0x20)
                    error.Add("MC");

                if((registers.Error & 0x40) == 0x40)
                    error.Add("UNC");

                if((registers.Error & 0x80) == 0x80)
                    error.Add("BBK");

                _logSw.WriteLine("ATA reading LBA {0} error: status = {1}, error = {2}.", block,
                                 string.Join(' ', status), string.Join(' ', error));

                _logSw.Flush();
            }
        }

        /// <summary>Register a SCSI error after sending a command</summary>
        /// <param name="command">Command</param>
        /// <param name="osError"><c>true</c> if operating system returned an error status instead of the device</param>
        /// <param name="errno">Operating system error number</param>
        /// <param name="senseBuffer">REQUEST SENSE response buffer</param>
        public void WriteLine(string command, bool osError, int errno, byte[] senseBuffer)
        {
            if(osError)
            {
                _logSw.WriteLine("SCSI command {0} operating system error: {1}.", command, errno);
                _logSw.Flush();

                return;
            }

            DecodedSense? decodedSense = Sense.Decode(senseBuffer);
            string        prettySense  = Sense.PrettifySense(senseBuffer);
            string        hexSense     = string.Join(' ', senseBuffer.Select(b => $"{b:X2}"));

            if(decodedSense.HasValue)
            {
                if(prettySense != null)
                {
                    if(prettySense.StartsWith("SCSI SENSE: ", StringComparison.Ordinal))
                        prettySense = prettySense.Substring(12);

                    if(prettySense.EndsWith('\n'))
                        prettySense = prettySense.Substring(0, prettySense.Length - 1);

                    prettySense = prettySense.Replace("\n", " - ");

                    _logSw.WriteLine("SCSI command {0} error: SENSE {1} ASC {2:X2}h ASCQ {3:X2}h, {4}, {5}.", command,
                                     decodedSense?.SenseKey, decodedSense?.ASC, decodedSense?.ASCQ, hexSense,
                                     prettySense);
                }
                else
                {
                    _logSw.WriteLine("SCSI command {0} error: SENSE {1} ASC {2:X2}h ASCQ {3:X2}h, {4}.", command,
                                     decodedSense?.SenseKey, decodedSense?.ASC, decodedSense?.ASCQ, hexSense);
                }
            }
            else
            {
                if(prettySense != null)
                {
                    if(prettySense.StartsWith("SCSI SENSE: ", StringComparison.Ordinal))
                        prettySense = prettySense.Substring(12);

                    if(prettySense.EndsWith('\n'))
                        prettySense = prettySense.Substring(0, prettySense.Length - 1);

                    prettySense = prettySense.Replace("\n", " - ");

                    _logSw.WriteLine("SCSI command {0} error: {1}, {2}.", command, hexSense, prettySense);
                }
                else
                {
                    _logSw.WriteLine("SCSI command {0} error: {1}", command, hexSense);
                }
            }

            _logSw.Flush();
        }

        /// <summary>Register an SCSI error after trying to read</summary>
        /// <param name="block">Starting block</param>
        /// <param name="osError"><c>true</c> if operating system returned an error status instead of the device</param>
        /// <param name="errno">Operating system error number</param>
        /// <param name="senseBuffer">REQUEST SENSE response buffer</param>
        public void WriteLine(ulong block, bool osError, int errno, byte[] senseBuffer)
        {
            if(osError)
            {
                _logSw.WriteLine("SCSI reading LBA {0} operating system error: {1}.", block, errno);
                _logSw.Flush();

                if(senseBuffer is null     ||
                   senseBuffer.Length == 0 ||
                   senseBuffer.All(s => s == 0))
                    return;
            }

            DecodedSense? decodedSense = Sense.Decode(senseBuffer);
            string        prettySense  = Sense.PrettifySense(senseBuffer);
            string        hexSense     = string.Join(' ', senseBuffer.Select(b => $"{b:X2}"));

            if(decodedSense.HasValue)
            {
                if(prettySense != null)
                {
                    if(prettySense.StartsWith("SCSI SENSE: ", StringComparison.Ordinal))
                        prettySense = prettySense.Substring(12);

                    if(prettySense.EndsWith('\n'))
                        prettySense = prettySense.Substring(0, prettySense.Length - 1);

                    prettySense = prettySense.Replace("\n", " - ");

                    _logSw.WriteLine("SCSI reading LBA {0} error: SENSE {1} ASC {2:X2}h ASCQ {3:X2}h, {4}, {5}.", block,
                                     decodedSense?.SenseKey, decodedSense?.ASC, decodedSense?.ASCQ, hexSense,
                                     prettySense);
                }
                else
                {
                    _logSw.WriteLine("SCSI reading LBA {0} error: SENSE {1} ASC {2:X2}h ASCQ {3:X2}h, {4}.", block,
                                     decodedSense?.SenseKey, decodedSense?.ASC, decodedSense?.ASCQ, hexSense);
                }
            }
            else
            {
                if(prettySense != null)
                {
                    if(prettySense.StartsWith("SCSI SENSE: ", StringComparison.Ordinal))
                        prettySense = prettySense.Substring(12);

                    if(prettySense.EndsWith('\n'))
                        prettySense = prettySense.Substring(0, prettySense.Length - 1);

                    prettySense = prettySense.Replace("\n", " - ");

                    _logSw.WriteLine("SCSI reading LBA {0} error: {1}, {2}.", block, hexSense, prettySense);
                }
                else
                {
                    _logSw.WriteLine("SCSI reading LBA {0} error: {1}", block, hexSense);
                }
            }

            _logSw.Flush();
        }

        /// <summary>Register a SecureDigital / MultiMediaCard error after sending a command</summary>
        /// <param name="command">Command</param>
        /// <param name="osError"><c>true</c> if operating system returned an error status instead of the device</param>
        /// <param name="errno">Operating system error number</param>
        /// <param name="response">Response</param>
        public void WriteLine(string command, bool osError, int errno, uint[] response)
        {
            if(osError)
            {
                _logSw.WriteLine("SD/MMC command {0} operating system error: {1}.", command, errno);
                _logSw.Flush();

                return;
            }

            // TODO: Decode response
            _logSw.WriteLine("SD/MMC command {0} error: {1}", command,
                             string.Join(" - ", response.Select(r => $"0x{r:X8}")));

            _logSw.Flush();
        }

        /// <summary>Register a SecureDigital / MultiMediaCard error after trying to read</summary>
        /// <param name="block">Starting block</param>
        /// <param name="osError"><c>true</c> if operating system returned an error status instead of the device</param>
        /// <param name="errno">Operating system error number</param>
        /// <param name="byteAddressed">Byte addressed</param>
        /// <param name="response">Response</param>
        public void WriteLine(ulong block, bool osError, int errno, bool byteAddressed, uint[] response)

        {
            if(osError)
            {
                _logSw.WriteLine("SD/MMC reading LBA {0} ({1}-addressed) operating system error: {2}.", block,
                                 byteAddressed ? "byte" : "block", errno);

                _logSw.Flush();

                return;
            }

            _logSw.WriteLine("SD/MMC reading LBA {0} ({1}-addressed) error: {2}", block,
                             byteAddressed ? "byte" : "block", string.Join(" - ", response.Select(r => $"0x{r:X8}")));

            throw new NotImplementedException();
        }
    }
}