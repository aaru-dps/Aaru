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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Dto;
using DiscImageChef.Server.Models;
using Newtonsoft.Json;

namespace DiscImageChef.Server.Controllers
{
    public class UpdateController : ApiController
    {
        /// <summary>
        ///     Receives a report from DiscImageChef.Core, verifies it's in the correct format and stores it on the server
        /// </summary>
        /// <returns>HTTP response</returns>
        [Route("api/update")]
        [HttpGet]
        public HttpResponseMessage UploadReport(long timestamp)
        {
            DicServerContext ctx = new DicServerContext();

            SyncDto  sync     = new SyncDto();
            DateTime lastSync = DateHandlers.UnixToDateTime(timestamp);

            sync.UsbVendors = new List<UsbVendorDto>();
            foreach(UsbVendor vendor in ctx.UsbVendors.Where(v => v.ModifiedWhen > lastSync))
                sync.UsbVendors.Add(new UsbVendorDto {VendorId = (ushort)vendor.VendorId, Vendor = vendor.Vendor});

            sync.UsbProducts = new List<UsbProductDto>();
            foreach(UsbProduct product in ctx.UsbProducts.Where(p => p.ModifiedWhen > lastSync))
                sync.UsbProducts.Add(new UsbProductDto
                {
                    Id        = product.Id,
                    Product   = product.Product,
                    ProductId = (ushort)product.ProductId,
                    VendorId  = (ushort)product.Vendor.VendorId
                });

            sync.Offsets = new List<CdOffsetDto>();
            foreach(CompactDiscOffset offset in ctx.CdOffsets.Where(o => o.ModifiedWhen > lastSync))
                sync.Offsets.Add(new CdOffsetDto(offset, offset.Id));

            sync.Devices = new List<DeviceDto>();
            foreach(Device device in ctx.Devices.Where(d => d.ModifiedWhen > lastSync).ToList())
                sync.Devices.Add(new
                                     DeviceDto(JsonConvert.DeserializeObject<DeviceReportV2>(JsonConvert.SerializeObject(device)),
                                               device.Id, device.OptimalMultipleSectorsRead));

            JsonSerializer js = JsonSerializer.Create();
            StringWriter   sw = new StringWriter();
            js.Serialize(sw, sync);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(sw.ToString(), Encoding.UTF8, "application/json")
            };
        }
    }
}