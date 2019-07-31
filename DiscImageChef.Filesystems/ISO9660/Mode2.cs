using System.IO;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        byte[] ReadSectors(ulong start, uint count)
        {
            MemoryStream ms = new MemoryStream();

            for(ulong sector = start; sector < start + count; sector++)
            {
                byte[] data = image.ReadSector(sector);

                switch(data.Length)
                {
                    case 2048:
                        // Mode 1 sector
                        ms.Write(data, 0, 2048);
                        break;
                    case 2352:
                        // Not a data sector
                        if(data[0] != 0    || data[1] != 0xFF || data[2] != 0xFF || data[3] != 0xFF ||
                           data[4] != 0xFF ||
                           data[5] != 0xFF || data[6]  != 0xFF || data[7]  != 0xFF || data[8] != 0xFF ||
                           data[9] != 0xFF || data[10] != 0xFF || data[11] != 0x00)
                        {
                            ms.Write(data, 0, 2352);
                            break;
                        }

                        switch(data[15])
                        {
                            // Mode 0 sector
                            case 0:
                                ms.Write(new byte[2048], 0, 2048);
                                break;
                            // Mode 1 sector
                            case 1:
                                ms.Write(data, 16, 2048);
                                break;
                            case 2:
                                // Mode 2 form 1 sector
                                if((data[16] & MODE2_FORM2) != 0)
                                {
                                    ms.Write(data, 24, 2048);
                                    break;
                                }

                                // Mode 2 form 2 sector
                                ms.Write(data, 24, 2324);
                                break;
                            // Unknown, audio?
                            default:
                                ms.Write(data, 0, 2352);
                                break;
                        }

                        break;
                    case 2336:
                        // Mode 2 form 1 sector
                        if((data[16] & MODE2_FORM2) == 0)
                        {
                            ms.Write(data, 8, 2048);
                            break;
                        }

                        // Mode 2 form 2 sector
                        ms.Write(data, 8, 2324);
                        break;
                    // Should not happen, but, just in case
                    default:
                        ms.Write(data, 0, data.Length);
                        break;
                }
            }

            return ms.ToArray();
        }
    }
}