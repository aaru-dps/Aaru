// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Remote.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles connections to Aaru.Server.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Dto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using CdOffset = Aaru.Database.Models.CdOffset;
using Version = Aaru.CommonTypes.Metadata.Version;

namespace Aaru.Core
{
    /// <summary>
    ///     Handles connections to Aaru.Server
    /// </summary>
    public static class Remote
    {
        /// <summary>
        ///     Submits a device report
        /// </summary>
        /// <param name="report">Device report</param>
        public static void SubmitReport(DeviceReportV2 report)
        {
            Thread submitThread = new Thread(() =>
            {
                try
                {
                    #if DEBUG
                    System.Console.WriteLine("Uploading device report");
                    #else
                    Aaru.Console.DicConsole.DebugWriteLine("Submit stats", "Uploading device report");
                    #endif

                    string json = JsonConvert.SerializeObject(report, Formatting.Indented,
                                                              new JsonSerializerSettings
                                                              {
                                                                  NullValueHandling = NullValueHandling.Ignore
                                                              });
                    byte[]     jsonBytes = Encoding.UTF8.GetBytes(json);
                    WebRequest request   = WebRequest.Create("https://www.discimagechef.app/api/uploadreportv2");
                    ((HttpWebRequest)request).UserAgent = $"Aaru {typeof(Version).Assembly.GetName().Version}";
                    request.Method                      = "POST";
                    request.ContentLength               = jsonBytes.Length;
                    request.ContentType                 = "application/json";
                    Stream reqStream = request.GetRequestStream();
                    reqStream.Write(jsonBytes, 0, jsonBytes.Length);
                    reqStream.Close();
                    WebResponse response = request.GetResponse();

                    if(((HttpWebResponse)response).StatusCode != HttpStatusCode.OK) return;

                    Stream       data   = response.GetResponseStream();
                    StreamReader reader = new StreamReader(data ?? throw new InvalidOperationException());

                    reader.ReadToEnd();
                    data.Close();
                    response.Close();
                }
                catch(WebException)
                {
                    // Can't connect to the server, do nothing
                }
                // ReSharper disable once RedundantCatchClause
                catch
                {
                    #if DEBUG
                    if(Debugger.IsAttached) throw;
                    #endif
                }
            });
            submitThread.Start();
        }

