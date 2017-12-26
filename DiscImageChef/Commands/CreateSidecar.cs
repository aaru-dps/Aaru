// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CreateSidecar.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'create-sidecar' verb.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;
using Schemas;

namespace DiscImageChef.Commands
{
    static class CreateSidecar
    {
        internal static void DoSidecar(CreateSidecarOptions options)
        {
            Sidecar.InitProgressEvent += Progress.InitProgress;
            Sidecar.UpdateProgressEvent += Progress.UpdateProgress;
            Sidecar.EndProgressEvent += Progress.EndProgress;
            Sidecar.InitProgressEvent2 += Progress.InitProgress2;
            Sidecar.UpdateProgressEvent2 += Progress.UpdateProgress2;
            Sidecar.EndProgressEvent2 += Progress.EndProgress2;
            Sidecar.UpdateStatusEvent += Progress.UpdateStatus;

            Encoding encoding = null;

            if(options.EncodingName != null)
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(options.EncodingName);
                    if(options.Verbose) DicConsole.VerboseWriteLine("Using encoding for {0}.", encoding.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");
                    return;
                }

            if(File.Exists(options.InputFile))
            {
                if(options.Tape)
                {
                    DicConsole.ErrorWriteLine("You cannot use --tape option when input is a file.");
                    return;
                }

                FiltersList filtersList = new FiltersList();
                Filter inputFilter = filtersList.GetFilter(options.InputFile);

                if(inputFilter == null)
                {
                    DicConsole.ErrorWriteLine("Cannot open specified file.");
                    return;
                }

                try
                {
                    ImagePlugin imageFormat = ImageFormat.Detect(inputFilter);

                    if(imageFormat == null)
                    {
                        DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");
                        return;
                    }

                    if(options.Verbose)
                        DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", imageFormat.Name,
                                                    imageFormat.PluginUuid);
                    else DicConsole.WriteLine("Image format identified by {0}.", imageFormat.Name);

                    try
                    {
                        if(!imageFormat.OpenImage(inputFilter))
                        {
                            DicConsole.WriteLine("Unable to open image format");
                            DicConsole.WriteLine("No error given");
                            return;
                        }

                        DicConsole.DebugWriteLine("Analyze command", "Correctly opened image file.");
                    }
                    catch(Exception ex)
                    {
                        DicConsole.ErrorWriteLine("Unable to open image format");
                        DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                        return;
                    }

                    Core.Statistics.AddMediaFormat(imageFormat.ImageFormat);
                    Core.Statistics.AddFilter(inputFilter.Name);

                    CICMMetadataType sidecar =
                        Sidecar.Create(imageFormat, options.InputFile, inputFilter.UUID, encoding);

                    DicConsole.WriteLine("Writing metadata sidecar");

                    FileStream xmlFs =
                        new
                            FileStream(Path.Combine(Path.GetDirectoryName(options.InputFile) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(options.InputFile) + ".cicm.xml"),
                                       FileMode.CreateNew);

                    XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                    xmlSer.Serialize(xmlFs, sidecar);
                    xmlFs.Close();

                    Core.Statistics.AddCommand("create-sidecar");
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                    DicConsole.DebugWriteLine("Analyze command", ex.StackTrace);
                }
            }
            else if(Directory.Exists(options.InputFile))
            {
                if(!options.Tape)
                {
                    DicConsole.ErrorWriteLine("Cannot create a sidecar from a directory.");
                    return;
                }

                string[] contents = Directory.GetFiles(options.InputFile, "*", SearchOption.TopDirectoryOnly);
                List<string> files = contents.Where(file => new FileInfo(file).Length % options.BlockSize == 0)
                                             .ToList();

                files.Sort(StringComparer.CurrentCultureIgnoreCase);

                CICMMetadataType sidecar =
                    Sidecar.Create(Path.GetFileName(options.InputFile), files, options.BlockSize);

                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs =
                    new
                        FileStream(Path.Combine(Path.GetDirectoryName(options.InputFile) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(options.InputFile) + ".cicm.xml"),
                                   FileMode.CreateNew);

                XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();

                Core.Statistics.AddCommand("create-sidecar");
            }
            else DicConsole.ErrorWriteLine("The specified input file cannot be found.");
        }
    }
}