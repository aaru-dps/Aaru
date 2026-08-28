using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Localization;
using Aaru.Logging;
using Version = Aaru.CommonTypes.Interop.Version;


namespace Aaru.Core.Image;

public sealed partial class Merger
{
    /// <summary>Reads a resume file.</summary>
    /// <param name="resumeFilePath">Path to read, or null when there is nothing to read.</param>
    /// <param name="explicitlyRequested">
    ///     Whether the user named this file on the command line. A file the user asked for has to be usable, so a
    ///     missing or unparseable one stops the merge. One we merely found next to the image is only a preference over
    ///     the dump hardware embedded in the image, so we warn and fall back to that instead.
    /// </param>
    private (bool success, Resume resume) LoadMetadata(string resumeFilePath, bool explicitlyRequested)
    {
        Resume resume;

        if(resumeFilePath == null) return (true, null);

        if(File.Exists(resumeFilePath))
        {
            try
            {
                if(resumeFilePath.EndsWith(".resume.json", StringComparison.CurrentCultureIgnoreCase))
                {
                    var fs = new FileStream(resumeFilePath, FileMode.Open);

                    resume =
                        (JsonSerializer.Deserialize(fs, typeof(ResumeJson), ResumeJsonContext.Default) as ResumeJson)
                      ?.Resume;

                    fs.Close();
                }
                else
                {
                    // Bypassed by JSON source generator used above
#pragma warning disable IL2026
                    var xs = new XmlSerializer(typeof(Resume));
#pragma warning restore IL2026

                    var sr = new StreamReader(resumeFilePath);

                    // Bypassed by JSON source generator used above
#pragma warning disable IL2026
                    resume = (Resume)xs.Deserialize(sr);
#pragma warning restore IL2026

                    sr.Close();
                }
            }
            catch(Exception ex)
            {
                if(explicitlyRequested)
                {
                    StoppingErrorMessage?.Invoke(UI.Incorrect_resume_file_not_continuing);
                    AaruLogging.Exception(ex, UI.Incorrect_resume_file_not_continuing);

                    return (false, null);
                }

                ErrorMessage?.Invoke(UI.Incorrect_resume_file_cannot_use_it);
                AaruLogging.Exception(ex, UI.Incorrect_resume_file_cannot_use_it);

                return (true, null);
            }
        }
        else
        {
            if(!explicitlyRequested) return (true, null);

            StoppingErrorMessage?.Invoke(UI.Could_not_find_resume_file);

            return (false, null);
        }

        return (true, resume);
    }

    ErrorNumber SetImageMetadata(IMediaImage primaryImage, IMediaImage secondaryImage, IWritableImage outputImage)
    {
        if(_aborted) return ErrorNumber.NoError;

        var imageInfo = new CommonTypes.Structs.ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetInformationalVersion(),
            Comments           = comments ?? primaryImage.Info.Comments ?? secondaryImage.Info.Comments,
            Creator            = creator  ?? primaryImage.Info.Creator  ?? secondaryImage.Info.Creator,
            DriveFirmwareRevision =
                driveFirmwareRevision ??
                primaryImage.Info.DriveFirmwareRevision ?? secondaryImage.Info.DriveFirmwareRevision,
            DriveManufacturer =
                driveManufacturer ?? primaryImage.Info.DriveManufacturer ?? secondaryImage.Info.DriveManufacturer,
            DriveModel = driveModel ?? primaryImage.Info.DriveModel ?? secondaryImage.Info.DriveModel,
            DriveSerialNumber =
                driveSerialNumber ?? primaryImage.Info.DriveSerialNumber ?? secondaryImage.Info.DriveSerialNumber,
            LastMediaSequence = lastMediaSequence != 0
                                    ? lastMediaSequence
                                    : primaryImage.Info.LastMediaSequence != 0
                                        ? primaryImage.Info.LastMediaSequence
                                        : secondaryImage.Info.LastMediaSequence,
            MediaBarcode = mediaBarcode ?? primaryImage.Info.MediaBarcode ?? secondaryImage.Info.MediaBarcode,
            MediaManufacturer =
                mediaManufacturer ?? primaryImage.Info.MediaManufacturer ?? secondaryImage.Info.MediaManufacturer,
            MediaModel = mediaModel ?? primaryImage.Info.MediaModel ?? secondaryImage.Info.MediaModel,
            MediaPartNumber =
                mediaPartNumber ?? primaryImage.Info.MediaPartNumber ?? secondaryImage.Info.MediaPartNumber,
            MediaSequence = mediaSequence != 0
                                ? mediaSequence
                                : primaryImage.Info.MediaSequence != 0
                                    ? primaryImage.Info.MediaSequence
                                    : secondaryImage.Info.MediaSequence,
            MediaSerialNumber =
                mediaSerialNumber ?? primaryImage.Info.MediaSerialNumber ?? secondaryImage.Info.MediaSerialNumber,
            MediaTitle = mediaTitle ?? primaryImage.Info.MediaTitle ?? secondaryImage.Info.MediaTitle
        };

        if(outputImage.SetImageInfo(imageInfo)) return ErrorNumber.NoError;

        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_setting_metadata_not_continuing,
                                                   outputImage.ErrorMessage));

        return ErrorNumber.WriteError;
    }
}