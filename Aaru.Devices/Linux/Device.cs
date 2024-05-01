// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Device.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prepares a Linux device for direct access.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SecureDigital;
using Aaru.Helpers;
using Marshal = System.Runtime.InteropServices.Marshal;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using VendorString = Aaru.Decoders.MMC.VendorString;

namespace Aaru.Devices.Linux;

/// <inheritdoc />
[SupportedOSPlatform("linux")]
partial class Device : Devices.Device
{
    /// <summary>Gets the file handle representing this device</summary>
    /// <value>The file handle</value>
    int _fileDescriptor;

    Device() {}

    internal new static Device Create(string devicePath, out ErrorNumber errno)
    {
        errno = ErrorNumber.NoError;

        var dev = new Device
        {
            PlatformId      = DetectOS.GetRealPlatformID(),
            Timeout         = 15,
            Error           = false,
            IsRemovable     = false,
            _fileDescriptor = Extern.open(devicePath, FileFlags.ReadWrite | FileFlags.NonBlocking | FileFlags.CreateNew)
        };

        if(dev._fileDescriptor < 0)
        {
            dev.LastError = Marshal.GetLastWin32Error();

            if(dev.LastError is 13 or 30) // EACCES or EROFS
            {
                dev._fileDescriptor = Extern.open(devicePath, FileFlags.Readonly | FileFlags.NonBlocking);

                if(dev._fileDescriptor < 0)
                {
                    dev.Error     = true;
                    dev.LastError = Marshal.GetLastWin32Error();
                }
            }
            else
                dev.Error = true;

            dev.LastError = Marshal.GetLastWin32Error();
        }

        if(dev.Error)
        {
            errno = (ErrorNumber)dev.LastError;

            return null;
        }

        // Seems ioctl(2) does not allow the atomicity needed
        if(dev.PlatformId == PlatformID.Linux) ReadMultipleBlockCannotSetBlockCount = true;

        dev.Type     = DeviceType.Unknown;
        dev.ScsiType = PeripheralDeviceTypes.UnknownDevice;

        if(dev.Error)
        {
            errno = (ErrorNumber)dev.LastError;

            return null;
        }

        string devPath;

        if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
           devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
           devicePath.StartsWith("/dev/st", StringComparison.Ordinal) ||
           devicePath.StartsWith("/dev/sg", StringComparison.Ordinal))
        {
            if(!dev.ScsiInquiry(out byte[] _, out _))
                dev.Type = DeviceType.SCSI;

            // MultiMediaCard and SecureDigital go here
            else if(devicePath.StartsWith("/dev/mmcblk", StringComparison.Ordinal))
            {
                devPath = devicePath[5..];

                if(File.Exists("/sys/block/" + devPath + "/device/csd"))
                {
                    int len = ConvertFromFileHexAscii("/sys/block/" + devPath + "/device/csd", out dev.CachedCsd);

                    if(len == 0) dev.CachedCsd = null;
                }

                if(File.Exists("/sys/block/" + devPath + "/device/cid"))
                {
                    int len = ConvertFromFileHexAscii("/sys/block/" + devPath + "/device/cid", out dev.CachedCid);

                    if(len == 0) dev.CachedCid = null;
                }

                if(File.Exists("/sys/block/" + devPath + "/device/scr"))
                {
                    int len = ConvertFromFileHexAscii("/sys/block/" + devPath + "/device/scr", out dev.CachedScr);

                    if(len == 0) dev.CachedScr = null;
                }

                if(File.Exists("/sys/block/" + devPath + "/device/ocr"))
                {
                    int len = ConvertFromFileHexAscii("/sys/block/" + devPath + "/device/ocr", out dev.CachedOcr);

                    if(len == 0) dev.CachedOcr = null;
                }
            }
        }

#region SecureDigital / MultiMediaCard

