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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscImageChef.Database
{
    public sealed class DicContext : DbContext
    {
        public DicContext(DbContextOptions options) : base(options) { }

        public DbSet<Device>                Devices                { get; set; }
        public DbSet<Report>                Reports                { get; set; }
        public DbSet<Command>               Commands               { get; set; }
        public DbSet<Filesystem>            Filesystems            { get; set; }
        public DbSet<Filter>                Filters                { get; set; }
        public DbSet<MediaFormat>           MediaFormats           { get; set; }
        public DbSet<Partition>             Partitions             { get; set; }
        public DbSet<Media>                 Medias                 { get; set; }
        public DbSet<DeviceStat>            SeenDevices            { get; set; }
        public DbSet<OperatingSystem>       OperatingSystems       { get; set; }
        public DbSet<Version>               Versions               { get; set; }
        public DbSet<UsbVendor>             UsbVendors             { get; set; }
        public DbSet<UsbProduct>            UsbProducts            { get; set; }
        public DbSet<CdOffset>              CdOffsets              { get; set; }
        public DbSet<RemoteApplication>     RemoteApplications     { get; set; }
        public DbSet<RemoteArchitecture>    RemoteArchitectures    { get; set; }
        public DbSet<RemoteOperatingSystem> RemoteOperatingSystems { get; set; }

        // Note: If table does not appear check that last migration has been REALLY added to the project

        public static DicContext Create(string dbPath)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseLazyLoadingProxies().UseSqlite($"Data Source={dbPath}");

            return new DicContext(optionsBuilder.Options);
        }
    }
}