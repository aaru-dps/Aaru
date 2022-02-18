// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Helpers;
using static Aaru.Devices.FreeBSD.Extern;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Devices.FreeBSD
{
    [Obsolete]
    internal static class ListDevices
    {
        /// <summary>Gets a list of all known storage devices on FreeBSD</summary>
        /// <returns>List of devices</returns>
        [Obsolete]
        internal static DeviceInfo[] GetList()
        {
            string[]         passDevices = Directory.GetFiles("/dev/", "pass*", SearchOption.TopDirectoryOnly);
            List<DeviceInfo> listDevices = new List<DeviceInfo>();

            foreach(string passDevice in passDevices)
            {
                var    deviceInfo = new DeviceInfo();
                IntPtr dev        = cam_open_device(passDevice, FileFlags.ReadWrite);
                var    camDevice  = (CamDevice)Marshal.PtrToStructure(dev, typeof(CamDevice));

                IntPtr ccbPtr = cam_getccb(dev);

                if(ccbPtr.ToInt64() == 0)
                    continue;

                var cgd = (CcbGetdev)Marshal.PtrToStructure(ccbPtr, typeof(CcbGetdev));

                cgd.ccb_h.func_code = XptOpcode.XptGdevType;

                Marshal.StructureToPtr(cgd, ccbPtr, false);

                int error = cam_send_ccb(dev, ccbPtr);

                if(error < 0)
                {
                    cam_freeccb(ccbPtr);

                    continue;
                }

                cgd = (CcbGetdev)Marshal.PtrToStructure(ccbPtr, typeof(CcbGetdev));

                cam_freeccb(ccbPtr);
                cam_close_device(dev);

                string simName = StringHandlers.CToString(camDevice.SimName);
                deviceInfo.Path = passDevice;
                byte[] serialNumber = new byte[camDevice.SerialNumLen];
                Array.Copy(camDevice.SerialNum, 0, serialNumber, 0, serialNumber.Length);
                deviceInfo.Serial = StringHandlers.CToString(serialNumber);

                switch(cgd.protocol)
                {
                    case CamProto.ProtoAta:
                    case CamProto.ProtoAtapi:
                    case CamProto.ProtoSatapm:
                    {
                        // Little-endian FreeBSD gives it resorted
                        // Big-endian FreeBSD, no idea
                        byte[] atadTneid = new byte[512];

                        for(int aIndex = 0; aIndex < 512; aIndex += 2)
                        {
                            atadTneid[aIndex] = cgd.ident_data[aIndex + 1];
                            atadTneid[aIndex                          + 1] = cgd.ident_data[aIndex];
                        }

                        Identify.IdentifyDevice? idt = Identify.Decode(atadTneid);

                        if(idt.HasValue)
                        {
                            string[] separated = idt.Value.Model.Split(' ');

                            if(separated.Length == 1)
                            {
                                deviceInfo.Vendor = "ATA";
                                deviceInfo.Model  = separated[0];
                            }
                            else
                            {
                                deviceInfo.Vendor = separated[0];
                                deviceInfo.Model  = separated[^1];
                            }

                            deviceInfo.Serial    = idt.Value.SerialNumber;
                            deviceInfo.Bus       = simName == "ahcich" ? "SATA" : "ATA";
                            deviceInfo.Supported = simName != "ata";
                        }

                        if(cgd.protocol == CamProto.ProtoAtapi)
                            goto case CamProto.ProtoScsi;

                        break;
                    }
                    case CamProto.ProtoScsi:
                    {
                        Inquiry? inq = Inquiry.Decode(cgd.inq_data);

                        if(inq.HasValue)
                        {
                            deviceInfo.Vendor    = StringHandlers.CToString(inq.Value.VendorIdentification).Trim();
                            deviceInfo.Model     = StringHandlers.CToString(inq.Value.ProductIdentification).Trim();
                            deviceInfo.Bus       = simName == "ata" || simName == "ahcich" ? "ATAPI" : "SCSI";
                            deviceInfo.Supported = simName != "ata";
                        }

                        break;
                    }
                    case CamProto.ProtoNvme:
                        deviceInfo.Bus       = "NVMe";
                        deviceInfo.Supported = false;

                        break;
                    case CamProto.ProtoMmcsd:
                        deviceInfo.Model     = "Unknown card";
                        deviceInfo.Bus       = "MMC/SD";
                        deviceInfo.Supported = false;

                        break;
                }

                listDevices.Add(deviceInfo);
            }

            return listDevices.Count > 0 ? listDevices.OrderBy(t => t.Path).ToArray() : null;
        }
    }
}