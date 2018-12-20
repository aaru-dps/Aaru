// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UploadReportController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles report uploads.
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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes.Metadata;
using Newtonsoft.Json;

namespace DiscImageChef.Server.Controllers
{
    public class UploadReportController : ApiController
    {
        /// <summary>
        ///     Receives a report from DiscImageChef.Core, verifies it's in the correct format and stores it on the server
        /// </summary>
        /// <returns>HTTP response</returns>
        [Route("api/uploadreport")]
        [HttpPost]
        public HttpResponseMessage UploadReport()
        {
            HttpResponseMessage response = new HttpResponseMessage {StatusCode = HttpStatusCode.OK};

            try
            {
                DeviceReport newReport = new DeviceReport();
                HttpRequest  request   = HttpContext.Current.Request;

                XmlSerializer xs = new XmlSerializer(newReport.GetType());
                newReport = (DeviceReport)xs.Deserialize(request.InputStream);

                if(newReport == null)
                {
                    response.Content = new StringContent("notstats", Encoding.UTF8, "text/plain");
                    return response;
                }

                Random rng      = new Random();
                string filename = $"NewReport_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{rng.Next()}.xml";
                while(File.Exists(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                               "Upload", filename)))
                    filename = $"NewReport_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{rng.Next()}.xml";

                FileStream newFile =
                    new
                        FileStream(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Upload", filename),
                                   FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                xs.Serialize(newFile, newReport);
                newFile.Close();

                response.Content = new StringContent("ok", Encoding.UTF8, "text/plain");
                return response;
            }
            // ReSharper disable once RedundantCatchClause
            catch
            {
                #if DEBUG
                throw;
                #else
                response.Content = new StringContent("error", System.Text.Encoding.UTF8, "text/plain");
                return response;
#endif
            }
        }
        /// <summary>
        ///     Receives a report from DiscImageChef.Core, verifies it's in the correct format and stores it on the server
        /// </summary>
        /// <returns>HTTP response</returns>
        [Route("api/uploadreportv2")]
        [HttpPost]
        public HttpResponseMessage UploadReportV2()
        {
            HttpResponseMessage response = new HttpResponseMessage {StatusCode = HttpStatusCode.OK};

            try
            {
                HttpRequest  request   = HttpContext.Current.Request;

                StreamReader sr = new StreamReader(request.InputStream);
                string jsonData = sr.ReadToEnd();
                DeviceReportV2 newReport = JsonConvert.DeserializeObject<DeviceReportV2>(jsonData);
                
                if(newReport == null)
                {
                    response.Content = new StringContent("notstats", Encoding.UTF8, "text/plain");
                    return response;
                }

                Random rng      = new Random();
                string filename = $"NewReport_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{rng.Next()}.json";
                while(File.Exists(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                               "Upload", filename)))
                    filename = $"NewReport_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{rng.Next()}.json";

                FileStream newFile =
                    new
                        FileStream(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Upload", filename),
                                   FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                StreamWriter sw = new StreamWriter(newFile);
                sw.Write(jsonData);
                sw.Close();
                newFile.Close();

                response.Content = new StringContent("ok", Encoding.UTF8, "text/plain");
                return response;
            }
            // ReSharper disable once RedundantCatchClause
            catch
            {
                #if DEBUG
                throw;
                #else
                response.Content = new StringContent("error", System.Text.Encoding.UTF8, "text/plain");
                return response;
#endif
            }
        }
    }
}