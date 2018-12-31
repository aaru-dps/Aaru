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
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.CommonTypes
{
    public class FiltersList
    {
        public SortedDictionary<string, IFilter> Filters;

        /// <summary>
        ///     Fills the list of all known filters
        /// </summary>
        public FiltersList()
        {
            Assembly assembly = Assembly.Load("DiscImageChef.Filters");
            Filters = new SortedDictionary<string, IFilter>();

            foreach(Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IFilter))))
                try
                {
                    IFilter filter = (IFilter)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                    if(filter != null && !Filters.ContainsKey(filter.Name.ToLower()))
                        Filters.Add(filter.Name.ToLower(), filter);
                }
                catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }
        }

        /// <summary>
        ///     Gets the filter that allows to read the specified path
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>The filter that allows reading the specified path</returns>
        public IFilter GetFilter(string path)
        {
            try
            {
                IFilter noFilter = null;
                foreach(IFilter filter in Filters.Values)
                    if(filter.Id != new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                    {
                        if(!filter.Identify(path)) continue;

                        IFilter foundFilter =
                            (IFilter)filter.GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });

                        foundFilter?.Open(path);

                        if(foundFilter?.IsOpened() == true) return foundFilter;
                    }
                    else
                        noFilter = filter;

                if(!noFilter?.Identify(path) == true) return noFilter;

                noFilter?.Open(path);

                return noFilter;
            }
            catch(IOException) { return null; }
        }

        /// <summary>
        ///     Gets all known filters
        /// </summary>
        /// <returns>Known filters</returns>
        public SortedDictionary<string, IFilter> GetFiltersList() => Filters;
    }
}