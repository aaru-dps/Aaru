// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Constructor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prepares a device for direct access.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        /// <summary>
        /// Opens the device for sending direct commands
        /// </summary>
        /// <param name="devicePath">Device path</param>
        public Device(string devicePath)
        {
            platformID = Interop.DetectOS.GetRealPlatformID();
            Timeout = 15;
            error = false;
            removable = false;

            switch(platformID)
            {
                case Interop.PlatformID.Win32NT:
                    {
                        fd = Windows.Extern.CreateFile(devicePath,
                            Windows.FileAccess.GenericRead | Windows.FileAccess.GenericWrite,
                            Windows.FileShare.Read | Windows.FileShare.Write,
                            IntPtr.Zero, Windows.FileMode.OpenExisting,
                            Windows.FileAttributes.Normal, IntPtr.Zero);

                        if(((SafeFileHandle)fd).IsInvalid)
                        {
                            error = true;
                            lastError = Marshal.GetLastWin32Error();
                        }

                        break;
                    }
                case Interop.PlatformID.Linux:
                    {
                        fd = Linux.Extern.open(devicePath, Linux.FileFlags.Readonly | Linux.FileFlags.NonBlocking);

                        if((int)fd < 0)
                        {
                            error = true;
                            lastError = Marshal.GetLastWin32Error();
                        }

                        break;
                    }
                default:
                    throw new InvalidOperationException(string.Format("Platform {0} not yet supported.", platformID));
            }

            if(error)
                throw new SystemException(string.Format("Error {0} opening device.", lastError));

            type = DeviceType.Unknown;
            scsiType = Decoders.SCSI.PeripheralDeviceTypes.UnknownDevice;

            // TODO: This is getting error -110 in Linux. Apparently I should set device to standby, request CID/CSD, put device to transition. However I can't get it right now.
            /*
            try
            {
                byte[] csdBuf;
                byte[] scrBuf;
                uint[] mmcResponse;
                double mmcDuration;

                bool mmcSense = ReadCID(out csdBuf, out mmcResponse, 0, out mmcDuration);

                if(!mmcSense)
                {
                    mmcSense = ReadSCR(out scrBuf, out mmcResponse, 0, out mmcDuration);

                    if(!mmcSense)
                        type = DeviceType.SecureDigital;
                    else
                        type = DeviceType.MMC;

                    manufacturer = "To be filled manufacturer";
                    model = "To be filled model";
                    revision = "To be filled revision";
                    serial = "To be filled serial";
                    scsiType = Decoders.SCSI.PeripheralDeviceTypes.DirectAccess;
                    removable = false;
                    return;
                }
                else
                    System.Console.WriteLine("Error {0}: {1}", error, lastError);
            }
            catch(NotImplementedException) { }
            catch(InvalidOperationException) { }
            */

            AtaErrorRegistersCHS errorRegisters;

            byte[] ataBuf;
            byte[] senseBuf;
            byte[] inqBuf;

            bool scsiSense = ScsiInquiry(out inqBuf, out senseBuf);

            if(error)
                throw new SystemException(string.Format("Error {0} trying device.", lastError));

            #region USB
            if(platformID == Interop.PlatformID.Linux)
            {
                if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) || devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) || devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                {
                    string devPath = devicePath.Substring(5);
                    if(System.IO.Directory.Exists("/sys/block/" + devPath))
                    {
                        string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);
                        resolvedLink = "/sys" + resolvedLink.Substring(2);
                        if(!string.IsNullOrEmpty(resolvedLink))
                        {
                            while(resolvedLink.Contains("usb"))
                            {
                                resolvedLink = System.IO.Path.GetDirectoryName(resolvedLink);
                                if(System.IO.File.Exists(resolvedLink + "/descriptors") &&
                                    System.IO.File.Exists(resolvedLink + "/idProduct") &&
                                    System.IO.File.Exists(resolvedLink + "/idVendor"))
                                {
                                    System.IO.FileStream usbFs;
                                    System.IO.StreamReader usbSr;
                                    string usbTemp;

                                    usbFs = new System.IO.FileStream(resolvedLink + "/descriptors", System.IO.FileMode.Open, System.IO.FileAccess.Read);
                                    byte[] usbBuf = new byte[65536];
                                    int usbCount = usbFs.Read(usbBuf, 0, 65536);
                                    usbDescriptors = new byte[usbCount];
                                    Array.Copy(usbBuf, 0, usbDescriptors, 0, usbCount);
                                    usbFs.Close();

                                    usbSr = new System.IO.StreamReader(resolvedLink + "/idProduct");
                                    usbTemp = usbSr.ReadToEnd();
                                    ushort.TryParse(usbTemp, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out usbProduct);
                                    usbSr.Close();

                                    usbSr = new System.IO.StreamReader(resolvedLink + "/idVendor");
                                    usbTemp = usbSr.ReadToEnd();
                                    ushort.TryParse(usbTemp, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out usbVendor);
                                    usbSr.Close();

                                    if(System.IO.File.Exists(resolvedLink + "/manufacturer"))
                                    {
                                        usbSr = new System.IO.StreamReader(resolvedLink + "/manufacturer");
                                        usbManufacturerString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    if(System.IO.File.Exists(resolvedLink + "/product"))
                                    {
                                        usbSr = new System.IO.StreamReader(resolvedLink + "/product");
                                        usbProductString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    if(System.IO.File.Exists(resolvedLink + "/serial"))
                                    {
                                        usbSr = new System.IO.StreamReader(resolvedLink + "/serial");
                                        usbSerialString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    usb = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            // TODO: Implement for other operating systems
            else
                usb = false;
            #endregion USB

            #region FireWire
            if(platformID == Interop.PlatformID.Linux)
            {
                if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) || devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) || devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                {
                    string devPath = devicePath.Substring(5);
                    if(System.IO.Directory.Exists("/sys/block/" + devPath))
                    {
                        string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);
                        resolvedLink = "/sys" + resolvedLink.Substring(2);
                        if(!string.IsNullOrEmpty(resolvedLink))
                        {
                            while(resolvedLink.Contains("firewire"))
                            {
                                resolvedLink = System.IO.Path.GetDirectoryName(resolvedLink);
                                if(System.IO.File.Exists(resolvedLink + "/model") &&
                                    System.IO.File.Exists(resolvedLink + "/vendor") &&
                                    System.IO.File.Exists(resolvedLink + "/guid"))
                                {
                                    System.IO.StreamReader fwSr;
                                    string fwTemp;

                                    fwSr = new System.IO.StreamReader(resolvedLink + "/model");
                                    fwTemp = fwSr.ReadToEnd();
                                    uint.TryParse(fwTemp, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out firewireModel);
                                    fwSr.Close();

                                    fwSr = new System.IO.StreamReader(resolvedLink + "/vendor");
                                    fwTemp = fwSr.ReadToEnd();
                                    uint.TryParse(fwTemp, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out firewireVendor);
                                    fwSr.Close();

                                    fwSr = new System.IO.StreamReader(resolvedLink + "/guid");
                                    fwTemp = fwSr.ReadToEnd();
                                    ulong.TryParse(fwTemp, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out firewireGuid);
                                    fwSr.Close();

                                    if(System.IO.File.Exists(resolvedLink + "/model_name"))
                                    {
                                        fwSr = new System.IO.StreamReader(resolvedLink + "/model_name");
                                        firewireModelName = fwSr.ReadToEnd().Trim();
                                        fwSr.Close();
                                    }

                                    if(System.IO.File.Exists(resolvedLink + "/vendor_name"))
                                    {
                                        fwSr = new System.IO.StreamReader(resolvedLink + "/vendor_name");
                                        firewireVendorName = fwSr.ReadToEnd().Trim();
                                        fwSr.Close();
                                    }

                                    firewire = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            // TODO: Implement for other operating systems
            else
                firewire = false;
            #endregion FireWire

            #region PCMCIA
            if(platformID == Interop.PlatformID.Linux)
            {
                if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) || devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) || devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                {
                    string devPath = devicePath.Substring(5);
                    if(System.IO.Directory.Exists("/sys/block/" + devPath))
                    {
                        string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);
                        resolvedLink = "/sys" + resolvedLink.Substring(2);
                        if(!string.IsNullOrEmpty(resolvedLink))
                        {
                            while(resolvedLink.Contains("/sys/devices"))
                            {
                                resolvedLink = System.IO.Path.GetDirectoryName(resolvedLink);
                                if(System.IO.Directory.Exists(resolvedLink + "/pcmcia_socket"))
                                {
                                    string[] subdirs = System.IO.Directory.GetDirectories(resolvedLink + "/pcmcia_socket", "pcmcia_socket*", System.IO.SearchOption.TopDirectoryOnly);

                                    if(subdirs.Length > 0)
                                    {
                                        string possibleDir = System.IO.Path.Combine(resolvedLink, "pcmcia_socket", subdirs[0]);
                                        if(System.IO.File.Exists(possibleDir + "/card_type") &&
                                           System.IO.File.Exists(possibleDir + "/cis"))
                                        {
                                            System.IO.FileStream cisFs;

                                            cisFs = new System.IO.FileStream(possibleDir + "/cis", System.IO.FileMode.Open, System.IO.FileAccess.Read);
                                            byte[] cisBuf = new byte[65536];
                                            int cisCount = cisFs.Read(cisBuf, 0, 65536);
                                            cis = new byte[cisCount];
                                            Array.Copy(cisBuf, 0, cis, 0, cisCount);
                                            cisFs.Close();

                                            pcmcia = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // TODO: Implement for other operating systems
            else
                pcmcia = false;
            #endregion PCMCIA

            if(!scsiSense)
            {
                Decoders.SCSI.Inquiry.SCSIInquiry? Inquiry = Decoders.SCSI.Inquiry.Decode(inqBuf);

                type = DeviceType.SCSI;
                bool serialSense = ScsiInquiry(out inqBuf, out senseBuf, 0x80);
                if(!serialSense)
                    serial = Decoders.SCSI.EVPD.DecodePage80(inqBuf);

                if(Inquiry.HasValue)
                {
                    string tmp = StringHandlers.CToString(Inquiry.Value.ProductRevisionLevel);
                    if(tmp != null)
                        revision = tmp.Trim();
                    tmp = StringHandlers.CToString(Inquiry.Value.ProductIdentification);
                    if(tmp != null)
                        model = tmp.Trim();
                    tmp = StringHandlers.CToString(Inquiry.Value.VendorIdentification);
                    if(tmp != null)
                        manufacturer = tmp.Trim();
                    removable = Inquiry.Value.RMB;

                    scsiType = (Decoders.SCSI.PeripheralDeviceTypes)Inquiry.Value.PeripheralDeviceType;
                }

                bool atapiSense = AtapiIdentify(out ataBuf, out errorRegisters);

                if(!atapiSense)
                {
                    type = DeviceType.ATAPI;
                    Identify.IdentifyDevice? ATAID = Identify.Decode(ataBuf);

                    if(ATAID.HasValue)
                        serial = ATAID.Value.SerialNumber;
                }
            }

            if((scsiSense && (usb || firewire)) || manufacturer == "ATA")
            {
                bool ataSense = AtaIdentify(out ataBuf, out errorRegisters);
                if(!ataSense)
                {
                    type = DeviceType.ATA;
                    Identify.IdentifyDevice? ATAID = Identify.Decode(ataBuf);

                    if(ATAID.HasValue)
                    {
                        string[] separated = ATAID.Value.Model.Split(' ');

                        if(separated.Length == 1)
                            model = separated[0];
                        else
                        {
                            manufacturer = separated[0];
                            model = separated[separated.Length - 1];
                        }

                        revision = ATAID.Value.FirmwareRevision;
                        serial = ATAID.Value.SerialNumber;

                        scsiType = Decoders.SCSI.PeripheralDeviceTypes.DirectAccess;

                        if((ushort)ATAID.Value.GeneralConfiguration != 0x848A)
                        {
                            removable |= (ATAID.Value.GeneralConfiguration & Identify.GeneralConfigurationBit.Removable) == Identify.GeneralConfigurationBit.Removable;
                        }
                        else
                            compactFlash = true;
                    }
                }
            }

            if(type == DeviceType.Unknown)
            {
                manufacturer = null;
                model = null;
                revision = null;
                serial = null;
            }

            if(usb)
            {
                if(string.IsNullOrEmpty(manufacturer))
                    manufacturer = usbManufacturerString;
                if(string.IsNullOrEmpty(model))
                    model = usbProductString;
                if(string.IsNullOrEmpty(serial))
                    serial = usbSerialString;
                else
                {
                    foreach(char c in serial)
                    {
                        if(char.IsControl(c))
                            serial = usbSerialString;
                    }
                }
            }

            if(firewire)
            {
                if(string.IsNullOrEmpty(manufacturer))
                    manufacturer = firewireVendorName;
                if(string.IsNullOrEmpty(model))
                    model = firewireModelName;
                if(string.IsNullOrEmpty(serial))
                    serial = string.Format("{0:X16}", firewireGuid);
                else
                {
                    foreach(char c in serial)
                    {
                        if(char.IsControl(c))
                            serial = string.Format("{0:X16}", firewireGuid);
                    }
                }
            }
        }
    }
}

