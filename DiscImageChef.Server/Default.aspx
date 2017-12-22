<%@ Page Language="C#" Inherits="DiscImageChef.Server.Default" %>
<%@ Register TagPrefix="velyo" Namespace="Velyo.AspNet.Markdown" Assembly="Velyo.AspNet.Markdown" %>
<%--
// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Default.aspx
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
--%>
<!DOCTYPE html>
<html>
<meta charset="UTF-8">
<head runat="server">
    <link href="dos.css" rel="stylesheet" type="text/css"/>
    <title>DiscImageChef</title>
</head>
<body id="body" runat="server">
<h1 align="center">Welcome to <i>
        <a href="http://github.com/claunia/discimagechef" target="_blank">DiscImageChef</a>
    </i> Server version <asp:Label id="lblVersion" runat="server"/>
</h1>
</body>
</html>