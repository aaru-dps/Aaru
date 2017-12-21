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

using System.IO;
using System.Net;
using System.Threading;

namespace DiscImageChef.Core
{
    public static class Remote
    {
        public static void SubmitReport(Metadata.DeviceReport report)
        {
            Thread submitThread = new Thread(() =>
            {
                try
                {
#if DEBUG
                    System.Console.WriteLine("Uploading device report");
#else
                    DicConsole.DebugWriteLine("Submit stats", "Uploading device report");
#endif

                    MemoryStream xmlStream = new MemoryStream();
                    System.Xml.Serialization.XmlSerializer xmlSer =
                        new System.Xml.Serialization.XmlSerializer(typeof(Metadata.DeviceReport));
                    xmlSer.Serialize(xmlStream, report);
                    xmlStream.Seek(0, SeekOrigin.Begin);
                    WebRequest request = WebRequest.Create("http://discimagechef.claunia.com/api/uploadreport");
                    ((HttpWebRequest)request).UserAgent =
                        string.Format("DiscImageChef {0}", typeof(Version).Assembly.GetName().Version);
                    request.Method = "POST";
                    request.ContentLength = xmlStream.Length;
                    request.ContentType = "application/xml";
                    Stream reqStream = request.GetRequestStream();
                    xmlStream.CopyTo(reqStream);
                    reqStream.Close();
                    WebResponse response = request.GetResponse();

                    if(((HttpWebResponse)response).StatusCode != HttpStatusCode.OK) return;

                    Stream data = response.GetResponseStream();
                    StreamReader reader = new StreamReader(data);

                    string responseFromServer = reader.ReadToEnd();
                    data.Close();
                    response.Close();
                    xmlStream.Close();
                }
                catch(WebException)
                {
                    // Can't connect to the server, do nothing
                    return;
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