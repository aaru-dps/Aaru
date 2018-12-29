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

using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Server.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;

namespace DiscImageChef.Server.Controllers
{
    public class UploadReportController : ApiController
    {
        DicServerContext ctx = new DicServerContext();

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

                DeviceReportV2 reportV2 = new DeviceReportV2(newReport);
                StringWriter   jsonSw   = new StringWriter();
                jsonSw.Write(JsonConvert.SerializeObject(reportV2, Formatting.Indented,
                                                         new JsonSerializerSettings
                                                         {
                                                             NullValueHandling = NullValueHandling.Ignore
                                                         }));
                string reportV2String = jsonSw.ToString();
                jsonSw.Close();

                ctx.Reports.Add(new UploadedReport(reportV2));
                ctx.SaveChanges();

                MimeMessage message = new MimeMessage
                {
                    Subject = "New device report (old version)",
                    Body    = new TextPart("plain") {Text = reportV2String}
                };
                message.From.Add(new MailboxAddress("DiscImageChef",  "dic@claunia.com"));
                message.To.Add(new MailboxAddress("Natalia Portillo", "claunia@claunia.com"));

                using(SmtpClient client = new SmtpClient())
                {
                    client.Connect("mail.claunia.com", 25, false);
                    client.Send(message);
                    client.Disconnect(true);
                }

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
                HttpRequest request = HttpContext.Current.Request;

                StreamReader   sr         = new StreamReader(request.InputStream);
                string         reportJson = sr.ReadToEnd();
                DeviceReportV2 newReport  = JsonConvert.DeserializeObject<DeviceReportV2>(reportJson);

                if(newReport == null)
                {
                    response.Content = new StringContent("notstats", Encoding.UTF8, "text/plain");
                    return response;
                }

                ctx.Reports.Add(new UploadedReport(newReport));
                ctx.SaveChanges();

                MimeMessage message = new MimeMessage
                {
                    Subject = "New device report", Body = new TextPart("plain") {Text = reportJson}
                };
                message.From.Add(new MailboxAddress("DiscImageChef",  "dic@claunia.com"));
                message.To.Add(new MailboxAddress("Natalia Portillo", "claunia@claunia.com"));

                using(SmtpClient client = new SmtpClient())
                {
                    client.Connect("mail.claunia.com", 25, false);
                    client.Send(message);
                    client.Disconnect(true);
                }

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
    }
}