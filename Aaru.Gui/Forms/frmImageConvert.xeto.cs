// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : frmImageConvert.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Image conversion window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements converting media image.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see ;http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Gui.Forms
{
    public class frmImageConvert : Form
    {
        bool                   cancel;
        CICMMetadataType       cicmMetadata;
        List<DumpHardwareType> dumpHardware;
        IMediaImage            inputFormat;

        public frmImageConvert(IMediaImage inputFormat, string imageSource)
        {
            this.inputFormat = inputFormat;
            XamlReader.Load(this);
            cancel = false;

            txtSource.Text               = imageSource;
            btnCreator.Visible           = !string.IsNullOrWhiteSpace(inputFormat.Info.Creator);
            btnMediaTitle.Visible        = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaTitle);
            btnComments.Visible          = !string.IsNullOrWhiteSpace(inputFormat.Info.Comments);
            btnMediaManufacturer.Visible = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaManufacturer);
            btnMediaModel.Visible        = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaModel);
            btnMediaSerialNumber.Visible = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaSerialNumber);
            btnMediaBarcode.Visible      = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaBarcode);
            btnMediaPartNumber.Visible   = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaPartNumber);
            btnMediaSequence.Visible =
                inputFormat.Info.MediaSequence != 0 && inputFormat.Info.LastMediaSequence != 0;
            btnLastMediaSequence.Visible =
                inputFormat.Info.MediaSequence != 0 && inputFormat.Info.LastMediaSequence != 0;
            btnDriveManufacturer.Visible     = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveManufacturer);
            btnDriveModel.Visible            = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveModel);
            btnDriveSerialNumber.Visible     = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveSerialNumber);
            btnDriveFirmwareRevision.Visible = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveFirmwareRevision);

            ObservableCollection<IWritableImage> lstPlugins = new ObservableCollection<IWritableImage>();
            PluginBase                           plugins    = GetPluginBase.Instance;
            foreach(IWritableImage plugin in
                plugins.WritableImages.Values.Where(p => p.SupportedMediaTypes.Contains(inputFormat.Info.MediaType)))
                lstPlugins.Add(plugin);
            cmbFormat.ItemTextBinding = Binding.Property((IWritableImage p) => p.Name);
            cmbFormat.ItemKeyBinding  = Binding.Property((IWritableImage p) => p.Id.ToString());
            cmbFormat.DataStore       = lstPlugins;

            btnCicmXmlFromImage.Visible    = inputFormat.CicmMetadata != null;
            btnResumeFileFromImage.Visible = inputFormat.DumpHardware != null && inputFormat.DumpHardware.Any();
            cicmMetadata                   = inputFormat.CicmMetadata;
            dumpHardware = inputFormat.DumpHardware != null && inputFormat.DumpHardware.Any()
                               ? inputFormat.DumpHardware
                               : null;

            txtCicmXml.Text    = cicmMetadata == null ? "" : "<From image>";
            txtResumeFile.Text = dumpHardware == null ? "" : "<From image>";
        }

        protected void OnBtnStart(object sender, EventArgs e)
        {
            if(!(cmbFormat.SelectedValue is IWritableImage plugin))
            {
                MessageBox.Show("Error trying to find selected plugin", MessageBoxType.Error);
                return;
            }

            new Thread(DoWork).Start(plugin);
        }

        void DoWork(object plugin)
        {
            bool warning = false;
            if(!(plugin is IWritableImage outputFormat))
            {
                MessageBox.Show("Error trying to find selected plugin", MessageBoxType.Error);
                return;
            }

            IOpticalMediaImage    inputOptical  = inputFormat as IOpticalMediaImage;
            IWritableOpticalImage outputOptical = outputFormat as IWritableOpticalImage;

            List<Track> tracks;

            try { tracks = inputOptical?.Tracks; }
            catch(Exception) { tracks = null; }

            // Prepare UI
            Application.Instance.Invoke(() =>
            {
                btnClose.Visible       = false;
                btnStart.Visible       = false;
                btnStop.Visible        = true;
                stkProgress.Visible    = true;
                stkOptions.Visible     = false;
                btnStop.Enabled        = true;
                cmbFormat.ReadOnly     = true;
                btnDestination.Visible = false;

                prgProgress.MaxValue =  1;
                prgProgress.MaxValue += inputFormat.Info.ReadableMediaTags.Count;
                prgProgress.MaxValue++;

                if(tracks != null) prgProgress.MaxValue++;

                if(tracks == null)
                {
                    prgProgress.MaxValue += 2;

                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                    {
                        switch(tag)
                        {
                            case SectorTagType.AppleSectorTag:
                            case SectorTagType.CdSectorSync:
                            case SectorTagType.CdSectorHeader:
                            case SectorTagType.CdSectorSubHeader:
                            case SectorTagType.CdSectorEdc:
                            case SectorTagType.CdSectorEccP:
                            case SectorTagType.CdSectorEccQ:
                            case SectorTagType.CdSectorEcc:
                                // This tags are inline in long sector
                                continue;
                        }

                        if(chkForce.Checked == true && !outputFormat.SupportedSectorTags.Contains(tag)) continue;

                        prgProgress.MaxValue++;
                    }
                }
                else
                {
                    prgProgress.MaxValue += tracks.Count;

                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t))
                    {
                        switch(tag)
                        {
                            case SectorTagType.AppleSectorTag:
                            case SectorTagType.CdSectorSync:
                            case SectorTagType.CdSectorHeader:
                            case SectorTagType.CdSectorSubHeader:
                            case SectorTagType.CdSectorEdc:
                            case SectorTagType.CdSectorEccP:
                            case SectorTagType.CdSectorEccQ:
                            case SectorTagType.CdSectorEcc:
                                // This tags are inline in long sector
                                continue;
                        }

                        if(chkForce.Checked == true && !outputFormat.SupportedSectorTags.Contains(tag)) continue;

                        prgProgress.MaxValue += tracks.Count;
                    }
                }

                if(dumpHardware != null) prgProgress.MaxValue++;
                if(cicmMetadata != null) prgProgress.MaxValue++;

                prgProgress.MaxValue++;
            });

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(outputFormat.SupportedMediaTags.Contains(mediaTag) || chkForce.Checked == true) continue;

                Application.Instance.Invoke(() =>
                {
                    MessageBox
                       .Show($"Converting image will lose media tag {mediaTag}, not continuing...",
                             MessageBoxType.Error);
                });

                return;
            }

            bool useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

            foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags)
            {
                if(outputFormat.SupportedSectorTags.Contains(sectorTag)) continue;

                if(chkForce.Checked == true)
                {
                    if(sectorTag != SectorTagType.CdTrackFlags && sectorTag != SectorTagType.CdTrackIsrc &&
                       sectorTag != SectorTagType.CdSectorSubchannel) useLong = false;
                    continue;
                }

                Application.Instance.Invoke(() =>
                {
                    MessageBox
                       .Show($"Converting image will lose sector tag {sectorTag}, not continuing...",
                             MessageBoxType.Error);
                });
                return;
            }

            Dictionary<string, string> parsedOptions = new Dictionary<string, string>();

            if(grpOptions.Content is StackLayout stkImageOptions)
                foreach(Control option in stkImageOptions.Children)
                {
                    if(cancel) break;

                    string value;

                    switch(option)
                    {
                        case CheckBox optBoolean:
                            value = optBoolean.Checked?.ToString();
                            break;
                        case NumericStepper optNumber:
                            value = optNumber.Value.ToString(CultureInfo.CurrentCulture);
                            break;
                        case TextBox optString:
                            value = optString.Text;
                            break;
                        default: continue;
                    }

                    string key = option.ID.Substring(3);

                    parsedOptions.Add(key, value);
                }

            Application.Instance.Invoke(() =>
            {
                lblProgress.Text           = "Creating output image";
                lblProgress2.Text          = "";
                prgProgress2.Indeterminate = true;
            });

            if(!outputFormat.Create(txtDestination.Text, inputFormat.Info.MediaType, parsedOptions,
                                    inputFormat.Info.Sectors, inputFormat.Info.SectorSize))
            {
                Application.Instance.Invoke(() =>
                {
                    MessageBox
                       .Show($"Error {outputFormat.ErrorMessage} creating output image.",
                             MessageBoxType.Error);
                });

                AaruConsole.ErrorWriteLine("Error {0} creating output image.", outputFormat.ErrorMessage);
                return;
            }

            Application.Instance.Invoke(() =>
            {
                lblProgress.Text = "Setting image metadata";
                prgProgress.Value++;
                lblProgress2.Text          = "";
                prgProgress2.Indeterminate = true;
            });

            ImageInfo metadata = new ImageInfo
            {
                Application           = "Aaru",
                ApplicationVersion    = Version.GetVersion(),
                Comments              = txtComments.Text,
                Creator               = txtCreator.Text,
                DriveFirmwareRevision = txtDriveFirmwareRevision.Text,
                DriveManufacturer     = txtDriveManufacturer.Text,
                DriveModel            = txtDriveModel.Text,
                DriveSerialNumber     = txtDriveSerialNumber.Text,
                LastMediaSequence     = (int)numLastMediaSequence.Value,
                MediaBarcode          = txtMediaBarcode.Text,
                MediaManufacturer     = txtMediaManufacturer.Text,
                MediaModel            = txtMediaModel.Text,
                MediaPartNumber       = txtMediaPartNumber.Text,
                MediaSequence         = (int)numMediaSequence.Value,
                MediaSerialNumber     = txtMediaSerialNumber.Text,
                MediaTitle            = txtMediaTitle.Text
            };

            if(!cancel)
                if(!outputFormat.SetMetadata(metadata))
                {
                    AaruConsole.ErrorWrite("Error {0} setting metadata, ", outputFormat.ErrorMessage);
                    if(chkForce.Checked != true)
                    {
                        Application.Instance.Invoke(() =>
                        {
                            MessageBox
                               .Show($"Error {outputFormat.ErrorMessage} setting metadata, not continuing...",
                                     MessageBoxType.Error);
                        });

                        AaruConsole.ErrorWriteLine("not continuing...");
                        return;
                    }

                    warning = true;
                    AaruConsole.ErrorWriteLine("continuing...");
                }

            if(tracks != null && !cancel && outputOptical != null)
            {
                Application.Instance.Invoke(() =>
                {
                    lblProgress.Text = "Setting tracks list";
                    prgProgress.Value++;
                    lblProgress2.Text          = "";
                    prgProgress2.Indeterminate = true;
                });

                if(!outputOptical.SetTracks(tracks))
                {
                    Application.Instance.Invoke(() =>
                    {
                        MessageBox
                           .Show($"Error {outputFormat.ErrorMessage} sending tracks list to output image.",
                                 MessageBoxType.Error);
                    });

                    AaruConsole.ErrorWriteLine("Error {0} sending tracks list to output image.",
                                              outputFormat.ErrorMessage);
                    return;
                }
            }

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(cancel) break;

                Application.Instance.Invoke(() =>
                {
                    lblProgress.Text = $"Converting media tag {mediaTag}";
                    prgProgress.Value++;
                    lblProgress2.Text          = "";
                    prgProgress2.Indeterminate = true;
                });

                if(chkForce.Checked == true && !outputFormat.SupportedMediaTags.Contains(mediaTag)) continue;

                byte[] tag = inputFormat.ReadDiskTag(mediaTag);
                if(outputFormat.WriteMediaTag(tag, mediaTag)) continue;

                if(chkForce.Checked == true)
                {
                    warning = true;
                    AaruConsole.ErrorWriteLine("Error {0} writing media tag, continuing...", outputFormat.ErrorMessage);
                }
                else
                {
                    Application.Instance.Invoke(() =>
                    {
                        MessageBox
                           .Show($"Error {outputFormat.ErrorMessage} writing media tag, not continuing...",
                                 MessageBoxType.Error);
                    });

                    AaruConsole.ErrorWriteLine("Error {0} writing media tag, not continuing...",
                                              outputFormat.ErrorMessage);
                    return;
                }
            }

            ulong doneSectors = 0;

            if(tracks == null && !cancel)
            {
                Application.Instance.Invoke(() =>
                {
                    lblProgress.Text =
                        $"Setting geometry to {inputFormat.Info.Cylinders} cylinders, {inputFormat.Info.Heads} heads and {inputFormat.Info.SectorsPerTrack} sectors per track";
                    prgProgress.Value++;
                    lblProgress2.Text          = "";
                    prgProgress2.Indeterminate = true;
                });

                if(!outputFormat.SetGeometry(inputFormat.Info.Cylinders, inputFormat.Info.Heads,
                                             inputFormat.Info.SectorsPerTrack))
                {
                    warning = true;
                    AaruConsole.ErrorWriteLine("Error {0} setting geometry, image may be incorrect, continuing...",
                                              outputFormat.ErrorMessage);
                }

                Application.Instance.Invoke(() =>
                {
                    lblProgress.Text = "Converting sectors";
                    prgProgress.Value++;
                    lblProgress2.Text          = "";
                    prgProgress2.Indeterminate = false;
                    prgProgress2.MaxValue      = (int)(inputFormat.Info.Sectors / numCount.Value);
                });

                while(doneSectors < inputFormat.Info.Sectors)
                {
                    if(cancel) break;

                    byte[] sector;

                    uint sectorsToDo;
                    if(inputFormat.Info.Sectors - doneSectors >= (ulong)numCount.Value)
                        sectorsToDo  = (uint)numCount.Value;
                    else sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                    ulong sectors = doneSectors;
                    Application.Instance.Invoke(() =>
                    {
                        lblProgress2.Text =
                            $"Converting sectors {sectors} to {sectors + sectorsToDo} ({sectors / (double)inputFormat.Info.Sectors:P2} done)";
                        ;
                        prgProgress2.Value = (int)(sectors / numCount.Value);
                    });

                    bool result;
                    if(useLong)
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSectorLong(doneSectors);
                            result = outputFormat.WriteSectorLong(sector, doneSectors);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsLong(doneSectors, sectorsToDo);
                            result = outputFormat.WriteSectorsLong(sector, doneSectors, sectorsToDo);
                        }
                    else
                    {
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSector(doneSectors);
                            result = outputFormat.WriteSector(sector, doneSectors);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectors(doneSectors, sectorsToDo);
                            result = outputFormat.WriteSectors(sector, doneSectors, sectorsToDo);
                        }
                    }

                    if(!result)
                        if(chkForce.Checked == true)
                        {
                            warning = true;
                            AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                      outputFormat.ErrorMessage, doneSectors);
                        }
                        else
                        {
                            Application.Instance.Invoke(() =>
                            {
                                MessageBox
                                   .Show($"Error {outputFormat.ErrorMessage} writing sector {doneSectors}, not continuing...",
                                         MessageBoxType.Error);
                            });

                            AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                      outputFormat.ErrorMessage, doneSectors);
                            return;
                        }

                    doneSectors += sectorsToDo;
                }

                Application.Instance.Invoke(() =>
                {
                    lblProgress2.Text =
                        $"Converting sectors {inputFormat.Info.Sectors} to {inputFormat.Info.Sectors} ({1.0:P2} done)";
                    ;
                    prgProgress2.Value = prgProgress2.MaxValue;
                });

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                {
                    if(!useLong || cancel) break;

                    switch(tag)
                    {
                        case SectorTagType.AppleSectorTag:
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubHeader:
                        case SectorTagType.CdSectorEdc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                        case SectorTagType.CdSectorEcc:
                            // This tags are inline in long sector
                            continue;
                    }

                    if(chkForce.Checked == true && !outputFormat.SupportedSectorTags.Contains(tag)) continue;

                    Application.Instance.Invoke(() =>
                    {
                        lblProgress.Text = $"Converting tag {tag}";
                        prgProgress.Value++;
                        lblProgress2.Text          = "";
                        prgProgress2.Indeterminate = false;
                        prgProgress2.MaxValue      = (int)(inputFormat.Info.Sectors / numCount.Value);
                    });

                    doneSectors = 0;
                    while(doneSectors < inputFormat.Info.Sectors)
                    {
                        if(cancel) break;

                        byte[] sector;

                        uint sectorsToDo;
                        if(inputFormat.Info.Sectors - doneSectors >= (ulong)numCount.Value)
                            sectorsToDo  = (uint)numCount.Value;
                        else sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                        ulong sectors = doneSectors;
                        Application.Instance.Invoke(() =>
                        {
                            lblProgress2.Text =
                                $"Converting tag {sectors / (double)inputFormat.Info.Sectors} for sectors {sectors} to {sectors + sectorsToDo} ({sectors / (double)inputFormat.Info.Sectors:P2} done)";
                            prgProgress2.Value = (int)(sectors / numCount.Value);
                        });

                        bool result;
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSectorTag(doneSectors, tag);
                            result = outputFormat.WriteSectorTag(sector, doneSectors, tag);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsTag(doneSectors, sectorsToDo, tag);
                            result = outputFormat.WriteSectorsTag(sector, doneSectors, sectorsToDo, tag);
                        }

                        if(!result)
                            if(chkForce.Checked == true)
                            {
                                warning = true;
                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                          outputFormat.ErrorMessage, doneSectors);
                            }
                            else
                            {
                                Application.Instance.Invoke(() =>
                                {
                                    MessageBox
                                       .Show($"Error {outputFormat.ErrorMessage} writing sector {doneSectors}, not continuing...",
                                             MessageBoxType.Error);
                                });

                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                          outputFormat.ErrorMessage, doneSectors);
                                return;
                            }

                        doneSectors += sectorsToDo;
                    }

                    Application.Instance.Invoke(() =>
                    {
                        lblProgress2.Text =
                            $"Converting tag {tag} for sectors {inputFormat.Info.Sectors} to {inputFormat.Info.Sectors} ({1.0:P2} done)";
                        prgProgress2.Value = prgProgress2.MaxValue;
                    });
                }
            }
            else
            {
                foreach(Track track in tracks)
                {
                    if(cancel) break;

                    doneSectors = 0;
                    ulong trackSectors = track.TrackEndSector - track.TrackStartSector + 1;

                    Application.Instance.Invoke(() =>
                    {
                        lblProgress.Text = $"Converting sectors in track {track.TrackSequence}";
                        prgProgress.Value++;
                        lblProgress2.Text          = "";
                        prgProgress2.Indeterminate = false;
                        prgProgress2.MaxValue      = (int)(trackSectors / numCount.Value);
                    });

                    while(doneSectors < trackSectors)
                    {
                        if(cancel) break;

                        byte[] sector;

                        uint sectorsToDo;
                        if(trackSectors - doneSectors >= (ulong)numCount.Value) sectorsToDo = (uint)numCount.Value;
                        else
                            sectorsToDo =
                                (uint)(trackSectors - doneSectors);

                        ulong sectors = doneSectors;
                        Application.Instance.Invoke(() =>
                        {
                            lblProgress2.Text =
                                $"Converting sectors {sectors + track.TrackStartSector} to {sectors + sectorsToDo + track.TrackStartSector} in track {track.TrackSequence} ({(sectors + track.TrackStartSector) / (double)inputFormat.Info.Sectors:P2} done)";
                            prgProgress2.Value = (int)(sectors / numCount.Value);
                        });

                        bool result;
                        if(useLong)
                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSectorLong(doneSectors           + track.TrackStartSector);
                                result = outputFormat.WriteSectorLong(sector, doneSectors + track.TrackStartSector);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectorsLong(doneSectors + track.TrackStartSector, sectorsToDo);
                                result = outputFormat.WriteSectorsLong(sector, doneSectors + track.TrackStartSector,
                                                                       sectorsToDo);
                            }
                        else
                        {
                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSector(doneSectors           + track.TrackStartSector);
                                result = outputFormat.WriteSector(sector, doneSectors + track.TrackStartSector);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectors(doneSectors + track.TrackStartSector, sectorsToDo);
                                result = outputFormat.WriteSectors(sector, doneSectors + track.TrackStartSector,
                                                                   sectorsToDo);
                            }
                        }

                        if(!result)
                            if(chkForce.Checked == true)
                            {
                                warning = true;
                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                          outputFormat.ErrorMessage, doneSectors);
                            }
                            else
                            {
                                Application.Instance.Invoke(() =>
                                {
                                    MessageBox
                                       .Show($"Error {outputFormat.ErrorMessage} writing sector {doneSectors}, not continuing...",
                                             MessageBoxType.Error);
                                });

                                return;
                            }

                        doneSectors += sectorsToDo;
                    }
                }

                Application.Instance.Invoke(() =>
                {
                    lblProgress2.Text =
                        $"Converting sectors {inputFormat.Info.Sectors} to {inputFormat.Info.Sectors} in track {tracks.Count} ({1.0:P2} done)";
                    prgProgress2.Value = prgProgress2.MaxValue;
                });

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t))
                {
                    if(!useLong || cancel) break;

                    switch(tag)
                    {
                        case SectorTagType.AppleSectorTag:
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubHeader:
                        case SectorTagType.CdSectorEdc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                        case SectorTagType.CdSectorEcc:
                            // This tags are inline in long sector
                            continue;
                    }

                    if(chkForce.Checked == true && !outputFormat.SupportedSectorTags.Contains(tag)) continue;

                    foreach(Track track in tracks)
                    {
                        if(cancel) break;

                        doneSectors = 0;
                        ulong  trackSectors = track.TrackEndSector - track.TrackStartSector + 1;
                        byte[] sector;
                        bool   result;

                        Application.Instance.Invoke(() =>
                        {
                            lblProgress.Text = $"Converting tag {tag} in track {track.TrackSequence}.";
                            prgProgress.Value++;
                            lblProgress2.Text          = "";
                            prgProgress2.Indeterminate = false;
                            prgProgress2.MaxValue      = (int)(trackSectors / numCount.Value);
                        });

                        switch(tag)
                        {
                            case SectorTagType.CdTrackFlags:
                            case SectorTagType.CdTrackIsrc:

                                sector = inputFormat.ReadSectorTag(track.TrackStartSector, tag);
                                result = outputFormat.WriteSectorTag(sector, track.TrackStartSector, tag);
                                if(!result)
                                    if(chkForce.Checked == true)
                                    {
                                        warning = true;
                                        AaruConsole.ErrorWriteLine("Error {0} writing tag, continuing...",
                                                                  outputFormat.ErrorMessage);
                                    }
                                    else
                                    {
                                        Application.Instance.Invoke(() =>
                                        {
                                            MessageBox
                                               .Show($"Error {outputFormat.ErrorMessage} writing tag, not continuing...",
                                                     MessageBoxType.Error);
                                        });

                                        return;
                                    }

                                continue;
                        }

                        while(doneSectors < trackSectors)
                        {
                            if(cancel) break;

                            uint sectorsToDo;
                            if(trackSectors - doneSectors >= (ulong)numCount.Value) sectorsToDo = (uint)numCount.Value;
                            else
                                sectorsToDo =
                                    (uint)(trackSectors - doneSectors);

                            ulong sectors = doneSectors;
                            Application.Instance.Invoke(() =>
                            {
                                lblProgress2.Text =
                                    $"Converting tag {tag} for sectors {sectors + track.TrackStartSector} to {sectors + sectorsToDo + track.TrackStartSector} in track {track.TrackSequence} ({(sectors + track.TrackStartSector) / (double)inputFormat.Info.Sectors:P2} done)";
                                prgProgress2.Value = (int)(sectors / numCount.Value);
                            });

                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSectorTag(doneSectors           + track.TrackStartSector, tag);
                                result = outputFormat.WriteSectorTag(sector, doneSectors + track.TrackStartSector, tag);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectorsTag(doneSectors + track.TrackStartSector, sectorsToDo,
                                                                    tag);
                                result = outputFormat.WriteSectorsTag(sector, doneSectors + track.TrackStartSector,
                                                                      sectorsToDo, tag);
                            }

                            if(!result)
                                if(chkForce.Checked == true)
                                {
                                    warning = true;
                                    AaruConsole.ErrorWriteLine("Error {0} writing tag for sector {1}, continuing...",
                                                              outputFormat.ErrorMessage, doneSectors);
                                }
                                else
                                {
                                    Application.Instance.Invoke(() =>
                                    {
                                        MessageBox
                                           .Show($"Error {outputFormat.ErrorMessage} writing tag for sector {doneSectors}, not continuing...",
                                                 MessageBoxType.Error);
                                    });

                                    return;
                                }

                            doneSectors += sectorsToDo;
                        }
                    }
                }
            }

            Application.Instance.Invoke(() =>
            {
                lblProgress2.Visible = false;
                prgProgress2.Visible = false;
            });

            bool ret = false;
            if(dumpHardware != null && !cancel)
            {
                Application.Instance.Invoke(() =>
                {
                    lblProgress.Text = "Writing dump hardware list to output image.";
                    prgProgress.Value++;
                });

                ret = outputFormat.SetDumpHardware(dumpHardware);
                if(!ret)
                    AaruConsole.WriteLine("Error {0} writing dump hardware list to output image.",
                                         outputFormat.ErrorMessage);
            }

            ret = false;
            if(cicmMetadata != null && !cancel)
            {
                Application.Instance.Invoke(() =>
                {
                    lblProgress.Text = "Writing CICM XML metadata to output image.";
                    prgProgress.Value++;
                });

                outputFormat.SetCicmMetadata(cicmMetadata);
                if(!ret)
                    AaruConsole.WriteLine("Error {0} writing CICM XML metadata to output image.",
                                         outputFormat.ErrorMessage);
            }

            Application.Instance.Invoke(() =>
            {
                lblProgress.Text          = "Closing output image.";
                prgProgress.Indeterminate = true;
            });

            if(cancel)
            {
                Application.Instance.Invoke(() =>
                {
                    MessageBox.Show("Operation canceled, the output file is not correct.", MessageBoxType.Error);
                    btnClose.Visible    = true;
                    btnStop.Visible     = false;
                    stkProgress.Visible = false;
                });

                return;
            }

            if(!outputFormat.Close())
            {
                Application.Instance.Invoke(() =>
                {
                    MessageBox
                       .Show($"Error {outputFormat.ErrorMessage} closing output image... Contents are not correct.",
                             MessageBoxType.Error);
                });

                return;
            }

            Application.Instance.Invoke(() =>
            {
                MessageBox.Show(warning
                                    ? "Some warnings happened. Check console for more information. Image should be correct."
                                    : "Image converted successfully.");

                btnClose.Visible    = true;
                btnStop.Visible     = false;
                stkProgress.Visible = false;
            });

            Statistics.AddCommand("convert-image");
        }

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        protected void OnBtnStop(object sender, EventArgs e)
        {
            cancel          = true;
            btnStop.Enabled = false;
        }

        void OnCmbFormatSelectedIndexChanged(object sender, EventArgs e)
        {
            txtDestination.Text = "";

            if(!(cmbFormat.SelectedValue is IWritableImage plugin))
            {
                grpOptions.Visible     = false;
                btnDestination.Enabled = false;
                return;
            }

            btnDestination.Enabled = true;

            if(!plugin.SupportedOptions.Any())
            {
                grpOptions.Content = null;
                grpOptions.Visible = false;
                return;
            }

            chkForce.Visible = false;
            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(plugin.SupportedMediaTags.Contains(mediaTag)) continue;

                chkForce.Visible = true;
                chkForce.Checked = true;
                break;
            }

            foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags)
            {
                if(plugin.SupportedSectorTags.Contains(sectorTag)) continue;

                chkForce.Visible = true;
                chkForce.Checked = true;
                break;
            }

            grpOptions.Visible = true;

            StackLayout stkImageOptions = new StackLayout {Orientation = Orientation.Vertical};

            foreach((string name, Type type, string description, object @default) option in plugin.SupportedOptions)
                switch(option.type.ToString())
                {
                    case "System.Boolean":
                        CheckBox optBoolean = new CheckBox();
                        optBoolean.ID      = "opt" + option.name;
                        optBoolean.Text    = option.description;
                        optBoolean.Checked = (bool)option.@default;
                        stkImageOptions.Items.Add(optBoolean);
                        break;
                    case "System.SByte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        StackLayout stkNumber = new StackLayout();
                        stkNumber.Orientation = Orientation.Horizontal;
                        NumericStepper optNumber = new NumericStepper();
                        optNumber.ID    = "opt" + option.name;
                        optNumber.Value = Convert.ToDouble(option.@default);
                        stkNumber.Items.Add(optNumber);
                        Label lblNumber = new Label();
                        lblNumber.Text = option.description;
                        stkNumber.Items.Add(lblNumber);
                        stkImageOptions.Items.Add(stkNumber);
                        break;
                    case "System.Byte":
                    case "System.UInt16":
                    case "System.UInt32":
                    case "System.UInt64":
                        StackLayout stkUnsigned = new StackLayout();
                        stkUnsigned.Orientation = Orientation.Horizontal;
                        NumericStepper optUnsigned = new NumericStepper();
                        optUnsigned.ID       = "opt" + option.name;
                        optUnsigned.MinValue = 0;
                        optUnsigned.Value    = Convert.ToDouble(option.@default);
                        stkUnsigned.Items.Add(optUnsigned);
                        Label lblUnsigned = new Label();
                        lblUnsigned.Text = option.description;
                        stkUnsigned.Items.Add(lblUnsigned);
                        stkImageOptions.Items.Add(stkUnsigned);
                        break;
                    case "System.Single":
                    case "System.Double":
                        StackLayout stkFloat = new StackLayout();
                        stkFloat.Orientation = Orientation.Horizontal;
                        NumericStepper optFloat = new NumericStepper();
                        optFloat.ID            = "opt" + option.name;
                        optFloat.DecimalPlaces = 2;
                        optFloat.Value         = Convert.ToDouble(option.@default);
                        stkFloat.Items.Add(optFloat);
                        Label lblFloat = new Label();
                        lblFloat.Text = option.description;
                        stkFloat.Items.Add(lblFloat);
                        stkImageOptions.Items.Add(stkFloat);
                        break;
                    case "System.Guid":
                        // TODO
                        break;
                    case "System.String":
                        StackLayout stkString = new StackLayout();
                        stkString.Orientation = Orientation.Horizontal;
                        Label lblString = new Label();
                        lblString.Text = option.description;
                        stkString.Items.Add(lblString);
                        TextBox optString = new TextBox();
                        optString.ID   = "opt" + option.name;
                        optString.Text = (string)option.@default;
                        stkString.Items.Add(optString);
                        stkImageOptions.Items.Add(stkString);
                        break;
                }

            grpOptions.Content = stkImageOptions;
        }

        void OnBtnDestinationClick(object sender, EventArgs e)
        {
            if(!(cmbFormat.SelectedValue is IWritableImage plugin)) return;

            SaveFileDialog dlgDestination = new SaveFileDialog {Title = "Choose destination file"};
            dlgDestination.Filters.Add(new FileFilter(plugin.Name, plugin.KnownExtensions.ToArray()));

            DialogResult result = dlgDestination.ShowDialog(this);

            if(result != DialogResult.Ok)
            {
                txtDestination.Text = "";
                return;
            }

            if(string.IsNullOrEmpty(Path.GetExtension(dlgDestination.FileName)))
                dlgDestination.FileName += plugin.KnownExtensions.First();

            txtDestination.Text = dlgDestination.FileName;
        }

        void OnBtnCreator(object sender, EventArgs e)
        {
            txtCreator.Text = inputFormat.Info.Creator;
        }

        void OnBtnMediaTitle(object sender, EventArgs e)
        {
            txtMediaTitle.Text = inputFormat.Info.MediaTitle;
        }

        void OnBtnComments(object sender, EventArgs e)
        {
            txtComments.Text = inputFormat.Info.Comments;
        }

        void OnBtnMediaManufacturer(object sender, EventArgs e)
        {
            txtMediaManufacturer.Text = inputFormat.Info.MediaManufacturer;
        }

        void OnBtnMediaModel(object sender, EventArgs e)
        {
            txtMediaModel.Text = inputFormat.Info.MediaModel;
        }

        void OnBtnMediaSerialNumber(object sender, EventArgs e)
        {
            txtMediaSerialNumber.Text = inputFormat.Info.MediaSerialNumber;
        }

        void OnBtnMediaBarcode(object sender, EventArgs e)
        {
            txtMediaBarcode.Text = inputFormat.Info.MediaBarcode;
        }

        void OnBtnMediaPartNumber(object sender, EventArgs e)
        {
            txtMediaPartNumber.Text = inputFormat.Info.MediaPartNumber;
        }

        void OnBtnMediaSequence(object sender, EventArgs e)
        {
            numMediaSequence.Value = inputFormat.Info.MediaSequence;
        }

        void OnBtnLastMediaSequence(object sender, EventArgs e)
        {
            numLastMediaSequence.Value = inputFormat.Info.LastMediaSequence;
        }

        void OnBtnDriveManufacturer(object sender, EventArgs e)
        {
            txtDriveManufacturer.Text = inputFormat.Info.DriveManufacturer;
        }

        void OnBtnDriveModel(object sender, EventArgs e)
        {
            txtDriveModel.Text = inputFormat.Info.DriveModel;
        }

        void OnBtnDriveSerialNumber(object sender, EventArgs e)
        {
            txtDriveSerialNumber.Text = inputFormat.Info.DriveSerialNumber;
        }

        void OnBtnDriveFirmwareRevision(object sender, EventArgs e)
        {
            txtDriveFirmwareRevision.Text = inputFormat.Info.DriveFirmwareRevision;
        }

        void OnBtnCicmXmlFromImageClick(object sender, EventArgs e)
        {
            txtCicmXml.Text = "<From image>";
            cicmMetadata    = inputFormat.CicmMetadata;
        }

        void OnBtnCicmXmlClick(object sender, EventArgs e)
        {
            cicmMetadata    = null;
            txtCicmXml.Text = "";
            OpenFileDialog dlgMetadata =
                new OpenFileDialog {Title = "Choose existing metadata sidecar", CheckFileExists = true};
            dlgMetadata.Filters.Add(new FileFilter("CICM XML metadata", ".xml"));

            DialogResult result = dlgMetadata.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            XmlSerializer sidecarXs = new XmlSerializer(typeof(CICMMetadataType));
            try
            {
                StreamReader sr = new StreamReader(dlgMetadata.FileName);
                cicmMetadata = (CICMMetadataType)sidecarXs.Deserialize(sr);
                sr.Close();
                txtCicmXml.Text = dlgMetadata.FileName;
            }
            catch { MessageBox.Show("Incorrect metadata sidecar file...", MessageBoxType.Error); }
        }

        void OnBtnResumeFileFromImageClick(object sender, EventArgs e)
        {
            txtResumeFile.Text = "<From image>";
            dumpHardware       = inputFormat.DumpHardware;
        }

        void OnBtnResumeFileClick(object sender, EventArgs e)
        {
            dumpHardware       = null;
            txtResumeFile.Text = "";
            OpenFileDialog dlgMetadata =
                new OpenFileDialog {Title = "Choose existing resume file", CheckFileExists = true};
            dlgMetadata.Filters.Add(new FileFilter("CICM XML metadata", ".xml"));

            DialogResult result = dlgMetadata.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            XmlSerializer sidecarXs = new XmlSerializer(typeof(Resume));
            try
            {
                StreamReader sr     = new StreamReader(dlgMetadata.FileName);
                Resume       resume = (Resume)sidecarXs.Deserialize(sr);

                if(resume.Tries != null && !resume.Tries.Any())
                {
                    dumpHardware       = resume.Tries;
                    txtResumeFile.Text = dlgMetadata.FileName;
                }
                else MessageBox.Show("Resume file does not contain dump hardware information...", MessageBoxType.Error);

                sr.Close();
            }
            catch { MessageBox.Show("Incorrect resume file...", MessageBoxType.Error); }
        }

        #region XAML IDs
        TextBox        txtSource;
        ComboBox       cmbFormat;
        TextBox        txtDestination;
        Button         btnDestination;
        StackLayout    stkOptions;
        NumericStepper numCount;
        Label          txtCount;
        CheckBox       chkForce;
        Label          lblCreator;
        TextBox        txtCreator;
        Button         btnCreator;
        GroupBox       grpMetadata;
        Label          lblMediaTitle;
        TextBox        txtMediaTitle;
        Button         btnMediaTitle;
        Label          lblMediaManufacturer;
        TextBox        txtMediaManufacturer;
        Button         btnMediaManufacturer;
        Label          lblMediaModel;
        TextBox        txtMediaModel;
        Button         btnMediaModel;
        Label          lblMediaSerialNumber;
        TextBox        txtMediaSerialNumber;
        Button         btnMediaSerialNumber;
        Label          lblMediaBarcode;
        TextBox        txtMediaBarcode;
        Button         btnMediaBarcode;
        Label          lblMediaPartNumber;
        TextBox        txtMediaPartNumber;
        Button         btnMediaPartNumber;
        Label          lblMediaSequence;
        NumericStepper numMediaSequence;
        Button         btnMediaSequence;
        Label          lblLastMediaSequence;
        NumericStepper numLastMediaSequence;
        Button         btnLastMediaSequence;
        Label          lblDriveManufacturer;
        TextBox        txtDriveManufacturer;
        Button         btnDriveManufacturer;
        Label          lblDriveModel;
        TextBox        txtDriveModel;
        Button         btnDriveModel;
        Label          lblDriveSerialNumber;
        TextBox        txtDriveSerialNumber;
        Button         btnDriveSerialNumber;
        Label          lblDriveFirmwareRevision;
        TextBox        txtDriveFirmwareRevision;
        Button         btnDriveFirmwareRevision;
        TextArea       txtComments;
        Button         btnComments;
        TextBox        txtCicmXml;
        Button         btnCicmXmlFromImage;
        Button         btnCicmXml;
        TextBox        txtResumeFile;
        Button         btnResumeFileFromImage;
        Button         btnResumeFile;
        GroupBox       grpOptions;
        StackLayout    stkProgress;
        StackLayout    stkProgress1;
        Label          lblProgress;
        ProgressBar    prgProgress;
        StackLayout    stkProgress2;
        Label          lblProgress2;
        ProgressBar    prgProgress2;
        Button         btnStart;
        Button         btnClose;
        Button         btnStop;
        #endregion
    }
}