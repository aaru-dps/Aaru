// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UploadStatsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles statistics uploads.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml.Serialization;
using DiscImageChef.Metadata;

namespace DiscImageChef.Server.Controllers
{
    public class UploadStatsController : ApiController
    {
        [Route("api/uploadstats")]
        [HttpPost]
        public HttpResponseMessage UploadStats()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;

            try
            {
                Stats newStats = new Stats();
                HttpRequest request = HttpContext.Current.Request;

                XmlSerializer xs = new XmlSerializer(newStats.GetType());
                newStats = (Stats)xs.Deserialize(request.InputStream);

                if(newStats == null)
                {
                    response.Content = new StringContent("notstats", Encoding.UTF8, "text/plain");
                    return response;
                }

                FileStream fs =
                    WaitForFile(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Statistics", "Statistics.xml"),
                                FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                if(fs == null)
                {
                    response.Content = new StringContent("retry", Encoding.UTF8, "text/plain");
                    return response;
                }

                Stats oldStats = new Stats();
                xs = new XmlSerializer(oldStats.GetType());
                oldStats = (Stats)xs.Deserialize(fs);

                if(newStats.Commands != null)
                    if(oldStats.Commands == null) oldStats.Commands = newStats.Commands;
                    else
                    {
                        oldStats.Commands.Analyze += newStats.Commands.Analyze;
                        oldStats.Commands.Benchmark += newStats.Commands.Benchmark;
                        oldStats.Commands.Checksum += newStats.Commands.Checksum;
                        oldStats.Commands.Compare += newStats.Commands.Compare;
                        oldStats.Commands.CreateSidecar += newStats.Commands.CreateSidecar;
                        oldStats.Commands.Decode += newStats.Commands.Decode;
                        oldStats.Commands.DeviceInfo += newStats.Commands.DeviceInfo;
                        oldStats.Commands.DeviceReport += newStats.Commands.DeviceReport;
                        oldStats.Commands.DumpMedia += newStats.Commands.DumpMedia;
                        oldStats.Commands.Entropy += newStats.Commands.Entropy;
                        oldStats.Commands.Formats += newStats.Commands.Formats;
                        oldStats.Commands.MediaInfo += newStats.Commands.MediaInfo;
                        oldStats.Commands.MediaScan += newStats.Commands.MediaScan;
                        oldStats.Commands.PrintHex += newStats.Commands.PrintHex;
                        oldStats.Commands.Verify += newStats.Commands.Verify;
                        oldStats.Commands.Ls += newStats.Commands.Ls;
                        oldStats.Commands.ExtractFiles += newStats.Commands.ExtractFiles;
                        oldStats.Commands.ListDevices += newStats.Commands.ListDevices;
                        oldStats.Commands.ListEncodings += newStats.Commands.ListEncodings;
                    }

                if(newStats.OperatingSystems != null)
                    if(oldStats.OperatingSystems == null) oldStats.OperatingSystems = newStats.OperatingSystems;
                    else
                        foreach(OsStats newNvs in newStats.OperatingSystems)
                        {
                            OsStats removeNvs = null;
                            OsStats addNvs = null;

                            foreach(OsStats oldNvs in oldStats.OperatingSystems.Where(oldNvs => oldNvs.name == newNvs.name && oldNvs.version == newNvs.version)) {
                                addNvs = new OsStats
                                {
                                    name = oldNvs.name,
                                    Value = oldNvs.Value + newNvs.Value,
                                    version = oldNvs.version
                                };
                                removeNvs = oldNvs;
                                break;
                            }

                            if(removeNvs != null)
                            {
                                oldStats.OperatingSystems.Remove(removeNvs);
                                oldStats.OperatingSystems.Add(addNvs);
                            }
                            else oldStats.OperatingSystems.Add(newNvs);
                        }
                else
                {
                    if(oldStats.OperatingSystems == null)
                        oldStats.OperatingSystems =
                            new List<OsStats> {new OsStats {name = "Linux", Value = 1}};
                    else
                    {
                        OsStats removeNvs = null;
                        OsStats addNvs = null;

                        foreach(OsStats oldNvs in oldStats.OperatingSystems.Where(oldNvs => oldNvs.name == "Linux")) {
                            addNvs = new OsStats
                            {
                                name = oldNvs.name,
                                Value = oldNvs.Value + 1,
                                version = oldNvs.version
                            };
                            removeNvs = oldNvs;
                            break;
                        }

                        if(removeNvs != null)
                        {
                            oldStats.OperatingSystems.Remove(removeNvs);
                            oldStats.OperatingSystems.Add(addNvs);
                        }
                        else oldStats.OperatingSystems.Add(new OsStats {name = "Linux", Value = 1});
                    }
                }

                if(newStats.Versions != null)
                    if(oldStats.Versions == null) oldStats.Versions = newStats.Versions;
                    else
                        foreach(NameValueStats newNvs in newStats.Versions)
                        {
                            NameValueStats removeNvs = null;
                            NameValueStats addNvs = null;

                            foreach(NameValueStats oldNvs in oldStats.Versions.Where(oldNvs => oldNvs.name == newNvs.name)) {
                                addNvs = new NameValueStats
                                {
                                    name = oldNvs.name,
                                    Value = oldNvs.Value + newNvs.Value
                                };
                                removeNvs = oldNvs;
                                break;
                            }

                            if(removeNvs != null)
                            {
                                oldStats.Versions.Remove(removeNvs);
                                oldStats.Versions.Add(addNvs);
                            }
                            else oldStats.Versions.Add(newNvs);
                        }
                else
                {
                    if(oldStats.Versions == null)
                        oldStats.Versions =
                            new List<NameValueStats>
                            {
                                new NameValueStats {name = "previous", Value = 1}
                            };
                    else
                    {
                        NameValueStats removeNvs = null;
                        NameValueStats addNvs = null;

                        foreach(NameValueStats oldNvs in oldStats.Versions.Where(oldNvs => oldNvs.name == "previous")) {
                            addNvs = new NameValueStats {name = oldNvs.name, Value = oldNvs.Value + 1};
                            removeNvs = oldNvs;
                            break;
                        }

                        if(removeNvs != null)
                        {
                            oldStats.Versions.Remove(removeNvs);
                            oldStats.Versions.Add(addNvs);
                        }
                        else oldStats.Versions.Add(new NameValueStats {name = "previous", Value = 1});
                    }
                }

                if(newStats.Filesystems != null)
                    if(oldStats.Filesystems == null) oldStats.Filesystems = newStats.Filesystems;
                    else
                        foreach(NameValueStats newNvs in newStats.Filesystems)
                        {
                            NameValueStats removeNvs = null;
                            NameValueStats addNvs = null;

                            foreach(NameValueStats oldNvs in oldStats.Filesystems.Where(oldNvs => oldNvs.name == newNvs.name)) {
                                addNvs = new NameValueStats
                                {
                                    name = oldNvs.name,
                                    Value = oldNvs.Value + newNvs.Value
                                };
                                removeNvs = oldNvs;
                                break;
                            }

                            if(removeNvs != null)
                            {
                                oldStats.Filesystems.Remove(removeNvs);
                                oldStats.Filesystems.Add(addNvs);
                            }
                            else oldStats.Filesystems.Add(newNvs);
                        }

                if(newStats.Partitions != null)
                    if(oldStats.Partitions == null) oldStats.Partitions = newStats.Partitions;
                    else
                        foreach(NameValueStats newNvs in newStats.Partitions)
                        {
                            NameValueStats removeNvs = null;
                            NameValueStats addNvs = null;

                            foreach(NameValueStats oldNvs in oldStats.Partitions.Where(oldNvs => oldNvs.name == newNvs.name)) {
                                addNvs = new NameValueStats
                                {
                                    name = oldNvs.name,
                                    Value = oldNvs.Value + newNvs.Value
                                };
                                removeNvs = oldNvs;
                                break;
                            }

                            if(removeNvs != null)
                            {
                                oldStats.Partitions.Remove(removeNvs);
                                oldStats.Partitions.Add(addNvs);
                            }
                            else oldStats.Partitions.Add(newNvs);
                        }

                if(newStats.MediaImages != null)
                    if(oldStats.MediaImages == null) oldStats.MediaImages = newStats.MediaImages;
                    else
                        foreach(NameValueStats newNvs in newStats.MediaImages)
                        {
                            NameValueStats removeNvs = null;
                            NameValueStats addNvs = null;

                            foreach(NameValueStats oldNvs in oldStats.MediaImages.Where(oldNvs => oldNvs.name == newNvs.name)) {
                                addNvs = new NameValueStats
                                {
                                    name = oldNvs.name,
                                    Value = oldNvs.Value + newNvs.Value
                                };
                                removeNvs = oldNvs;
                                break;
                            }

                            if(removeNvs != null)
                            {
                                oldStats.MediaImages.Remove(removeNvs);
                                oldStats.MediaImages.Add(addNvs);
                            }
                            else oldStats.MediaImages.Add(newNvs);
                        }

                if(newStats.Filters != null)
                    if(oldStats.Filters == null) oldStats.Filters = newStats.Filters;
                    else
                        foreach(NameValueStats newNvs in newStats.Filters)
                        {
                            NameValueStats removeNvs = null;
                            NameValueStats addNvs = null;

                            foreach(NameValueStats oldNvs in oldStats.Filters.Where(oldNvs => oldNvs.name == newNvs.name)) {
                                addNvs = new NameValueStats
                                {
                                    name = oldNvs.name,
                                    Value = oldNvs.Value + newNvs.Value
                                };
                                removeNvs = oldNvs;
                                break;
                            }

                            if(removeNvs != null)
                            {
                                oldStats.Filters.Remove(removeNvs);
                                oldStats.Filters.Add(addNvs);
                            }
                            else oldStats.Filters.Add(newNvs);
                        }

                if(newStats.Devices != null)
                    if(oldStats.Devices == null) oldStats.Devices = newStats.Devices;
                    else
                        foreach(DeviceStats newDev in from newDev in newStats.Devices
                                                      let found =
                                                          oldStats.Devices.Any(oldDev =>
                                                                                   oldDev.Manufacturer ==
                                                                                   newDev.Manufacturer &&
                                                                                   oldDev.Model == newDev.Model &&
                                                                                   oldDev.Revision == newDev.Revision &&
                                                                                   oldDev.Bus == newDev.Bus)
                                                      where !found
                                                      select newDev) { oldStats.Devices.Add(newDev); }

                if(newStats.Medias != null)
                    if(oldStats.Medias == null) oldStats.Medias = newStats.Medias;
                    else
                        foreach(MediaStats newMstat in newStats.Medias)
                        {
                            MediaStats removeMstat = null;
                            MediaStats addMstat = null;

                            foreach(MediaStats oldMstat in oldStats.Medias.Where(oldMstat => oldMstat.real == newMstat.real && oldMstat.type == newMstat.type)) {
                                addMstat = new MediaStats
                                {
                                    real = oldMstat.real,
                                    type = oldMstat.type,
                                    Value = oldMstat.Value + newMstat.Value
                                };
                                removeMstat = oldMstat;
                                break;
                            }

                            if(removeMstat != null && addMstat != null)
                            {
                                oldStats.Medias.Remove(removeMstat);
                                oldStats.Medias.Add(addMstat);
                            }
                            else oldStats.Medias.Add(newMstat);
                        }

                if(newStats.MediaScan != null)
                    if(oldStats.MediaScan == null) oldStats.MediaScan = newStats.MediaScan;
                    else
                    {
                        if(oldStats.MediaScan.Sectors == null) oldStats.MediaScan.Sectors = newStats.MediaScan.Sectors;
                        else
                        {
                            oldStats.MediaScan.Sectors.Correct = newStats.MediaScan.Sectors.Correct;
                            oldStats.MediaScan.Sectors.Error = newStats.MediaScan.Sectors.Error;
                            oldStats.MediaScan.Sectors.Total = newStats.MediaScan.Sectors.Total;
                            oldStats.MediaScan.Sectors.Unverifiable = newStats.MediaScan.Sectors.Unverifiable;
                        }

                        if(oldStats.MediaScan.Times == null) oldStats.MediaScan.Times = newStats.MediaScan.Times;
                        else
                        {
                            oldStats.MediaScan.Times.LessThan10ms = newStats.MediaScan.Times.LessThan10ms;
                            oldStats.MediaScan.Times.LessThan150ms = newStats.MediaScan.Times.LessThan150ms;
                            oldStats.MediaScan.Times.LessThan3ms = newStats.MediaScan.Times.LessThan3ms;
                            oldStats.MediaScan.Times.LessThan500ms = newStats.MediaScan.Times.LessThan500ms;
                            oldStats.MediaScan.Times.LessThan50ms = newStats.MediaScan.Times.LessThan50ms;
                            oldStats.MediaScan.Times.MoreThan500ms = newStats.MediaScan.Times.MoreThan500ms;
                        }
                    }

                if(newStats.Verify != null)
                    if(oldStats.Verify == null) oldStats.Verify = newStats.Verify;
                    else
                    {
                        if(oldStats.Verify.Sectors == null) oldStats.Verify.Sectors = newStats.Verify.Sectors;
                        else
                        {
                            oldStats.Verify.Sectors.Correct = newStats.Verify.Sectors.Correct;
                            oldStats.Verify.Sectors.Error = newStats.Verify.Sectors.Error;
                            oldStats.Verify.Sectors.Total = newStats.Verify.Sectors.Total;
                            oldStats.Verify.Sectors.Unverifiable = newStats.Verify.Sectors.Unverifiable;
                        }

                        if(oldStats.Verify.MediaImages == null)
                            oldStats.Verify.MediaImages = newStats.Verify.MediaImages;
                        else
                        {
                            oldStats.Verify.MediaImages.Correct = newStats.Verify.MediaImages.Correct;
                            oldStats.Verify.MediaImages.Failed = newStats.Verify.MediaImages.Failed;
                        }
                    }

                if(oldStats.Devices != null)
                    oldStats.Devices = oldStats.Devices.OrderBy(device => device.Manufacturer)
                                               .ThenBy(device => device.Model).ThenBy(device => device.Revision)
                                               .ThenBy(device => device.Bus).ToList();

                Random rng = new Random();
                string filename = string.Format("BackupStats_{0:yyyyMMddHHmmssfff}_{1}.xml", DateTime.UtcNow,
                                                rng.Next());
                while(File.Exists(Path.Combine(HostingEnvironment.MapPath("~"), "Statistics",
                                               filename)))
                    filename = string.Format("BackupStats_{0:yyyyMMddHHmmssfff}_{1}.xml", DateTime.UtcNow, rng.Next());

                FileStream backup =
                    new
                        FileStream(Path.Combine(HostingEnvironment.MapPath("~"), "Statistics", filename),
                                   FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                fs.Seek(0, SeekOrigin.Begin);
                fs.CopyTo(backup);
                backup.Close();
                fs.Seek(0, SeekOrigin.Begin);
                xs = new XmlSerializer(oldStats.GetType());
                xs.Serialize(fs, oldStats);
                fs.SetLength(fs.Position);
                fs.Close();

                response.Content = new StringContent("ok", Encoding.UTF8, "text/plain");
                return response;
            }
            catch(Exception ex)
            {
#if DEBUG
                Console.WriteLine("{0} {1}", ex.Message, ex.InnerException);
                throw;
#else
                response.Content = new StringContent("error", System.Text.Encoding.UTF8, "text/plain");
                return response;
#endif
            }
        }

        FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for(int numTries = 0; numTries < 100; numTries++)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullPath, mode, access, share);
                    return fs;
                }
                catch(IOException)
                {
                    if(fs != null) fs.Dispose();
                    Thread.Sleep(50);
                }
            }

            return null;
        }
    }
}