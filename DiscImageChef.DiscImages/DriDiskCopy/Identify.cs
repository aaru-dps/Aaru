// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Digital Research's DISKCOPY disk images.
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class DriDiskCopy
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if((stream.Length - Marshal.SizeOf(typeof(DriFooter))) % 512 != 0) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(DriFooter))];
            stream.Seek(-buffer.Length, SeekOrigin.End);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            DriFooter tmpFooter = (DriFooter)Marshal.PtrToStructure(ftrPtr, typeof(DriFooter));
            Marshal.FreeHGlobal(ftrPtr);

            string sig = StringHandlers.CToString(tmpFooter.signature);

            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.signature = \"{0}\"", sig);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.five = {0}",      tmpFooter.bpb.five);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.driveCode = {0}", tmpFooter.bpb.driveCode);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown = {0}",   tmpFooter.bpb.unknown);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.cylinders = {0}", tmpFooter.bpb.cylinders);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown2 = {0}",  tmpFooter.bpb.unknown2);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.bps = {0}",       tmpFooter.bpb.bps);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.spc = {0}",       tmpFooter.bpb.spc);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.rsectors = {0}",  tmpFooter.bpb.rsectors);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.fats_no = {0}",   tmpFooter.bpb.fats_no);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sectors = {0}",   tmpFooter.bpb.sectors);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.media_descriptor = {0}",
                                      tmpFooter.bpb.media_descriptor);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.spfat = {0}",    tmpFooter.bpb.spfat);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sptrack = {0}",  tmpFooter.bpb.sptrack);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.heads = {0}",    tmpFooter.bpb.heads);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.hsectors = {0}", tmpFooter.bpb.hsectors);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.drive_no = {0}", tmpFooter.bpb.drive_no);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown3 = {0}", tmpFooter.bpb.unknown3);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown4 = {0}", tmpFooter.bpb.unknown4);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sptrack2 = {0}", tmpFooter.bpb.sptrack2);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin",
                                      "ArrayHelpers.ArrayIsNullOrEmpty(tmp_footer.bpb.unknown5) = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(tmpFooter.bpb.unknown5));

            Regex regexSignature = new Regex(REGEX_DRI);
            Match matchSignature = regexSignature.Match(sig);

            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "MatchSignature.Success? = {0}", matchSignature.Success);

            if(!matchSignature.Success) return false;

            if(tmpFooter.bpb.sptrack * tmpFooter.bpb.cylinders * tmpFooter.bpb.heads != tmpFooter.bpb.sectors)
                return false;

            return tmpFooter.bpb.sectors * tmpFooter.bpb.bps + Marshal.SizeOf(tmpFooter) == stream.Length;
        }
    }
}