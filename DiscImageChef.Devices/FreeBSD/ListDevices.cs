// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ListDevices.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FreeBSD direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Gets a list of known devices.
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
using System.Runtime.InteropServices;
using static DiscImageChef.Devices.FreeBSD.Extern;

namespace DiscImageChef.Devices.FreeBSD
{
    static class ListDevices
    {
        internal static DeviceInfo[] GetList()
        {
            string[] passDevices = Directory.GetFiles("/dev/", "pass*", SearchOption.TopDirectoryOnly);
            List<DeviceInfo> listDevices = new List<DeviceInfo>();

            foreach(string passDevice in passDevices)
            {
                DeviceInfo deviceInfo = new DeviceInfo();
                IntPtr dev = cam_open_device(passDevice, FileFlags.ReadWrite);
                cam_device camDevice = (cam_device)Marshal.PtrToStructure(dev, typeof(cam_device));

                IntPtr ccbPtr = cam_getccb(dev);

                if(ccbPtr.ToInt64() == 0) continue;

                ccb_getdev cgd = (ccb_getdev)Marshal.PtrToStructure(ccbPtr, typeof(ccb_getdev));

                cgd.ccb_h.func_code = xpt_opcode.XPT_GDEV_TYPE;

                Marshal.StructureToPtr(cgd, ccbPtr, false);

                int error = cam_send_ccb(dev, ccbPtr);

                if(error < 0)
                {
                    cam_freeccb(ccbPtr);
                    continue;
                }

                cgd = (ccb_getdev)Marshal.PtrToStructure(ccbPtr, typeof(ccb_getdev));

                cam_freeccb(ccbPtr);
                cam_close_device(dev);

                string simName = StringHandlers.CToString(camDevice.sim_name);
                deviceInfo.path = passDevice;
                byte[] serialNumber = new byte[camDevice.serial_num_len];
                Array.Copy(camDevice.serial_num, 0, serialNumber, 0, serialNumber.Length);
                deviceInfo.serial = StringHandlers.CToString(serialNumber);

                switch(cgd.protocol)
                {
                    case cam_proto.PROTO_ATA:
                    case cam_proto.PROTO_ATAPI:
                    case cam_proto.PROTO_SATAPM:
                    {
                        // Little-endian FreeBSD gives it resorted
                        // Big-endian FreeBSD, no idea
                        byte[] atadTneid = new byte[512];
                        for(int aIndex = 0; aIndex < 512; aIndex += 2)
                        {
                            atadTneid[aIndex] = cgd.ident_data[aIndex + 1];
                            atadTneid[aIndex + 1] = cgd.ident_data[aIndex];
                        }

                        Decoders.ATA.Identify.IdentifyDevice? idt = Decoders.ATA.Identify.Decode(atadTneid);
                        if(idt.HasValue)
                        {
                            string[] separated = idt.Value.Model.Split(' ');

                            if(separated.Length == 1)
                            {
                                deviceInfo.vendor = "ATA";
                                deviceInfo.model = separated[0];
                            }
                            else
                            {
                                deviceInfo.vendor = separated[0];
                                deviceInfo.model = separated[separated.Length - 1];
                            }

                            deviceInfo.serial = idt.Value.SerialNumber;
                            deviceInfo.bus = simName == "ahcich" ? "SATA" : "ATA";
                            deviceInfo.supported = simName != "ata";
                        }
                        if(cgd.protocol == cam_proto.PROTO_ATAPI) goto case cam_proto.PROTO_SCSI;
                        break;
                    }
                    case cam_proto.PROTO_SCSI:
                    {
                        Decoders.SCSI.Inquiry.SCSIInquiry? inq = Decoders.SCSI.Inquiry.Decode(cgd.inq_data);
                        if(inq.HasValue)
                        {
                            deviceInfo.vendor = StringHandlers.CToString(inq.Value.VendorIdentification).Trim();
                            deviceInfo.model = StringHandlers.CToString(inq.Value.ProductIdentification).Trim();
                            deviceInfo.bus = simName == "ata" || simName == "ahcich" ? "ATAPI" : "SCSI";
                            deviceInfo.supported = simName != "ata";
                        }
                        break;
                    }
                    case cam_proto.PROTO_NVME:
                        deviceInfo.bus = "NVMe";
                        deviceInfo.supported = false;
                        break;
                    case cam_proto.PROTO_MMCSD:
                        deviceInfo.model = "Unknown card";
                        deviceInfo.bus = "MMC/SD";
                        deviceInfo.supported = false;
                        break;
                }

                listDevices.Add(deviceInfo);
            }

            return listDevices.Count > 0 ? listDevices.OrderBy(t => t.path).ToArray() : null;
        }
    }
}