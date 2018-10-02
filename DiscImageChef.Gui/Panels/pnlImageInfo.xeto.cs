// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlDeviceInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the device information panel.
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
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Panels
{
    public class pnlImageInfo : Panel
    {
        public pnlImageInfo(string imagePath, IFilter filter, IMediaImage imageFormat)
        {
            XamlReader.Load(this);

            lblImagePath.Text   = $"Path: {imagePath}";
            lblFilter.Text      = $"Filter: {filter.Name}";
            lblImageFormat.Text = $"Image format identified by {imageFormat.Name} ({imageFormat.Id}).";

            lblImageFormat.Text = !string.IsNullOrWhiteSpace(imageFormat.Info.Version)
                                      ? $"Format: {imageFormat.Format} version {imageFormat.Info.Version}"
                                      : $"Format: {imageFormat.Format}";

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application) &&
               !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion))
                lblApplication.Text =
                    $"Was created with {imageFormat.Info.Application} version {imageFormat.Info.ApplicationVersion}";
            else if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application))
                lblApplication.Text = $"Was created with {imageFormat.Info.Application}";
            lblImageSize.Text = $"Image without headers is {imageFormat.Info.ImageSize} bytes long";
            lblSectors.Text =
                $"Contains a media of {imageFormat.Info.Sectors} sectors with a maximum sector size of {imageFormat.Info.SectorSize} bytes (if all sectors are of the same size this would be {imageFormat.Info.Sectors * imageFormat.Info.SectorSize} bytes)";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
                lblCreator.Text = $"Created by: {imageFormat.Info.Creator}";
            if(imageFormat.Info.CreationTime != DateTime.MinValue)
                lblCreationTime.Text = $"Created on {imageFormat.Info.CreationTime}";
            if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
                lblLastModificationTime.Text = $"Last modified on {imageFormat.Info.LastModificationTime}";
            lblMediaType.Text =
                $"Contains a media of type {imageFormat.Info.MediaType} and XML type {imageFormat.Info.XmlMediaType}";
            lblHasPartitions.Text = $"{(imageFormat.Info.HasPartitions ? "Has" : "Doesn't have")} partitions";
            lblHasSessions.Text   = $"{(imageFormat.Info.HasSessions ? "Has" : "Doesn't have")} sessions";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Comments)) txtComments.Text = imageFormat.Info.Comments;
            if(imageFormat.Info.MediaSequence != 0 && imageFormat.Info.LastMediaSequence != 0)
                lblMediaSequence.Text =
                    $"Media is number {imageFormat.Info.MediaSequence} on a set of {imageFormat.Info.LastMediaSequence} medias";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
                lblMediaTitle.Text = $"Media title: {imageFormat.Info.MediaTitle}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
                lblMediaManufacturer.Text = $"Media manufacturer: {imageFormat.Info.MediaManufacturer}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
                lblMediaModel.Text = $"Media model: {imageFormat.Info.MediaModel}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
                lblMediaSerialNumber.Text = $"Media serial number: {imageFormat.Info.MediaSerialNumber}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
                lblMediaBarcode.Text = $"Media barcode: {imageFormat.Info.MediaBarcode}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
                lblMediaPartNumber.Text = $"Media part number: {imageFormat.Info.MediaPartNumber}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
                lblDriveManufacturer.Text = $"Drive manufacturer: {imageFormat.Info.DriveManufacturer}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
                lblDriveModel.Text = $"Drive model: {imageFormat.Info.DriveModel}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
                lblDriveSerialNumber.Text = $"Drive serial number: {imageFormat.Info.DriveSerialNumber}";
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
                lblDriveFirmwareRevision.Text = $"Drive firmware info: {imageFormat.Info.DriveFirmwareRevision}";
            if(imageFormat.Info.Cylinders       > 0 && imageFormat.Info.Heads > 0 &&
               imageFormat.Info.SectorsPerTrack > 0 &&
               imageFormat.Info.XmlMediaType    != XmlMediaType.OpticalDisc)
                lblMediaGeometry.Text =
                    $"Media geometry: {imageFormat.Info.Cylinders} cylinders, {imageFormat.Info.Heads} heads, {imageFormat.Info.SectorsPerTrack} sectors per track";
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        Label    lblImagePath;
        Label    lblFilter;
        Label    lblImageFormat;
        Label    lblApplication;
        Label    lblImageSize;
        Label    lblSectors;
        Label    lblCreator;
        Label    lblCreationTime;
        Label    lblLastModificationTime;
        Label    lblMediaType;
        Label    lblHasPartitions;
        Label    lblHasSessions;
        Label    lblComments;
        TextArea txtComments;
        Label    lblMediaSequence;
        Label    lblMediaTitle;
        Label    lblMediaManufacturer;
        Label    lblMediaModel;
        Label    lblMediaSerialNumber;
        Label    lblMediaBarcode;
        Label    lblMediaPartNumber;
        Label    lblDriveManufacturer;
        Label    lblDriveModel;
        Label    lblDriveSerialNumber;
        Label    lblDriveFirmwareRevision;
        Label    lblMediaGeometry;

        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}