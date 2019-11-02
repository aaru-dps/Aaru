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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Server.Models;
using Newtonsoft.Json;
using OperatingSystem = DiscImageChef.Server.Models.OperatingSystem;
using Version = DiscImageChef.Server.Models.Version;

namespace DiscImageChef.Server.Controllers
{
    public class UploadStatsController : ApiController
    {
        /// <summary>
        ///     Receives statistics from DiscImageChef.Core, processes them and adds them to a server-side global statistics XML
        /// </summary>
        /// <returns>HTTP response</returns>
        [Route("api/uploadstats")]
        [HttpPost]
        public HttpResponseMessage UploadStats()
        {
            HttpResponseMessage response = new HttpResponseMessage {StatusCode = HttpStatusCode.OK};

            try
            {
                Stats       newStats = new Stats();
                HttpRequest request  = HttpContext.Current.Request;

                XmlSerializer xs = new XmlSerializer(newStats.GetType());
                newStats = (Stats)xs.Deserialize(request.InputStream);

                if(newStats == null)
                {
                    response.Content = new StringContent("notstats", Encoding.UTF8, "text/plain");
                    return response;
                }

                StatsConverter.Convert(newStats);

                response.Content = new StringContent("ok", Encoding.UTF8, "text/plain");
                return response;
            }
            catch(Exception ex)
            {
                #if DEBUG
                if(Debugger.IsAttached) throw;
                #endif
                response.Content = new StringContent("error", Encoding.UTF8, "text/plain");
                return response;
            }
        }

        /// <summary>
        ///     Receives a report from DiscImageChef.Core, verifies it's in the correct format and stores it on the server
        /// </summary>
        /// <returns>HTTP response</returns>
        [Route("api/uploadstatsv2")]
        [HttpPost]
        public HttpResponseMessage UploadStatsV2()
        {
            HttpResponseMessage response = new HttpResponseMessage {StatusCode = HttpStatusCode.OK};

            try
            {
                HttpRequest request = HttpContext.Current.Request;

                StreamReader sr       = new StreamReader(request.InputStream);
                StatsDto     newstats = JsonConvert.DeserializeObject<StatsDto>(sr.ReadToEnd());

                if(newstats == null)
                {
                    response.Content = new StringContent("notstats", Encoding.UTF8, "text/plain");
                    return response;
                }

                DicServerContext ctx = new DicServerContext();

                if(newstats.Commands != null)
                    foreach(NameValueStats nvs in newstats.Commands)
                    {
                        Command existing = ctx.Commands.FirstOrDefault(c => c.Name == nvs.name);

                        if(existing == null) ctx.Commands.Add(new Command {Name = nvs.name, Count = nvs.Value});
                        else existing.Count += nvs.Value;
                    }

                if(newstats.Versions != null)
                    foreach(NameValueStats nvs in newstats.Versions)
                    {
                        Version existing = ctx.Versions.FirstOrDefault(c => c.Value == nvs.name);

                        if(existing == null) ctx.Versions.Add(new Version {Value = nvs.name, Count = nvs.Value});
                        else existing.Count += nvs.Value;
                    }

                if(newstats.Filesystems != null)
                    foreach(NameValueStats nvs in newstats.Filesystems)
                    {
                        Filesystem existing = ctx.Filesystems.FirstOrDefault(c => c.Name == nvs.name);

                        if(existing == null) ctx.Filesystems.Add(new Filesystem {Name = nvs.name, Count = nvs.Value});
                        else existing.Count += nvs.Value;
                    }

                if(newstats.Partitions != null)
                    foreach(NameValueStats nvs in newstats.Partitions)
                    {
                        Partition existing = ctx.Partitions.FirstOrDefault(c => c.Name == nvs.name);

                        if(existing == null) ctx.Partitions.Add(new Partition {Name = nvs.name, Count = nvs.Value});
                        else existing.Count += nvs.Value;
                    }

                if(newstats.MediaFormats != null)
                    foreach(NameValueStats nvs in newstats.MediaFormats)
                    {
                        MediaFormat existing = ctx.MediaFormats.FirstOrDefault(c => c.Name == nvs.name);

                        if(existing == null) ctx.MediaFormats.Add(new MediaFormat {Name = nvs.name, Count = nvs.Value});
                        else existing.Count += nvs.Value;
                    }

                if(newstats.Filters != null)
                    foreach(NameValueStats nvs in newstats.Filters)
                    {
                        Filter existing = ctx.Filters.FirstOrDefault(c => c.Name == nvs.name);

                        if(existing == null) ctx.Filters.Add(new Filter {Name = nvs.name, Count = nvs.Value});
                        else existing.Count += nvs.Value;
                    }

                if(newstats.OperatingSystems != null)
                    foreach(OsStats operatingSystem in newstats.OperatingSystems)
                    {
                        OperatingSystem existing =
                            ctx.OperatingSystems.FirstOrDefault(c => c.Name    == operatingSystem.name &&
                                                                     c.Version == operatingSystem.version);

                        if(existing == null)
                            ctx.OperatingSystems.Add(new OperatingSystem
                            {
                                Name    = operatingSystem.name,
                                Version = operatingSystem.version,
                                Count   = operatingSystem.Value
                            });
                        else existing.Count += operatingSystem.Value;
                    }

                if(newstats.Medias != null)
                    foreach(MediaStats media in newstats.Medias)
                    {
                        Media existing = ctx.Medias.FirstOrDefault(c => c.Type == media.type && c.Real == media.real);

                        if(existing == null)
                            ctx.Medias.Add(new Media {Type = media.type, Real = media.real, Count = media.Value});
                        else existing.Count += media.Value;
                    }

                if(newstats.Devices != null)
                    foreach(DeviceStats device in newstats.Devices)
                    {
                        DeviceStat existing =
                            ctx.DeviceStats.FirstOrDefault(c => c.Bus          == device.Bus          &&
                                                                c.Manufacturer == device.Manufacturer &&
                                                                c.Model        == device.Model        &&
                                                                c.Revision     == device.Revision);

                        if(existing == null)
                            ctx.DeviceStats.Add(new DeviceStat
                            {
                                Bus          = device.Bus,
                                Manufacturer = device.Manufacturer,
                                Model        = device.Model,
                                Revision     = device.Revision
                            });
                    }

                ctx.SaveChanges();

                response.Content = new StringContent("ok", Encoding.UTF8, "text/plain");
                return response;
            }
            // ReSharper disable once RedundantCatchClause
            catch
            {
                #if DEBUG
                if(Debugger.IsAttached) throw;
                #endif
                response.Content = new StringContent("error", Encoding.UTF8, "text/plain");
                return response;
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