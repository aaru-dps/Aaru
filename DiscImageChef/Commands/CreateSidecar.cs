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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using Schemas;

namespace DiscImageChef.Commands
{
    public static class CreateSidecar
    {
        public static void doSidecar(CreateSidecarOptions options)
        {
            Sidecar.InitProgressEvent += Progress.InitProgress;
            Sidecar.UpdateProgressEvent += Progress.UpdateProgress;
            Sidecar.EndProgressEvent += Progress.EndProgress;
            Sidecar.InitProgressEvent2 += Progress.InitProgress2;
            Sidecar.UpdateProgressEvent2 += Progress.UpdateProgress2;
            Sidecar.EndProgressEvent2 += Progress.EndProgress2;
            Sidecar.UpdateStatusEvent += Progress.UpdateStatus;

            if(File.Exists(options.InputFile))
            {
                if(options.Tape)
                {
                    DicConsole.ErrorWriteLine("You cannot use --tape option when input is a file.");
                    return;
                }

                ImagePlugin _imageFormat;

                FiltersList filtersList = new FiltersList();
                Filter inputFilter = filtersList.GetFilter(options.InputFile);

                if(inputFilter == null)
                {
                    DicConsole.ErrorWriteLine("Cannot open specified file.");
                    return;
                }

                try
                {
                    _imageFormat = ImageFormat.Detect(inputFilter);

                    if(_imageFormat == null)
                    {
                        DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");
                        return;
                    }
                    else
                    {
                        if(options.Verbose)
                            DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", _imageFormat.Name, _imageFormat.PluginUUID);
                        else
                            DicConsole.WriteLine("Image format identified by {0}.", _imageFormat.Name);
                    }

                    try
                    {
                        if(!_imageFormat.OpenImage(inputFilter))
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

                    Core.Statistics.AddMediaFormat(_imageFormat.GetImageFormat());
                    Core.Statistics.AddFilter(inputFilter.Name);

                    CICMMetadataType sidecar = Sidecar.Create(_imageFormat, options.InputFile, inputFilter.UUID);

                    DicConsole.WriteLine("Writing metadata sidecar");

                    FileStream xmlFs = new FileStream(Path.GetDirectoryName(options.InputFile) +
                                       //Path.PathSeparator +
                                       Path.GetFileNameWithoutExtension(options.InputFile) + ".cicm.xml",
                                           FileMode.CreateNew);

                    System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));
                    xmlSer.Serialize(xmlFs, sidecar);
                    xmlFs.Close();

                    Core.Statistics.AddCommand("create-sidecar");
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine(string.Format("Error reading file: {0}", ex.Message));
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
                List<string> files = new List<string>();

                foreach(string file in contents)
                {
                    if(new FileInfo(file).Length % options.BlockSize == 0)
                        files.Add(file);
                }

                files.Sort(StringComparer.CurrentCultureIgnoreCase);

                CICMMetadataType sidecar = Sidecar.Create(Path.GetFileName(options.InputFile), files, options.BlockSize);

                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(Path.GetDirectoryName(options.InputFile) +
                                   //Path.PathSeparator +
                                   Path.GetFileName(options.InputFile) + ".cicm.xml",
                                       FileMode.CreateNew);

                System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();

                Core.Statistics.AddCommand("create-sidecar");
            }
            else
            {
                DicConsole.ErrorWriteLine("The specified input file cannot be found.");
                return;
            }
        }
    }
}

