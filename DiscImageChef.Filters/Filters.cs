// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Filters.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Enumerates all filters and instantiates the correct one.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using DiscImageChef.Console;

namespace DiscImageChef.Filters
{
    public class FiltersList
    {
        public SortedDictionary<string, Filter> filtersList;

        public FiltersList()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Filter));
            filtersList = new SortedDictionary<string, Filter>();

            foreach(Type type in assembly.GetTypes())
            {
                try
                {
                    if(type.IsSubclassOf(typeof(Filter)))
                    {
                        Filter filter = (Filter)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        if(!filtersList.ContainsKey(filter.Name.ToLower()))
                        {
                            filtersList.Add(filter.Name.ToLower(), filter);
                        }
                    }
                }
                catch(Exception exception)
                {
                    DicConsole.ErrorWriteLine("Exception {0}", exception);
                }
            }
        }

        public Filter GetFilter(string path)
        {
            Filter noFilter = null;

            foreach(Filter filter in filtersList.Values)
            {
                if(filter.UUID != new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                {
                    if(filter.Identify(path))
                    {
                        Filter foundFilter = (Filter)filter.GetType().GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        foundFilter.Open(path);

                        if(foundFilter.IsOpened())
                            return foundFilter;
                    }
                }
                else
                    noFilter = filter;
            }

            if(noFilter.Identify(path))
            {
                noFilter.Open(path);

                if(noFilter.IsOpened())
                    return noFilter;
            }

            return noFilter;
        }

        public SortedDictionary<string, Filter> GetFiltersList()
        {
            return filtersList;
        }
    }
}