        public static void UpdateMasterDatabase(bool create)
        {
            DicContext mctx = DicContext.Create(Aaru.Settings.Settings.MasterDbPath);
            mctx.Database.Migrate();
            mctx.SaveChanges();

            try
            {
                long     lastUpdate = 0;
                DateTime latest     = DateTime.MinValue;

                if(!create)
                {
                    List<DateTime> latestAll = new List<DateTime>();
                    if(mctx.UsbVendors.Any()) latestAll.Add(mctx.UsbVendors.Max(v => v.ModifiedWhen));
                    if(mctx.UsbProducts.Any()) latestAll.Add(mctx.UsbProducts.Max(p => p.ModifiedWhen));
                    if(mctx.CdOffsets.Any()) latestAll.Add(mctx.CdOffsets.Max(o => o.ModifiedWhen));
                    if(mctx.Devices.Any()) latestAll.Add(mctx.Devices.Max(d => d.LastSynchronized));

                    if(latestAll.Any())
                    {
                        latest     = latestAll.Max(t => t);
                        lastUpdate = (latest.ToFileTimeUtc() - new DateTime(1970, 1, 1).ToFileTimeUtc()) / 10000000;
                    }
                }

                if(lastUpdate == 0)
                {
                    create = true;
                    DicConsole.WriteLine("Creating master database");
                }
                else
                {
                    DicConsole.WriteLine("Updating master database");
                    DicConsole.WriteLine("Last update: {0}", latest);
                }

                DateTime updateStart = DateTime.UtcNow;

                WebRequest request =
                    WebRequest.Create($"https://www.discimagechef.app/api/update?timestamp={lastUpdate}");
                ((HttpWebRequest)request).UserAgent = $"Aaru {typeof(Version).Assembly.GetName().Version}";
                request.Method                      = "GET";
                request.ContentType                 = "application/json";
                WebResponse response = request.GetResponse();

                if(((HttpWebResponse)response).StatusCode != HttpStatusCode.OK)
                {
                    DicConsole.ErrorWriteLine("Error {0} when trying to get updated entities.",
                                              ((HttpWebResponse)response).StatusCode);
                    return;
                }

                Stream       data   = response.GetResponseStream();
                StreamReader reader = new StreamReader(data ?? throw new InvalidOperationException());
                SyncDto      sync   = JsonConvert.DeserializeObject<SyncDto>(reader.ReadToEnd());

                if(create)
                {
                    DicConsole.WriteLine("Adding USB vendors");
                    foreach(UsbVendorDto vendor in sync.UsbVendors)
                        mctx.UsbVendors.Add(new UsbVendor(vendor.VendorId, vendor.Vendor));

                    DicConsole.WriteLine("Added {0} usb vendors", sync.UsbVendors.Count);

                    DicConsole.WriteLine("Adding USB products");
                    foreach(UsbProductDto product in sync.UsbProducts)
                        mctx.UsbProducts.Add(new UsbProduct(product.VendorId, product.ProductId, product.Product));

                    DicConsole.WriteLine("Added {0} usb products", sync.UsbProducts.Count);

                    DicConsole.WriteLine("Adding CompactDisc read offsets");
                    foreach(CdOffsetDto offset in sync.Offsets)
                        mctx.CdOffsets.Add(new CdOffset(offset) {Id = offset.Id});

                    DicConsole.WriteLine("Added {0} CompactDisc read offsets", sync.Offsets.Count);

                    DicConsole.WriteLine("Adding known devices");
                    foreach(DeviceDto device in sync.Devices) mctx.Devices.Add(new Device(device) {Id = device.Id});

                    DicConsole.WriteLine("Added {0} known devices", sync.Devices.Count);
                }
                else
                {
                    long addedVendors     = 0;
                    long addedProducts    = 0;
                    long addedOffsets     = 0;
                    long addedDevices     = 0;
                    long modifiedVendors  = 0;
                    long modifiedProducts = 0;
                    long modifiedOffsets  = 0;
                    long modifiedDevices  = 0;

                    DicConsole.WriteLine("Updating USB vendors");
                    foreach(UsbVendorDto vendor in sync.UsbVendors)
                    {
                        UsbVendor existing = mctx.UsbVendors.FirstOrDefault(v => v.Id == vendor.VendorId);

                        if(existing != null)
                        {
                            modifiedVendors++;
                            existing.Vendor       = vendor.Vendor;
                            existing.ModifiedWhen = updateStart;
                            mctx.UsbVendors.Update(existing);
                        }
                        else
                        {
                            addedVendors++;
                            mctx.UsbVendors.Add(new UsbVendor(vendor.VendorId, vendor.Vendor));
                        }
                    }

                    DicConsole.WriteLine("Added {0} USB vendors",    addedVendors);
                    DicConsole.WriteLine("Modified {0} USB vendors", modifiedVendors);

                    DicConsole.WriteLine("Updating USB products");
                    foreach(UsbProductDto product in sync.UsbProducts)
                    {
                        UsbProduct existing =
                            mctx.UsbProducts.FirstOrDefault(p => p.VendorId  == product.VendorId &&
                                                                 p.ProductId == product.ProductId);

                        if(existing != null)
                        {
                            modifiedProducts++;
                            existing.Product      = product.Product;
                            existing.ModifiedWhen = updateStart;
                            mctx.UsbProducts.Update(existing);
                        }
                        else
                        {
                            addedProducts++;
                            mctx.UsbProducts.Add(new UsbProduct(product.VendorId, product.ProductId, product.Product));
                        }
                    }

                    DicConsole.WriteLine("Added {0} USB products",    addedProducts);
                    DicConsole.WriteLine("Modified {0} USB products", modifiedProducts);

                    DicConsole.WriteLine("Updating CompactDisc read offsets");
                    foreach(CdOffsetDto offset in sync.Offsets)
                    {
                        CdOffset existing = mctx.CdOffsets.FirstOrDefault(o => o.Id == offset.Id);

                        if(existing != null)
                        {
                            modifiedOffsets++;
                            existing.Agreement    = offset.Agreement;
                            existing.Manufacturer = offset.Manufacturer;
                            existing.Model        = offset.Model;
                            existing.Submissions  = offset.Submissions;
                            existing.Offset       = offset.Offset;
                            existing.ModifiedWhen = updateStart;
                            mctx.CdOffsets.Update(existing);
                        }
                        else
                        {
                            addedOffsets++;
                            mctx.CdOffsets.Add(new CdOffset(offset) {Id = offset.Id});
                        }
                    }

                    DicConsole.WriteLine("Added {0} CompactDisc read offsets",    addedOffsets);
                    DicConsole.WriteLine("Modified {0} CompactDisc read offsets", modifiedOffsets);

                    DicConsole.WriteLine("Updating known devices");
                    foreach(DeviceDto device in sync.Devices)
                    {
                        Device existing = mctx.Devices.FirstOrDefault(d => d.Id == device.Id);

                        if(existing != null)
                        {
                            modifiedDevices++;

                            mctx.Remove(existing);

                            existing = new Device(device)
                            {
                                Id = device.Id, OptimalMultipleSectorsRead = device.OptimalMultipleSectorsRead
                            };

                            mctx.Devices.Add(existing);
                        }
                        else
                        {
                            addedDevices++;
                            mctx.Devices.Add(new Device(device)
                            {
                                Id                         = device.Id,
                                OptimalMultipleSectorsRead = device.OptimalMultipleSectorsRead
                            });
                        }
                    }

                    DicConsole.WriteLine("Added {0} known devices",    addedDevices);
                    DicConsole.WriteLine("Modified {0} known devices", modifiedDevices);
                }
            }
            catch(Exception ex) { DicConsole.ErrorWriteLine("Exception {0} when updating database.", ex); }
            finally
            {
                DicConsole.WriteLine("Saving changes...");
                mctx.SaveChanges();
            }
        }
    }
}