        if(dev.CachedCid != null)
        {
            dev.ScsiType    = PeripheralDeviceTypes.DirectAccess;
            dev.IsRemovable = false;

            if(dev.CachedScr != null)
            {
                dev.Type = DeviceType.SecureDigital;
                CID decoded = Decoders.SecureDigital.Decoders.DecodeCID(dev.CachedCid);
                dev.Manufacturer = VendorString.Prettify(decoded.Manufacturer);
                dev.Model        = decoded.ProductName;

                dev.FirmwareRevision =
                    $"{(decoded.ProductRevision & 0xF0) >> 4:X2}.{decoded.ProductRevision & 0x0F:X2}";

                dev.Serial = $"{decoded.ProductSerialNumber}";
            }
            else
            {
                dev.Type = DeviceType.MMC;
                Decoders.MMC.CID decoded = Decoders.MMC.Decoders.DecodeCID(dev.CachedCid);
                dev.Manufacturer = VendorString.Prettify(decoded.Manufacturer);
                dev.Model        = decoded.ProductName;

                dev.FirmwareRevision =
                    $"{(decoded.ProductRevision & 0xF0) >> 4:X2}.{decoded.ProductRevision & 0x0F:X2}";

                dev.Serial = $"{decoded.ProductSerialNumber}";
            }

            return dev;
        }

#endregion SecureDigital / MultiMediaCard

#region USB

        string resolvedLink;

        if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
           devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
           devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
        {
            devPath = devicePath[5..];

            if(Directory.Exists("/sys/block/" + devPath))
            {
                resolvedLink = ReadLink("/sys/block/" + devPath);

                if(!string.IsNullOrEmpty(resolvedLink))
                {
                    resolvedLink = "/sys" + resolvedLink[2..];

                    while(resolvedLink?.Contains("usb") == true)
                    {
                        resolvedLink = Path.GetDirectoryName(resolvedLink);

                        if(!File.Exists(resolvedLink + "/descriptors") ||
                           !File.Exists(resolvedLink + "/idProduct")   ||
                           !File.Exists(resolvedLink + "/idVendor"))
                            continue;

                        var usbFs = new FileStream(resolvedLink + "/descriptors", FileMode.Open, FileAccess.Read);

                        var usbBuf   = new byte[65536];
                        int usbCount = usbFs.EnsureRead(usbBuf, 0, 65536);
                        dev.UsbDescriptors = new byte[usbCount];
                        Array.Copy(usbBuf, 0, dev.UsbDescriptors, 0, usbCount);
                        usbFs.Close();

                        var    usbSr   = new StreamReader(resolvedLink + "/idProduct");
                        string usbTemp = usbSr.ReadToEnd();

                        ushort.TryParse(usbTemp,
                                        NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture,
                                        out dev.UsbProduct);

                        usbSr.Close();

                        usbSr   = new StreamReader(resolvedLink + "/idVendor");
                        usbTemp = usbSr.ReadToEnd();

                        ushort.TryParse(usbTemp,
                                        NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture,
                                        out dev.UsbVendor);

                        usbSr.Close();

                        if(File.Exists(resolvedLink + "/manufacturer"))
                        {
                            usbSr                     = new StreamReader(resolvedLink + "/manufacturer");
                            dev.UsbManufacturerString = usbSr.ReadToEnd().Trim();
                            usbSr.Close();
                        }

                        if(File.Exists(resolvedLink + "/product"))
                        {
                            usbSr                = new StreamReader(resolvedLink + "/product");
                            dev.UsbProductString = usbSr.ReadToEnd().Trim();
                            usbSr.Close();
                        }

                        if(File.Exists(resolvedLink + "/serial"))
                        {
                            usbSr               = new StreamReader(resolvedLink + "/serial");
                            dev.UsbSerialString = usbSr.ReadToEnd().Trim();
                            usbSr.Close();
                        }

                        dev.IsUsb = true;

                        break;
                    }
                }
            }
        }

#endregion USB

#region FireWire

