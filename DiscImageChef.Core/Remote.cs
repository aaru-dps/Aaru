// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Remote.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles connections to DiscImageChef.Server.
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
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes.Metadata;
using Newtonsoft.Json;
using Version = DiscImageChef.CommonTypes.Metadata.Version;

namespace DiscImageChef.Core
{
    /// <summary>
    ///     Handles connections to DiscImageChef.Server
    /// </summary>
    public static class Remote
    {
        /// <summary>
        ///     Submits a device report
        /// </summary>
        /// <param name="report">Device report</param>
        public static void SubmitReport(DeviceReport report)
        {
            Thread submitThread = new Thread(() =>
            {
                try
                {
                    #if DEBUG
                    System.Console.WriteLine("Uploading device report");
                    #else
                    DiscImageChef.Console.DicConsole.DebugWriteLine("Submit stats", "Uploading device report");
                    #endif

                    MemoryStream  xmlStream = new MemoryStream();
                    XmlSerializer xmlSer    = new XmlSerializer(typeof(DeviceReport));
                    xmlSer.Serialize(xmlStream, report);
                    xmlStream.Seek(0, SeekOrigin.Begin);
                    WebRequest request = WebRequest.Create("http://discimagechef.claunia.com/api/uploadreport");
                    ((HttpWebRequest)request).UserAgent = $"DiscImageChef {typeof(Version).Assembly.GetName().Version}";
                    request.Method                      = "POST";
                    request.ContentLength               = xmlStream.Length;
                    request.ContentType                 = "application/xml";
                    Stream reqStream = request.GetRequestStream();
                    xmlStream.CopyTo(reqStream);
                    reqStream.Close();
                    WebResponse response = request.GetResponse();

                    if(((HttpWebResponse)response).StatusCode != HttpStatusCode.OK) return;

                    Stream       data   = response.GetResponseStream();
                    StreamReader reader = new StreamReader(data ?? throw new InvalidOperationException());

                    reader.ReadToEnd();
                    data.Close();
                    response.Close();
                    xmlStream.Close();
                }
                catch(WebException)
                {
                    // Can't connect to the server, do nothing
                }
                // ReSharper disable once RedundantCatchClause
                catch
                {
                    #if DEBUG
                    throw;
                    #endif
                }
            });
            submitThread.Start();
        }

        /// <summary>
        ///     Submits a device report
        /// </summary>
        /// <param name="report">Device report</param>
        public static void SubmitReportV2(DeviceReportV2 report)
        {
            Thread submitThread = new Thread(() =>
            {
                try
                {
                    #if DEBUG
                    System.Console.WriteLine("Uploading device report");
                    #else
                    DiscImageChef.Console.DicConsole.DebugWriteLine("Submit stats", "Uploading device report");
                    #endif

                    MemoryStream xmlStream = new MemoryStream();
                    StreamWriter jsonSw    = new StreamWriter(xmlStream);

                    jsonSw.Write(JsonConvert.SerializeObject(report, Formatting.Indented,
                                                             new JsonSerializerSettings
                                                             {
                                                                 NullValueHandling = NullValueHandling.Ignore
                                                             }));
                    jsonSw.Close();
                    xmlStream.Seek(0, SeekOrigin.Begin);
                    WebRequest request = WebRequest.Create("http://discimagechef.claunia.com/api/uploadreportv2");
                    ((HttpWebRequest)request).UserAgent = $"DiscImageChef {typeof(Version).Assembly.GetName().Version}";
                    request.Method                      = "POST";
                    request.ContentLength               = xmlStream.Length;
                    request.ContentType                 = "application/json";
                    Stream reqStream = request.GetRequestStream();
                    xmlStream.CopyTo(reqStream);
                    reqStream.Close();
                    WebResponse response = request.GetResponse();

                    if(((HttpWebResponse)response).StatusCode != HttpStatusCode.OK) return;

                    Stream       data   = response.GetResponseStream();
                    StreamReader reader = new StreamReader(data ?? throw new InvalidOperationException());

                    reader.ReadToEnd();
                    data.Close();
                    response.Close();
                    xmlStream.Close();
                }
                catch(WebException)
                {
                    // Can't connect to the server, do nothing
                }
                // ReSharper disable once RedundantCatchClause
                catch
                {
                    #if DEBUG
                    throw;
                    #endif
                }
            });
            submitThread.Start();
        }
    }
}