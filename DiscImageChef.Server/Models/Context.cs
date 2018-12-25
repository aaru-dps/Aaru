// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Entity framework database context.
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

using System.Data.Entity;
using MySql.Data.EntityFramework;

namespace DiscImageChef.Server.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public sealed class DicServerContext : DbContext
    {
        public DbSet<Device>            Devices          { get; set; }
        public DbSet<UploadedReport>    Reports          { get; set; }
        public DbSet<Command>           Commands         { get; set; }
        public DbSet<DeviceStat>        DeviceStats      { get; set; }
        public DbSet<Filesystem>        Filesystems      { get; set; }
        public DbSet<Filter>            Filters          { get; set; }
        public DbSet<Media>             Medias           { get; set; }
        public DbSet<MediaFormat>       MediaFormats     { get; set; }
        public DbSet<OperatingSystem>   OperatingSystems { get; set; }
        public DbSet<Partition>         Partitions       { get; set; }
        public DbSet<Version>           Versions         { get; set; }
        public DbSet<UsbVendor>         UsbVendors       { get; set; }
        public DbSet<UsbProduct>        UsbProducts      { get; set; }
        public DbSet<CompactDiscOffset> CdOffsets        { get; set; }
    }
}