        if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
           devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
           devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
        {
            devPath = devicePath[5..];

            if(Directory.Exists("/sys/block/" + devPath))
            {
                resolvedLink = ReadLink("/sys/block/" + devPath);
                resolvedLink = "/sys" + resolvedLink[2..];

                if(!string.IsNullOrEmpty(resolvedLink))
                {
                    while(resolvedLink?.Contains("firewire") == true)
                    {
                        resolvedLink = Path.GetDirectoryName(resolvedLink);

                        if(!File.Exists(resolvedLink + "/model")  ||
                           !File.Exists(resolvedLink + "/vendor") ||
                           !File.Exists(resolvedLink + "/guid"))
                            continue;

                        var    fwSr   = new StreamReader(resolvedLink + "/model");
                        string fwTemp = fwSr.ReadToEnd();

                        uint.TryParse(fwTemp,
                                      NumberStyles.HexNumber,
                                      CultureInfo.InvariantCulture,
                                      out dev.FirewireModel);

                        fwSr.Close();

                        fwSr   = new StreamReader(resolvedLink + "/vendor");
                        fwTemp = fwSr.ReadToEnd();

                        uint.TryParse(fwTemp,
                                      NumberStyles.HexNumber,
                                      CultureInfo.InvariantCulture,
                                      out dev.FirewireVendor);

                        fwSr.Close();

                        fwSr   = new StreamReader(resolvedLink + "/guid");
                        fwTemp = fwSr.ReadToEnd();

                        ulong.TryParse(fwTemp,
                                       NumberStyles.HexNumber,
                                       CultureInfo.InvariantCulture,
                                       out dev.FirewireGuid);

                        fwSr.Close();

                        if(File.Exists(resolvedLink + "/model_name"))
                        {
                            fwSr                  = new StreamReader(resolvedLink + "/model_name");
                            dev.FireWireModelName = fwSr.ReadToEnd().Trim();
                            fwSr.Close();
                        }

                        if(File.Exists(resolvedLink + "/vendor_name"))
                        {
                            fwSr                   = new StreamReader(resolvedLink + "/vendor_name");
                            dev.FireWireVendorName = fwSr.ReadToEnd().Trim();
                            fwSr.Close();
                        }

                        dev.IsFireWire = true;

                        break;
                    }
                }
            }
        }

#endregion FireWire

#region PCMCIA

        if(!devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) &&
           !devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) &&
           !devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
            return dev;

        devPath = devicePath[5..];

        if(!Directory.Exists("/sys/block/" + devPath)) return dev;

        resolvedLink = ReadLink("/sys/block/" + devPath);
        resolvedLink = "/sys" + resolvedLink[2..];

        if(string.IsNullOrEmpty(resolvedLink)) return dev;

        while(resolvedLink.Contains("/sys/devices"))
        {
            resolvedLink = Path.GetDirectoryName(resolvedLink);

            if(!Directory.Exists(resolvedLink + "/pcmcia_socket")) continue;

            string[] subdirs = Directory.GetDirectories(resolvedLink + "/pcmcia_socket",
                                                        "pcmcia_socket*",
                                                        SearchOption.TopDirectoryOnly);

            if(subdirs.Length <= 0) continue;

            string possibleDir = Path.Combine(resolvedLink, "pcmcia_socket", subdirs[0]);

            if(!File.Exists(possibleDir + "/card_type") || !File.Exists(possibleDir + "/cis")) continue;

            var cisFs = new FileStream(possibleDir + "/cis", FileMode.Open, FileAccess.Read);

            var cisBuf   = new byte[65536];
            int cisCount = cisFs.EnsureRead(cisBuf, 0, 65536);
            dev.Cis = new byte[cisCount];
            Array.Copy(cisBuf, 0, dev.Cis, 0, cisCount);
            cisFs.Close();

            dev.IsPcmcia = true;

            break;
        }

#endregion PCMCIA

        return dev;
    }

    static int ConvertFromFileHexAscii(string file, out byte[] outBuf)
    {
        var    sr  = new StreamReader(file);
        string ins = sr.ReadToEnd().Trim();

        int count = Helpers.Marshal.ConvertFromHexAscii(ins, out outBuf);

        sr.Close();

        return count;
    }

    /// <inheritdoc />
    public override void Close()
    {
        if(_fileDescriptor == 0) return;

        Extern.close(_fileDescriptor);

        _fileDescriptor = 0;
    }
}