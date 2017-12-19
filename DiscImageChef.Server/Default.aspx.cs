// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Default.aspx.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Renders README.md.
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

using System;
using System.Web;
using System.Web.UI;
using Velyo.AspNet.Markdown;
using System.IO;

namespace DiscImageChef.Server
{

    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MarkdownContent mkdown = new MarkdownContent();
            StreamReader sr = new StreamReader(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "docs", "README.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mkdown.Content = mdcontent.Replace(".md)", ".aspx)");
            body.Controls.Add(mkdown);

            lblVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
