using System;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SCSI;
using Aaru.Devices;

namespace Aaru.Core.Devices.Dumping
{
    public partial class Dump
    {
        static readonly byte[] FatSignature =
        {
            0x46, 0x41, 0x54, 0x31, 0x36, 0x20, 0x20, 0x20
        };
        static readonly byte[] IsoExtension =
        {
            0x49, 0x53, 0x4F
        };

        /// <summary>Dumps a CFW PlayStation Portable UMD</summary>
        void PlayStationPortable()
        {
            if(!_outputPlugin.SupportedMediaTypes.Contains(MediaType.MemoryStickDuo)    &&
               !_outputPlugin.SupportedMediaTypes.Contains(MediaType.MemoryStickProDuo) &&
               !_outputPlugin.SupportedMediaTypes.Contains(MediaType.UMD))
            {
                _dumpLog.WriteLine("Selected output plugin does not support MemoryStick Duo or UMD, cannot dump...");

                StoppingErrorMessage?.
                    Invoke("Selected output plugin does not support MemoryStick Duo or UMD, cannot dump...");

                return;
            }

            UpdateStatus?.Invoke("Checking if media is UMD or MemoryStick...");
            _dumpLog.WriteLine("Checking if media is UMD or MemoryStick...");

            bool sense = _dev.ModeSense6(out byte[] buffer, out _, false, ScsiModeSensePageControl.Current, 0,
                                         _dev.Timeout, out _);

            if(sense)
            {
                _dumpLog.WriteLine("Could not get MODE SENSE...");
                StoppingErrorMessage?.Invoke("Could not get MODE SENSE...");

                return;
            }

            Modes.DecodedMode? decoded = Modes.DecodeMode6(buffer, PeripheralDeviceTypes.DirectAccess);

            if(!decoded.HasValue)
            {
                _dumpLog.WriteLine("Could not decode MODE SENSE...");
                StoppingErrorMessage?.Invoke("Could not decode MODE SENSE...");

                return;
            }

            // UMDs are always write protected
            if(!decoded.Value.Header.WriteProtected)
            {
                DumpMs();

                return;
            }

            sense = _dev.Read12(out buffer, out _, 0, false, true, false, false, 0, 512, 0, 1, false, _dev.Timeout,
                                out _);

            if(sense)
            {
                _dumpLog.WriteLine("Could not read...");
                StoppingErrorMessage?.Invoke("Could not read...");

                return;
            }

            byte[] tmp = new byte[8];

            Array.Copy(buffer, 0x36, tmp, 0, 8);

            // UMDs are stored inside a FAT16 volume
            if(!tmp.SequenceEqual(FatSignature))
            {
                DumpMs();

                return;
            }

            ushort fatStart      = (ushort)((buffer[0x0F] << 8) + buffer[0x0E]);
            ushort sectorsPerFat = (ushort)((buffer[0x17] << 8) + buffer[0x16]);
            ushort rootStart     = (ushort)((sectorsPerFat * 2) + fatStart);

            UpdateStatus?.Invoke($"Reading root directory in sector {rootStart}...");
            _dumpLog.WriteLine("Reading root directory in sector {0}...", rootStart);

            sense = _dev.Read12(out buffer, out _, 0, false, true, false, false, rootStart, 512, 0, 1, false,
                                _dev.Timeout, out _);

            if(sense)
            {
                StoppingErrorMessage?.Invoke("Could not read...");
                _dumpLog.WriteLine("Could not read...");

                return;
            }

            tmp = new byte[3];
            Array.Copy(buffer, 0x28, tmp, 0, 3);

            if(!tmp.SequenceEqual(IsoExtension))
            {
                DumpMs();

                return;
            }

            UpdateStatus?.Invoke($"FAT starts at sector {fatStart} and runs for {sectorsPerFat} sectors...");
            _dumpLog.WriteLine("FAT starts at sector {0} and runs for {1} sectors...", fatStart, sectorsPerFat);

            UpdateStatus?.Invoke("Reading FAT...");
            _dumpLog.WriteLine("Reading FAT...");

            byte[] fat = new byte[sectorsPerFat * 512];

            uint position = 0;

            while(position < sectorsPerFat)
            {
                uint transfer = 64;

                if(transfer + position > sectorsPerFat)
                    transfer = sectorsPerFat - position;

                sense = _dev.Read12(out buffer, out _, 0, false, true, false, false, position + fatStart, 512, 0,
                                    transfer, false, _dev.Timeout, out _);

                if(sense)
                {
                    StoppingErrorMessage?.Invoke("Could not read...");
                    _dumpLog.WriteLine("Could not read...");

                    return;
                }

                Array.Copy(buffer, 0, fat, position * 512, transfer * 512);

                position += transfer;
            }

            UpdateStatus?.Invoke("Traversing FAT...");
            _dumpLog.WriteLine("Traversing FAT...");

            ushort previousCluster = BitConverter.ToUInt16(fat, 4);

            for(int i = 3; i < fat.Length / 2; i++)
            {
                ushort nextCluster = BitConverter.ToUInt16(fat, i * 2);

                if(nextCluster == previousCluster + 1)
                {
                    previousCluster = nextCluster;

                    continue;
                }

                if(nextCluster == 0xFFFF)
                    break;

                DumpMs();

                return;
            }

            if(_outputPlugin is IWritableOpticalImage)
                DumpUmd();
            else
                StoppingErrorMessage?.Invoke("The specified plugin does not support storing optical disc images.");
        }
    }
}