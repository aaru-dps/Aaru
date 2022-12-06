// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Partitions.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Logic to handle name=value option pairs.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Aaru.Core
{
    /// <summary>Option parsing</summary>
    public static class Options
    {
        /// <summary>Parses a string with options</summary>
        /// <param name="options">Options string</param>
        /// <returns>Options name-value dictionary</returns>
        public static Dictionary<string, string> Parse(string options)
        {
            Dictionary<string, string> parsed  = new Dictionary<string, string>();
            bool                       escaped = false;
            bool                       quoted  = false;
            bool                       inValue = false;
            string                     name    = null;
            string                     value;
            var                        sb = new StringBuilder();

            if(options == null)
                return parsed;

            for(int index = 0; index < options.Length; index++)
            {
                char c = options[index];

                switch(c)
                {
                    case '\\' when !escaped:
                        escaped = true;

                        break;
                    case '"' when !escaped:
                        quoted = !quoted;

                        break;
                    case '=' when quoted:
                        sb.Append(c);

                        break;
                    case '=':
                        name    = sb.ToString().ToLower(CultureInfo.CurrentCulture);
                        sb      = new StringBuilder();
                        inValue = true;

                        break;
                    case ',' when quoted:
                        sb.Append(c);

                        break;
                    case ',' when inValue:
                        value   = sb.ToString();
                        sb      = new StringBuilder();
                        inValue = false;

                        if(string.IsNullOrEmpty(name) ||
                           string.IsNullOrEmpty(value))
                            continue;

                        if(parsed.ContainsKey(name))
                            parsed.Remove(name);

                        parsed.Add(name, value);

                        break;
                    default:
                        if(escaped)
                            switch(c)
                            {
                                case 'a':
                                    sb.Append('\a');
                                    escaped = false;

                                    break;
                                case 'b':
                                    sb.Append('\b');
                                    escaped = false;

                                    break;
                                case 'f':
                                    sb.Append('\f');
                                    escaped = false;

                                    break;
                                case 'n':
                                    sb.Append('\n');
                                    escaped = false;

                                    break;
                                case 'r':
                                    sb.Append('\r');
                                    escaped = false;

                                    break;
                                case 't':
                                    sb.Append('\t');
                                    escaped = false;

                                    break;
                                case 'v':
                                    sb.Append('\v');
                                    escaped = false;

                                    break;
                                case '\\':
                                    sb.Append('\\');
                                    escaped = false;

                                    break;
                                case '\'':
                                    sb.Append('\'');
                                    escaped = false;

                                    break;
                                case '"':
                                    sb.Append('"');
                                    escaped = false;

                                    break;
                                case '0':
                                    sb.Append('\0');
                                    escaped = false;

                                    break;
                                case 'u':
                                    string unicode = options.Substring(index + 1, 4);
                                    sb.Append((char)int.Parse(unicode, NumberStyles.HexNumber));
                                    escaped =  false;
                                    index   += 4;

                                    break;
                                case 'U':
                                    string longUnicode = options.Substring(index + 1, 8);
                                    sb.Append((char)int.Parse(longUnicode, NumberStyles.HexNumber));
                                    escaped =  false;
                                    index   += 8;

                                    break;
                                default:
                                    sb.Append(c);
                                    escaped = false;

                                    break;
                            }
                        else
                            sb.Append(c);

                        break;
                }
            }

            if(!inValue)
                return parsed;

            value = sb.ToString();

            if(string.IsNullOrEmpty(name) ||
               string.IsNullOrEmpty(value))
                return parsed;

            if(parsed.ContainsKey(name))
                parsed.Remove(name);

            parsed.Add(name, value);

            return parsed;
        }
    }
}