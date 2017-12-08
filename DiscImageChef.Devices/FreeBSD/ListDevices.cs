using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.Console;
using path_id_t = System.UInt32;
using target_id_t = System.UInt32;
using lun_id_t = System.UInt32;

namespace DiscImageChef.Devices.FreeBSD
{
    public static class ListDevices
    {
        const string XPT_DEVICE = "/dev/xpt0";
        const path_id_t CAM_XPT_PATH_ID = 0xFFFFFFFF;
        const target_id_t CAM_TARGET_WILDCARD = 0xFFFFFFFF;
        const lun_id_t CAM_LUN_WILDCARD = 0xFFFFFFFF;
        const int ccb_size = 1240;

        public static DeviceInfo[] GetList()
        {
            int fd = Extern.open(XPT_DEVICE, FileFlags.ReadWrite);

            // MMC support was added to CAM in FreeBSD 12
            int not_mmc_data = Marshal.SizeOf(typeof(mmc_params));
            if(Environment.OSVersion.Version.Major >= 12)
                not_mmc_data = 0;

            int dev_match_result_size = Marshal.SizeOf(typeof(dev_match_result)) - not_mmc_data;

            if(fd == -1)
            {
                DicConsole.ErrorWriteLine("Error {0} opening {1}", Marshal.GetLastWin32Error(), XPT_DEVICE);
                return null;
            }

            int bufsize = dev_match_result_size * 100;
            ccb_dev_match cdm = new ccb_dev_match();
            cdm.ccb_h = new ccb_hdr();
            cdm.ccb_h.path_id = CAM_XPT_PATH_ID;
            cdm.ccb_h.target_id = CAM_TARGET_WILDCARD;
            cdm.ccb_h.target_lun = CAM_LUN_WILDCARD;
            cdm.ccb_h.func_code = xpt_opcode.XPT_DEV_MATCH;
            cdm.match_buf_len = (uint)bufsize;
            cdm.matches = Marshal.AllocHGlobal(bufsize);

            IntPtr ccb = Marshal.AllocHGlobal(ccb_size);
            Marshal.StructureToPtr(cdm, ccb, false);
            int res = Extern.ioctl(fd, FreebsdIoctl.CAMIOCOMMAND, ccb);

            if(res == -1)
            {
                DicConsole.ErrorWriteLine("Error {0} sending ioctl to CAM", Marshal.GetLastWin32Error());
                Extern.close(fd);
                Marshal.FreeHGlobal(cdm.matches);
                Marshal.FreeHGlobal(ccb);
                return null;
            }

            cdm = (ccb_dev_match)Marshal.PtrToStructure(ccb, typeof(ccb_dev_match));
            DicConsole.DebugWriteLine("FreeBSD devices", "CAM returned {0} matches", cdm.num_matches);

            if(cdm.num_matches == 0)
                return null;

            dev_match_result[] matches = new dev_match_result[cdm.num_matches];

            byte[] buffer = new byte[bufsize];
            Marshal.Copy(cdm.matches, buffer, 0, bufsize);

            List<DeviceInfo> listDevices = new List<DeviceInfo>();
            DeviceInfo deviceInfo = new DeviceInfo();
            bool pathFound = false;
            bool skipDevice = false;

            for(int i = 0; i < matches.Length; i++)
            {
                dev_match_type matchType = (dev_match_type)BitConverter.ToUInt32(buffer, i * dev_match_result_size);

                if(matchType == dev_match_type.DEV_MATCH_DEVICE)
                {
                    byte[] data = new byte[Marshal.SizeOf(typeof(device_match_result))];
                    Buffer.BlockCopy(buffer, (i * dev_match_result_size) + 4, data, 0,
                        Marshal.SizeOf(typeof(device_match_result)));
                    IntPtr matchPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(device_match_result)));
                    Marshal.Copy(data, 0, matchPtr, Marshal.SizeOf(typeof(device_match_result)));
                    device_match_result dmr =
                        (device_match_result)Marshal.PtrToStructure(matchPtr, typeof(device_match_result));
                    Marshal.FreeHGlobal(matchPtr);

                    if(dmr.flags.HasFlag(dev_result_flags.DEV_RESULT_UNCONFIGURED))
                    {
                        skipDevice = true;
                        continue;
                    }

                    if(pathFound)
                    {
                        listDevices.Add(deviceInfo);
                        deviceInfo = new DeviceInfo();
                        pathFound = false;
                    }

                    System.Console.WriteLine("{0}", dmr.protocol);

                    skipDevice = false;
                    switch(dmr.protocol)
                    {
                        case cam_proto.PROTO_ATA:
                        case cam_proto.PROTO_ATAPI:
                        case cam_proto.PROTO_SATAPM:
                        {
                            // Little-endian FreeBSD gives it resorted
                            // Big-endian FreeBSD, no idea
                            byte[] atad_tneid = new byte[512];
                            for(int aIndex = 0; aIndex < 512; aIndex += 2)
                            {
                                atad_tneid[aIndex] = dmr.ident_data[aIndex + 1];
                                atad_tneid[aIndex + 1] = dmr.ident_data[aIndex];
                            }

                            Decoders.ATA.Identify.IdentifyDevice? idt = Decoders.ATA.Identify.Decode(atad_tneid);
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
                                deviceInfo.bus = "ATA";
                                deviceInfo.supported = false;
                            }
                            if(dmr.protocol == cam_proto.PROTO_ATAPI)
                                goto case cam_proto.PROTO_SCSI;
                            break;
                        }
                        case cam_proto.PROTO_SCSI:
                        {

                            Decoders.SCSI.Inquiry.SCSIInquiry? inq = Decoders.SCSI.Inquiry.Decode(dmr.inq_data);
                            if(inq.HasValue)
                            {
                                deviceInfo.vendor = StringHandlers.CToString(inq.Value.VendorIdentification).Trim();
                                deviceInfo.model = StringHandlers.CToString(inq.Value.ProductIdentification).Trim();
                                deviceInfo.bus = dmr.protocol == cam_proto.PROTO_ATAPI ? "ATAPI" : "SCSI";
                                deviceInfo.supported = false;
                            }
                            break;
                        }
                        case cam_proto.PROTO_NVME:
                            deviceInfo.bus = "NVMe";
                            deviceInfo.supported = false;
                            break;
                        case cam_proto.PROTO_MMCSD:
                            if(!ArrayHelpers.ArrayIsNullOrEmpty(dmr.mmc_ident_data.model))
                                deviceInfo.model = StringHandlers.CToString(dmr.mmc_ident_data.model);
                            else
                                deviceInfo.model = string.Format("{0} card",
                                    dmr.mmc_ident_data.card_features.HasFlag(mmc_card_features.CARD_FEATURE_SDIO)
                                        ? "SDIO"
                                        : "Unknown");

                            if(dmr.mmc_ident_data.card_features.HasFlag(mmc_card_features.CARD_FEATURE_SD20) ||
                               dmr.mmc_ident_data.card_features.HasFlag(mmc_card_features.CARD_FEATURE_SDHC) ||
                               dmr.mmc_ident_data.card_features.HasFlag(mmc_card_features.CARD_FEATURE_SDIO))
                                deviceInfo.bus = "SD";
                            else if(dmr.mmc_ident_data.card_features.HasFlag(mmc_card_features.CARD_FEATURE_MMC))
                                deviceInfo.bus = "MMC";
                            else
                                deviceInfo.bus = "MMC/SD";

                            deviceInfo.supported = false;
                            break;
                        default:
                            skipDevice = true;
                            break;
                    }
                }
                else if(matchType == dev_match_type.DEV_MATCH_PERIPH)
                {
                    if(skipDevice)
                        continue;

                    byte[] data = new byte[Marshal.SizeOf(typeof(periph_match_result))];
                    Buffer.BlockCopy(buffer, (i * dev_match_result_size) + 4, data, 0,
                        Marshal.SizeOf(typeof(periph_match_result)));
                    IntPtr matchPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(periph_match_result)));
                    Marshal.Copy(data, 0, matchPtr, Marshal.SizeOf(typeof(periph_match_result)));
                    periph_match_result pmr =
                        (periph_match_result)Marshal.PtrToStructure(matchPtr, typeof(periph_match_result));
                    Marshal.FreeHGlobal(matchPtr);

                    if(deviceInfo.path == null || StringHandlers.CToString(pmr.periph_name) == "pass")
                    {
                        deviceInfo.path = string.Format("/dev/{0}{1}", StringHandlers.CToString(pmr.periph_name),
                            pmr.unit_number);
                        pathFound = true;
                    }
                }
            }

            if(pathFound)
                listDevices.Add(deviceInfo);


            Marshal.FreeHGlobal(cdm.matches);
            Marshal.FreeHGlobal(ccb);

            Extern.close(fd);

            if(listDevices.Count > 0)
                return listDevices.OrderBy(t => t.path).ToArray();

            return null;
        }
    }
}