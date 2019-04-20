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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Mono.Options;
using Schemas;

namespace DiscImageChef.Commands
{
    class CreateSidecarCommand : Command
    {
        int    blockSize;
        string encodingName;
        string inputFile;
        bool   showHelp;
        bool   tape;

        public CreateSidecarCommand() : base("create-sidecar", "Creates CICM Metadata XML sidecar.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] input",
                "",
                Help,
                {
                    "block-size|b=",
                    "Only used for tapes, indicates block size. Files in the folder whose size is not a multiple of this value will simply be ignored.",
                    (int i) => blockSize = i
                },
                {"encoding|e=", "Name of character encoding to use.", s => encodingName = s},
                {
                    "tape|t",
                    "When used indicates that input is a folder containing alphabetically sorted files extracted from a linear block-based tape with fixed block size (e.g. a SCSI tape device).",
                    b => tape = b != null
                },
                {"help|h|?", "Show this message and exit.", v => showHelp = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return (int)ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();
            if(MainClass.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
            if(MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
            Statistics.AddCommand("create-sidecar");

            if(extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int)ErrorNumber.UnexpectedArgumentCount;
            }

            if(extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing input image.");
                return (int)ErrorNumber.MissingArgument;
            }

            inputFile = extra[0];

            DicConsole.DebugWriteLine("Create sidecar command", "--block-size={0}", blockSize);
            DicConsole.DebugWriteLine("Create sidecar command", "--debug={0}",      MainClass.Debug);
            DicConsole.DebugWriteLine("Create sidecar command", "--encoding={0}",   encodingName);
            DicConsole.DebugWriteLine("Create sidecar command", "--input={0}",      inputFile);
            DicConsole.DebugWriteLine("Create sidecar command", "--tape={0}",       tape);
            DicConsole.DebugWriteLine("Create sidecar command", "--verbose={0}",    MainClass.Verbose);

            Encoding encoding = null;

            if(encodingName != null)
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(encodingName);
                    if(MainClass.Verbose) DicConsole.VerboseWriteLine("Using encoding for {0}.", encoding.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");
                    return (int)ErrorNumber.EncodingUnknown;
                }

            if(File.Exists(inputFile))
            {
                if(tape)
                {
                    DicConsole.ErrorWriteLine("You cannot use --tape option when input is a file.");
                    return (int)ErrorNumber.ExpectedDirectory;
                }

                FiltersList filtersList = new FiltersList();
                IFilter     inputFilter = filtersList.GetFilter(inputFile);

                if(inputFilter == null)
                {
                    DicConsole.ErrorWriteLine("Cannot open specified file.");
                    return (int)ErrorNumber.CannotOpenFile;
                }

                try
                {
                    IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                    if(imageFormat == null)
                    {
                        DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");
                        return (int)ErrorNumber.UnrecognizedFormat;
                    }

                    if(MainClass.Verbose)
                        DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", imageFormat.Name,
                                                    imageFormat.Id);
                    else DicConsole.WriteLine("Image format identified by {0}.", imageFormat.Name);

                    try
                    {
                        if(!imageFormat.Open(inputFilter))
                        {
                            DicConsole.WriteLine("Unable to open image format");
                            DicConsole.WriteLine("No error given");
                            return (int)ErrorNumber.CannotOpenFormat;
                        }

                        DicConsole.DebugWriteLine("Analyze command", "Correctly opened image file.");
                    }
                    catch(Exception ex)
                    {
                        DicConsole.ErrorWriteLine("Unable to open image format");
                        DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                        return (int)ErrorNumber.CannotOpenFormat;
                    }

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddFilter(inputFilter.Name);

                    Sidecar sidecarClass = new Sidecar(imageFormat, inputFile, inputFilter.Id, encoding);
                    sidecarClass.InitProgressEvent    += Progress.InitProgress;
                    sidecarClass.UpdateProgressEvent  += Progress.UpdateProgress;
                    sidecarClass.EndProgressEvent     += Progress.EndProgress;
                    sidecarClass.InitProgressEvent2   += Progress.InitProgress2;
                    sidecarClass.UpdateProgressEvent2 += Progress.UpdateProgress2;
                    sidecarClass.EndProgressEvent2    += Progress.EndProgress2;
                    sidecarClass.UpdateStatusEvent    += Progress.UpdateStatus;
                    System.Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        sidecarClass.Abort();
                    };
                    CICMMetadataType sidecar = sidecarClass.Create();

                    DicConsole.WriteLine("Writing metadata sidecar");

                    FileStream xmlFs =
                        new
                            FileStream(Path.Combine(Path.GetDirectoryName(inputFile) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(inputFile) + ".cicm.xml"),
                                       FileMode.CreateNew);

                    XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                    xmlSer.Serialize(xmlFs, sidecar);
                    xmlFs.Close();
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                    DicConsole.DebugWriteLine("Analyze command", ex.StackTrace);
                    return (int)ErrorNumber.UnexpectedException;
                }
            }
            else if(Directory.Exists(inputFile))
            {
                if(!tape)
                {
                    DicConsole.ErrorWriteLine("Cannot create a sidecar from a directory.");
                    return (int)ErrorNumber.ExpectedFile;
                }

                string[]     contents = Directory.GetFiles(inputFile, "*", SearchOption.TopDirectoryOnly);
                List<string> files    = contents.Where(file => new FileInfo(file).Length % blockSize == 0).ToList();

                files.Sort(StringComparer.CurrentCultureIgnoreCase);

                Sidecar sidecarClass = new Sidecar();
                sidecarClass.InitProgressEvent    += Progress.InitProgress;
                sidecarClass.UpdateProgressEvent  += Progress.UpdateProgress;
                sidecarClass.EndProgressEvent     += Progress.EndProgress;
                sidecarClass.InitProgressEvent2   += Progress.InitProgress2;
                sidecarClass.UpdateProgressEvent2 += Progress.UpdateProgress2;
                sidecarClass.EndProgressEvent2    += Progress.EndProgress2;
                sidecarClass.UpdateStatusEvent    += Progress.UpdateStatus;
                CICMMetadataType sidecar = sidecarClass.BlockTape(Path.GetFileName(inputFile), files, blockSize);

                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs =
                    new
                        FileStream(Path.Combine(Path.GetDirectoryName(inputFile) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(inputFile) + ".cicm.xml"),
                                   FileMode.CreateNew);

                XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }
            else DicConsole.ErrorWriteLine("The specified input file cannot be found.");

            return (int)ErrorNumber.NoError;
        }
    }
}