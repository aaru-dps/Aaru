// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ContextFactory.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Entity framework database context factory.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using Microsoft.EntityFrameworkCore.Design;

namespace Aaru.Database
{
    /// <inheritdoc />
    /// <summary>Database context factory, for design time</summary>
    public class AaruContextFactory : IDesignTimeDbContextFactory<AaruContext>
    {
        /// <inheritdoc />
        /// <summary>Creates a database context</summary>
        /// <param name="args">Ignored parameters</param>
        /// <returns>A database context</returns>
        public AaruContext CreateDbContext(string[] args) => AaruContext.Create("aaru.db");
    }
}